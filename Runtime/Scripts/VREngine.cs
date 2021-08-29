using UnityEngine;
using System.Linq;

namespace IVLab.MinVR3 {

    [AddComponentMenu("MinVR/Engine/VREngine")]
    [DisallowMultipleComponent]
    public class VREngine : MonoBehaviour
    {
        /// <summary>
        /// Similar to how Unity's Camera.mainCamera static variable works, this provides access to the
        /// VREngine component in the scene that has been tagged with MainVREngine.  It's rare, but possible,
        /// to have more than one VREngine in an application.  However, there should never be more than one
        /// tagged as the main one.  If you need to change which VREngine is the main one, set the tags accordingly
        /// then call VREngine.ValidateMainVREngine().
        /// </summary>
        public static VREngine main {
            get {
                if ((s_Main == null) && (!s_ShuttingDown)) {
                    ValidateMainVREngine();
                }
                return s_Main;
            }
        }


        public Token inputFocus {
            get {
                if ((m_InputFocus == null) && (!s_ShuttingDown)) {
                    ValidateInputFocusComponent();
                }
                return m_InputFocus;
            }
        }

        public VREventManager eventManager {
            get {
                if ((m_EventManager == null) && (!s_ShuttingDown)) {
                    ValidateEventManagerComponent();
                }
                return m_EventManager;
            }
        }


        private void Reset()
        {
            ValidateEventManagerComponent();
            ValidateInputFocusComponent();
            ValidateMainVREngine();
        }

        private void OnValidate()
        {
            ValidateEventManagerComponent();
            ValidateInputFocusComponent();
            ValidateMainVREngine();
        }


        public void ValidateInputFocusComponent()
        {
            // make sure the gameobject has one and only one Input Focus token attached
            Token[] inputFocusTokens = gameObject.GetComponents<Token>()
                .Where(c => c.tokenName == INPUT_FOCUS_TOKEN_NAME)
                .ToArray<Token>();
            if (inputFocusTokens.Length == 1) {
                m_InputFocus = inputFocusTokens[0];
            } else if (inputFocusTokens.Length == 0) {
                Token t = gameObject.AddComponent<Token>();
                t.tokenName = INPUT_FOCUS_TOKEN_NAME;
                m_InputFocus = t;
            } else {
                for (int i = 1; i < inputFocusTokens.Length; i++) {
                    Destroy(inputFocusTokens[i]);
                }
                m_InputFocus = inputFocusTokens[0];
            }
        }

        public void ValidateEventManagerComponent()
        {
            // make sure the gameobject has one and only one VREventManager attached
            VREventManager[] mgrs = gameObject.GetComponents<VREventManager>().ToArray<VREventManager>();
            if (mgrs.Length == 1) {
                m_EventManager = mgrs[0];
            } else if (mgrs.Length == 0) {
                m_EventManager = gameObject.AddComponent<VREventManager>();
            } else {
                for (int i = 1; i < mgrs.Length; i++) {
                    Destroy(mgrs[i]);
                }
                m_EventManager = mgrs[0];
            }
        }


        public static void ValidateMainVREngine() {
            // Note: The editor script VREngineTagManager.cs create the VRENGINE_MAIN_TAG automatically
            // if it is not already found in Unity's built-in tag asset.  That must be done from an
            // editor script.  This method can safely assume the tag exists or that the VREngineTagManager
            // script printed an error to the Console if it failed for any reason.

            // Verify that at least one VREngine object exists, if not create one and mark it main
            VREngine[] vrEngines = FindObjectsOfType<VREngine>(true);
            if (vrEngines.Length == 0) {
                GameObject obj = new GameObject(VRENGINE_DEFAULT_GAMEOBJECT_NAME);
                obj.tag = VREngine.VRENGINE_MAIN_TAG;
                s_Main = obj.AddComponent<VREngine>();
            }

            // If one VREngine exists, make sure it is marked as main
            else if (vrEngines.Length == 1) {
                if (!vrEngines[0].CompareTag(VRENGINE_MAIN_TAG)) {
                    vrEngines[0].tag = VREngine.VRENGINE_MAIN_TAG;
                }
                s_Main = vrEngines[0];
            }

            // If more than one VREngine exists, make sure only one is marked as main
            else {
                int nMarkedMain = 0;
                VREngine firstMarkedMain = null;
                for (int i = 0; i < vrEngines.Length; i++) {
                    if (vrEngines[i].CompareTag(VRENGINE_MAIN_TAG)) {
                        Debug.Log(vrEngines[i]);
                        nMarkedMain++;
                        if (nMarkedMain == 1) {
                            firstMarkedMain = vrEngines[i];
                        }
                    }
                }
                if (nMarkedMain == 0) {
                    Debug.LogWarning($"No VREngine is tagged with {VRENGINE_MAIN_TAG}; tagging the first one found.");
                    vrEngines[0].tag = VRENGINE_MAIN_TAG;
                    s_Main = vrEngines[0];
                } else if (nMarkedMain == 1) {
                    s_Main = firstMarkedMain;
                } else {
                    Debug.LogError("More than one VREngine is tagged with " + VRENGINE_MAIN_TAG);
                    s_Main = firstMarkedMain;
                }
            }

            if (Application.isPlaying) {
                DontDestroyOnLoad(s_Main.gameObject);
            }
        }

        private void OnDestroy()
        {
            s_ShuttingDown = true;
        }

        public const string VRENGINE_DEFAULT_GAMEOBJECT_NAME = "Main VREngine";
        public const string VRENGINE_MAIN_TAG = "MainVREngine";
        public const string INPUT_FOCUS_TOKEN_NAME = "Input Focus";

        // cached access to other components attached to the gameobject
        private Token m_InputFocus;
        private VREventManager m_EventManager;

        // to avoid creating any objects during shutdown
        static private bool s_ShuttingDown = false;

        // static ref to the engine tagged as main
        static private VREngine s_Main;
    }

} // namespace
