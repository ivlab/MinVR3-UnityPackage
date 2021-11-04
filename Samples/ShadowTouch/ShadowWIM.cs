using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using IVLab.MinVR3;

public class ShadowWIM : MonoBehaviour
{
	public class TouchData
	{
		public TouchData()
		{
			pos = new Vector2();
			lastPos = new Vector2();
			total = Vector3.zero;
			active = false;
		}
		public void Init(Vector2 curPos)
		{
			// make current and last positions equal so delta is initially zero
			lastPos = curPos;
			pos = curPos;
			total = Vector3.zero;
			active = true;
		}
		public void Update(Vector2 curPos)
		{
			lastPos = pos;
			pos = curPos;
			total += pos - lastPos;
		}
		public void ResetTotal()
		{
			total = Vector3.zero;
		}
		public bool active;
		public Vector2 pos;
		public Vector2 lastPos;
		public Vector2 total;
	}


	public void Reset()
	{
		m_Finger0DownEvent = VREventPrototypeVector2.Create("Touch/Finger 0 DOWN");
		m_Finger0MoveEvent = VREventPrototypeVector2.Create("Touch/Finger 0/Position");
		m_Finger0UpEvent = VREventPrototypeVector2.Create("Touch/Finger 0 UP");

		m_Finger1DownEvent = VREventPrototypeVector2.Create("Touch/Finger 1 DOWN");
		m_Finger1MoveEvent = VREventPrototypeVector2.Create("Touch/Finger 1/Position");
		m_Finger1UpEvent = VREventPrototypeVector2.Create("Touch/Finger 1 UP");

		m_Finger2DownEvent = VREventPrototypeVector2.Create("Touch/Finger 2 DOWN");
		m_Finger2MoveEvent = VREventPrototypeVector2.Create("Touch/Finger 2/Position");
		m_Finger2UpEvent = VREventPrototypeVector2.Create("Touch/Finger 2 UP");
	}

