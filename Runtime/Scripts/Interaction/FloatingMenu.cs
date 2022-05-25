using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IVLab.MinVR3
{

    /// <summary>
    /// Simple 3D menu that floats in space and is activated by placing a tracked cursor
    /// inside the titlebar or box that holds each menu item and then clicking.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("MinVR/Interaction/Floating Menu")]
    public class FloatingMenu : MonoBehaviour, IVREventListener
    {
        [Header("Menu Items")]
        public string title = "My Menu";
        public List<string> menuItems = new List<string>() { "Item 1", "Item 2" };

        [Header("Input")]
        [Tooltip("[Optional] If set, the menu will only respond to input when the token is available (i.e., not already held by someone else).")]
        public SharedToken inputFocusToken;

        [Tooltip("The cursor position event.")]
        public VREventPrototypeVector3 cursorPositionEvent = new VREventPrototypeVector3();

        [Tooltip("[Optional] The cursor rotation event if you want to be able to grab the menu bar and rotate the menu.")]
        public VREventPrototypeQuaternion cursorRotationEvent = new VREventPrototypeQuaternion();

        [Tooltip("The cursor selection activate event (i.e., primary button down).")]
        public VREventPrototype buttonDownEvent = new VREventPrototype();

        [Tooltip("The cursor selection deactivate event (i.e., primary button up).")]
        public VREventPrototype buttonUpEvent = new VREventPrototype();

        [Header("OnSelectionMade")]
        [Tooltip("Register a function with this event to receive a callback when a selection is made, the int argument is the index of the menu item that was selected.")]
        public VRCallbackInt onMenuItemSelected = new VRCallbackInt();


        [Header("Appearance")]
        public Font font;
        public Material fontMaterial;

        public Color titleColor = new Color(1.0f, 1.0f, 1.0f);
        public Color titleBGColor = new Color(85.0f / 255.0f, 83.0f / 255.0f, 83.0f / 255.0f);
        public Color titleHighColor = new Color(19.0f / 255.0f, 0.0f / 255.0f, 239.0f / 255.0f);

        public Color itemColor = new Color(0.0f, 0.0f, 0.0f);
        public Color itemBGColor = new Color(230.0f / 255.0f, 230.0f / 255.0f, 221.0f / 221.0f);
        public Color itemHighColor = new Color(255.0f / 255.0f, 195.0f / 255.0f, 0.0f / 255.0f);

        public Color pressColor = new Color(226.0f / 255.0f, 0.0f / 255.0f, 23.0f / 255.0f);

        public float textSizeInWorldUnits = 0.2f;
        public float itemSep = 0.08f;
        public Vector2 padding = new Vector2(0.05f, 0.05f);
        public float depth = 0.1f;
        public float zEpsilon = 0.001f;




        Vector2 TextExtents(TextMesh textMesh)
        {
            // https://forum.unity.com/threads/computing-exact-size-of-text-line-with-textmesh.485767/
            Vector2 extents = new Vector2();
            foreach (char symbol in textMesh.text) {
                CharacterInfo info;
                if (textMesh.font.GetCharacterInfo(symbol, out info, textMesh.fontSize, textMesh.fontStyle)) {
                    extents[0] += info.advance;
                    if (info.glyphHeight > extents[1]) {
                        extents[1] = info.glyphHeight;
                    }
                }
            }
            extents[0] = extents[0] * textMesh.characterSize * 0.1f;
            extents[1] = extents[1] * textMesh.characterSize * 0.1f;
            return extents;
        }

        // true if point p lies inside a Cube primitive that has been translated,
        // rotated, and scaled to create a rectangular box, like a 3D button
        bool InsideTransformedCube(Vector3 p, GameObject boxObj)
        {
            Vector3 pBoxSpace = boxObj.transform.InverseTransformPoint(p);
            return (Mathf.Abs(pBoxSpace[0]) <= 0.5) &&
                (Mathf.Abs(pBoxSpace[1]) <= 0.5) &&
                (Mathf.Abs(pBoxSpace[2]) <= 0.5);
        }

        // Start is called before the first frame update
        void Start()
        {
            RebuildMenu();
        }

        void OnValidate()
        {
            dirty = true;
        }

        void RebuildMenu()
        {
            Material tmpMat;

            // Remove any menu geometry previously created
            labelMeshes = new List<TextMesh>();
            labelBoxes = new List<GameObject>();
            titleBoxObj = null;
            bgBox = null;
            for (int i = this.transform.childCount; i > 0; --i) {
                DestroyImmediate(this.transform.GetChild(0).gameObject);
            }


            // Create a title box and label
            GameObject titleTextObj = new GameObject(title + " Label");
            titleTextObj.transform.SetParent(this.transform);
            TextMesh titleTextMesh = titleTextObj.AddComponent<TextMesh>();
            titleTextMesh.font = font;
            titleTextMesh.GetComponent<MeshRenderer>().sharedMaterial = new Material(fontMaterial);
            titleTextMesh.text = title.ToUpper();
            titleTextMesh.color = titleColor;
            titleTextMesh.anchor = TextAnchor.MiddleLeft;
            titleTextMesh.fontSize = 100;
            titleTextMesh.characterSize = textSizeInWorldUnits * 10.0f / titleTextMesh.fontSize;


            titleBoxObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            titleBoxObj.name = title + " Box";
            titleBoxObj.transform.SetParent(this.transform);
            tmpMat = new Material(titleBoxObj.GetComponent<Renderer>().sharedMaterial);
            tmpMat.color = titleBGColor;
            titleBoxObj.GetComponent<Renderer>().sharedMaterial = tmpMat;


            // Create a box and label for each item
            for (int i = 0; i < menuItems.Count; i++) {
                GameObject textObj = new GameObject(menuItems[i] + " Label");
                textObj.transform.SetParent(this.transform);
                TextMesh textMesh = textObj.AddComponent<TextMesh>();
                textMesh.font = font;
                textMesh.GetComponent<MeshRenderer>().sharedMaterial = new Material(fontMaterial);
                textMesh.text = menuItems[i];
                textMesh.color = itemColor;
                textMesh.anchor = TextAnchor.MiddleLeft;
                textMesh.fontSize = 100;
                textMesh.characterSize = textSizeInWorldUnits * 10.0f / textMesh.fontSize;

                GameObject boxObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boxObj.name = menuItems[i] + " Box";
                boxObj.transform.SetParent(this.transform);
                tmpMat = new Material(boxObj.GetComponent<Renderer>().sharedMaterial);
                tmpMat.color = itemBGColor;
                boxObj.GetComponent<Renderer>().sharedMaterial = tmpMat;

                labelMeshes.Add(textMesh);
                labelBoxes.Add(boxObj);
            }


            // Calculate the max extents
            Vector2 max_text_extents = new Vector2();
            for (int i = 0; i < labelMeshes.Count; i++) {
                Vector2 text_extents = TextExtents(labelMeshes[i]);
                max_text_extents[0] = Mathf.Max(text_extents[0], max_text_extents[0]);
                max_text_extents[1] = Mathf.Max(text_extents[1], max_text_extents[1]);
            }

            // size of activatable box
            Vector3 menu_box_dims = new Vector3(max_text_extents[0] + 2.0f * padding[0],
                                                max_text_extents[1] + 2.0f * padding[1],
                                                depth);

            float height = menuItems.Count * menu_box_dims[1] + (menuItems.Count - 1) * itemSep;

            // special case: title bar taller than items
            Vector2 title_extents = TextExtents(titleTextMesh);
            if (height < title_extents[0]) {
                height = title_extents[0] + 2.0f * padding[0];
            }

            // set transforms to use for drawing boxes and labels

            titleBoxObj.transform.localPosition = new Vector3(-0.5f * menu_box_dims[0] - 0.5f * menu_box_dims[1], 0f, 0f);
            titleBoxObj.transform.localScale = new Vector3(menu_box_dims[1], height, depth);

            titleTextMesh.transform.localPosition = new Vector3(-0.5f * menu_box_dims[0] - 0.5f * menu_box_dims[1],
                                                           -0.5f * height + padding[0],
                                                           -0.5f * depth - zEpsilon);
            titleTextMesh.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90.0f));

            float y = 0.5f * height - 0.5f * menu_box_dims[1];
            for (int i = 0; i < menuItems.Count; i++) {
                labelBoxes[i].transform.localPosition = new Vector3(0.0f, y, 0.0f);
                labelBoxes[i].transform.localScale = menu_box_dims;
                labelMeshes[i].transform.localPosition = new Vector3(-0.5f * menu_box_dims[0] + padding[0], y, -0.5f * depth - zEpsilon);
                y -= menu_box_dims[1] + itemSep;
            }

            bgBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bgBox.name = "Background Box";
            bgBox.transform.SetParent(this.transform);
            bgBox.GetComponent<Renderer>().sharedMaterial.color = itemBGColor;
            bgBox.transform.localPosition = new Vector3(zEpsilon, 0f, 0.5f * itemSep + zEpsilon);
            bgBox.transform.localScale = new Vector3(menu_box_dims[0] - zEpsilon, height - 2.0f * zEpsilon, menu_box_dims[2] - itemSep);
            dirty = false;
        }


        public void Update()
        {
            if (dirty) {
                RebuildMenu();
            }

            if (Application.isPlaying) {
                // Convert the tracker's position and rotation to a Matrix4x4 format.
                Matrix4x4 trackerMat = Matrix4x4.TRS(m_TrackerPos, m_TrackerRot, Vector3.one);

                if ((buttonPressed) && (selected == 0)) {
                    // Dragging while holding onto the menu title bar, 
                    // move the menu based on motion of the tracker

                    // Get the menu's Transform in Matrix4x4 format            
                    Matrix4x4 origMenuMat = transform.localToWorldMatrix;

                    // Calc change in tracker pos and rot from the last frame until now
                    Matrix4x4 deltaTracker = trackerMat * lastTrackerMat.inverse;

                    // Apply this change to the menu's current transformation to find its new transform
                    Matrix4x4 newMenuMat = deltaTracker * origMenuMat;

                    // Save the result, converting from Matrix4x4 back to unity's Transform class.
                    transform.position = Matrix4x4Extensions.GetTranslation(newMenuMat);
                    transform.rotation = Matrix4x4Extensions.GetRotation(newMenuMat);
                } else if (!buttonPressed) {
                    // Clear selection and highlighting
                    if ((inputFocusToken != null) && (selected >= 0)) {
                        inputFocusToken.ReleaseToken(this);
                    }
                    selected = -1;
                    Material tmpMat = titleBoxObj.GetComponent<Renderer>().sharedMaterial;
                    tmpMat.color = titleBGColor;
                    titleBoxObj.GetComponent<Renderer>().sharedMaterial = tmpMat;
                    for (int i = 0; i < labelBoxes.Count; i++) {
                        tmpMat = labelBoxes[i].GetComponent<Renderer>().sharedMaterial;
                        tmpMat.color = itemBGColor;
                        labelBoxes[i].GetComponent<Renderer>().sharedMaterial = tmpMat;
                    }

                    // Update selection
                    if (InsideTransformedCube(m_TrackerPos, titleBoxObj)) {
                        if ((inputFocusToken == null) || (inputFocusToken.RequestToken(this))) {
                            selected = 0;
                            tmpMat = titleBoxObj.GetComponent<Renderer>().sharedMaterial;
                            tmpMat.color = titleHighColor;
                            titleBoxObj.GetComponent<Renderer>().sharedMaterial = tmpMat;
                        }
                    } else {
                        for (int i = 0; i < labelBoxes.Count; i++) {
                            if (InsideTransformedCube(m_TrackerPos, labelBoxes[i])) {
                                if ((inputFocusToken == null) || (inputFocusToken.RequestToken(this))) {
                                    selected = i + 1;
                                    tmpMat = labelBoxes[i].GetComponent<Renderer>().sharedMaterial;
                                    tmpMat.color = itemHighColor;
                                    labelBoxes[i].GetComponent<Renderer>().sharedMaterial = tmpMat;
                                }
                            }
                        }
                    }
                }

                lastTrackerMat = trackerMat;
            }
        }

        private void OnEnable()
        {
            if (font == null) {
                font = Resources.Load<Font>("Fonts/Futura_Medium_BT");
            }
            if (fontMaterial == null) {
                fontMaterial = Resources.Load<Material>("Material/Futura_Medium_BT_WithOcclusion");
            }
            if (Application.isPlaying) {
                selected = -1;
                StartListening();
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying) {
                StopListening();
            }
        }

        public void OnTrackerMove(Vector3 pos)
        {
            m_TrackerPos = pos;
        }

        public void OnTrackerRotate(Quaternion rot)
        {
            m_TrackerRot = rot;
        }

        public void OnButtonDown()
        {
            buttonPressed = true;
            if (selected == 0) {
                titleBoxObj.GetComponent<Renderer>().sharedMaterial.color = pressColor;
            } else if (selected > 0) {
                labelBoxes[selected - 1].GetComponent<Renderer>().sharedMaterial.color = pressColor;

                Debug.Log("Selected menu item " + (selected - 1));
                onMenuItemSelected.Invoke(selected - 1);
            }
        }

        public void OnButtonUp()
        {
            buttonPressed = false;
        }


        public void OnVREvent(VREvent vrEvent)
        {
            if (enabled) {
                if (vrEvent.Matches(buttonDownEvent)) {
                    OnButtonDown();
                } else if (vrEvent.Matches(buttonUpEvent)) {
                    OnButtonUp();
                } else if (vrEvent.Matches(cursorPositionEvent)) {
                    OnTrackerMove(vrEvent.GetData<Vector3>());
                } else if (vrEvent.Matches(cursorRotationEvent)) {
                    OnTrackerRotate(vrEvent.GetData<Quaternion>());
                }
            }
        }

        public void StartListening()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        public void StopListening()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }

        // refs to dynamically created geometry
        private bool dirty = true;
        private List<TextMesh> labelMeshes = new List<TextMesh>();
        private List<GameObject> labelBoxes = new List<GameObject>();
        private GameObject titleBoxObj;
        private GameObject bgBox;

        // runtime UI management
        private Matrix4x4 lastTrackerMat;
        private Vector3 m_TrackerPos;
        private Quaternion m_TrackerRot = Quaternion.identity;
        // -1 = nothing, 0 = titlebar, 1..items.Count = menu items
        private int selected = -1;
        private bool buttonPressed = false;
    }

} // namespace
