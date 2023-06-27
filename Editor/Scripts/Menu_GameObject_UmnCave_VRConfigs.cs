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
        private const string VRPNServerHost = "localhost";
        private const int MotiveServerPort = 3883;
        private const int WixelServerPort = 3884;

        private const string HeadVrpnDeviceName = "head";
        private const string HeadPositionEventName = "Head/Position";
        private const string HeadRotationEventName = "Head/Rotation";
        private const string WandVrpnDeviceName = "wand";
        private const string WandPositionEventName = "Wand/Position";
        private const string WandRotationEventName = "Wand/Rotation";
        private const string PenVrpnDeviceName = "pen";
        private const string PenPositionEventName = "Pen/Position";
        private const string PenRotationEventName = "Pen/Rotation";

        private const string PenWixelDeviceName = "PenButton";
        private const string WandWixelDeviceName = "WandButton";
        private const string ButtonEventName = "Button";




        private const int WindowOffsetX = 0; // Assumes "primary" projector is left top from viewer's perspective
        private const int WindowOffsetY = 0;
        private const int SingleWindowWidth = 5120;
        private const int SingleWindowHeight = 1280; // technically CAVE height is 1440px but bottom 200 or so px are cut off
        private const int MultiWindowWidth = 1280;
        private const int MultiWindowHeight = 1280; // technically CAVE height is 1440px but bottom 200 or so px are cut off
        private const float StereoSeparation = 0.06f; // 6cm IPD by default
        private const bool ShowWindowBorders = false;

        private const float CameraNearPlane = 0.01f;
        private const string CameraTag = "MainCamera";

        private const string ClusterServerIP = "127.0.0.1";
        private const int ClusterServerPort = 3490;

        private const string LauncherScriptName = "LaunchCave4Wall.bat";
        private static string LauncherScript { get => $@"@rem Start one graphics program per wall
START ""Left Wall"" {Application.productName}.exe -vrmode stereo -vrconfig ""Left Wall (Server)"" -logFile .\left.log
TIMEOUT /t 5
START ""Front Wall"" {Application.productName}.exe -vrmode stereo -vrconfig ""Front Wall (Client)""  -logFile .\front.log
TIMEOUT /t 5
START ""Right Wall"" {Application.productName}.exe -vrmode stereo -vrconfig ""Right Wall (Client)"" -logFile .\right.log
TIMEOUT /t 5
START ""Floor Wall"" {Application.productName}.exe -vrmode stereo -vrconfig ""Floor Wall (Client)"" -logFile .\floor.log
"; }

        // [MenuItem("GameObject/MinVR/VRConfig/VRConfig_UMN CAVE 3-Wall (Single Window)", false, MenuHelpers.vrConfigSec1Priority)]
        public static void CreateVRConfig3WallCaveSingleWindow(MenuCommand command)
        {
            CreateCaveConfigSingleWindow(3, command);
        }


        //[MenuItem("GameObject/MinVR/VRConfig/VRConfig_UMN CAVE 4-Wall (Single Window)", false, MenuHelpers.vrConfigSec1Priority)]
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
            trackerHead.vrpnServer = VRPNServerHost;
#else
            Debug.LogWarning("MinVR3 VRPN plugin not found. Please install the plugin or select a different source for the perspective tracking events.");
#endif

            // Window configuration (set position, width, height, window borders)
            GameObject windowConfiguration = new GameObject("Window Configuration");
            windowConfiguration.transform.SetParent(config.transform);
            WindowSettings wcfg = windowConfiguration.AddComponent<WindowSettings>();
            wcfg.upperLeftX = WindowOffsetX;
            wcfg.upperLeftY = WindowOffsetY;
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
                cam.stereoSeparation = StereoSeparation;

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

        // set up a 4 wall, 4-window cave
        [MenuItem("GameObject/MinVR/VRConfig/VRConfig_UMN CAVE 4-Wall (4-Window, Default)", false, MenuHelpers.vrConfigSec1Priority)]
        private static void CreateCaveConfigClustered(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            const int NumWalls = 4;

            // Create root object
            GameObject parentObject = command.context as GameObject;
            GameObject caveRoot = new GameObject("UMNCave-4Wall");

            // Add launcher script copyer
            CreateTextFileOnPostBuild buildScript = caveRoot.AddComponent<CreateTextFileOnPostBuild>();
            buildScript.settings.copyLocation = CreateTextFileOnPostBuild.PostBuildCopyLocation.BuildFolder;
            buildScript.settings.fileName = LauncherScriptName;
            buildScript.settings.fileText = LauncherScript;

            if (parentObject != null)
            {
                caveRoot.transform.SetParent(parentObject.transform);
            }


            // Define stuff needed for walls (dimensions, etc)
            var wallNameList = new string[] { "Left", "Front", "Right", "Floor" };
            var wallColorList = new Color[NumWalls];
            var cornerList = new TrackedProjectionScreen.ScreenCorners[]
            {
                // left wall
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(-CaveSize, CaveSize, -CaveSize),
                    bottomLeft = new Vector3(-CaveSize, -CaveSize, -CaveSize),
                    topRight = new Vector3(-CaveSize, CaveSize, CaveSize),
                    bottomRight = new Vector3(-CaveSize, -CaveSize, CaveSize),
                },
                // front wall
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(-CaveSize, CaveSize, CaveSize),
                    bottomLeft = new Vector3(-CaveSize, -CaveSize, CaveSize),
                    topRight = new Vector3(CaveSize, CaveSize, CaveSize),
                    bottomRight = new Vector3(CaveSize, -CaveSize, CaveSize),
                },
                // right wall
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(CaveSize, CaveSize, CaveSize),
                    bottomLeft = new Vector3(CaveSize, -CaveSize, CaveSize),
                    topRight = new Vector3(CaveSize, CaveSize, -CaveSize),
                    bottomRight = new Vector3(CaveSize, -CaveSize, -CaveSize),
                },
                // floor
                new TrackedProjectionScreen.ScreenCorners
                {
                    topLeft = new Vector3(-CaveSize, -CaveSize, CaveSize),
                    bottomLeft = new Vector3(-CaveSize, -CaveSize, -CaveSize),
                    topRight = new Vector3(CaveSize, -CaveSize, CaveSize),
                    bottomRight = new Vector3(CaveSize, -CaveSize, -CaveSize),
                }
            };

            GameObject[] walls = new GameObject[NumWalls];
            for (int i = 0; i < NumWalls; i++)
            {
                string wallName = wallNameList[i] + " Wall";

                // first one is the server
                if (i != 0)
                {
                    wallName += " (Client)";
                }
                else
                {
                    wallName += " (Server)";
                }

                GameObject wall = new GameObject(wallName);
                wall.AddComponent<VRConfig>();
                wall.transform.SetParent(caveRoot.transform);

                // Add cluster setup
                if (i != 0)
                {
                    ClusterClient client = wall.AddComponent<ClusterClient>();
                    client.serverIPAddress = ClusterServerIP;
                    client.serverPort = ClusterServerPort;
                }
                else
                {
                    ClusterServer server = wall.AddComponent<ClusterServer>();
                    server.serverPort = ClusterServerPort;
                    server.numClients = NumWalls - 1;
                }

                // Set up window settings
                WindowSettings wcfg = wall.AddComponent<WindowSettings>();
                wcfg.upperLeftX = WindowOffsetX + MultiWindowWidth * i;
                wcfg.upperLeftY = WindowOffsetY;
                wcfg.width = MultiWindowWidth;
                wcfg.height = MultiWindowHeight;
                wcfg.showWindowBorders = ShowWindowBorders;
                wcfg.windowTitle = wallName;

                // Setup camera
                var cam = wall.AddComponent<Camera>();
                cam.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                cam.stereoSeparation = StereoSeparation;
                cam.nearClipPlane = CameraNearPlane;
                cam.tag = CameraTag;

                // avoid a bunch of C floating point errors when camera is on the floor
                if (wallNameList[i].Contains("Floor"))
                {
                    cam.transform.position = new Vector3(0, 0.001f, 0);
                }

                // Setup tracked projection screen (off axis projection)
                var tps = wall.AddComponent<TrackedProjectionScreen>();
                tps.trackingSpaceCorners = cornerList[i];
                tps.debugColor = Color.Lerp(Color.white, Color.green, i / (float)wallNameList.Length);

#if MINVR3_HAS_VRPN_PLUGIN
                tps.headTrackingPosEvent = VREventPrototypeVector3.Create(HeadPositionEventName);
                tps.headTrackingRotEvent = VREventPrototypeQuaternion.Create(HeadRotationEventName);
#endif

                walls[i] = wall;
            }

            // Server (wall 0) needs the input devices attached
            GameObject inputDevices = new GameObject("Input Devices");
            inputDevices.transform.SetParent(walls[0].transform);

            // version defined in IVLab.MinVR3.UmnCave.Editor.asmdef
