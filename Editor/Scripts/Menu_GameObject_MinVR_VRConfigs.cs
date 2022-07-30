using UnityEditor;
using UnityEngine;


// disable warnings about unused functions because these editor menu functions can look to the compiler
// as though they are never called
#pragma warning disable IDE0051

namespace IVLab.MinVR3
{
    public class Menu_GameObject_MinVR_VRConfigs : MonoBehaviour
    {

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

            inputDevicesChild?.AddComponent<MouseAndKeyboard>();
            inputDevicesChild?.AddComponent<FakeTrackers>();

            Camera c = displayDevicesChild?.AddComponent<Camera>();
            c.tag = "MainCamera";
            displayDevicesChild?.AddComponent<TrackedDesktopCamera>();

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
            leftCam.rect = new Rect(0, 0, 0.499f, 1);
            leftCam.stereoTargetEye = StereoTargetEyeMask.Left;
            leftCam.tag = "MainCamera";

            GameObject rightObj = MenuHelpers.CreateAndPlaceGameObject("Right Camera", displayDevicesChild, typeof(Camera));
            Camera rightCam = rightObj.GetComponent<Camera>();
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
            clipCam.rect = new Rect(0, 0, 1, 0.75f);
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
            c.stereoTargetEye = StereoTargetEyeMask.Both;
            c.tag = "MainCamera";
            TrackedProjectionScreen trackedScreen = displayDevicesChild.AddComponent<TrackedProjectionScreen>();
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
