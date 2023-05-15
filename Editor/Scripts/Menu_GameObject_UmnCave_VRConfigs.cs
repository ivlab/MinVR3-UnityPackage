using UnityEditor;
using UnityEngine;
using IVLab.MinVR3;

#if MINVR3_HAS_VRPN_PLUGIN
using IVLab.MinVR3.VRPN;
#endif


// disable warnings about unused functions because these editor menu functions can look to the compiler
// as though they are never called
#pragma warning disable IDE0051

namespace IVLab.MinVR3
{
    public class Menu_GameObject_UmnCave_VRConfigs : MonoBehaviour
    {
        private const float CaveSize = 1.2192f; // meters (4 feet from center of CAVE)
        private const string HeadVrpnDeviceName = "head";
        private const string MotiveServerIp = "10.0.50.203";
        private const string HeadPositionEventName = "Head/Position";
        private const string HeadRotationEventName = "Head/Rotation";

        private const int SingleWindowPositionX = -1280; // Assumes "primary" projector is front top from viewer's perspective
        private const int SingleWindowPositionY = 0;
        private const int SingleWindowWidth = 5120;
        private const int SingleWindowHeight = 1280; // technically CAVE height is 1440px but bottom 200 or so px are cut off
        private const bool ShowWindowBorders = false;

        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_UMN CAVE 3-Wall (Single Window)", false, MenuHelpers.vrConfigSec1Priority)]
        public static void CreateVRConfig3WallCaveSingleWindow(MenuCommand command)
        {
            CreateCaveConfigSingleWindow(3, command);
        }
        

        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_UMN CAVE 4-Wall (Single Window)", false, MenuHelpers.vrConfigSec1Priority)]
        public static void CreateVRConfig4WallCaveSingleWindow(MenuCommand command)
        {
            CreateCaveConfigSingleWindow(4, command);
        }

