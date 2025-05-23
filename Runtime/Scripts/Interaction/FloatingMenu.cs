using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    /// <summary>
    /// Simple 3D menu that floats in space and is activated by placing a tracked cursor
    /// inside the titlebar or box that holds each menu item and then clicking.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("MinVR Interaction/Widgets/Menus/Floating Menu")]
    public class FloatingMenu : MonoBehaviour, IVREventListener, IVREventProducer
    {
        /// <summary>
        /// Title displayed in all caps on the left side of the menu
        /// </summary>
        public string title {
            get => m_Title;
            set => m_Title = value;
        }

        /// <summary>
        /// Ordered list of strings for the choices that can be selected from the menu,
        /// displayed top to bottom. 
        /// </summary>
        public List<string> menuItems {
            get => m_MenuItems;
            set => m_MenuItems = value;
        }

        /// <summary>
        /// If set, the menu will only respond to input when the token is available 
        /// (i.e., not already held by someone else).
        /// </summary>
        public SharedToken inputFocusToken {
            get => m_InputFocusToken;
            set => m_InputFocusToken = value;
        }


        /// <summary>
        /// Restores default values, called whenever the component is added to a GameObject in the editor
        /// </summary>
        public void Reset()
        {
            m_Title = "My Menu";
            m_MenuItems = new List<string>() { "Item 1", "Item 2" };

            m_ActivationDepth = 0f;
            m_CursorPositionEvent = new VREventPrototypeVector3();
            m_CursorRotationEvent = new VREventPrototypeQuaternion();
            m_ButtonDownEvent = new VREventPrototype();
            m_ButtonUpEvent = new VREventPrototype();
            m_OnMenuItemSelected = new VRCallbackInt();

            m_Font = Resources.Load<Font>("Fonts/Futura_Medium_BT");
            m_FontMaterial = Resources.Load<Material>("Material/Futura_Medium_BT_WithOcclusion");
            
            m_TitleColor = new Color(1.0f, 1.0f, 1.0f);
            m_TitleBGColor = new Color(85.0f / 255.0f, 83.0f / 255.0f, 83.0f / 255.0f);
            m_TitleHighColor = new Color(19.0f / 255.0f, 0.0f / 255.0f, 239.0f / 255.0f);

            m_ItemColor = new Color(0.0f, 0.0f, 0.0f);
            m_ItemBGColor = new Color(230.0f / 255.0f, 230.0f / 255.0f, 221.0f / 221.0f);
            m_ItemHighColor = new Color(255.0f / 255.0f, 195.0f / 255.0f, 0.0f / 255.0f);

            m_PressColor = new Color(226.0f / 255.0f, 0.0f / 255.0f, 23.0f / 255.0f);

            m_TextSizeInWorldUnits = 0.2f;
            m_ItemSep = 0.08f;
            m_Padding = new Vector2(0.05f, 0.05f);
            m_Depth = 0.1f;
            m_ZEpsilon = 0.001f;
        }


        void Start()
        {
            m_Dirty = true;
            RebuildMenu();
        }

        // Called when values are changed in the Inspector window in the editor
        void OnValidate()
        {
            m_Dirty = true;
        }

        public void Update()
        {
            if (m_Dirty) {
                RebuildMenu();
            }
            if (Application.isPlaying) {
                HandleUserInput();
            }
        }

        void OverrideMaterialColor(GameObject go, Color c)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_Color", c);
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.SetPropertyBlock(propertyBlock);
            }
        }

        public void RebuildMenu()
        {
            m_LabelMeshes = new List<TextMesh>();
            m_LabelBoxes = new List<GameObject>();
            m_TitleBoxObj = null;
            m_BgBox = null;

            // wipe out any previously created menu geometry
            Transform t = transform.Find(k_GeometryParentName);
            if (t != null) {
                DestroyImmediate(t.gameObject);
            }

            m_GeometryParent = new GameObject(k_GeometryParentName);

            // Create a title box and label
            GameObject titleTextObj = new GameObject(title + " Label");
            titleTextObj.transform.SetParent(m_GeometryParent.transform, false);
            TextMesh titleTextMesh = titleTextObj.AddComponent<TextMesh>();
            titleTextMesh.font = m_Font;
            titleTextMesh.GetComponent<MeshRenderer>().sharedMaterial = new Material(m_FontMaterial);
            titleTextMesh.text = title.ToUpper();
            titleTextMesh.color = m_TitleColor;
            titleTextMesh.anchor = TextAnchor.MiddleLeft;
            titleTextMesh.fontSize = 100;
            titleTextMesh.characterSize = m_TextSizeInWorldUnits * 10.0f / titleTextMesh.fontSize;


            m_TitleBoxObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_TitleBoxObj.name = title + " Box";
            m_TitleBoxObj.transform.SetParent(m_GeometryParent.transform, false);
            OverrideMaterialColor(m_TitleBoxObj, m_TitleBGColor);

            // Create a box and label for each item
            for (int i = 0; i < menuItems.Count; i++) {
                GameObject textObj = new GameObject(menuItems[i] + " Label");
                textObj.transform.SetParent(m_GeometryParent.transform, false);
                TextMesh textMesh = textObj.AddComponent<TextMesh>();
                textMesh.font = m_Font;
                textMesh.GetComponent<MeshRenderer>().sharedMaterial = new Material(m_FontMaterial);
                textMesh.text = menuItems[i];
                textMesh.color = m_ItemColor;
                textMesh.anchor = TextAnchor.MiddleLeft;
                textMesh.fontSize = 100;
                textMesh.characterSize = m_TextSizeInWorldUnits * 10.0f / textMesh.fontSize;

                GameObject boxObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boxObj.name = menuItems[i] + " Box";
                boxObj.transform.SetParent(m_GeometryParent.transform, false);
                OverrideMaterialColor(boxObj, m_ItemBGColor);

                m_LabelMeshes.Add(textMesh);
                m_LabelBoxes.Add(boxObj);
            }


            // Calculate the max extents
            Vector2 max_text_extents = new Vector2();
            for (int i = 0; i < m_LabelMeshes.Count; i++) {
                Vector2 text_extents = TextExtents(m_LabelMeshes[i]);
                max_text_extents[0] = Mathf.Max(text_extents[0], max_text_extents[0]);
                max_text_extents[1] = Mathf.Max(text_extents[1], max_text_extents[1]);
            }

            // size of activatable box
            Vector3 menu_box_dims = new Vector3(max_text_extents[0] + 2.0f * m_Padding[0],
                                                max_text_extents[1] + 2.0f * m_Padding[1],
                                                m_Depth);

            float height = menuItems.Count * menu_box_dims[1] + (menuItems.Count - 1) * m_ItemSep;

            // special case: title bar taller than items
            Vector2 title_extents = TextExtents(titleTextMesh);
            if (height < title_extents[0]) {
                height = title_extents[0] + 2.0f * m_Padding[0];
            }

            // set transforms to use for drawing boxes and labels

            m_TitleBoxObj.transform.localPosition = new Vector3(-0.5f * menu_box_dims[0] - 0.5f * menu_box_dims[1], 0f, 0f);
            m_TitleBoxObj.transform.localScale = new Vector3(menu_box_dims[1], height, m_Depth);

            titleTextMesh.transform.localPosition = new Vector3(-0.5f * menu_box_dims[0] - 0.5f * menu_box_dims[1],
                                                           -0.5f * height + m_Padding[0],
                                                           -0.5f * m_Depth - m_ZEpsilon);
            titleTextMesh.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90.0f));

            float y = 0.5f * height - 0.5f * menu_box_dims[1];
            for (int i = 0; i < menuItems.Count; i++) {
                m_LabelBoxes[i].transform.localPosition = new Vector3(0.0f, y, 0.0f);
                m_LabelBoxes[i].transform.localScale = menu_box_dims;
                m_LabelMeshes[i].transform.localPosition = new Vector3(-0.5f * menu_box_dims[0] + m_Padding[0], y, -0.5f * m_Depth - m_ZEpsilon);
                y -= menu_box_dims[1] + m_ItemSep;
            }

            m_BgBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_BgBox.name = "Background Box";
            m_BgBox.transform.SetParent(m_GeometryParent.transform, false);
            OverrideMaterialColor(m_BgBox, m_ItemBGColor);
            m_BgBox.transform.localPosition = new Vector3(m_ZEpsilon, 0f, 0.5f * m_ItemSep + m_ZEpsilon);
            m_BgBox.transform.localScale = new Vector3(menu_box_dims[0] - m_ZEpsilon, height - 2.0f * m_ZEpsilon, menu_box_dims[2] - m_ItemSep);

            m_InteractionZoneBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_InteractionZoneBox.name = "Interaction Zone";
            m_InteractionZoneBox.transform.SetParent(m_GeometryParent.transform, false);
            m_InteractionZoneBox.GetComponent<Renderer>().enabled = false;
            m_InteractionZoneBox.transform.localPosition = new Vector3(-0.5f * menu_box_dims[1], 0f, -0.5f * m_ActivationDepth);
            m_InteractionZoneBox.transform.localScale = new Vector3(menu_box_dims[0] + menu_box_dims[1], height, menu_box_dims[2]+ m_ActivationDepth);


            // add geometry to the hierarchy under the Menu object
            m_GeometryParent.transform.SetParent(this.transform, false);
            m_Dirty = false;
        }


        void HandleUserInput()
        {
            // Convert the tracker's position and rotation to a Matrix4x4 format (assumes data is coming in roomspace).            
            Matrix4x4 trackerMat = Matrix4x4.TRS(m_TrackerPos, m_TrackerRot, Vector3.one);
            Matrix4x4 trackerMatInWorld = IVLab.MinVR3.VREngine.instance.roomSpaceOrigin.transform.localToWorldMatrix * trackerMat;

            if ((m_ButtonPressed) && (m_Selected == 0)) {
                // Dragging while holding onto the menu title bar, 
                // move the menu based on motion of the tracker

                // Get the menu's Transform in Matrix4x4 format in worldspace      
                Matrix4x4 origMenuMat = transform.localToWorldMatrix;

                // Calc change in tracker pos and rot from the last frame until now
                Matrix4x4 deltaTracker = trackerMatInWorld * m_LastTrackerMat.inverse;

                // Apply this change to the menu's current transformation to find its new transform
                Matrix4x4 newMenuMat = deltaTracker * origMenuMat;

                // Save the result, converting from Matrix4x4 back to unity's Transform class.
                transform.position = Matrix4x4Extensions.GetTranslationFast(newMenuMat);
                transform.rotation = Matrix4x4Extensions.GetRotationFast(newMenuMat);
            } else if (!m_ButtonPressed) {
                if (m_inActivationZone && !InsideTransformedCube(m_TrackerPos, m_InteractionZoneBox)) {
                    // moved outside the activation zone
                    if ((m_InputFocusToken != null)) {
                        m_InputFocusToken.ReleaseToken(this);
                    }
                    m_inActivationZone = false;
                    ClearSelection();
                    VREngine.instance.eventManager.InsertInQueue(new VREvent(gameObject.name + k_ExitActivationEventName));
                }
                else if (!m_inActivationZone && InsideTransformedCube(m_TrackerPos, m_InteractionZoneBox)){
                    if ((m_InputFocusToken == null) || (m_InputFocusToken.RequestToken(this))) {
                        m_inActivationZone = true;
                        VREngine.instance.eventManager.InsertInQueue(new VREvent(gameObject.name + k_EnterActivationEventName));
                    }
                }                    
                
                if (m_inActivationZone) {
                    ClearSelection();
                
                    // Update selection
                    if (InsideTransformedCube(m_TrackerPos, m_TitleBoxObj)) {
                        m_Selected = 0;
                        OverrideMaterialColor(m_TitleBoxObj, m_TitleHighColor);
                    } else {
                        OverrideMaterialColor(m_TitleBoxObj, m_TitleBGColor);
                        for (int i = 0; i < m_LabelBoxes.Count; i++) {
                            if (InsideTransformedCube(m_TrackerPos, m_LabelBoxes[i]))
                            {
                                m_Selected = i + 1;
                                OverrideMaterialColor(m_LabelBoxes[i], m_ItemHighColor);
                            }
                            else
                            {
                                OverrideMaterialColor(m_LabelBoxes[i], m_ItemBGColor);
                            }
                        }
                    }
                }
            }

            m_LastTrackerMat = trackerMatInWorld;
        }

        private void ClearSelection(){
            m_Selected = -1;
            OverrideMaterialColor(m_TitleBoxObj, m_TitleBGColor);
            for (int i = 0; i < m_LabelBoxes.Count; i++) {
                OverrideMaterialColor(m_LabelBoxes[i], m_ItemBGColor);
            }
        }


        private void OnEnable()
        {
            if (Application.isPlaying) {
                m_Selected = -1;
                StartListening();
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying) {
                StopListening();
                if (m_InputFocusToken != null && m_InputFocusToken.currentOwner == this){
                    m_InputFocusToken.ReleaseToken(this);
                }
                if (m_inActivationZone){
                    m_inActivationZone = false;
                    ClearSelection();
                    VREngine.instance.eventManager.InsertInQueue(new VREvent(gameObject.name + k_ExitActivationEventName));
                }
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


        public string GetEventNameForMenuItem(int itemId)
        {
            return gameObject.name + "/Select Item " + itemId;
        }


        public void OnButtonDown()
        {
            m_ButtonPressed = true;
            if (m_Selected == 0) {
                OverrideMaterialColor(m_TitleBoxObj, m_PressColor);
            } else if (m_Selected > 0) {
                OverrideMaterialColor(m_LabelBoxes[m_Selected - 1], m_PressColor);

                Debug.Log("Selected menu item " + (m_Selected - 1));

                // There are multiple ways developer's code can respond to a menu selection:

                // 1: In the editor, subscribe to the OnMenuItemSelected UnityEvent
                m_OnMenuItemSelected.Invoke(m_Selected - 1);

                // 2. With a VREventListener that listens for a new MinVR3 event, named based on the name
				// of the GameObject this script is attached to.
                VREngine.instance.eventManager.InsertInQueue(new VREvent(GetEventNameForMenuItem(m_Selected - 1)));
            }
        }

        public void OnButtonUp()
        {
            m_ButtonPressed = false;
        }

        public void OnVREvent(VREvent vrEvent)
        {
            if (enabled) {
                if (vrEvent.Matches(m_ButtonDownEvent)) {
                    OnButtonDown();
                } else if (vrEvent.Matches(m_ButtonUpEvent)) {
                    OnButtonUp();
                } else if (vrEvent.Matches(m_CursorPositionEvent)) {
                    OnTrackerMove(vrEvent.GetData<Vector3>());
                } else if (vrEvent.Matches(m_CursorRotationEvent)) {
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


        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventPrototypes = new List<IVREventPrototype>();
            for (int i = 0; i < m_MenuItems.Count; i++) {
                eventPrototypes.Add(VREventPrototype.Create(GetEventNameForMenuItem(i)));
            }
            eventPrototypes.Add(VREventPrototype.Create(gameObject.name + k_EnterActivationEventName));
            eventPrototypes.Add(VREventPrototype.Create(gameObject.name + k_ExitActivationEventName));
            return eventPrototypes;
        }


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
            Vector3 pBoxSpace = boxObj.transform.RoomPointToLocalSpace(p);
            return (Mathf.Abs(pBoxSpace[0]) <= 0.5) &&
                (Mathf.Abs(pBoxSpace[1]) <= 0.5) &&
                (Mathf.Abs(pBoxSpace[2]) <= 0.5);
        }


        [Header("Content")]
        [Tooltip("Title displayed in all caps on the left side of the menu.")]
        [SerializeField] private string m_Title;
        [Tooltip("Ordered list of strings for the choices that can be selected from the menu, " +
            "displayed top to bottom.")]
        [SerializeField] private List<string> m_MenuItems;

        [Header("Input")]
        [Tooltip("[Optional] If set, the menu will only respond to input when the token is available " +
            "(i.e., not already held by someone else).")]
        [SerializeField] private SharedToken m_InputFocusToken;

        [Tooltip("Specifies the activation zone depth in front of the menu. " +
            "moving the cursor into this zone will cause the menu to try to acquire the token "+
            "and spawn enter/exit events that can be used to change the cursor.")]
        [SerializeField] private float m_ActivationDepth;

        [Tooltip("The cursor position event.")]
        [SerializeField] private VREventPrototypeVector3 m_CursorPositionEvent;

        [Tooltip("[Optional] The cursor rotation event if you want to be able to grab the menu bar " +
            "and rotate the menu.")]
        [SerializeField] private VREventPrototypeQuaternion m_CursorRotationEvent;

        [Tooltip("The cursor selection activate event (i.e., primary button down).")]
        [SerializeField] private VREventPrototype m_ButtonDownEvent;

        [Tooltip("The cursor selection deactivate event (i.e., primary button up).")]
        [SerializeField] private VREventPrototype m_ButtonUpEvent;

        [Header("Callback Function(s)")]
        [Tooltip("Register a function with this event to receive a callback when a selection is made, " +
            "the int argument is the index of the menu item that was selected.")]
        [SerializeField] private VRCallbackInt m_OnMenuItemSelected;


        [Header("Appearance")]
        [SerializeField] private Font m_Font;
        [SerializeField] private Material m_FontMaterial;

        [SerializeField] private Color m_TitleColor;
        [SerializeField] private Color m_TitleBGColor;
        [SerializeField] private Color m_TitleHighColor;

        [SerializeField] private Color m_ItemColor;
        [SerializeField] private Color m_ItemBGColor;
        [SerializeField] private Color m_ItemHighColor;

        [SerializeField] private Color m_PressColor;

        [SerializeField] private float m_TextSizeInWorldUnits;
        [SerializeField] private float m_ItemSep;
        [SerializeField] private Vector2 m_Padding;
        [SerializeField] private float m_Depth;
        [SerializeField] private float m_ZEpsilon;

        



        // dynamically created geometry
        private GameObject m_GeometryParent;
        private const string k_GeometryParentName = "Menu Geometry [Generated]";

        private bool m_Dirty = true;
        private List<TextMesh> m_LabelMeshes;
        private List<GameObject> m_LabelBoxes;
        private GameObject m_TitleBoxObj;
        private GameObject m_BgBox;
        private GameObject m_InteractionZoneBox;

        // runtime UI management
        private Matrix4x4 m_LastTrackerMat;
        private Vector3 m_TrackerPos;
        private Quaternion m_TrackerRot = Quaternion.identity;
        // -1 = nothing, 0 = titlebar, 1..items.Count = menu items
        private int m_Selected = -1;
        private bool m_ButtonPressed = false;
        private bool m_inActivationZone = false;
        private const string k_EnterActivationEventName = "/Enter Activation Zone";
        private const string k_ExitActivationEventName = "/Exit Activation Zone";
    }

} // namespace
