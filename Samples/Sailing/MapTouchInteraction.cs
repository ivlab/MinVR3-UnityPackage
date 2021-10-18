using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using IVLab.MinVR3;

public class MapTouchInteraction : MonoBehaviour
{
    public void Reset()
    {
		m_Finger0DownEvent = new VREventPrototype<Vector2>("Mobile/Finger0 DOWN");
		m_Finger0MoveEvent = new VREventPrototype<Vector2>("Mobile/Finger0/Position");
		m_Finger0UpEvent   = new VREventPrototype<Vector2>("Mobile/Finger0 UP");

		m_Finger0DownEvent = new VREventPrototype<Vector2>("Mobile/Finger1 DOWN");
		m_Finger0MoveEvent = new VREventPrototype<Vector2>("Mobile/Finger1/Position");
		m_Finger0UpEvent   = new VREventPrototype<Vector2>("Mobile/Finger1 UP");
	}

	public void Start()
	{
		m_FSM = new FSM();
		m_FSM.AddState("START");
		m_FSM.AddState("Touch0");
		m_FSM.AddState("Touch1");
		m_FSM.AddState("TouchBoth");

		m_FSM.AddArc("START",     "Touch0",    m_Finger0DownEvent, InitTouch0);
		m_FSM.AddArc("Touch0",    "TouchBoth", m_Finger1DownEvent, InitTouch1);
		m_FSM.AddArc("TouchBoth", "Touch0",    m_Finger1UpEvent);
		m_FSM.AddArc("Touch0",    "START",     m_Finger0UpEvent);

		m_FSM.AddArc("START",     "Touch1",    m_Finger1DownEvent, InitTouch1);
		m_FSM.AddArc("Touch1",    "TouchBoth", m_Finger0DownEvent, InitTouch0);
		m_FSM.AddArc("TouchBoth", "Touch1",    m_Finger0UpEvent);
		m_FSM.AddArc("Touch1",    "START",     m_Finger1UpEvent);

		m_FSM.AddArc("Touch0",    "Touch0",    m_Finger0MoveEvent, Translate0);
		m_FSM.AddArc("Touch1",    "Touch1",    m_Finger1MoveEvent, Translate1);
		m_FSM.AddArc("TouchBoth", "TouchBoth", m_Finger0MoveEvent, TransRotScale0);
		m_FSM.AddArc("TouchBoth", "TouchBoth", m_Finger1MoveEvent, TransRotScale1);
	}


	public void InitTouch0(Vector2 pos)
	{
		m_LastPos0 = pos;	
	}

	public void InitTouch1(Vector2 pos)
	{
		m_LastPos1 = pos;
	}

	public void Translate0(Vector2 pos)
	{
		Translate(pos - m_LastPos0);
		m_LastPos0 = pos;
	}

	public void Translate1(Vector2 pos)
	{
		Translate(pos - m_LastPos1);
		m_LastPos1 = pos;
	}

	public void TransRotScale0(Vector2 pos)
	{
		TransRotScale(pos, m_LastPos1);
		m_LastPos0 = pos;
	}

	public void TransRotScale1(Vector2 pos)
	{
		TransRotScale(m_LastPos0, pos);
		m_LastPos1 = pos;
	}


	public void Translate(Vector2 delta)
	{

	}

	public void TransRotScale(Vector2 pos0, Vector2 pos1)
	{

	}

	// can be set / saved in the editor
	[SerializeField] private VREventPrototype<Vector2> m_Finger0DownEvent;
	[SerializeField] private VREventPrototype<Vector2> m_Finger0MoveEvent;
	[SerializeField] private VREventPrototype<Vector2> m_Finger0UpEvent;

	[SerializeField] private VREventPrototype<Vector2> m_Finger1DownEvent;
	[SerializeField] private VREventPrototype<Vector2> m_Finger1MoveEvent;
	[SerializeField] private VREventPrototype<Vector2> m_Finger1UpEvent;

	// runtime only
	private FSM m_FSM;
	private Vector2 m_LastPos0;
	private Vector2 m_LastPos1;
}