#if MINVR3_HAS_VRPN_PLUGIN
            // Create Trackers (from Motive / TrackingTools)
            VRPNTracker trackerHead = MenuHelpers.CreateAndPlaceGameObject("VRPN Tracker 'head'", inputDevices, typeof(VRPNTracker)).GetComponent<VRPNTracker>();
            VRPNTracker trackerPen = MenuHelpers.CreateAndPlaceGameObject("VRPN Tracker 'pen'", inputDevices, typeof(VRPNTracker)).GetComponent<VRPNTracker>();
            VRPNTracker trackerWand = MenuHelpers.CreateAndPlaceGameObject("VRPN Tracker 'wand'", inputDevices, typeof(VRPNTracker)).GetComponent<VRPNTracker>();

            VRPNTracker[] trackers = new VRPNTracker[] { trackerHead, trackerPen, trackerWand };

            foreach (VRPNTracker tracker in trackers)
            {
                // Motive tracking is right-handed y-up
                tracker.incomingCoordinateSystem = new CoordConversion.CoordSystem
                (
                    CoordConversion.CoordSystem.Handedness.RightHanded,
                    CoordConversion.CoordSystem.Axis.PosY,
                    CoordConversion.CoordSystem.Axis.NegZ
                );
                tracker.vrpnServer = VRPNServerHost + ":" + MotiveServerPort;
            }

            trackerHead.minVR3PositionEventName = HeadPositionEventName;
            trackerHead.minVR3RotationEventName = HeadRotationEventName;
            trackerHead.vrpnDevice = HeadVrpnDeviceName;

            trackerWand.minVR3PositionEventName = WandPositionEventName;
            trackerWand.minVR3RotationEventName = WandRotationEventName;
            trackerWand.vrpnDevice = WandVrpnDeviceName;

            trackerPen.minVR3PositionEventName = PenPositionEventName;
            trackerPen.minVR3RotationEventName = PenRotationEventName;
            trackerPen.vrpnDevice = PenVrpnDeviceName;

            // Create buttons (from Wixels)
            string[] buttonDevices = new string[] { PenWixelDeviceName, WandWixelDeviceName };
            int numButtons = 2;
            foreach (string buttonDevice in buttonDevices)
            {
                for (int b = 0; b < numButtons; b++)
                {
                    string deviceName = $"{buttonDevice}/{ButtonEventName}{b}";
                    VRPNButton button = MenuHelpers.CreateAndPlaceGameObject($"VRPN Button '{deviceName}'", inputDevices, typeof(VRPNButton)).GetComponent<VRPNButton>();
                    button.vrpnDevice = buttonDevice;
                    button.vrpnServer = VRPNServerHost + ":" + WixelServerPort;
                    button.vrpnButton = b;
                    button.minVR3EventName = deviceName;
                }
            }
#else
            Debug.LogWarning("MinVR3 VRPN plugin not found. Please install the plugin or select a different source for the perspective tracking events.");
#endif

        }

    } // end class

} // end namespace