	public void Start()
	{
		m_TouchPlane = new Plane(Vector3.up, Vector3.zero);
		m_TouchData = new TouchData[3];
		m_TouchWorld = new Vector3[3];
		for (int i = 0; i < 3; i++) {
			m_TouchData[i] = new TouchData();
			m_TouchWorld[i] = new Vector3();
		}
		m_TwoTouchMode = TwoTouchMode.Default;

		m_FSM = this.gameObject.AddComponent<FSM>();
		m_FSM.AddState("START");
		m_FSM.AddState("Touch0");
		m_FSM.AddState("Touch1");
		m_FSM.AddState("TouchBoth", VRCallback.CreateRuntime(OnTouchBothEnter));

		m_FSM.AddArc("START", "Touch0", VREventCallbackAny.CreateRuntime(m_Finger0DownEvent, pos => OnTouchDown(0, pos)));
		m_FSM.AddArc("Touch0", "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger1DownEvent, pos => OnTouchDown(1, pos)));
		m_FSM.AddArc("TouchBoth", "Touch0", VREventCallbackAny.CreateRuntime(m_Finger1UpEvent, pos => OnTouchUp(1, pos)));
		m_FSM.AddArc("Touch0", "START", VREventCallbackAny.CreateRuntime(m_Finger0UpEvent, pos => OnTouchUp(0, pos)));

		m_FSM.AddArc("START", "Touch1", VREventCallbackAny.CreateRuntime(m_Finger1DownEvent, pos => OnTouchDown(1, pos)));
		m_FSM.AddArc("Touch1", "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger0DownEvent, pos => OnTouchDown(0, pos)));
		m_FSM.AddArc("TouchBoth", "Touch1", VREventCallbackAny.CreateRuntime(m_Finger0UpEvent, pos => OnTouchUp(0, pos)));
		m_FSM.AddArc("Touch1", "START", VREventCallbackAny.CreateRuntime(m_Finger1UpEvent, pos => OnTouchUp(1, pos)));

		m_FSM.AddArc("Touch0", "Touch0", VREventCallbackAny.CreateRuntime(m_Finger0MoveEvent, pos => OneTouchMove(0, pos)));
		m_FSM.AddArc("Touch1", "Touch1", VREventCallbackAny.CreateRuntime(m_Finger1MoveEvent, pos => OneTouchMove(1, pos)));
		m_FSM.AddArc("TouchBoth", "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger0MoveEvent, pos => TwoTouchMove(0, pos)));
		m_FSM.AddArc("TouchBoth", "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger1MoveEvent, pos => TwoTouchMove(1, pos)));

		m_FSM.AddArc("TouchBoth", "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger2DownEvent, pos => OnTouchDown(2, pos)));
		m_FSM.AddArc("TouchBoth", "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger2MoveEvent, ThirdTouchMove));
		m_FSM.AddArc("TouchBoth", "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger2UpEvent, pos => OnTouchUp(2, pos)));
	}


	public void OnTouchDown(int cursorID, Vector2 pos)
	{
		m_TouchData[cursorID].Init(pos);
		m_TouchWorld[cursorID] = TouchToWorld(pos);
	}

	public void OnTouchUp(int cursorID, Vector2 pos)
	{
		m_TouchData[cursorID].active = false;
		m_TouchWorld[cursorID] = TouchToWorld(pos);
	}

	public void OneTouchMove(int cursorID, Vector2 pos)
	{
		m_TouchData[cursorID].Update(pos);
		m_TouchWorld[cursorID] = TouchToWorld(pos);

		Vector3 lastTouchWorld = TouchToWorld(m_TouchData[cursorID].lastPos);
		Vector3 deltaWorld = m_TouchWorld[cursorID] - lastTouchWorld;
		m_Object.TranslateByWorldVector(deltaWorld);
	}

	public void OnTouchBothEnter()
	{
		m_TwoTouchMode = TwoTouchMode.Default;
		m_TouchData[0].ResetTotal();
		m_TouchData[1].ResetTotal();
	}


	public Vector3 TouchToWorld(Vector2 touchInViewportCoords)
	{
		Ray ray = m_ShadowCamera.ViewportPointToRay(touchInViewportCoords);
		float dist;
		if (m_TouchPlane.Raycast(ray, out dist)) {
			return ray.origin + dist * ray.direction;
		} else {
			return Vector3.zero;
		}
	}

	public Vector3 TouchToLocal(Vector2 touchInViewportCoords, Transform localFrame)
	{
		Vector3 touchWorld = TouchToWorld(touchInViewportCoords);
		//Debug.Log("W: " + touchWorld);
		Vector3 touchLocal = localFrame.InverseTransformPoint(touchWorld);
		//Debug.Log("L: " + touchLocal);
		return touchLocal;
	}

	public void TwoTouchMove(int cursorID, Vector2 pos)
	{
		// update based on latest input
		m_TouchData[cursorID].Update(pos);
		m_TouchWorld[cursorID] = TouchToWorld(pos);

		// Two-Touch always starts in the default "pinch zoom" mode, but can enter a roll or pitch mode
		// Start with the logic to change modes
		float x0 = m_TouchData[0].total.x;
		float x1 = m_TouchData[1].total.x;

		float y0 = m_TouchData[0].total.y;
		float y1 = m_TouchData[1].total.y;

		bool x0MovedEnough = Mathf.Abs(x0) > MOVE_EPSILON;
		bool x1MovedEnough = Mathf.Abs(x1) > MOVE_EPSILON;
		bool xsMovedEnough = x0MovedEnough && x1MovedEnough;

		bool y0MovedEnough = Mathf.Abs(y0) > MOVE_EPSILON;
		bool y1MovedEnough = Mathf.Abs(y1) > MOVE_EPSILON;
		bool ysMovedEnough = y0MovedEnough && y1MovedEnough;

		bool xsMovedInSameDir = x0 * x1 > 0.0f;
		bool ysMovedInSameDir = y0 * y1 > 0.0f;

		if ((m_TwoTouchMode != TwoTouchMode.Roll) && xsMovedEnough && xsMovedInSameDir) {
			m_TwoTouchMode = TwoTouchMode.Roll;
			m_TouchData[0].ResetTotal();
			m_TouchData[1].ResetTotal();
		} else if ((m_TwoTouchMode != TwoTouchMode.Pitch) && ysMovedEnough && ysMovedInSameDir) {
			m_TwoTouchMode = TwoTouchMode.Pitch;
			m_TouchData[0].ResetTotal();
			m_TouchData[1].ResetTotal();
		} else if ((m_TwoTouchMode != TwoTouchMode.Default) &&
			((!x0MovedEnough && x1MovedEnough) ||
			 (!x1MovedEnough && x0MovedEnough) ||
			 (!y0MovedEnough && y1MovedEnough) ||
			 (!y1MovedEnough && y0MovedEnough) ||
			 (!xsMovedInSameDir && (Mathf.Abs(x0) + Mathf.Abs(x1) > MOVE_EPSILON)) ||
			 (!ysMovedInSameDir && (Mathf.Abs(y0) + Mathf.Abs(y1) > MOVE_EPSILON)))) {
			m_TwoTouchMode = TwoTouchMode.Default;
			m_TouchData[0].ResetTotal();
			m_TouchData[1].ResetTotal();
		}


		// Respond differently based on the current mode:

		if (m_TwoTouchMode == TwoTouchMode.Default) {
			// Two Touch Default Mode (typical multi-touch rotate, pinch, & zoom)

			// t1 is id of the touch that has moved since the last call
			int t1 = cursorID;
			// t0 is the other touch that has not moved
			int t0 = t1 == 0 ? 1 : 0;

			Vector3 t1LocalOrig = TouchToLocal(m_TouchData[t1].lastPos, m_Object);
			Vector3 t1LocalDesired = TouchToLocal(m_TouchData[t1].pos, m_Object);

			// orig and desired are the same for t0 since it has not moved this frame
			Vector3 t0Local = TouchToLocal(m_TouchData[t0].pos, m_Object);

			// strategy: since t0 hasn't moved (this frame), it can act as a pivot and we can rotate and scale
			// around t0 to get t1 to the correct place.

			// we'll actually place the pivot point at the same height as the object, so there is
			// no undesired vertical movement when scaling
			Vector3 pivotWorld = TouchToWorld(m_TouchData[t0].pos);
			pivotWorld.y = m_Object.position.y;
			Vector3 pivotLocal = m_Object.InverseTransformPoint(pivotWorld);

			// axis spanning the two touch points and pointing from t0 to t1
			Vector3 origAxisLocal = t1LocalOrig - t0Local;
			Vector3 desiredAxisLocal = t1LocalDesired - t0Local;

			Quaternion deltaRot = Quaternion.FromToRotation(origAxisLocal, desiredAxisLocal);
			m_Object.RotateAroundLocalPoint(pivotLocal, deltaRot);

			float deltaScale = desiredAxisLocal.magnitude / origAxisLocal.magnitude;
			m_Object.ScaleAroundLocalPoint(pivotLocal, deltaScale);

		} else {
			// Coordinated horizontal or forward/backward movement of the fingers rolls or pitches the model

			Vector3 rotAxis;
			int rotDirection;
			float minTrans;
			if (m_TwoTouchMode == TwoTouchMode.Roll) {
				// Two Touch Roll Mode (from coordinated horizontal movement of the fingers)
				rotAxis = Vector3.forward;
				rotDirection = x0 > 0.0f ? -1 : 1;
				minTrans = Mathf.Min(Mathf.Abs(x0), Mathf.Abs(x1));
			} else {
				// Two Touch Pitch Mode (from coordinated forward/backward movement of the fingers)
				rotAxis = Vector3.right;
				rotDirection = y0 > 0.0f ? 1 : -1;
				minTrans = Mathf.Min(Mathf.Abs(y0), Mathf.Abs(y1));
			}

			if (Mathf.Abs(minTrans) > ROTATION_EPSILON) {
				m_TouchData[0].ResetTotal();
				m_TouchData[1].ResetTotal();
				Vector3 axis = m_Object.InverseTransformVector(rotAxis);
				float angle = Mathf.Rad2Deg * ROTATION_ANGLE_SCALE * minTrans * rotDirection;
				m_Object.RotateAroundLocalOrigin(Quaternion.AngleAxis(angle, axis));
			}
		}
	}


	public void ThirdTouchMove(Vector2 pos)
	{
		Vector3 touchWorldLast = m_TouchWorld[2];
		Vector3 touchWorld = TouchToWorld(pos);

		// update based on latest input
		m_TouchData[2].Update(pos);
		m_TouchWorld[2] = touchWorld;

		Vector3 touch01Axis = Vector3.Normalize(m_TouchWorld[1] - m_TouchWorld[0]);
		Vector3 movement = touchWorld - touchWorldLast;
		Vector3 movementPerpToAxis = movement - Vector3.Dot(touch01Axis, movement) * touch01Axis;
		float dist = VERTICAL_TRANSLATION_SCALE * movementPerpToAxis.magnitude;
		if (Vector3.Dot(Vector3.Cross(movementPerpToAxis, touch01Axis), Vector3.up) < 0) {
			dist = -dist;
		}
		m_Object.TranslateByWorldVector(dist * Vector3.up);
	}


	private void OnDrawGizmos()
    {
		if (m_TouchData != null) {
			if (m_TouchData[0].active) {
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(m_TouchWorld[0], 0.15f);
			}
			if (m_TouchData[1].active) {
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(m_TouchWorld[1], 0.15f);
			}
			if (m_TouchData[2].active) {
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(m_TouchWorld[2], 0.15f);
			}
		}
	}


	[Tooltip("Object to manipulate")]
	[SerializeField] private Transform m_Object;

	[Tooltip("Orthographic camera that generates the virtual view of the table")]
	[SerializeField] private Camera m_ShadowCamera;

	[SerializeField] private float MOVE_EPSILON = 0.025f;
	[SerializeField] private float ROTATION_EPSILON = 0.010f;
	[SerializeField] private float ROTATION_ANGLE_SCALE = 5.0f;
	[SerializeField] private float VERTICAL_TRANSLATION_SCALE = 1.5f;


	[SerializeField] private VREventPrototypeVector2 m_Finger0DownEvent;
	[SerializeField] private VREventPrototypeVector2 m_Finger0MoveEvent;
	[SerializeField] private VREventPrototypeVector2 m_Finger0UpEvent;

	[SerializeField] private VREventPrototypeVector2 m_Finger1DownEvent;
	[SerializeField] private VREventPrototypeVector2 m_Finger1MoveEvent;
	[SerializeField] private VREventPrototypeVector2 m_Finger1UpEvent;

	[SerializeField] private VREventPrototypeVector2 m_Finger2DownEvent;
	[SerializeField] private VREventPrototypeVector2 m_Finger2MoveEvent;
	[SerializeField] private VREventPrototypeVector2 m_Finger2UpEvent;

	// runtime only
	private FSM m_FSM;
	private TouchData[] m_TouchData;
	private Plane m_TouchPlane;
	private Vector3[] m_TouchWorld;

	enum TwoTouchMode
	{
		Default,
		Roll,
		Pitch
	}
	private TwoTouchMode m_TwoTouchMode;
}

