using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    /// <summary>
    /// Partial implmentation of the classic bimanual UI in MultiGen's SmartScene
    /// application (1997-ish), originally developed for use with pinch gloves.  The
    /// technique is actually rather similar to modern multi-Cursor gestures but works
    /// in 3D to translate, rotate, and scale a scene using two hands in combination.
    /// Demo of the original 3DUI: https://www.youtube.com/watch?v=q4XmprH1S0s
    /// </summary>
    public class SmartScene : MonoBehaviour
    {

		public void Reset()
		{
			m_Cursor0DownEvent = VREventPrototype.Create("DHand/Activate DOWN");
			m_Cursor0PosEvent = VREventPrototypeVector3.Create("DHand/Position");
			m_Cursor0RotEvent = VREventPrototypeQuaternion.Create("DHand/Rotation");
			m_Cursor0UpEvent = VREventPrototype.Create("DHand/Activate UP");

			m_Cursor1DownEvent = VREventPrototype.Create("NDHand/Activate DOWN");
			m_Cursor1PosEvent = VREventPrototypeVector3.Create("NDHand/Position");
			m_Cursor1RotEvent = VREventPrototypeQuaternion.Create("DHand/Rotation");
			m_Cursor1UpEvent = VREventPrototype.Create("NDHand/Activate UP");

			m_ObjectSelectedEvent = VREventPrototypeGameObject.Create("Select");
			m_ObjectDeselectedEvent = VREventPrototypeGameObject.Create("Deselect");
		}

		public void Awake()
		{
			m_LastPos = new Vector3[2];
			m_LastPos[0] = new Vector3();
			m_LastPos[1] = new Vector3();

			m_FSM = this.gameObject.AddComponent<FSM>();
			m_FSM.AddState("START");
			m_FSM.AddState("Grab0");
			m_FSM.AddState("Grab1");
			m_FSM.AddState("GrabBoth");

			m_FSM.AddArc("START", "Grab0", VREventCallbackAny.CreateRuntime(m_Cursor0DownEvent, InitManipulation), m_RequireToken);
			m_FSM.AddArc("Grab0", "GrabBoth", VREventCallbackAny.CreateRuntime(m_Cursor1DownEvent));
			m_FSM.AddArc("GrabBoth", "Grab0", VREventCallbackAny.CreateRuntime(m_Cursor1UpEvent));
			m_FSM.AddArc("Grab0", "START", VREventCallbackAny.CreateRuntime(m_Cursor0UpEvent), null, m_RequireToken);

			m_FSM.AddArc("START", "Grab1", VREventCallbackAny.CreateRuntime(m_Cursor1DownEvent, InitManipulation), m_RequireToken);
			m_FSM.AddArc("Grab1", "GrabBoth", VREventCallbackAny.CreateRuntime(m_Cursor0DownEvent));
			m_FSM.AddArc("GrabBoth", "Grab1", VREventCallbackAny.CreateRuntime(m_Cursor0UpEvent));
			m_FSM.AddArc("Grab1", "START", VREventCallbackAny.CreateRuntime(m_Cursor1UpEvent), null, m_RequireToken);

			m_FSM.AddArc("Grab0", "Grab0", VREventCallbackAny.CreateRuntime(m_Cursor0PosEvent, pos => OneHandMove(0, pos)));
			m_FSM.AddArc("Grab0", "Grab0", VREventCallbackAny.CreateRuntime(m_Cursor1PosEvent, pos => m_LastPos[1] = pos));
			m_FSM.AddArc("Grab1", "Grab1", VREventCallbackAny.CreateRuntime(m_Cursor0PosEvent, pos => m_LastPos[0] = pos));
			m_FSM.AddArc("Grab1", "Grab1", VREventCallbackAny.CreateRuntime(m_Cursor1PosEvent, pos => OneHandMove(1, pos)));
			m_FSM.AddArc("GrabBoth", "GrabBoth", VREventCallbackAny.CreateRuntime(m_Cursor0PosEvent, pos => TwoHandMove(0, pos)));
			m_FSM.AddArc("GrabBoth", "GrabBoth", VREventCallbackAny.CreateRuntime(m_Cursor1PosEvent, pos => TwoHandMove(1, pos)));

			// these are not part of the FSM; they are always active
			m_SelectObjCallback = VREventCallbackGameObject.CreateRuntime(m_ObjectSelectedEvent, (go) => m_SelectedObj = go);
			m_DeselectObjCallback = VREventCallbackGameObject.CreateRuntime(m_ObjectDeselectedEvent, (go) => m_SelectedObj = null);
		}

		void OnEnable()
		{
			m_SelectObjCallback.StartListening();
			m_DeselectObjCallback.StartListening();
		}

		void OnDisable()
		{
			m_SelectObjCallback.StopListening();
			m_DeselectObjCallback.StopListening();
		}

		public void InitManipulation()
		{
			m_GrabInitialized = false;
			m_ManipulatingObj = m_SelectedObj;
		}

		public void OneHandMove(int cursorID, Vector3 pos)
		{
			if (m_GrabInitialized) {
				Vector3 deltaHandRoom = pos - m_LastPos[cursorID];
				Vector3 deltaHandWorld = m_RoomToWorld.TransformVector(deltaHandRoom);

				if (m_ManipulatingObj != null) {
					m_ManipulatingObj.transform.TranslateByWorldVector(deltaHandWorld);
				} else {
					m_RoomToWorld.TranslateByWorldVector(-deltaHandWorld);
				}
			}
			m_GrabInitialized = true;
			m_LastPos[cursorID] = pos;
		}

		public Vector3 RoomToWorld(Vector3 roomPoint)
		{
			return m_RoomToWorld.TransformPoint(roomPoint);
		}

		public Vector3 WorldToLocal(Vector3 worldPoint, Transform localFrame)
		{
			return localFrame.InverseTransformPoint(worldPoint);
		}

		public void TwoHandMove(int cursorID, Vector3 pos)
		{
			// Two Cursor Default Mode (typical multi-Cursor rotate, pinch, & zoom)

			// t1 is id of the Cursor that has moved since the last call
			int t1 = cursorID;
			// t0 is the other Cursor that has not moved
			int t0 = t1 == 0 ? 1 : 0;

			Vector3 t1LocalOrig = m_LastPos[t1];
			Vector3 t1LocalDesired = pos;

			// orig and desired are the same for t0 since it has not moved this frame
			Vector3 t0Local = m_LastPos[t0];

			// strategy: since t0 hasn't moved (this frame), it can act as a pivot and we can rotate and scale
			// around t0 to get t1 to the correct place.
			Vector3 pivotLocal = t0Local;
			Vector3 pivotWorld = m_RoomToWorld.TransformPoint(t0Local);

			// axis spanning the two Cursor points and pointing from t0 to t1
			Vector3 origAxisLocal = t1LocalOrig - t0Local;
			Vector3 desiredAxisLocal = t1LocalDesired - t0Local;
			Vector3 origAxisWorld = m_RoomToWorld.TransformVector(origAxisLocal);
			Vector3 desiredAxisWorld = m_RoomToWorld.TransformVector(desiredAxisLocal);

			Quaternion deltaRotLocal = Quaternion.FromToRotation(origAxisLocal, desiredAxisLocal);
			Quaternion deltaRotWorld = Quaternion.FromToRotation(origAxisWorld, desiredAxisWorld);

			float deltaScale = desiredAxisLocal.magnitude / origAxisLocal.magnitude;

			if (m_ManipulatingObj != null) {
				m_ManipulatingObj.transform.RotateAroundWorldPoint(pivotWorld, deltaRotWorld);
				m_ManipulatingObj.transform.ScaleAroundWorldPoint(pivotWorld, deltaScale);
			} else {
				m_RoomToWorld.RotateAroundLocalPoint(pivotLocal, Quaternion.Inverse(deltaRotLocal));
				m_RoomToWorld.ScaleAroundLocalPoint(pivotLocal, 1.0f / deltaScale);
			}

			m_LastPos[cursorID] = pos;
		}

		[Tooltip("MinVRRoot (Room Space)")]
		[SerializeField] private Transform m_RoomToWorld;

		[SerializeField] private SharedToken m_RequireToken;

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

		// runtime only
		private FSM m_FSM;
		private bool m_GrabInitialized;
		private Vector3[] m_LastPos;
		private VREventCallbackGameObject m_SelectObjCallback;
		private VREventCallbackGameObject m_DeselectObjCallback;

		// currently selected as reported by whatever selection script is being used
		private GameObject m_SelectedObj;
		// currently being manipulated because it was selected at the time the manipulation started
		// different from above in case the object becomes unselected after the manipulation has
		// begun
		private GameObject m_ManipulatingObj;
	}

} // end namespace
