using UnityEngine;
using IVLab.MinVR3.ExtensionMethods;

namespace IVLab.MinVR3
{
    /// <summary>
    /// This class implements yet another technique for bimanual object
    /// manipulation. This implementation is based off the 6DOF technique
    /// described in the paper Mid-Air Interactions Above Stereoscopic
    /// Interactive Tables (Mendes / Fonesca / Araujo 2014), but similar
    /// techniques exist prior to that paper as well.
    /// </summary>
    public class BimanualObjectManipulator : MonoBehaviour
    {
        [SerializeField] private VREventPrototype m_Cursor0DownEvent;
        [SerializeField] private VREventPrototypeVector3 m_Cursor0PosEvent;
        [SerializeField] private VREventPrototypeQuaternion m_Cursor0RotEvent;
        [SerializeField] private VREventPrototype m_Cursor0UpEvent;

        [SerializeField] private VREventPrototype m_Cursor1DownEvent;
        [SerializeField] private VREventPrototypeVector3 m_Cursor1PosEvent;
        [SerializeField] private VREventPrototypeQuaternion m_Cursor1RotEvent;
        [SerializeField] private VREventPrototype m_Cursor1UpEvent;

        [SerializeField] private VREventPrototypeGameObject m_ObjectSelectedEvent;
        [SerializeField] private VREventPrototypeGameObject m_ObjectDeselectedEvent;

        private FSM m_FSM;

        // Initialized?
        private bool lastPosInitialized;
        private bool lastRotInitialized;

        // two-handed manip variables
        private float initialDistBetweenCursors;
        private Vector3 initialObjectScale;
        private Matrix4x4 lastMidpointXform;

        private CursorState[] cursorStates;
        private class CursorState
        {
            public Matrix4x4 currentXform = Matrix4x4.identity;
            public Matrix4x4 lastXform = Matrix4x4.identity;
        }

        private VREventCallbackGameObject m_SelectObjCallback;
        private VREventCallbackGameObject m_DeselectObjCallback;

        // currently selected as reported by whatever selection script is being used
        private GameObject m_SelectedObj;
        // currently being manipulated because it was selected at the time the manipulation started
        // different from above in case the object becomes unselected after the manipulation has
        // begun
        private GameObject m_ManipulatingObj;

        [SerializeField]
        private GameObject hoveredObject;

        // Start is called before the first frame update
        void Start()
        {
            cursorStates = new CursorState[2];
            cursorStates[0] = new CursorState();
            cursorStates[1] = new CursorState();

            m_FSM = this.gameObject.AddComponent<FSM>();
            m_FSM.AddState("START");
            m_FSM.AddState("Grab0");
            m_FSM.AddState("Grab1");
            m_FSM.AddState("GrabBoth");

            m_FSM.AddArc("START", "Grab0", VREventCallbackAny.CreateRuntime(m_Cursor0DownEvent, InitManipulation));
            m_FSM.AddArc("Grab0", "GrabBoth", VREventCallbackAny.CreateRuntime(m_Cursor1DownEvent, InitTwoHandedManipulation));
            m_FSM.AddArc("GrabBoth", "Grab0", VREventCallbackAny.CreateRuntime(m_Cursor1UpEvent));
            m_FSM.AddArc("Grab0", "START", VREventCallbackAny.CreateRuntime(m_Cursor0UpEvent, CancelManipulation));

            m_FSM.AddArc("START", "Grab1", VREventCallbackAny.CreateRuntime(m_Cursor1DownEvent, InitManipulation));
            m_FSM.AddArc("Grab1", "GrabBoth", VREventCallbackAny.CreateRuntime(m_Cursor0DownEvent, InitTwoHandedManipulation));
            m_FSM.AddArc("GrabBoth", "Grab1", VREventCallbackAny.CreateRuntime(m_Cursor0UpEvent));
            m_FSM.AddArc("Grab1", "START", VREventCallbackAny.CreateRuntime(m_Cursor1UpEvent, CancelManipulation));

            m_FSM.AddArc("Grab0", "Grab0", VREventCallbackAny.CreateRuntime(m_Cursor0PosEvent, pos => UpdateCursorPosition(0, pos)));
            m_FSM.AddArc("Grab0", "Grab0", VREventCallbackAny.CreateRuntime(m_Cursor0RotEvent, rot => UpdateCursorRotation(0, rot)));

            m_FSM.AddArc("Grab1", "Grab1", VREventCallbackAny.CreateRuntime(m_Cursor1PosEvent, pos => UpdateCursorPosition(1, pos)));
            m_FSM.AddArc("Grab1", "Grab1", VREventCallbackAny.CreateRuntime(m_Cursor1RotEvent, rot => UpdateCursorRotation(1, rot)));

            m_FSM.AddArc("GrabBoth", "GrabBoth", VREventCallbackAny.CreateRuntime(m_Cursor0PosEvent, pos => UpdateCursorPosition(0, pos)));
            m_FSM.AddArc("GrabBoth", "GrabBoth", VREventCallbackAny.CreateRuntime(m_Cursor1PosEvent, pos => UpdateCursorPosition(1, pos)));
            m_FSM.AddArc("GrabBoth", "GrabBoth", VREventCallbackAny.CreateRuntime(m_Cursor0RotEvent, rot => UpdateCursorRotation(0, rot)));
            m_FSM.AddArc("GrabBoth", "GrabBoth", VREventCallbackAny.CreateRuntime(m_Cursor1RotEvent, rot => UpdateCursorRotation(1, rot)));

            // these are not part of the FSM; they are always active
            m_SelectObjCallback = VREventCallbackGameObject.CreateRuntime(m_ObjectSelectedEvent, (go) => m_SelectedObj = go);
            m_DeselectObjCallback = VREventCallbackGameObject.CreateRuntime(m_ObjectDeselectedEvent, (go) => m_SelectedObj = null);
            m_SelectObjCallback.StartListening();
            m_DeselectObjCallback.StartListening();
        }

