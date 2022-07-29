using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;


// disable warnings about unused functions because these editor menu functions can look to the compiler
// as though they are never called
#pragma warning disable IDE0051

namespace IVLab.MinVR3
{

    /// <summary>
    /// Adds MinVR items to Unity's GameObject menu.  Most options are pretty simple, just creating a new
    /// GameObject and adding a MinVR asset to it.  Several of the VRConfigs items are more complex as they
    /// can create several child objects and initialize them with appropriate settings.  A similar functionality
    /// could be achieved by instatiating prefabs, but this is not how the built-in functions in Unity's
    /// GameObject menu work.  Like the way objects created from Unity's GameObject menu work, the idea here
    /// is that these objects serve as useful starting points but the expectation is that the Programmer will
    /// want/need to edit and add to them.  If prefabs are used, this can create some confusion because some
    /// modifications, like reordering objects, are not allowed, and it's not completely clear to programmers if
    /// they should be updating the original prefab after changes are made or create a new one, etc.  As a
    /// lower-level support library,it seems best for MinVR to not use prefabs within the GameObject menu.
    /// </summary>
    public class Menu_GameObject_MinVR : MonoBehaviour
    {

        // ---- Section 1: Special "Get Started" Item ---

        [MenuItem("GameObject/MinVR/Get Started/Create \"VREngine\" and \"Room Space Origin\"", false, MenuHelpers.gameObjectMenuPriority)]
        public static void CreateVREngine(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
        }


        // ---- Section 2: All Other Items and Submenus ----


        // ---- VRCONFIGS SUBMENU ----
        // Split into Menu_GameObject_MinVR_VRConfigs.cs because it is so large


        // ---- VREVENT SUBMENU ----

        [MenuItem("GameObject/MinVR/VREvent/VREvent Alias", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateVREventVREventAlias(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("VREvent Alias", command.context as GameObject, typeof(VREventAlias));
        }

        [MenuItem("GameObject/MinVR/VREvent/Simple Event Listener", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateInteractionSimpleEventListener(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Event Listener", command.context as GameObject, typeof(SimpleEventListener));
        }



        // ---- INPUT DEVICES SUBMENU ----

        [MenuItem("GameObject/MinVR/Input Devices/Virtual/Proximity Event Producer", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateInputProximityEvent(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Proximity Event Producer", command.context as GameObject, typeof(ProximityEventProducer));
        }

        [MenuItem("GameObject/MinVR/Input Devices/Virtual/Modified Event Producer", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateInputModifiedEvent(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Modified Event Producer", command.context as GameObject, typeof(ModifiedEventProducer));
        }

        [MenuItem("GameObject/MinVR/Input Devices/Virtual/Callable Event Producer", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateInputCallableEvent(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Callable Event Producer", command.context as GameObject, typeof(CallableEventProducer));
        }

        [MenuItem("GameObject/MinVR/Input Devices/Virtual/Fake Tracking Device", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateInputFakeTrackers(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Fake Tracking Device", command.context as GameObject, typeof(FakeTrackers));
        }

        [MenuItem("GameObject/MinVR/Input Devices/Unity to MinVR/Unity XR", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateInputUnityXR(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("UnityXR", command.context as GameObject, typeof(UnityXR));
        }

        [MenuItem("GameObject/MinVR/Input Devices/Unity to MinVR/Mouse and Keyboard", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateInputMouseAndKeyboard(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Mouse and Keyboard", command.context as GameObject, typeof(MouseAndKeyboard));
        }

        [MenuItem("GameObject/MinVR/Input Devices/Unity to MinVR/Touch Builtin", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateInputTouch(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Touch Builtin", command.context as GameObject, typeof(TouchBuiltin));
        }

        [MenuItem("GameObject/MinVR/Input Devices/Unity to MinVR/Mobile Sensors", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateInputMobileSensors(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Mobile Sensors", command.context as GameObject, typeof(MobileSensors));
        }

        [MenuItem("GameObject/MinVR/Input Devices/Unity to MinVR/Input Actions (New Input System)", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateInputInputActions(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Input Actions", command.context as GameObject, typeof(InputActionsToVREvents));
        }



        // ---- DISPLAY DEVICES SUBMENU ----

        [MenuItem("GameObject/MinVR/Display Devices/Tracked Projection Screen", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateDisplayTrackedProjectionScreen(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Tracked Projection Screen", command.context as GameObject, typeof(TrackedProjectionScreen));
        }

        [MenuItem("GameObject/MinVR/Display Devices/Tracked Desktop Camera", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateDisplayTrackedDesktopCamera(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Tracked Desktop Camera", command.context as GameObject, typeof(TrackedDesktopCamera));
        }

        [MenuItem("GameObject/MinVR/Display Devices/Window Settings", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateDisplayWindowSettings(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Window Settings", command.context as GameObject, typeof(WindowSettings));
        }



        // ---- DEBUG SUBMENU ----

        [MenuItem("GameObject/MinVR/Debug/Draw Trackers", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateDebugDrawTrackers(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Draw Trackers", command.context as GameObject, typeof(DrawTrackers));
        }

        [MenuItem("GameObject/MinVR/Debug/Draw Frames Per Second", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateDebugDrawFPS(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Draw FPS", command.context as GameObject, typeof(DrawFPS));
        }

        [MenuItem("GameObject/MinVR/Debug/Draw Eyes", false, MenuHelpers.minVRSec2Priority)]
        public static void CreateDebugDrawEyes(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Draw Eyes", command.context as GameObject, typeof(DrawEyes));
        }

    } // end class

} // end namespace

#pragma warning restore IDE0051       
