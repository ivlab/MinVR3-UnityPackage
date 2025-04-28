using System.Collections.Generic;
using UnityEngine;
using System;

namespace IVLab.MinVR3
{

    /// <summary>
    /// Simple 3D menu that floats in space and is activated by placing a tracked cursor
    /// inside the titlebar or box that holds each menu item and then clicking.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("MinVR Interaction/Widgets/Menus/Floating Toggle Buttons")]
    public class FloatingToggleButtons : MonoBehaviour, IVREventListener, IVREventProducer
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
        public List<MenuItem> menuItems {
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
            m_TreatAsToggleGroup = false;
            m_MenuItems = new List<MenuItem>();
            m_MenuItems.Add(new MenuItem("Item 1", false));
            m_MenuItems.Add(new MenuItem("Item 2", false));

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
            if (mr != null) {
                mr.SetPropertyBlock(propertyBlock);
            }
        }

        void RebuildMenu()
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

            Material tmpMat;
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
                textMesh.text = menuItems[i].name;
                textMesh.color = m_ItemColor;
                textMesh.anchor = TextAnchor.MiddleLeft;
                textMesh.fontSize = 100;
                textMesh.characterSize = m_TextSizeInWorldUnits * 10.0f / textMesh.fontSize;

                GameObject boxObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boxObj.name = menuItems[i].name + " Box";
                boxObj.transform.SetParent(m_GeometryParent.transform, false);
                if (menuItems[i].pressed) {
                    OverrideMaterialColor(boxObj, m_PressColor);
                }
                else {
                    OverrideMaterialColor(boxObj, m_ItemBGColor);
                }
                //tmpMat = new Material(boxObj.GetComponent<Renderer>().sharedMaterial);
                //if (menuItems[i].pressed)
                //{
                //    tmpMat.color = m_PressColor;
                //}
                //else
                //{
                //    tmpMat.color = m_ItemBGColor;
                //}
                //boxObj.GetComponent<Renderer>().sharedMaterial = tmpMat;

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
            //tmpMat = new Material(m_BgBox.GetComponent<Renderer>().sharedMaterial);
            //tmpMat.color = m_ItemBGColor;
            //m_BgBox.GetComponent<Renderer>().sharedMaterial = tmpMat;
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

            if ((m_ButtonPressed) && (m_Highlighted == 0))
            {
                // CASE 1: In the middle of a drag operation

                // Get the menu's Transform in Matrix4x4 format in worldspace      
                Matrix4x4 origMenuMat = transform.localToWorldMatrix;

                // Calc change in tracker pos and rot from the last frame until now
                Matrix4x4 deltaTracker = trackerMatInWorld * m_LastTrackerMat.inverse;

                // Apply this change to the menu's current transformation to find its new transform
                Matrix4x4 newMenuMat = deltaTracker * origMenuMat;

                // Save the result, converting from Matrix4x4 back to unity's Transform class.
                transform.position = Matrix4x4Extensions.GetTranslationFast(newMenuMat);
                transform.rotation = Matrix4x4Extensions.GetRotationFast(newMenuMat);
            }
            else
            {
                // CASE 2: Normal operation

                if ((!m_InteractingWithMenu) && (InsideTransformedCube(m_TrackerPos, m_InteractionZoneBox)))
                {
                    // Cursor just moved INTO the interaction zone, try to aquire token
                    if ((m_InputFocusToken == null) || (m_InputFocusToken.RequestToken(this)))
                    {
                        m_InteractingWithMenu = true;
                        VREngine.instance.eventManager.InsertInQueue(new VREvent(gameObject.name + k_EnterActivationEventName));
                    }
                }

                if (m_InteractingWithMenu)
                {
                    if (!InsideTransformedCube(m_TrackerPos, m_InteractionZoneBox))
                    {
                        // Cursor just moved OUT OF the activation zone, release token
                        if ((m_InputFocusToken != null))
                        {
                            m_InputFocusToken.ReleaseToken(this);
                        }
                        m_InteractingWithMenu = false;
                        VREngine.instance.eventManager.InsertInQueue(new VREvent(gameObject.name + k_ExitActivationEventName));
                    }

                    // Major task while interacting with the menu is to update highlighting the title or menu item
					// currently under the cursor
                    UpdateHighlightAndColors();
                } 
            }
            