        void OnDisable()
        {
            m_SelectObjCallback.StopListening();
            m_DeselectObjCallback.StopListening();
        }

        void Update()
        {
            if (m_ManipulatingObj != null)
            {
                // Handle two-handed manipulation
                if (m_FSM.currentStateID == m_FSM.GetStateID("GrabBoth"))
                {
                    Vector3 cur0Pos = cursorStates[0].currentXform.GetPosition();
                    Vector3 cur1Pos = cursorStates[1].currentXform.GetPosition();
                    float currentDistBetweenCursors = (cur1Pos - cur0Pos).magnitude;
                    float distRatio = currentDistBetweenCursors / initialDistBetweenCursors;

                    Vector3 averagePos = (cur0Pos + cur1Pos) / 2;
                    Vector3 between = cur1Pos - cur0Pos;
                    Vector3 averageUp = (cursorStates[0].currentXform.GetColumn(2) + cursorStates[1].currentXform.GetColumn(2)) / 2;

                    Vector3 averageForward = Vector3.Cross(between, averageUp);
                    averageUp = -Vector3.Cross(between, averageForward);

                    Quaternion averageRot = Quaternion.LookRotation(averageForward, averageUp);
                    float sharedScale = initialObjectScale.x * distRatio;

                    Matrix4x4 currentMidpointXform = Matrix4x4.TRS(averagePos, averageRot, sharedScale * Vector3.one);
                    Matrix4x4 deltaMatrix = currentMidpointXform * lastMidpointXform.inverse;

                    Matrix4x4 newObjMat = deltaMatrix * m_ManipulatingObj.transform.localToWorldMatrix;
                    m_ManipulatingObj.transform.FromMatrix(newObjMat);

                    lastMidpointXform = currentMidpointXform;
                }
                else
                {
                    // Handle one-handed manipulation
                    for (int cursorID = 0; cursorID < cursorStates.Length; cursorID++)
                    {
                        CursorState state = cursorStates[cursorID];
                        Matrix4x4 deltaMatrix = state.currentXform * state.lastXform.inverse;

                        // Move the manipulating object a bit each frame (the deltaMatrix)
                        Matrix4x4 newObjMat = deltaMatrix * m_ManipulatingObj.transform.localToWorldMatrix;
                        m_ManipulatingObj.transform.FromMatrix(newObjMat);
                    }
                }
            }

            // Update cursors for next frame
            for (int cursorID = 0; cursorID < cursorStates.Length; cursorID++)
            {
                cursorStates[cursorID].lastXform = cursorStates[cursorID].currentXform;
            }
        }

        public void InitManipulation()
        {
            lastPosInitialized = false;
            lastRotInitialized = false;
            m_ManipulatingObj = m_SelectedObj;
        }

        public void InitTwoHandedManipulation()
        {
            if (m_ManipulatingObj == null)
                return;
            initialDistBetweenCursors = (cursorStates[1].currentXform.GetPosition() - cursorStates[0].currentXform.GetPosition()).magnitude;
            initialObjectScale = m_ManipulatingObj.transform.localScale;
            Vector3 cur0Pos = cursorStates[0].currentXform.GetPosition();
            Vector3 cur1Pos = cursorStates[1].currentXform.GetPosition();
            float currentDistBetweenCursors = (cur1Pos - cur0Pos).magnitude;
            float distRatio = currentDistBetweenCursors / initialDistBetweenCursors;

            Vector3 averagePos = (cur0Pos + cur1Pos) / 2;
            Vector3 between = cur1Pos - cur0Pos;
            Vector3 averageUp = (cursorStates[0].currentXform.GetColumn(2) + cursorStates[1].currentXform.GetColumn(2)) / 2;

            Vector3 averageForward = Vector3.Cross(between, averageUp).normalized;
            averageUp = -Vector3.Cross(between, averageForward).normalized;

            Quaternion averageRot = Quaternion.LookRotation(averageForward, averageUp);
            float sharedScale = initialObjectScale.x * distRatio;

            lastMidpointXform = Matrix4x4.TRS(averagePos, averageRot, sharedScale * Vector3.one);
        }

        public void CancelManipulation()
        {
            m_ManipulatingObj = null;
        }

        public void UpdateCursorPosition(int cursorID, Vector3 pos)
        {
            cursorStates[cursorID].currentXform.SetPosition(pos);
            // Check if already initialized to avoid "snapping" back into place (initialize to first position on this move)
            if (!lastPosInitialized)
            {
                cursorStates[cursorID].lastXform.SetPosition(pos);
            }
            lastPosInitialized = true;
        }
        public void UpdateCursorRotation(int cursorID, Quaternion rot)
        {
            cursorStates[cursorID].currentXform.SetRotation(rot.normalized);
            // Check if already initialized to avoid "snapping" back into place (initialize to first rotation on this move)
            if (!lastRotInitialized)
            {
                cursorStates[cursorID].lastXform.SetRotation(rot.normalized);
            }
            lastRotInitialized = true;
        }
    }
}