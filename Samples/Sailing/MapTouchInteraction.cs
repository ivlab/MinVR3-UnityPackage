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
		}
		public void Init(Vector2 curPos)
		{
			// make current and last positions equal so delta is initially zero
			lastPos = curPos;
			pos = curPos;
		}
		public void Update(Vector2 curPos)
		{
			lastPos = pos;
			pos = curPos;
		}
		public Vector2 pos;
		public Vector2 lastPos;
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
		m_TouchData[0] = new TouchData();
		m_TouchData[1] = new TouchData();

		m_FSM = this.gameObject.AddComponent<FSM>();
		m_FSM.AddState("START");
		m_FSM.AddState("Touch0");
		m_FSM.AddState("Touch1");
		m_FSM.AddState("TouchBoth");

		m_FSM.AddArc("START",     "Touch0", VREventCallbackAny.CreateRuntime(m_Finger0DownEvent, InitTouch0));
		m_FSM.AddArc("Touch0",    "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger1DownEvent, InitTouch1));
		m_FSM.AddArc("TouchBoth", "Touch0", VREventCallbackAny.CreateRuntime(m_Finger1UpEvent));
		m_FSM.AddArc("Touch0",    "START", VREventCallbackAny.CreateRuntime(m_Finger0UpEvent));

		m_FSM.AddArc("START",     "Touch1", VREventCallbackAny.CreateRuntime(m_Finger1DownEvent, InitTouch1));
		m_FSM.AddArc("Touch1",    "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger0DownEvent, InitTouch0));
		m_FSM.AddArc("TouchBoth", "Touch1", VREventCallbackAny.CreateRuntime(m_Finger0UpEvent));
		m_FSM.AddArc("Touch1",    "START", VREventCallbackAny.CreateRuntime(m_Finger1UpEvent));

		m_FSM.AddArc("Touch0",    "Touch0", VREventCallbackAny.CreateRuntime(m_Finger0MoveEvent, Translate0));
		m_FSM.AddArc("Touch1",    "Touch1", VREventCallbackAny.CreateRuntime(m_Finger1MoveEvent, Translate1));
		m_FSM.AddArc("TouchBoth", "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger0MoveEvent, TransRotScale0));
		m_FSM.AddArc("TouchBoth", "TouchBoth", VREventCallbackAny.CreateRuntime(m_Finger1MoveEvent, TransRotScale1));

		Debug.Assert(m_MapCamera.orthographic, "The script assumes the map is rendered by an orthographic camera. " +
			"It might be possible to make it work with perspective if desired; m_TouchPlane would need to be adjusted " +
			"in that case to lie within the plane of the average height for the terrain or something like this.");
	}


	public void InitTouch0(Vector2 pos)
	{
		m_TouchData[0].Init(pos);
	}

	public void InitTouch1(Vector2 pos)
	{
		m_TouchData[1].Init(pos);
	}

	public void Translate0(Vector2 pos)
	{
		m_TouchData[0].Update(pos);
		DoTranslate(0);
	}

	public void Translate1(Vector2 pos)
	{
		m_TouchData[0].Update(pos);
		DoTranslate(1);
	}

	public void TransRotScale0(Vector2 pos)
	{
		m_TouchData[0].Update(pos);
		DoTransRotScale();
	}

	public void TransRotScale1(Vector2 pos)
	{
		m_TouchData[1].Update(pos);
		DoTransRotScale();
	}


	public void DoTranslate(int cursorID)
	{
		Vector3 hitPtWorld = new Vector3();
		ViewportTo3DTouchPlane(m_TouchData[cursorID].pos, ref hitPtWorld);
		Vector3 lastHitPtWorld = new Vector3();
		ViewportTo3DTouchPlane(m_TouchData[cursorID].lastPos, ref lastHitPtWorld);
		Vector3 deltaWorld = hitPtWorld - lastHitPtWorld;

		// transform the map camera
		m_MapCamera.transform.position -= deltaWorld;

		// recenter room space
		Vector3 pMapCam = m_MapCamera.transform.position;
		Vector3 pRoomSpace = m_RoomSpaceRoot.position;
		m_RoomSpaceRoot.position = new Vector3(pMapCam.x, pRoomSpace.y, pMapCam.z);
	}

	public void DoTransRotScale()
	{
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


		// recenter room space
		Vector3 pMapCam = m_MapCamera.transform.position;
		Vector3 pRoomSpace = m_RoomSpaceRoot.position;
		m_RoomSpaceRoot.position = new Vector3(pMapCam.x, pRoomSpace.y, pMapCam.z);

		// realign room space
		m_RoomSpaceRoot.forward = m_MapCamera.transform.up;
	}

	bool ViewportTo3DTouchPlane(Vector2 viewportPoint, ref Vector3 touchPlanePoint)
	{
		Ray ray = m_MapCamera.ViewportPointToRay(viewportPoint);
		float dist;
		if (m_TouchPlane.Raycast(ray, out dist)) {
			touchPlanePoint = ray.origin + dist * ray.direction;
			return true;
		} else {
			return false;
		}
	}


	[Tooltip("In MinVR terminology, this is the root transform for 'Room Space'.  As we navigate around the map, the" +
		"physical room and all virtual objects within it (e.g., trackers, the canoe we are in, the magic carpet we " +
		"are on) come with it.  This should point to the root Transform for all those objects.")]
	[SerializeField] private Transform m_RoomSpaceRoot;

	[Tooltip("Orthographic Camera that renders a map view by looking down on the scene from above. The camera will be " +
		"translated, rotated and scaled (orthographically) 'inversely' based on the touch input, so that the user " +
		"feels as though they are manipulating the map, rather than the camera viewing the map.")]
	[SerializeField] private Camera m_MapCamera;

	[SerializeField] private VREventPrototype<Vector2> m_Finger0DownEvent;
	[SerializeField] private VREventPrototype<Vector2> m_Finger0MoveEvent;
	[SerializeField] private VREventPrototype<Vector2> m_Finger0UpEvent;

	[SerializeField] private VREventPrototype<Vector2> m_Finger1DownEvent;
	[SerializeField] private VREventPrototype<Vector2> m_Finger1MoveEvent;
	[SerializeField] private VREventPrototype<Vector2> m_Finger1UpEvent;


	// runtime only
	private FSM m_FSM;
	Plane m_TouchPlane;
	TouchData[] m_TouchData;
}