/*
/// <summary>
/// Originally created by Jung Nam for the iPad forestry project in 2015ish
/// https://github.umn.edu/ivlab-cs/forestry/blob/master/Assets/Scripts/Navigation/TouchNavigator.cs
///
/// Modified by dfk to work with MinVR and serve as a demo of Clipboard VR, summer 2021
/// </summary>
public class MapTouchInteraction : MonoBehaviour
{
	public class TouchInfo
	{
		// http://answers.unity3d.com/questions/204490/android-delta-touch.html
		// Unity3D has touch.deltaPosition but it does not work well in some Android devices.
		// So, this class is used to store touch points and their previous position
		public int id;
		public Vector2 pos;
		public Vector2 lastPos;

		public TouchInfo(int id, Vector2 pos, Vector2 lastPos)
		{
			this.id = id;
			this.pos = pos;
			this.lastPos = lastPos;
		}
	}

	// the axes the touches (2D positions) reside in a world space 
	public enum TouchWorldAxes
	{
		XY,
		XZ
	};

	public int maxNumFingers = 2;
	public Camera orthoCamera;

	public bool isCameraMoving; // true, if the camera is the one moving; false, if the camera is static (a fixed transform). 
	public TouchWorldAxes axes;

	private List<TouchInfo> touchInfos = new List<TouchInfo>();
	private Vector3 initPos;
	private float initOrthographicSize;

	void Start()
	{
		initPos = transform.position;
		// zoom-in or out via changing the size of camera, if the camera is moving
		initOrthographicSize = orthoCamera.orthographicSize;
	}

	public void Finger0Down(Vector2 pos)
	{
		FingerDown(0, pos);
	}

	public void Finger0Move(Vector2 pos)
	{
		FingerMove(0, pos);
	}

	public void Finger0Up()
	{
		FingerUp(0);
	}

	public void Finger1Down(Vector2 pos)
	{
		FingerDown(1, pos);
	}

	public void Finger1Move(Vector2 pos)
	{
		FingerMove(1, pos);
	}

	public void Finger1Up()
	{
		FingerUp(1);
	}



	private void FingerDown(int id, Vector2 newPos)
	{
		TouchInfo info = new TouchInfo(id, newPos, newPos);
		//
		if (touchInfos.Count < maxNumFingers)
		{
			touchInfos.Add(info);
		}
	}

	private void FingerUp(int id)
	{
		int idx = GetIndexOfTouchInfo(true, id);
		//
		if (idx != -1)
		{
			touchInfos.RemoveAt(idx);
		}
	}

	private void FingerMove(int id, Vector2 newPos)
	{
		// find the touch info that corresponds to the touch
		int index = GetIndexOfTouchInfo(true, id);
		//
		if (index != -1)
		{
			// update the current touch info
			touchInfos[index].lastPos = touchInfos[index].pos;
			touchInfos[index].pos = newPos;

			// find the touch info that corresponds to a next touch, if there is one
			int index2 = GetIndexOfTouchInfo(false, id);

			// Now setup the transformation for the active window
			Vector3 src1, src2, src3, dst1, dst2, dst3;

			{ // compute first points
			  // transform the touch inputs to world positions
			  // if the camera is moving (instead of an object), it should have a reverse effect
				Vector3 currPt, prevPt;
				if (isCameraMoving)
				{
					currPt = orthoCamera.ScreenToWorldPoint(touchInfos[index].lastPos);
					prevPt = orthoCamera.ScreenToWorldPoint(touchInfos[index].pos);
				}
				else
				{
					currPt = orthoCamera.ScreenToWorldPoint(touchInfos[index].pos);
					prevPt = orthoCamera.ScreenToWorldPoint(touchInfos[index].lastPos);
				}
				if (axes.Equals(TouchWorldAxes.XZ))
				{
					currPt = new Vector3(currPt.x, initPos.y, currPt.z);
					prevPt = new Vector3(prevPt.x, initPos.y, prevPt.z);
				}
				else
				{
					// XY
					currPt = new Vector3(currPt.x, currPt.y, initPos.z);
					prevPt = new Vector3(prevPt.x, prevPt.y, initPos.z);
				}
				//
				dst1 = currPt;
				src1 = prevPt;
			}

			{ // compute second points
			  // if second touch, use position, otherwise use a dummy point
				if (index2 != -1)
				{
					Vector3 currPt2;
					if (isCameraMoving)
					{
						currPt2 = orthoCamera.ScreenToWorldPoint(touchInfos[index2].lastPos);
						// somehow does not use the current position (see TUIO example code)
					}
					else
					{
						currPt2 = orthoCamera.ScreenToWorldPoint(touchInfos[index2].pos);
						// somehow does not use the last position (see TUIO example code)
					}
					if (axes.Equals(TouchWorldAxes.XZ))
					{
						currPt2 = new Vector3(currPt2.x, initPos.y, currPt2.z);
					}
					else
					{
						// XY
						currPt2 = new Vector3(currPt2.x, currPt2.y, initPos.z);
					}
					//
					dst2 = currPt2;
					src2 = currPt2;
				}
				else
				{
					if (axes.Equals(TouchWorldAxes.XZ))
					{
						dst2 = dst1 + new Vector3(1f, 0f, 0f);
						src2 = src1 + new Vector3(1f, 0f, 0f);
					}
					else
					{
						// XY
						dst2 = dst1 + new Vector3(1f, 0f, 0f);
						src2 = src1 + new Vector3(1f, 0f, 0f);
					}
				}
			}

			{ // compute third points
			  // third point is a dummy as well
				if (axes.Equals(TouchWorldAxes.XZ))
				{
					// Unity uses a left hand coordinate system
					dst3 = dst1 + new Vector3(0f, -1f, 0f);
					src3 = src1 + new Vector3(0f, -1f, 0f);
				}
				else
				{
					// XY
					dst3 = dst1 + new Vector3(0f, 0f, 1f);
					src3 = src1 + new Vector3(0f, 0f, 1f);
				}
			}

			// compute the matrix that moves prev frame to current one
			Matrix4x4 m = this.AlignXform(src1, src2, src3, dst1, dst2, dst3) *
				gameObject.transform.localToWorldMatrix;

			// Unity does not allow set the pos, rot, scale via matrix
			//transform.localScale = new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
			transform.rotation = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
			transform.position = m.GetColumn(3);

			if (isCameraMoving)
			{
				orthoCamera.transform.position = transform.position;
				orthoCamera.orthographicSize = initOrthographicSize * transform.localScale.x;
			}
		}
	}

	// returns the index from the TouchInfo list that DOES or DOES NOT corresponds with the input id
	private int GetIndexOfTouchInfo(bool equals, int id)
	{
		for (int i = 0; i < touchInfos.Count; i++)
		{
			if (equals && touchInfos[i].id == id)
			{
				return i;
			}
			else if (!equals && touchInfos[i].id != id)
			{
				return i;
			}
		}
		return -1;
	}

	private Matrix4x4 AlignXform(Vector3 src1, Vector3 src2, Vector3 src3,
								  Vector3 dst1, Vector3 dst2, Vector3 dst3)
	{
		Matrix4x4 srcCF = this.CreateCoordinateFrameMatrix(src1, src2, src3);
		Matrix4x4 dstCF = this.CreateCoordinateFrameMatrix(dst1, dst2, dst3);

		float scale = (dst2 - dst1).magnitude / (src2 - src1).magnitude;
		Matrix4x4 scaleM = Matrix4x4.Scale(new Vector3(scale, scale, scale));

		Matrix4x4 transDst1M = Matrix4x4.identity;
		transDst1M.SetColumn(3, new Vector4(dst1.x, dst1.y, dst1.z, 1));

		Matrix4x4 transMinusDst1M = Matrix4x4.identity;
		transMinusDst1M.SetColumn(3, new Vector4(-dst1.x, -dst1.y, -dst1.z, 1));

		return transDst1M * scaleM * transMinusDst1M * dstCF * srcCF.inverse;
	}

	// Create a coordinate frame from three points (pt1 is the origin)
	private Matrix4x4 CreateCoordinateFrameMatrix(Vector3 pt1, Vector3 pt2, Vector3 pt3)
	{
		// compute axes
		Vector3 srcX = (pt2 - pt1);
		srcX.Normalize();
		Vector3 srcY = (pt3 - pt1);
		srcY.Normalize();
		Vector3 srcZ = Vector3.Cross(srcX, srcY);
		srcZ.Normalize();
		srcY = Vector3.Cross(srcZ, srcX);
		srcY.Normalize();

		// represent them in a matrix form
		Matrix4x4 cf = new Matrix4x4();
		cf.SetColumn(0, new Vector4(srcX.x, srcX.y, srcX.z, 0));
		cf.SetColumn(1, new Vector4(srcY.x, srcY.y, srcY.z, 0));
		cf.SetColumn(2, new Vector4(srcZ.x, srcZ.y, srcZ.z, 0));
		cf.SetColumn(3, new Vector4(pt1.x, pt1.y, pt1.z, 1));

		return cf;
	}
}
*/