            m_LastTrackerMat = trackerMatInWorld;
        }

        private void UpdateHighlightAndColors() {

            // reset to nothing highlighted
            m_Highlighted = -1;

            if ((m_InteractingWithMenu) && (InsideTransformedCube(m_TrackerPos, m_TitleBoxObj)))
            {
                // cursor interacting with title box
                m_Highlighted = 0;
                if (m_ButtonPressed)
                {
                    OverrideMaterialColor(m_TitleBoxObj, m_PressColor);
                }
                else
                {
                    OverrideMaterialColor(m_TitleBoxObj, m_TitleHighColor);
                }
            }
            else
            {
                // cursor not interacting with title box
                OverrideMaterialColor(m_TitleBoxObj, m_TitleBGColor);
            }

            for (int i = 0; i < m_LabelBoxes.Count; i++) {
                if ((m_InteractingWithMenu) && (InsideTransformedCube(m_TrackerPos, m_LabelBoxes[i]))) {
                    // cursor interacting with menu item
                    m_Highlighted = i + 1;
                    OverrideMaterialColor(m_LabelBoxes[i], m_ItemHighColor);
                } else {
                    // cursor not interacting with menu item
                    if (m_MenuItems[i].pressed) {
                        OverrideMaterialColor(m_LabelBoxes[i], m_PressColor);
                    } else {
                        OverrideMaterialColor(m_LabelBoxes[i], m_ItemBGColor);
                    }
                }
            }
        }


        private void OnEnable()
        {
            if (Application.isPlaying) {
                m_Highlighted = -1;
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
                if (m_InteractingWithMenu) {
                    m_InteractingWithMenu = false;
                    VREngine.instance.eventManager.InsertInQueue(new VREvent(gameObject.name + k_ExitActivationEventName));
                }
                UpdateHighlightAndColors();
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


        public string GetEventNameForMenuItem(int itemId, bool selected)
        {
            if (selected)
            {
                return gameObject.name + "/Select Item " + itemId;
            }
            else
            {
                return gameObject.name + "/Deselect Item " + itemId;
            }
        }


        public void OnButtonDown()
        {
            m_ButtonPressed = true;
            UpdateHighlightAndColors();

            if (m_Highlighted > 0) {
                int selectedMenuItem = m_Highlighted - 1;

                // There are multiple ways developer's code can respond to a menu selection:
                // 1: In the editor, subscribe to the OnMenuItemSelected UnityEvent
                // 2. With a VREventListener that listens for a new MinVR3 event, named based on the name
                // of the GameObject this script is attached to.

                if (m_TreatAsToggleGroup)
                {
                    // Toggling one button on turns off all others; trying to toggle a button that is already on
                    // does nothing
                    if (!m_MenuItems[selectedMenuItem].pressed)
                    {
                        int oldPressed = GetFirstPressed();
                        if (oldPressed != -1)
                        {
                            m_MenuItems[oldPressed].pressed = false;
                            Debug.Log("Deselected menu item " + selectedMenuItem);
                            m_OnMenuItemDeselected.Invoke(selectedMenuItem);
                            VREngine.instance.eventManager.InsertInQueue(new VREvent(GetEventNameForMenuItem(selectedMenuItem, false)));
                        }
                        
                        m_MenuItems[selectedMenuItem].pressed = true;
                        Debug.Log("Selected menu item " + selectedMenuItem);
                        m_OnMenuItemSelected.Invoke(selectedMenuItem);
                        VREngine.instance.eventManager.InsertInQueue(new VREvent(GetEventNameForMenuItem(selectedMenuItem, true)));
                    }
                }
                else
                {
                    // Each button can be toggled individually
                    m_MenuItems[selectedMenuItem].pressed = !m_MenuItems[selectedMenuItem].pressed;
                    if (m_MenuItems[selectedMenuItem].pressed)
                    {
                        Debug.Log("Selected menu item " + selectedMenuItem);
                        m_OnMenuItemSelected.Invoke(selectedMenuItem);
                        VREngine.instance.eventManager.InsertInQueue(new VREvent(GetEventNameForMenuItem(selectedMenuItem, true)));
                    }
                    else
                    {
                        Debug.Log("Deselected menu item " + selectedMenuItem);
                        m_OnMenuItemDeselected.Invoke(selectedMenuItem);
                        VREngine.instance.eventManager.InsertInQueue(new VREvent(GetEventNameForMenuItem(selectedMenuItem, false)));
                    }
                }
            }
        }

        public void OnButtonUp()
        {
            m_ButtonPressed = false;
            UpdateHighlightAndColors();
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
            if (m_TreatAsToggleGroup)
            {
                CheckSingleButtonPressed();
            }
        }

        public void StopListening()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }

        // returns index of first button that is pressed or -1 if there are no buttons or none are pressed
        public int GetFirstPressed()
        {
            for (int i = 0; i < m_MenuItems.Count; i++)
            {
                if (m_MenuItems[i].pressed)
                {
                    return i;
                }
            }
            return -1;
        }

        public void CheckSingleButtonPressed()
        {
            if (m_MenuItems.Count > 0)
            {
                int nPressed = 0;
                foreach (MenuItem m in m_MenuItems)
                {
                    if (m.pressed)
                    {
                        nPressed++;
                    }
                }
                if (nPressed == 0)
                {
                    m_MenuItems[0].pressed = true;
                }
                else if (nPressed > 1)
                {
                    bool firstOne = true;
                    foreach (MenuItem m in m_MenuItems)
                    {
                        if (m.pressed)
                        {
                            if (firstOne)
                            {
                                firstOne = false;
                            }
                            else
                            {
                                m.pressed = false;
                            }
                        }
                    }
                }
            }
        }


        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventPrototypes = new List<IVREventPrototype>();
            for (int i = 0; i < m_MenuItems.Count; i++) {
                eventPrototypes.Add(VREventPrototype.Create(GetEventNameForMenuItem(i, true)));
                eventPrototypes.Add(VREventPrototype.Create(GetEventNameForMenuItem(i, false)));
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

        [Tooltip("If true, one item must always be selected.")]
        [SerializeField] private bool m_TreatAsToggleGroup;

        [Serializable]
        public class MenuItem
        {
            public MenuItem(string n, bool p) { name = n; pressed = p; }
            public string name;
            public bool pressed;
        }
        [Tooltip("Ordered list of button names and initial states")]
        [SerializeField] private List<MenuItem> m_MenuItems;

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
        [SerializeField] private VRCallbackInt m_OnMenuItemDeselected;


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
        private int m_Highlighted = -1;
        private bool m_ButtonPressed = false;
        private bool m_InteractingWithMenu = false;
        private const string k_EnterActivationEventName = "/Enter Activation Zone";
        private const string k_ExitActivationEventName = "/Exit Activation Zone";
    }

} // namespace