        private static void CreateCaveConfigSingleWindow(int walls, MenuCommand command)
        {
            if (walls != 3 && walls != 4)
            {
                Debug.LogError("Only able to create a 3 or 4 wall CAVE");
                return;
            }

            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject parentObject = command.context as GameObject;
            GameObject config = new GameObject("VRConfig_UMNCave-" + walls + "Wall");
            config.AddComponent<VRConfig>();
            if (parentObject != null)
            {
                config.transform.SetParent(parentObject.transform);
            }

            GameObject inputDevices = new GameObject("Input Devices");
            inputDevices.transform.SetParent(config.transform);

// version defined in IVLab.MinVR3.UmnCave.Editor.asmdef
#if MINVR3_HAS_VRPN_PLUGIN
            VRPNTracker trackerHead = MenuHelpers.CreateAndPlaceGameObject("VRPN Tracker 'head'", inputDevices, typeof(VRPNTracker)).GetComponent<VRPNTracker>();

            // Motive tracking is right-handed y-up
            trackerHead.incomingCoordinateSystem = new CoordConversion.CoordSystem
            (
                CoordConversion.CoordSystem.Handedness.RightHanded,
                CoordConversion.CoordSystem.Axis.PosY,
                CoordConversion.CoordSystem.Axis.NegZ
            );
            trackerHead.minVR3PositionEventName = HeadPositionEventName;
            trackerHead.minVR3RotationEventName = HeadRotationEventName;
            trackerHead.vrpnDevice = HeadVrpnDeviceName;
            trackerHead.vrpnServer = MotiveServerIp;
#else
            Debug.LogWarning("MinVR3 VRPN plugin not found. Please install the plugin or select a different source for the perspective tracking events.");
#endif

            // Window configuration (set position, width, height, window borders)
            GameObject windowConfiguration = new GameObject("Window Configuration");
            windowConfiguration.transform.SetParent(config.transform);
            WindowSettings wcfg = windowConfiguration.AddComponent<WindowSettings>();
            wcfg.upperLeftX = SingleWindowPositionX;
            wcfg.upperLeftY = SingleWindowPositionY;
            wcfg.width = SingleWindowWidth;
            wcfg.height = SingleWindowHeight;
            wcfg.showWindowBorders = ShowWindowBorders;

            GameObject displayDevices = new GameObject("Display Devices");
            displayDevices.transform.SetParent(config.transform);

            var wallNameList = new string[] { "Left", "Front", "Right", "Floor" };
            var wallColorList = new Color[wallNameList.Length];
            var cornerList = new TrackedProjectionScreen.ScreenCorners[]
            {
                // left wall
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(-CaveSize, 2*CaveSize, -CaveSize),
                    bottomLeft = new Vector3(-CaveSize, 0, -CaveSize),
                    topRight = new Vector3(-CaveSize, 2*CaveSize, CaveSize),
                    bottomRight = new Vector3(-CaveSize, 0, CaveSize),
                },
                // front wall
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(-CaveSize, 2*CaveSize, CaveSize),
                    bottomLeft = new Vector3(-CaveSize, 0, CaveSize),
                    topRight = new Vector3(CaveSize, 2*CaveSize, CaveSize),
                    bottomRight = new Vector3(CaveSize, 0, CaveSize),
                },
                // right wall
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(CaveSize, 2*CaveSize, CaveSize),
                    bottomLeft = new Vector3(CaveSize, 0, CaveSize),
                    topRight = new Vector3(CaveSize, 2*CaveSize, -CaveSize),
                    bottomRight = new Vector3(CaveSize, 0, -CaveSize),
                },
                // floor
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(-CaveSize, 0, CaveSize),
                    bottomLeft = new Vector3(-CaveSize, 0, -CaveSize),
                    topRight = new Vector3(CaveSize, 0, CaveSize),
                    bottomRight = new Vector3(CaveSize, 0, -CaveSize),
                }
            };

            float camWidth = 1.0f / (float) wallNameList.Length;
            for (int i = 0; i < walls; i++)
            {
                GameObject wall = new GameObject(wallNameList[i] + " Wall");
                var corners = cornerList[i];

                wall.transform.SetParent(displayDevices.transform);
                var cam = wall.AddComponent<Camera>();
                cam.rect = new Rect(camWidth * i, 0, camWidth, 1.0f);

                // avoid a bunch of C floating point errors when camera is on the floor
                if (wallNameList[i].Contains("Floor"))
                {
                    cam.transform.position = new Vector3(0, 0.001f, 0);
                }

                var tps = wall.AddComponent<TrackedProjectionScreen>();
                tps.trackingSpaceCorners = corners;
                tps.debugColor = Color.Lerp(Color.white, Color.green, i / (float) wallNameList.Length);

#if MINVR3_HAS_VRPN_PLUGIN
                tps.headTrackingPosEvent = VREventPrototypeVector3.Create(HeadPositionEventName);
                tps.headTrackingRotEvent = VREventPrototypeQuaternion.Create(HeadRotationEventName);
#endif
            }
            
        }


        private static void CreateCaveConfigClustered(int walls, MenuCommand command)
        {
            if (walls != 3 && walls != 4)
            {
                Debug.LogError("Only able to create a 3 or 4 wall CAVE");
                return;
            }

            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            GameObject parentObject = command.context as GameObject;
            GameObject config = new GameObject("VRConfig_UMNCave-" + walls + "Wall");
            config.AddComponent<VRConfig>();
            if (parentObject != null)
            {
                config.transform.SetParent(parentObject.transform);
            }

            GameObject inputDevices = new GameObject("Input Devices");
            inputDevices.transform.SetParent(config.transform);

            // version defined in IVLab.MinVR3.UmnCave.Editor.asmdef
#if MINVR3_HAS_VRPN_PLUGIN
            VRPNTracker trackerHead = MenuHelpers.CreateAndPlaceGameObject("VRPN Tracker 'head'", inputDevices, typeof(VRPNTracker)).GetComponent<VRPNTracker>();

            // Motive tracking is right-handed y-up
            trackerHead.incomingCoordinateSystem = new CoordConversion.CoordSystem
            (
                CoordConversion.CoordSystem.Handedness.RightHanded,
                CoordConversion.CoordSystem.Axis.PosY,
                CoordConversion.CoordSystem.Axis.NegZ
            );
            trackerHead.minVR3PositionEventName = HeadPositionEventName;
            trackerHead.minVR3RotationEventName = HeadRotationEventName;
            trackerHead.vrpnDevice = HeadVrpnDeviceName;
            trackerHead.vrpnServer = MotiveServerIp;
#else
            Debug.LogWarning("MinVR3 VRPN plugin not found. Please install the plugin or select a different source for the perspective tracking events.");
#endif

            // Window configuration (set position, width, height, window borders)
            GameObject windowConfiguration = new GameObject("Window Configuration");
            windowConfiguration.transform.SetParent(config.transform);
            WindowSettings wcfg = windowConfiguration.AddComponent<WindowSettings>();
            wcfg.upperLeftX = SingleWindowPositionX;
            wcfg.upperLeftY = SingleWindowPositionY;
            wcfg.width = SingleWindowWidth;
            wcfg.height = SingleWindowHeight;
            wcfg.showWindowBorders = ShowWindowBorders;

            GameObject displayDevices = new GameObject("Display Devices");
            displayDevices.transform.SetParent(config.transform);

            var wallNameList = new string[] { "Left", "Front", "Right", "Floor" };
            var wallColorList = new Color[wallNameList.Length];
            var cornerList = new TrackedProjectionScreen.ScreenCorners[]
            {
                // left wall
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(-CaveSize, 2*CaveSize, -CaveSize),
                    bottomLeft = new Vector3(-CaveSize, 0, -CaveSize),
                    topRight = new Vector3(-CaveSize, 2*CaveSize, CaveSize),
                    bottomRight = new Vector3(-CaveSize, 0, CaveSize),
                },
                // front wall
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(-CaveSize, 2*CaveSize, CaveSize),
                    bottomLeft = new Vector3(-CaveSize, 0, CaveSize),
                    topRight = new Vector3(CaveSize, 2*CaveSize, CaveSize),
                    bottomRight = new Vector3(CaveSize, 0, CaveSize),
                },
                // right wall
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(CaveSize, 2*CaveSize, CaveSize),
                    bottomLeft = new Vector3(CaveSize, 0, CaveSize),
                    topRight = new Vector3(CaveSize, 2*CaveSize, -CaveSize),
                    bottomRight = new Vector3(CaveSize, 0, -CaveSize),
                },
                // floor
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(-CaveSize, 0, CaveSize),
                    bottomLeft = new Vector3(-CaveSize, 0, -CaveSize),
                    topRight = new Vector3(CaveSize, 0, CaveSize),
                    bottomRight = new Vector3(CaveSize, 0, -CaveSize),
                }
            };

            float camWidth = 1.0f / (float)wallNameList.Length;
            for (int i = 0; i < walls; i++)
            {
                GameObject wall = new GameObject(wallNameList[i] + " Wall");
                var corners = cornerList[i];

                wall.transform.SetParent(displayDevices.transform);
                var cam = wall.AddComponent<Camera>();
                cam.rect = new Rect(camWidth * i, 0, camWidth, 1.0f);

                // avoid a bunch of C floating point errors when camera is on the floor
                if (wallNameList[i].Contains("Floor"))
                {
                    cam.transform.position = new Vector3(0, 0.001f, 0);
                }

                var tps = wall.AddComponent<TrackedProjectionScreen>();
                tps.trackingSpaceCorners = corners;
                tps.debugColor = Color.Lerp(Color.white, Color.green, i / (float)wallNameList.Length);

#if MINVR3_HAS_VRPN_PLUGIN
                tps.headTrackingPosEvent = VREventPrototypeVector3.Create(HeadPositionEventName);
                tps.headTrackingRotEvent = VREventPrototypeQuaternion.Create(HeadRotationEventName);
#endif
            }

        }

    } // end class

} // end namespace
