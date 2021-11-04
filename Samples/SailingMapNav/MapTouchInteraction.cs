using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using IVLab.MinVR3;

public class MapTouchInteraction : MonoBehaviour
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
		m_Finger0UpEvent   = VREventPrototypeVector2.Create("Touch/Finger 0 UP");

		m_Finger1DownEvent = VREventPrototypeVector2.Create("Touch/Finger 1 DOWN");
		m_Finger1MoveEvent = VREventPrototypeVector2.Create("Touch/Finger 1/Position");
		m_Finger1UpEvent   = VREventPrototypeVector2.Create("Touch/Finger 1 UP");
	}

	public void Start()
	{
		m_TouchPlane = new Plane(-m_MapCamera.transform.forward, Vector3.zero);
		m_TouchData = new TouchData[2];
		m_TouchWorld = new Vector3[2];
		for (int i = 0; i < 2; i++) {
			m_TouchData[i] = new TouchData();
			m_TouchWorld[i] = new Vector3();
		}

		m_FSM = this.gameObject.AddComponent<FSM>();
		m_FSM.AddState("START");
		m_FSM.AddState("Touch0");
		m_FSM.AddState("Touch1");
		m_FSM.AddState("TouchBoth");

		m_FSM.AddArc("START",     "Touch0", VREventCallbackAny.CreateRuntime(m_Finger0DownEvent, pos => OnTouchDown(0, pos)));
		m_FSM.AddArc("Touch0",    "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger1DownEvent, pos => OnTouchDown(1, pos)));
		m_FSM.AddArc("TouchBoth", "Touch0", VREventCallbackAny.CreateRuntime(m_Finger1UpEvent, pos => OnTouchUp(1, pos)));
		m_FSM.AddArc("Touch0",    "START", VREventCallbackAny.CreateRuntime(m_Finger0UpEvent, pos => OnTouchUp(0, pos)));

		m_FSM.AddArc("START",     "Touch1", VREventCallbackAny.CreateRuntime(m_Finger1DownEvent, pos => OnTouchDown(1, pos)));
		m_FSM.AddArc("Touch1",    "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger0DownEvent, pos => OnTouchDown(0, pos)));
		m_FSM.AddArc("TouchBoth", "Touch1", VREventCallbackAny.CreateRuntime(m_Finger0UpEvent, pos => OnTouchUp(0, pos)));
		m_FSM.AddArc("Touch1",    "START", VREventCallbackAny.CreateRuntime(m_Finger1UpEvent, pos => OnTouchUp(1, pos)));

		m_FSM.AddArc("Touch0",    "Touch0", VREventCallbackAny.CreateRuntime(m_Finger0MoveEvent, pos => Translate(0, pos)));
		m_FSM.AddArc("Touch1",    "Touch1", VREventCallbackAny.CreateRuntime(m_Finger1MoveEvent, pos => Translate(1, pos)));
		m_FSM.AddArc("TouchBoth", "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger0MoveEvent, pos => TransRotScale(0, pos)));
		m_FSM.AddArc("TouchBoth", "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger1MoveEvent, pos => TransRotScale(1, pos)));

		Debug.Assert(m_MapCamera.orthographic, "The script assumes the map is rendered by an orthographic camera. " +
			"It might be possible to make it work with perspective if desired; m_TouchPlane would need to be adjusted " +
			"in that case to lie within the plane of the average height for the terrain or something like this.");
	}

	public Vector3 TouchToWorld(Vector2 touchInViewportCoords)
	{
		Ray ray = m_MapCamera.ViewportPointToRay(touchInViewportCoords);
		float dist;
		if (m_TouchPlane.Raycast(ray, out dist)) {
			return ray.origin + dist * ray.direction;
		} else {
			return Vector3.zero;
		}
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


	public void Translate(int cursorID, Vector2 pos)
	{
		m_TouchData[cursorID].Update(pos);
		m_TouchWorld[cursorID] = TouchToWorld(pos);

		Vector3 cur = m_TouchWorld[cursorID];
		Vector3 last = TouchToWorld(m_TouchData[cursorID].lastPos);
		Vector3 deltaWorld = cur - last;

		// transform the map camera
		m_MapCamera.transform.position -= deltaWorld;

		// recenter room space
		Vector3 pMapCam = m_MapCamera.transform.position;
		Vector3 pRoomSpace = m_RoomSpaceRoot.position;
		m_RoomSpaceRoot.position = new Vector3(pMapCam.x, pRoomSpace.y, pMapCam.z);
	}

	public void TransRotScale(int cursorID, Vector2 pos)
	{
		m_TouchData[cursorID].Update(pos);
		m_TouchWorld[cursorID] = TouchToWorld(pos);

		// t1 is id of the touch that has moved since the last call
		int t1 = cursorID;
		// t0 is the other touch that has not moved
		int t0 = t1 == 0 ? 1 : 0;

		Vector3 t1MapOrig = TouchToWorld(m_TouchData[t1].lastPos);
		Vector3 t1MapDesired = m_TouchWorld[t1];

		Vector3 t0Map = m_TouchWorld[t0];

		// strategy: since t0 hasn't moved (this frame), it can act as a pivot and we can rotate and scale
		// around t0 to get t1 to the correct place.

		// we'll actually place the pivot point at the same height as the object, so there is
		// no undesired vertical movement when scaling
		Vector3 pivotWorld = t0Map;

		// axis spanning the two touch points and pointing from t0 to t1
		Vector3 origAxis = t1MapOrig - t0Map;
		Vector3 desiredAxis = t1MapDesired - t0Map;

		Quaternion deltaRot = Quaternion.FromToRotation(origAxis, desiredAxis);
		m_MapCamera.transform.RotateAroundWorldPoint(pivotWorld, Quaternion.Inverse(deltaRot));

		float deltaScale = desiredAxis.magnitude / origAxis.magnitude;
		m_MapCamera.orthographicSize /= deltaScale;


		/*
		Vector3 hitPt0World = new Vector3();
		ViewportTo3DTouchPlane(m_TouchData[0].pos, ref hitPt0World);
		Vector3 lastHitPt0World = new Vector3();
		ViewportTo3DTouchPlane(m_TouchData[0].lastPos, ref lastHitPt0World);

		Vector3 hitPt1World = new Vector3();
		ViewportTo3DTouchPlane(m_TouchData[1].pos, ref hitPt1World);
		Vector3 lastHitPt1World = new Vector3();
		ViewportTo3DTouchPlane(m_TouchData[1].lastPos, ref lastHitPt1World);

		if ((lastHitPt0World == hitPt0World) && (lastHitPt1World == hitPt1World)) {
			// no measureable movement
			return;
		}

		// change in translation of touch 0
		Vector3 deltaTrans = hitPt0World - lastHitPt0World;

		// change in rotation relative to touch 0
		Vector3 up = m_TouchPlane.normal;
		Quaternion srcRot = Quaternion.LookRotation(lastHitPt1World - lastHitPt0World, up);
		Quaternion dstRot = Quaternion.LookRotation(hitPt1World - hitPt0World, up);
		Quaternion deltaRot = dstRot * Quaternion.Inverse(srcRot);

		// change in scale
		float deltaScale = (hitPt1World - hitPt0World).magnitude / (lastHitPt1World - lastHitPt0World).magnitude;

		// transform the map camera based on these
		m_MapCamera.transform.position  -= deltaTrans;
		m_MapCamera.transform.rotation = Quaternion.Inverse(deltaRot) * m_MapCamera.transform.rotation;
		m_MapCamera.orthographicSize /= deltaScale;
		*/

		// recenter room space
		Vector3 pMapCam = m_MapCamera.transform.position;
		Vector3 pRoomSpace = m_RoomSpaceRoot.position;
		m_RoomSpaceRoot.position = new Vector3(pMapCam.x, pRoomSpace.y, pMapCam.z);

		// realign room space
		m_RoomSpaceRoot.forward = m_MapCamera.transform.up;
	}


	[Tooltip("In MinVR terminology, this is the root transform for 'Room Space'.  As we navigate around the map, the" +
		"physical room and all virtual objects within it (e.g., trackers, the canoe we are in, the magic carpet we " +
		"are on) come with it.  This should point to the root Transform for all those objects.")]
	[SerializeField] private Transform m_RoomSpaceRoot;

	[Tooltip("Orthographic Camera that renders a map view by looking down on the scene from above. The camera will be " +
		"translated, rotated and scaled (orthographically) 'inversely' based on the touch input, so that the user " +
		"feels as though they are manipulating the map, rather than the camera viewing the map.")]
	[SerializeField] private Camera m_MapCamera;

	[SerializeField] private VREventPrototypeVector2 m_Finger0DownEvent;
	[SerializeField] private VREventPrototypeVector2 m_Finger0MoveEvent;
	[SerializeField] private VREventPrototypeVector2 m_Finger0UpEvent;

	[SerializeField] private VREventPrototypeVector2 m_Finger1DownEvent;
	[SerializeField] private VREventPrototypeVector2 m_Finger1MoveEvent;
	[SerializeField] private VREventPrototypeVector2 m_Finger1UpEvent;


	// runtime only
	private FSM m_FSM;
	Plane m_TouchPlane;
	TouchData[] m_TouchData;
	Vector3[] m_TouchWorld;
}

