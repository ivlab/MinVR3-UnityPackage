using UnityEditor;
using UnityEngine;


// disable warnings about unused functions because these editor menu functions can look to the compiler
// as though they are never called
#pragma warning disable IDE0051

namespace IVLab.MinVR3
{

    public class Menu_GameObject_MinVRInteraction : MonoBehaviour
    {

        // ---- BUILDING BLOCKS ----

        [MenuItem("GameObject/MinVR Interaction/Building Blocks/Simple Event Listener", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionSimpleEventListener(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Event Listener", command.context as GameObject, typeof(SimpleEventListener));
        }

        [MenuItem("GameObject/MinVR Interaction/Building Blocks/FSM", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionFSM(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("FSM", command.context as GameObject, typeof(FSM));
        }


        [MenuItem("GameObject/MinVR Interaction/Building Blocks/Shared Token (e.g., Input Focus Token)", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionSharedToken(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Input Focus Token", command.context as GameObject, typeof(SharedToken));
        }




        // ---- CURSORS -----


        [MenuItem("GameObject/MinVR Interaction/Cursors/Tracked Pose Driver", false, MenuHelpers.gameObjectMenuPriority)]
        public static void CreateInteractionTrackedPoseDriver(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Tracked Pose Driver", command.context as GameObject, typeof(TrackedPoseDriver));
        }

        [MenuItem("GameObject/MinVR Interaction/Cursors/CavePainting Brush (Dominant Hand)", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionCavePaintingBrush(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            // unless user has explicitly parented to another object, Cursors should be parented
            // to the Room Space Origin, since tracking data are provided in that coordiante frame.
            GameObject parent = command.context as GameObject;
            if (parent == null) {
                // find RoomSpaceOrigin should never fail here since we create it if needed above
                parent = FindObjectOfType<RoomSpaceOrigin>().gameObject;
            }

            GameObject cursorRoot = MenuHelpers.CreateAndPlaceGameObject("CavePainting Brush Cursor (Dominant Hand)", parent, typeof(TrackedPoseDriver));
            TrackedPoseDriver poseDriver = cursorRoot.GetComponent<TrackedPoseDriver>();
            poseDriver.positionEvent = VREventPrototypeVector3.Create("DH/Position");
            poseDriver.rotationEvent = VREventPrototypeQuaternion.Create("DH/Rotation");

            MenuHelpers.CreateAndPlaceGameObject("Brush Model", cursorRoot, typeof(CavePaintingBrushCursor));

            Selection.activeGameObject = cursorRoot;
        }

        [MenuItem("GameObject/MinVR Interaction/Cursors/Small Cone (Dominant Hand)", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionCone(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            // unless user has explicitly parented to another object, Cursors should be parented
            // to the Room Space Origin, since tracking data are provided in that coordiante frame.
            GameObject parent = command.context as GameObject;
            if (parent == null) {
                // find RoomSpaceOrigin should never fail here since we create it if needed above
                parent = FindObjectOfType<RoomSpaceOrigin>().gameObject;
            }

            GameObject cursorRoot = MenuHelpers.CreateAndPlaceGameObject("Small Cone (Dominant Hand)", parent, typeof(TrackedPoseDriver));
            TrackedPoseDriver poseDriver = cursorRoot.GetComponent<TrackedPoseDriver>();
            poseDriver.positionEvent = VREventPrototypeVector3.Create("DH/Position");
            poseDriver.rotationEvent = VREventPrototypeQuaternion.Create("DH/Rotation");

            GameObject coneObj = Instantiate(Resources.Load<GameObject>("Models/cone"));
            coneObj.transform.SetParent(cursorRoot.transform);
            coneObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            Selection.activeGameObject = cursorRoot;
        }

        [MenuItem("GameObject/MinVR Interaction/Cursors/Small Cube (Non-Dominant Hand)", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionSmallCubeCursor(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();

            // unless user has explicitly parented to another object, Cursors should be parented
            // to the Room Space Origin, since tracking data are provided in that coordiante frame.
            GameObject parent = command.context as GameObject;
            if (parent == null) {
                // find RoomSpaceOrigin should never fail here since we create it if needed above
                parent = FindObjectOfType<RoomSpaceOrigin>().gameObject;
            }

            GameObject cursorRoot = MenuHelpers.CreateAndPlaceGameObject("Small Cube Cursor (Non-Dominant Hand)", parent, typeof(TrackedPoseDriver));
            TrackedPoseDriver poseDriver = cursorRoot.GetComponent<TrackedPoseDriver>();
            poseDriver.positionEvent = VREventPrototypeVector3.Create("NDH/Position");
            poseDriver.rotationEvent = VREventPrototypeQuaternion.Create("NDH/Rotation");

            GameObject cubeObj = MenuHelpers.CreateAndPlacePrimitive("Cube Model", cursorRoot, PrimitiveType.Cube);
            DestroyImmediate(cubeObj.GetComponentInChildren<BoxCollider>());
            cubeObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            Selection.activeGameObject = cursorRoot;
        }




        // ---- WIDGETS ----

        [MenuItem("GameObject/MinVR Interaction/Widgets/Menus/Basic Floating Menu", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionFloatingMenu(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Floating Menu", command.context as GameObject, typeof(FloatingMenu));
        }




        // ---- SELECTION ----

        [MenuItem("GameObject/MinVR Interaction/Selection/Basic Object Selector", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionBasicObjectSelector(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Object Selector", command.context as GameObject, typeof(BasicObjectSelector));
        }

        [MenuItem("GameObject/MinVR Interaction/Selection/Bimanual Object Selector", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionBimanualObjectSelector(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Bimanual Object Selector", command.context as GameObject, typeof(BimanualObjectSelector));
        }

        [MenuItem("GameObject/MinVR Interaction/Selection/Basic Highlighter", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionBasicHighlighter(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Basic Highlighter", command.context as GameObject, typeof(BasicHighlighter));
        }




        // ---- NAVIGATION AND MANIPULATION ----

        [MenuItem("GameObject/MinVR Interaction/Navigation & Manipulation/Smart Scene", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionSmartScene(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Smart Scene", command.context as GameObject, typeof(SmartScene));
        }

        [MenuItem("GameObject/MinVR Interaction/Navigation & Manipulation/Bimanual Object Manipulation", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionBimanualObjectManipulator(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Bimanual Object Manipulator", command.context as GameObject, typeof(BimanualObjectManipulator));
        }





        // ---- DESKTOP ----

        [MenuItem("GameObject/MinVR Interaction/Desktop/Trackball Camera", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionTrackballCamera(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Trackball Camera", command.context as GameObject, typeof(TrackballCamera));
        }

        [MenuItem("GameObject/MinVR Interaction/Desktop/UniCam", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionUniCam(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("UniCam", command.context as GameObject, typeof(UniCam));
        }

        [MenuItem("GameObject/MinVR Interaction/Desktop/Mouse-Object Manipulator", false, MenuHelpers.mvriItemPriority)]
        public static void CreateInteractionMouseObjectManipulator(MenuCommand command)
        {
            MenuHelpers.CreateVREngineIfNeeded();
            MenuHelpers.CreateRoomSpaceOriginIfNeeded();
            MenuHelpers.CreateAndPlaceGameObject("Mouse-Object Manipulator", command.context as GameObject, typeof(MouseObjectManipulator));
        }
    } // end class

} // end namespace
