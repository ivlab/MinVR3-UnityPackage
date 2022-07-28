using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace IVLab.MinVR3
{

    public class MinVRGameObjectMenu : MonoBehaviour
    {
        // these editor menu functions can look to the compiler as though they are never called
#pragma warning disable IDE0051



        // --- Adding MinVR to a New Scene ---

        [MenuItem("GameObject/MinVR/Get Started/Create \"VREngine\" and \"Room Space Origin\"", false, -99)]
        public static void CreateVREngine(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
        }



        // --- Debugging Helpers ---

        [MenuItem("GameObject/MinVR/Debug/Draw Eyes", false, 10)]
        public static void CreateDebugDrawEyes(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Draw Eyes", command.context as GameObject, typeof(DrawEyes));
        }

        [MenuItem("GameObject/MinVR/Debug/Draw Frames Per Second", false, 10)]
        public static void CreateDebugDrawFPS(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Draw FPS", command.context as GameObject, typeof(DrawFPS));
        }

        [MenuItem("GameObject/MinVR/Debug/Draw Trackers", false, 10)]
        public static void CreateDebugDrawTrackers(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Draw Trackers", command.context as GameObject, typeof(DrawTrackers));
        }



        /// --- Interaction ---

        [MenuItem("GameObject/MinVR/Interaction/Building Blocks/FSM", false, 10)]
        public static void CreateInteractionFSM(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("FSM", command.context as GameObject, typeof(FSM));
        }


        [MenuItem("GameObject/MinVR/Interaction/Building Blocks/Shared Token (e.g., Input Focus Token)", false, 10)]
        public static void CreateInteractionSharedToken(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Input Focus Token", command.context as GameObject, typeof(SharedToken));
        }


        [MenuItem("GameObject/MinVR/Interaction/Building Blocks/Simple Event Listener", false, 10)]
        public static void CreateInteractionSimpleEventListener(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Event Listener", command.context as GameObject, typeof(SimpleEventListener));
        }


        [MenuItem("GameObject/MinVR/Interaction/Building Blocks/Tracked Pose Driver", false, 10)]
        public static void CreateInteractionTrackedPoseDriver(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Tracked Pose Driver", command.context as GameObject, typeof(TrackedPoseDriver));
        }

        [MenuItem("GameObject/MinVR/Interaction/Building Blocks/Cursors/CavePainting Paintbrush", false, 10)]
        public static void CreateInteractionCavePaintingBrush(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("CavePainting Brush", command.context as GameObject, typeof(CavePaintingBrushCursor));
        }

        [MenuItem("GameObject/MinVR/Interaction/Desktop/Trackball Camera", false, 10)]
        public static void CreateInteractionTrackballCamera(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Trackball Camera", command.context as GameObject, typeof(TrackballCamera));
        }

        [MenuItem("GameObject/MinVR/Interaction/Widgets/Menus/Basic Floating Menu", false, 10)]
        public static void CreateInteractionFloatingMenu(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Floating Menu", command.context as GameObject, typeof(FloatingMenu));
        }

        [MenuItem("GameObject/MinVR/Interaction/Selection/Basic Object Selector", false, 10)]
        public static void CreateInteractionBasicObjectSelector(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Object Selector", command.context as GameObject, typeof(BasicObjectSelector));
        }

        [MenuItem("GameObject/MinVR/Interaction/Selection/Bimanual Object Selector", false, 10)]
        public static void CreateInteractionBimanualObjectSelector(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Bimanual Object Selector", command.context as GameObject, typeof(BimanualObjectSelector));
        }

        [MenuItem("GameObject/MinVR/Interaction/Selection/Basic Highlighter", false, 10)]
        public static void CreateInteractionBasicHighlighter(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Basic Highlighter", command.context as GameObject, typeof(BasicHighlighter));
        }

        [MenuItem("GameObject/MinVR/Interaction/Navigation and Manipulation/Smart Scene", false, 10)]
        public static void CreateInteractionSmartScene(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Smart Scene", command.context as GameObject, typeof(SmartScene));
        }





        /// --- Configuring Input Devices and Display Devices ---


        [MenuItem("GameObject/MinVR/VRConfig/VRConfig Mask", false, 10)]
        public static void CreateVRConfigVRConfigMask(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();
            CreateAndPlaceGameObject("Config Mask", command.context as GameObject, typeof(VRConfigMask));
        }

        [MenuItem("GameObject/MinVR/VRConfig/Prefabs/New VRConfig (Template to Create Your Own)", false, -99)]
        public static void CreateVRConfigEmpty(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = CreateBasicVRConfig(command, "MyConfigName", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);
        }


        [MenuItem("GameObject/MinVR/VRConfig/Prefabs/Desktop (No VR)", false, 2)]
        public static void CreateVRConfigDesktop(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = CreateBasicVRConfig(command, "Desktop", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            inputDevicesChild?.AddComponent<MouseAndKeyboard>();
            inputDevicesChild?.AddComponent<TouchBuiltin>();

            Camera c = displayDevicesChild?.AddComponent<Camera>();
            c.tag = "MainCamera";
        }


        [MenuItem("GameObject/MinVR/VRConfig/Prefabs/Basic VR Simulator", false, 3)]
        public static void CreateVRConfigBasicVRSimulator(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = CreateBasicVRConfig(command, "BasicVRSimulator", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            inputDevicesChild?.AddComponent<MouseAndKeyboard>();
            inputDevicesChild?.AddComponent<FakeTrackers>();

            Camera c = displayDevicesChild?.AddComponent<Camera>();
            c.tag = "MainCamera";
            displayDevicesChild?.AddComponent<TrackedDesktopCamera>();

            AddTrackingAliases(eventAliasesChild, "Head", "FakeTrackers/Head");
            AddTrackingAliases(eventAliasesChild, "DH", "FakeTrackers/Tracker1");
            AddButtonAliases(eventAliasesChild, "DH", "Mouse/Left");
            AddTrackingAliases(eventAliasesChild, "NDH", "FakeTrackers/Tracker1");
            AddButtonAliases(eventAliasesChild, "DH", "Mouse/Right");
        }


        [MenuItem("GameObject/MinVR/VRConfig/Prefabs/UnityXR", false, 4)]
        public static void CreateVRConfigUnityXR(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = CreateBasicVRConfig(command, "UnityXR", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            inputDevicesChild.AddComponent<UnityXR>();

            Camera c = displayDevicesChild.AddComponent<Camera>();
            c.stereoTargetEye = StereoTargetEyeMask.Both;
            c.tag = "MainCamera";
            TrackedPoseDriver poseDriver = displayDevicesChild.AddComponent<TrackedPoseDriver>();
            poseDriver.positionEvent = VREventPrototypeVector3.Create("UnityXR/Head/Position");
            poseDriver.rotationEvent = VREventPrototypeQuaternion.Create("UnityXR/Head/Rotation");

            AddTrackingAliases(eventAliasesChild, "Head", "UnityXR/Head");
            AddTrackingAliases(eventAliasesChild, "DH", "UnityXR/RightHand");
            AddButtonAliases(eventAliasesChild, "DH", "UnityXR/RightHand/Trigger");
            AddTrackingAliases(eventAliasesChild, "NDH", "UnityXR/LeftHand");
            AddButtonAliases(eventAliasesChild, "DH", "UnityXR/LeftHand/Trigger");
        }


        [MenuItem("GameObject/MinVR/VRConfig/Prefabs/Cardboard VR", false, 5)]
        public static void CreateVRConfigCardboardVR(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = CreateBasicVRConfig(command, "Cardboard VR", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            inputDevicesChild.AddComponent<MobileSensors>();
            inputDevicesChild.AddComponent<TouchBuiltin>();

            displayDevicesChild.AddComponent<TrackedPoseDriver>();
            TrackedPoseDriver poseDriver = displayDevicesChild.GetComponent<TrackedPoseDriver>();
            poseDriver.rotationEvent = VREventPrototypeQuaternion.Create("Mobile/Rotation");

            GameObject leftObj = CreateAndPlaceGameObject("Left Camera", displayDevicesChild, typeof(Camera));
            Camera leftCam = leftObj.GetComponent<Camera>();
            leftCam.rect = new Rect(0, 0, 0.499f, 1);
            leftCam.stereoTargetEye = StereoTargetEyeMask.Left;
            leftCam.tag = "MainCamera";

            GameObject rightObj = CreateAndPlaceGameObject("Right Camera", displayDevicesChild, typeof(Camera));
            Camera rightCam = rightObj.GetComponent<Camera>();
            rightCam.rect = new Rect(0.501f, 0, 0.499f, 1);
            rightCam.stereoTargetEye = StereoTargetEyeMask.Right;

            AddTrackingAliases(eventAliasesChild, "Head", "Mobile");

            Selection.activeGameObject = vrConfigObj;
        }


        [MenuItem("GameObject/MinVR/VRConfig/Prefabs/Clipboard VR", false, 6)]
        public static void CreateVRConfigClipboardVR(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = CreateBasicVRConfig(command, "ClipboardVR", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            // input devices
            inputDevicesChild.AddComponent<MobileSensors>();
            inputDevicesChild.AddComponent<TouchBuiltin>();

            // display devices

            // stereo cameras
            GameObject stereoCamsObj = CreateAndPlaceGameObject("Stereo Cameras", displayDevicesChild, typeof(TrackedPoseDriver));

            TrackedPoseDriver poseDriver = stereoCamsObj.GetComponent<TrackedPoseDriver>();
            poseDriver.rotationEvent = VREventPrototypeQuaternion.Create("Mobile/Rotation");

            GameObject leftCamObj = CreateAndPlaceGameObject("Left Camera", stereoCamsObj, typeof(Camera));
            Camera leftCam = leftCamObj.GetComponent<Camera>();
            leftCam.rect = new Rect(0, 0.75f, 0.499f, 0.25f);
            leftCam.stereoTargetEye = StereoTargetEyeMask.Left;

            GameObject rightCamObj = CreateAndPlaceGameObject("Right Camera", stereoCamsObj, typeof(Camera));
            Camera rightCam = rightCamObj.GetComponent<Camera>();
            rightCam.rect = new Rect(0.501f, 0.75f, 0.499f, 0.25f);
            rightCam.stereoTargetEye = StereoTargetEyeMask.Right;

            // clipboard camera
            GameObject clipCamObj = CreateAndPlaceGameObject("Clipboard Camera", displayDevicesChild, typeof(Camera));
            Camera clipCam = clipCamObj.GetComponent<Camera>();
            clipCam.rect = new Rect(0, 0, 1, 0.75f);
            clipCam.tag = "MainCamera";

            AddTrackingAliases(eventAliasesChild, "Head", "Mobile");

            Selection.activeGameObject = vrConfigObj;
        }


        [MenuItem("GameObject/MinVR/VRConfig/Prefabs/Fishtank or Projected VR", false, 7)]
        public static void CreateVRConfigFishtankVR(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = CreateBasicVRConfig(command, "FishtankVR", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);


            Camera c = displayDevicesChild.AddComponent<Camera>();
            c.stereoTargetEye = StereoTargetEyeMask.Both;
            c.tag = "MainCamera";
            TrackedProjectionScreen trackedScreen = displayDevicesChild.AddComponent<TrackedProjectionScreen>();
        }

        [MenuItem("GameObject/MinVR/VRConfig/Prefabs/Tiled Display/Cluster Server", false, 7)]
        public static void CreateVRConfigClusterServer(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = CreateBasicVRConfig(command, "ClusterServer", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            vrConfigObj.AddComponent<ClusterServer>();

            Camera c = displayDevicesChild.AddComponent<Camera>();
            c.stereoTargetEye = StereoTargetEyeMask.Both;
            c.tag = "MainCamera";
            TrackedProjectionScreen trackedScreen = displayDevicesChild.AddComponent<TrackedProjectionScreen>();
        }

        [MenuItem("GameObject/MinVR/VRConfig/Prefabs/Tiled Display/Cluster Client", false, 7)]
        public static void CreateVRConfigClusterClient(MenuCommand command)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = CreateBasicVRConfig(command, "ClusterClient", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            vrConfigObj.AddComponent<ClusterClient>();

            Camera c = displayDevicesChild.AddComponent<Camera>();
            c.stereoTargetEye = StereoTargetEyeMask.Both;
            c.tag = "MainCamera";
            TrackedProjectionScreen trackedScreen = displayDevicesChild.AddComponent<TrackedProjectionScreen>();
        }

        // TODO: zSpace should be added by the zSpace optional package
        // TODO: Workbench and workbench simulator should be added by the TUIO optional package


#pragma warning restore IDE0051


        // Helpers for Placing Objects and Prefabs in the Scene

        private static void AddTrackingAliases(GameObject go, string aliasBaseName, string origEventBaseName)
        {
            VREventAlias posAlias = go.AddComponent<VREventAlias>();
            posAlias.aliasStrategy = VREventAlias.AliasStrategy.RenameClone;
            posAlias.aliasEventName = aliasBaseName + "/Position";
            posAlias.originalEvents = new List<VREventPrototypeAny>() { VREventPrototypeAny.Create(origEventBaseName + "/Position") };

            VREventAlias rotAlias = go.AddComponent<VREventAlias>();
            rotAlias.aliasStrategy = VREventAlias.AliasStrategy.RenameClone;
            rotAlias.aliasEventName = aliasBaseName + "/Rotation";
            rotAlias.originalEvents = new List<VREventPrototypeAny>() { VREventPrototypeAny.Create(origEventBaseName + "/Rotation") };
        }

        private static void AddButtonAliases(GameObject go, string aliasBaseName, string origEventBaseName)
        {
            VREventAlias downAlias = go.AddComponent<VREventAlias>();
            downAlias.aliasStrategy = VREventAlias.AliasStrategy.RenameClone;
            downAlias.aliasEventName = aliasBaseName + "/Down";
            downAlias.originalEvents = new List<VREventPrototypeAny>() { VREventPrototypeAny.Create(origEventBaseName + "/Down") };

            VREventAlias upAlias = go.AddComponent<VREventAlias>();
            upAlias.aliasStrategy = VREventAlias.AliasStrategy.RenameClone;
            upAlias.aliasEventName = aliasBaseName + "/Up";
            upAlias.originalEvents = new List<VREventPrototypeAny>() { VREventPrototypeAny.Create(origEventBaseName + "/Up") };
        }

        static GameObject CreateRoomSpaceOriginIfNeeded()
        {
            RoomSpaceOrigin rso = FindObjectOfType<RoomSpaceOrigin>();
            if (rso != null) {
                return rso.gameObject;
            } else {
                return CreateAndPlaceGameObject("Room Space", null, typeof(RoomSpaceOrigin));
            }
        }

        static GameObject CreateVREngineIfNeeded()
        {
            VREngine engine = FindObjectOfType<VREngine>();
            if (engine != null) {
                return engine.gameObject;
            } else {
                GameObject engineGO = CreateAndPlaceGameObject("VREngine", null, typeof(VREngine));
                engine = engineGO.GetComponent<VREngine>();

                // create a config file as well using the following template
                string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath("Assets/configvals-default.minvr.txt");
                string configText =
                    $"# MinVR3 ConfigVal File ({uniqueFileName})\n" +
                    "# This file is parsed when VREngine starts up.\n" +
                    "# \n" +
                    "# Notes: The typical use of this file is to define default settings for ConfigVals, where\n" +
                    "# \"default\" means the value works well for multiple VRConfigs.  For example, a good size in\n" +
                    "# meters for a VR menu might be the same for many VR HMDs, CAVEs, or Powerwalls so a default\n" +
                    "# size could be defined here like `MENU_SIZE = 1.5`.  MENU_SIZE would not be redefined in any\n" +
                    "# VRConfig-specific config files (e.g., settings-cave.minvr.txt) where 1.5 is a good size,\n" +
                    "# but MENU_SIZE would be redefined in VRConfig-specific config files where the size should\n" +
                    "# be different.  For example, the menu would need to be much smaller than 1.5 meters when\n" +
                    "# running in Desktop or zSpace modes, so these config files could overwrite the MENU_SIZE\n" +
                    "# setting by including a line like `MENU_SIZE = 0.25`.  In the menu's Start() function,\n" +
                    "# Programmers would apply the setting to the menu by writing something like:\n" +
                    "#    this.size = ConfigVal(\"MENU_SIZE\", 1.5);\n" +
                    "# Note, the second argument to the ConfigVal command is the default value to use if an\n" +
                    "# entry for MENU_SIZE is not found.\n" +
                    "\n" +
                    "MENU_SIZE = 1.5\n" +
                    "\n";
                File.WriteAllText(uniqueFileName, configText);
                AssetDatabase.ImportAsset(uniqueFileName);

                TextAsset newConfigFile = (TextAsset)AssetDatabase.LoadAssetAtPath(uniqueFileName, typeof(TextAsset));
                engine.configManager.AddConfigFile(newConfigFile);

                return engineGO;
            }
        }

        public static GameObject CreateBasicVRConfig(MenuCommand command, string name,
            ref GameObject inputDevChild, ref GameObject displayDevChild, ref GameObject eventAliasesChild)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();

            // unless user has explicitly parented to another object, VRConfigs should be parented
            // to the VREngine.
            GameObject parent = command.context as GameObject;
            if (parent == null) {
                // find RoomSpaceOrigin should never fail here since we create it if needed above
                parent = FindObjectOfType<RoomSpaceOrigin>().gameObject;
            }

            GameObject vrConfigObj = CreateAndPlaceGameObject("VRConfig_" + name, parent, typeof(VRConfig));
            VRConfig vrConfig = vrConfigObj.GetComponent<VRConfig>();
            eventAliasesChild = CreateAndPlaceGameObject("Event Aliases", vrConfigObj, new Type[] { });
            inputDevChild = CreateAndPlaceGameObject("Input Devices", vrConfigObj, new Type[] { });
            displayDevChild = CreateAndPlaceGameObject("Display Devices", vrConfigObj, new Type[] { });

            // create a config file as well using the following template
            string vrConfigNameLower = name.ToLower().Replace(" ", "");
            string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath($"Assets/configvals-{vrConfigNameLower}.minvr.txt");
            string configText =
                $"# MinVR3 ConfigVal File ({uniqueFileName})\n" +
                $"# This file is only parsed when VRConfig_{name} is used.\n" +
                $"# When parsed, settings in this file will override those with the same name in configvals-default.minvr.txt\n" +
                "\n" +
                "MENU_SIZE = 0.25\n" +
                "\n";
            File.WriteAllText(uniqueFileName, configText);
            AssetDatabase.ImportAsset(uniqueFileName);

            TextAsset newConfigFile = (TextAsset)AssetDatabase.LoadAssetAtPath(uniqueFileName, typeof(TextAsset));
            vrConfig.AddConfigFile(newConfigFile);

            Selection.activeGameObject = vrConfigObj;
            return vrConfigObj;
        }


        static GameObject CreateAndPlaceGameObject(string name, GameObject parent, params Type[] componentTypes)
        {
            GameObject go = ObjectFactory.CreateGameObject(name, componentTypes);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            if (parent != null) {
                Undo.SetTransformParent(go.transform, parent.transform, "Reparenting");
                ResetTransform(go.transform);
                go.layer = parent.gameObject.layer;
            }
            GameObjectUtility.EnsureUniqueNameForSibling(go);
            Selection.activeGameObject = go;
            return go;
        }

        static void ResetTransform(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (transform.parent is RectTransform) {
                var rectTransform = transform as RectTransform;
                if (rectTransform != null) {
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.sizeDelta = Vector2.zero;
                }
            }
        }


        static void InstatiatePrefabFromAsset(MenuCommand command, string searchStr)
        {
            UnityEngine.Object prefabAsset = null;
            string[] guids = AssetDatabase.FindAssets(searchStr);
            if (guids.Length > 0) {
                string fullPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                prefabAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullPath);
            }

            Debug.Assert(prefabAsset != null, "Cannot find requested prefab in the AssetDatabase using search string '" + searchStr + "'.");
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

    }

}
