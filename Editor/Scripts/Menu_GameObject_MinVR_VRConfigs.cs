using UnityEditor;
using UnityEngine;


// disable warnings about unused functions because these editor menu functions can look to the compiler
// as though they are never called
#pragma warning disable IDE0051

namespace IVLab.MinVR3
{
    public class Menu_GameObject_MinVR_VRConfigs : MonoBehaviour
    {
        public static Vector3 defaultCameraPosition = new Vector3(0.0f, 1.0f, -2.5f);


        /// ---- VRCONFIGS ----
        

        // ---- Section 1 -----
        // New from Template
        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_MyConfigName (Template to Create Your Own)", false, MenuHelpers.vrConfigSec1Priority)]
        public static void CreateVRConfigEmpty(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = MenuHelpers.CreateVRConfigTemplate(command, "MyConfigName", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);
        }




        // ---- Section 2 ----
        // Common VRConfigs
        // Not quite prefabs, these are like shortcuts to setup commonly used VRConfigs

        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_Desktop (No VR)", false, MenuHelpers.vrConfigSec2Priority)]
        public static void CreateVRConfigDesktop(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = MenuHelpers.CreateVRConfigTemplate(command, "Desktop", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            inputDevicesChild?.AddComponent<MouseAndKeyboard>();
            inputDevicesChild?.AddComponent<TouchBuiltin>();

            Camera c = displayDevicesChild?.AddComponent<Camera>();
            c.transform.position = defaultCameraPosition;
            c.tag = "MainCamera";
        }


        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_BasicVRSimulator (Desktop with Fake Trackers)", false, MenuHelpers.vrConfigSec2Priority)]
        public static void CreateVRConfigBasicVRSimulator(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = MenuHelpers.CreateVRConfigTemplate(command, "BasicVRSimulator", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            inputDevicesChild.AddComponent<MouseAndKeyboard>();
            inputDevicesChild.AddComponent<FakeTrackers>();

            Camera c = displayDevicesChild.AddComponent<Camera>();
            c.transform.position = defaultCameraPosition;
            c.stereoTargetEye = StereoTargetEyeMask.None;  // Need to be "None" to display tracker's position correctly on MacOS computers
            c.tag = "MainCamera";
            TrackedDesktopCamera trackedDesktopCamera = displayDevicesChild.AddComponent<TrackedDesktopCamera>();
            trackedDesktopCamera.positionEvent = VREventPrototypeVector3.Create("FakeTrackers/Head/Position");
            trackedDesktopCamera.rotationEvent = VREventPrototypeQuaternion.Create("FakeTrackers/Head/Rotation");

            MenuHelpers.AddTrackingAliases(eventAliasesChild, "Head", "FakeTrackers/Head");
            MenuHelpers.AddTrackingAliases(eventAliasesChild, "DH", "FakeTrackers/Tracker 1");
            MenuHelpers.AddButtonAliases(eventAliasesChild, "DH", "Mouse/Left");
            MenuHelpers.AddTrackingAliases(eventAliasesChild, "NDH", "FakeTrackers/Tracker 2");
            MenuHelpers.AddButtonAliases(eventAliasesChild, "NDH", "Mouse/Right");
        }


#if ENABLE_INPUT_SYSTEM
        // This functionality is only available in projects using Unity's New Input System
        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_UnityXR (HMDs supported by UnityEngine.XR)", false, MenuHelpers.vrConfigSec2Priority)]
        public static void CreateVRConfigUnityXR(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = MenuHelpers.CreateVRConfigTemplate(command, "UnityXR", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            inputDevicesChild.AddComponent<UnityXR>();

            Camera c = displayDevicesChild.AddComponent<Camera>();
            c.transform.position = defaultCameraPosition;
            c.stereoTargetEye = StereoTargetEyeMask.Both;
            c.tag = "MainCamera";
            TrackedPoseDriver poseDriver = displayDevicesChild.AddComponent<TrackedPoseDriver>();
            poseDriver.positionEvent = VREventPrototypeVector3.Create("UnityXR/Head/Position");
            poseDriver.rotationEvent = VREventPrototypeQuaternion.Create("UnityXR/Head/Rotation");

            MenuHelpers.AddTrackingAliases(eventAliasesChild, "Head", "UnityXR/Head");
            MenuHelpers.AddTrackingAliases(eventAliasesChild, "DH", "UnityXR/RightHand");
            MenuHelpers.AddButtonAliases(eventAliasesChild, "DH", "UnityXR/RightHand/Trigger");
            MenuHelpers.AddTrackingAliases(eventAliasesChild, "NDH", "UnityXR/LeftHand");
            MenuHelpers.AddButtonAliases(eventAliasesChild, "NDH", "UnityXR/LeftHand/Trigger");
        }
        
        // This functionality is only available in projects using Unity's New Input System
        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_Quest (Meta Quest 1, 2, or 3)", false, MenuHelpers.vrConfigSec2Priority)]
        public static void CreateVRConfigQuest(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = MenuHelpers.CreateVRConfigTemplate(command, "Quest", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            inputDevicesChild.AddComponent<UnityXR>();

            Camera c = displayDevicesChild.AddComponent<Camera>();
            c.transform.position = defaultCameraPosition;
            c.stereoTargetEye = StereoTargetEyeMask.Both;
            c.tag = "MainCamera";
            TrackedPoseDriver poseDriver = displayDevicesChild.AddComponent<TrackedPoseDriver>();
            poseDriver.positionEvent = VREventPrototypeVector3.Create("UnityXR/Head/Position");
            poseDriver.rotationEvent = VREventPrototypeQuaternion.Create("UnityXR/Head/Rotation");

            GameObject eventAliasesHead = new GameObject("Head");
            GameObject eventAliasesDH = new GameObject("DH");
            GameObject eventAliasesNDH = new GameObject("NDH");
            eventAliasesHead.transform.SetParent(eventAliasesChild.transform);
            eventAliasesDH.transform.SetParent(eventAliasesChild.transform);
            eventAliasesNDH.transform.SetParent(eventAliasesChild.transform);

            MenuHelpers.AddTrackingAliases(eventAliasesHead, "Head", "UnityXR/Head");

            CreateQuestHandAliases(eventAliasesDH, "DH", "UnityXR/RightHand");
            CreateQuestHandAliases(eventAliasesNDH, "NDH", "UnityXR/LeftHand");
        }

        static void CreateQuestHandAliases(GameObject aliasGO, string handAliasBaseName="DH", string origEventBaseName="UnityXR/RightHand")
        {
            MenuHelpers.AddQuestTrackingAliases(aliasGO, handAliasBaseName, origEventBaseName);

            VREventAlias button1Alias = MenuHelpers.AddButtonAlias(aliasGO, handAliasBaseName + "/Button1/Value", origEventBaseName + "/Trigger/Value");
            FloatEventToButtonEvents handButton1FloatEvent = aliasGO.AddComponent<FloatEventToButtonEvents>();
            handButton1FloatEvent.Init(button1Alias.aliasEventName, handAliasBaseName + "/Button1/Down", handAliasBaseName + "/Button1/Up", 0.0f, 1.0f, 0.1f, true, handAliasBaseName + "/Button1/NormalizedValue");
            
            VREventAlias button2Alias = MenuHelpers.AddButtonAlias(aliasGO, handAliasBaseName + "/Button2/Value", origEventBaseName + "/Grip/Value");
            FloatEventToButtonEvents handButton2FloatEvent = aliasGO.AddComponent<FloatEventToButtonEvents>();
            handButton2FloatEvent.Init(button2Alias.aliasEventName, handAliasBaseName + "/Button2/Down", handAliasBaseName + "/Button2/Up", 0.0f, 1.0f, 0.1f, true, handAliasBaseName + "/Button2/NormalizedValue");
            
            MenuHelpers.AddButtonAliases(aliasGO, handAliasBaseName + "/Button3", origEventBaseName + "/PrimaryButton");
            MenuHelpers.AddButtonAliases(aliasGO, handAliasBaseName + "/Button4", origEventBaseName + "/SecondaryButton");
        }
#endif

        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_CardboardVR (Phone or Tablet)", false, MenuHelpers.vrConfigSec2Priority)]
        public static void CreateVRConfigCardboardVR(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = MenuHelpers.CreateVRConfigTemplate(command, "Cardboard VR", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            inputDevicesChild.AddComponent<MobileSensors>();
            inputDevicesChild.AddComponent<TouchBuiltin>();

            displayDevicesChild.AddComponent<TrackedPoseDriver>();
            TrackedPoseDriver poseDriver = displayDevicesChild.GetComponent<TrackedPoseDriver>();
            poseDriver.rotationEvent = VREventPrototypeQuaternion.Create("Mobile/Rotation");

            GameObject leftObj = MenuHelpers.CreateAndPlaceGameObject("Left Camera", displayDevicesChild, typeof(Camera));
            Camera leftCam = leftObj.GetComponent<Camera>();
            leftCam.transform.position = defaultCameraPosition;
            leftCam.rect = new Rect(0, 0, 0.499f, 1);
            leftCam.stereoTargetEye = StereoTargetEyeMask.Left;
            leftCam.tag = "MainCamera";

            GameObject rightObj = MenuHelpers.CreateAndPlaceGameObject("Right Camera", displayDevicesChild, typeof(Camera));
            Camera rightCam = rightObj.GetComponent<Camera>();
            rightCam.transform.position = defaultCameraPosition;
            rightCam.rect = new Rect(0.501f, 0, 0.499f, 1);
            rightCam.stereoTargetEye = StereoTargetEyeMask.Right;

            MenuHelpers.AddTrackingAliases(eventAliasesChild, "Head", "Mobile");

            Selection.activeGameObject = vrConfigObj;
        }


        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_ClipboardVR (Tablet)", false, MenuHelpers.vrConfigSec2Priority)]
        public static void CreateVRConfigClipboardVR(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = MenuHelpers.CreateVRConfigTemplate(command, "ClipboardVR", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            // input devices
            inputDevicesChild.AddComponent<MobileSensors>();
            inputDevicesChild.AddComponent<TouchBuiltin>();

            // display devices

            // stereo cameras
            GameObject stereoCamsObj = MenuHelpers.CreateAndPlaceGameObject("Stereo Cameras", displayDevicesChild, typeof(TrackedPoseDriver));
            stereoCamsObj.transform.position = defaultCameraPosition;


            TrackedPoseDriver poseDriver = stereoCamsObj.GetComponent<TrackedPoseDriver>();
            poseDriver.rotationEvent = VREventPrototypeQuaternion.Create("Mobile/Rotation");

            GameObject leftCamObj = MenuHelpers.CreateAndPlaceGameObject("Left Camera", stereoCamsObj, typeof(Camera));
            Camera leftCam = leftCamObj.GetComponent<Camera>();
            leftCam.rect = new Rect(0, 0.75f, 0.499f, 0.25f);
            leftCam.stereoTargetEye = StereoTargetEyeMask.Left;

            GameObject rightCamObj = MenuHelpers.CreateAndPlaceGameObject("Right Camera", stereoCamsObj, typeof(Camera));
            Camera rightCam = rightCamObj.GetComponent<Camera>();
            rightCam.rect = new Rect(0.501f, 0.75f, 0.499f, 0.25f);
            rightCam.stereoTargetEye = StereoTargetEyeMask.Right;

            // clipboard camera
            GameObject clipCamObj = MenuHelpers.CreateAndPlaceGameObject("Clipboard Camera", displayDevicesChild, typeof(Camera));
            Camera clipCam = clipCamObj.GetComponent<Camera>();
            clipCam.transform.localPosition = new Vector3(0, 10, 0);
            clipCam.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
            clipCam.orthographic = true;
            clipCam.rect = new Rect(0, 0, 1, 0.749f);
            clipCam.tag = "MainCamera";

            MenuHelpers.AddTrackingAliases(eventAliasesChild, "Head", "Mobile");

            Selection.activeGameObject = vrConfigObj;
        }


        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_Fishtank (3DTV or Projected)", false, MenuHelpers.vrConfigSec2Priority)]
        public static void CreateVRConfigFishtankVR(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = MenuHelpers.CreateVRConfigTemplate(command, "FishtankVR", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);


            Camera c = displayDevicesChild.AddComponent<Camera>();
            c.transform.position = defaultCameraPosition;
            c.stereoTargetEye = StereoTargetEyeMask.Both;
            c.tag = "MainCamera";
            TrackedProjectionScreen trackedScreen = displayDevicesChild.AddComponent<TrackedProjectionScreen>();
        }


        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_Workbench (Fishtank + Touch Table)", false, MenuHelpers.vrConfigSec2Priority)]
        public static void CreateVRConfigWorkbench(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = MenuHelpers.CreateVRConfigTemplate(command, "Workbench", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);


            GameObject vertDisplay = MenuHelpers.CreateAndPlaceGameObject("Vertical Display", displayDevicesChild, typeof(Camera), typeof(TrackedProjectionScreen));

            Camera vertCam = vertDisplay.GetComponent<Camera>();
            vertCam.transform.position = defaultCameraPosition;
            vertCam.stereoTargetEye = StereoTargetEyeMask.Both;
            vertCam.tag = "MainCamera";
            vertCam.rect = new Rect(0, 0.499f, 1, 0.499f);

            TrackedProjectionScreen trackedScreen = vertDisplay.GetComponent<TrackedProjectionScreen>();

            GameObject touchDisplay = MenuHelpers.CreateAndPlaceGameObject("Touch Display", displayDevicesChild, typeof(Camera));
            Camera touchCam = touchDisplay.GetComponent<Camera>();
            touchCam.transform.localPosition = new Vector3(0, 10, 0);
            touchCam.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
            touchCam.orthographic = true;
            touchCam.rect = new Rect(0, 0, 1, 0.499f);

            Selection.activeGameObject = vrConfigObj;
        }



        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_ClusterServer (Tiled Display Server)", false, MenuHelpers.vrConfigSec2Priority)]
        public static void CreateVRConfigClusterServer(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = MenuHelpers.CreateVRConfigTemplate(command, "ClusterServer", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            vrConfigObj.AddComponent<ClusterServer>();

            Camera c = displayDevicesChild.AddComponent<Camera>();
            c.transform.position = defaultCameraPosition;
            c.stereoTargetEye = StereoTargetEyeMask.Both;
            c.tag = "MainCamera";
            TrackedProjectionScreen trackedScreen = displayDevicesChild.AddComponent<TrackedProjectionScreen>();
        }


        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_ClusterClient (Tiled Display Client)", false, MenuHelpers.vrConfigSec2Priority)]
        public static void CreateVRConfigClusterClient(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject inputDevicesChild = null;
            GameObject displayDevicesChild = null;
            GameObject eventAliasesChild = null;
            GameObject vrConfigObj = MenuHelpers.CreateVRConfigTemplate(command, "ClusterClient", ref inputDevicesChild, ref displayDevicesChild, ref eventAliasesChild);

            vrConfigObj.AddComponent<ClusterClient>();

            Camera c = displayDevicesChild.AddComponent<Camera>();
            c.transform.position = defaultCameraPosition;
            c.stereoTargetEye = StereoTargetEyeMask.Both;
            c.tag = "MainCamera";
            TrackedProjectionScreen trackedScreen = displayDevicesChild.AddComponent<TrackedProjectionScreen>();
        }






        // ---- Section 3 ----
        // Tools for working with VRConfigs.

        [MenuItem("GameObject/MinVR/VRConfig/VRConfig Mask", false, MenuHelpers.vrConfigSec3Priority)]
        public static void CreateVRConfigVRConfigMask(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Config Mask", command.context as GameObject, typeof(VRConfigMask));
        }


    } // end class

} // end namespace
