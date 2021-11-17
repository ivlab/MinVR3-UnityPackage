using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using IVLab.MinVR3;

public class TouchManip : MonoBehaviour
{

#if UNITY_EDITOR
    public void Reset()
    {
        m_Touch0Down = VREventCallbackVector2.CreateInEditor("Touch/Finger 0 DOWN", OnTouch0Down);
        m_Touch0Move = VREventCallbackVector2.CreateInEditor("Touch/Finger 0/Position", OnTouch0Move);
        m_Touch0Up = VREventCallbackVector2.CreateInEditor("Touch/Finger 0 UP", OnTouch0Up);

        m_Touch1Down = VREventCallbackVector2.CreateInEditor("Touch/Finger 1 DOWN", OnTouch1Down);
        m_Touch1Move = VREventCallbackVector2.CreateInEditor("Touch/Finger 1/Position", OnTouch1Move);
        m_Touch1Up = VREventCallbackVector2.CreateInEditor("Touch/Finger 1 UP", OnTouch1Up);
    }
#endif


    void OnTouch0Down(Vector2 pos)
    {
        RaycastHit hit;
        Ray ray = m_Camera.ViewportPointToRay(pos);
        if (Physics.Raycast(ray, out hit)) {
            m_TouchObjTransform = hit.transform;
            m_TouchPlane = new Plane(-m_Camera.transform.forward, hit.point);
            m_Last0HitPt = hit.point;
        } else {
            m_TouchObjTransform = null;
        }
    }

    void OnTouch0Move(Vector2 pos)
    {
        if (m_TouchObjTransform != null) {
            Ray ray = m_Camera.ViewportPointToRay(pos);
            float dist;
            if (m_TouchPlane.Raycast(ray, out dist)) {
                Vector3 hitPt = ray.origin + dist * ray.direction;
                Vector3 delta3D = hitPt - m_Last0HitPt;
                m_TouchObjTransform.position += delta3D;
                m_Last0HitPt = hitPt;
            }
        }
    }

    void OnTouch0Up(Vector2 pos)
    {
        m_TouchObjTransform = null;
    }


    void OnTouch1Down(Vector2 pos)
    {
        if (m_TouchObjTransform != null) {
            Ray ray = m_Camera.ViewportPointToRay(pos);
            float dist;
            if (m_TouchPlane.Raycast(ray, out dist)) {
                Vector3 hitPt = ray.origin + dist * ray.direction;
                m_Last1HitPt = hitPt;
            }
        }
    }

    void OnTouch1Move(Vector2 pos)
    {
        if (m_TouchObjTransform != null) {
            Ray ray = m_Camera.ViewportPointToRay(pos);
            float dist;
            if (m_TouchPlane.Raycast(ray, out dist)) {
                Vector3 hitPt = ray.origin + dist * ray.direction;

                Vector3 src1 = m_Last0HitPt;
                Vector3 dst1 = m_Last0HitPt;

                Vector3 src2 = m_Last1HitPt;
                Vector3 dst2 = hitPt;

                // dummy point
                Vector3 offset = Vector3.Cross(dst2 - dst1, dst2 - src2);
                Vector3 src3 = src1 + offset;
                Vector3 dst3 = dst1 + offset;

                // compute the matrix that moves prev frame to current one
                Matrix4x4 M = AlignAndScale(src1, src2, src3, dst1, dst2, dst3, false);
                Matrix4x4 M2 = M * m_TouchObjTransform.localToWorldMatrix;

                float scale = (dst2 - dst1).magnitude / (src2 - src1).magnitude;

                m_TouchObjTransform.position = M2.GetColumn(3);
                m_TouchObjTransform.rotation = Quaternion.LookRotation(M2.GetColumn(2), M2.GetColumn(1));
                m_TouchObjTransform.localScale *= scale;

                m_Last1HitPt = hitPt;
            }
        }
    }

    void OnTouch1Up(Vector2 pos)
    {

    }


    
    void OnEnable()
    {
        m_Touch0Down.StartListening();
        m_Touch0Move.StartListening();
        m_Touch0Up.StartListening();

        m_Touch1Down.StartListening();
        m_Touch1Move.StartListening();
        m_Touch1Up.StartListening();
    }

    void OnDisable()
    {
        m_Touch0Down?.StopListening();
        m_Touch0Move?.StopListening();
        m_Touch0Up?.StopListening();

        m_Touch1Down?.StopListening();
        m_Touch1Move?.StopListening();
        m_Touch1Up?.StopListening();
    }

    // Update is called once per frame
    void Update()
    {
    }


    Matrix4x4 AlignAndScale(Vector3 src1, Vector3 src2, Vector3 src3,
                            Vector3 dst1, Vector3 dst2, Vector3 dst3,
                            bool includeScale = true)
    {
        Vector3 srcX = Vector3.Normalize(src2 - src1);
        Vector3 srcY = Vector3.Normalize(src3 - src1);
        Vector3 srcZ = Vector3.Normalize(Vector3.Cross(srcX, srcY));
        srcY = Vector3.Normalize(Vector3.Cross(srcZ, srcX));
        Matrix4x4 srcCF = new Matrix4x4();
        srcCF.SetColumn(0, new Vector4(srcX.x, srcX.y, srcX.z, 0));
        srcCF.SetColumn(1, new Vector4(srcY.x, srcY.y, srcY.z, 0));
        srcCF.SetColumn(2, new Vector4(srcZ.x, srcZ.y, srcZ.z, 0));
        srcCF.SetColumn(3, new Vector4(src1.x, src1.y, src1.z, 1));

        Vector3 dstX = Vector3.Normalize(dst2 - dst1);
        Vector3 dstY = Vector3.Normalize(dst3 - dst1);
        Vector3 dstZ = Vector3.Normalize(Vector3.Cross(dstX, dstY));
        dstY = Vector3.Normalize(Vector3.Cross(dstZ, dstX));
        Matrix4x4 dstCF = new Matrix4x4();
        dstCF.SetColumn(0, new Vector4(dstX.x, dstX.y, dstX.z, 0));
        dstCF.SetColumn(1, new Vector4(dstY.x, dstY.y, dstY.z, 0));
        dstCF.SetColumn(2, new Vector4(dstZ.x, dstZ.y, dstZ.z, 0));
        dstCF.SetColumn(3, new Vector4(dst1.x, dst1.y, dst1.z, 1));

        Matrix4x4 scaleMat = Matrix4x4.identity;
        if (includeScale) {
            float scale = (dst2 - dst1).magnitude / (src2 - src1).magnitude;
            scaleMat = Matrix4x4.Scale(new Vector3(scale, scale, scale));
        }
        Matrix4x4 originToDst1 = Matrix4x4.Translate(dst1);
        Matrix4x4 dst1ToOrigin = Matrix4x4.Translate(-dst1);

        return originToDst1 * scaleMat * dst1ToOrigin * dstCF * srcCF.inverse;
    }

    Vector3 m_Last0HitPt;
    Vector3 m_Last1HitPt;
    Plane m_TouchPlane;
    Transform m_TouchObjTransform;

    [SerializeField] private Camera m_Camera;

    [SerializeField] private VREventCallbackVector2 m_Touch0Down;
    [SerializeField] private VREventCallbackVector2 m_Touch0Move;
    [SerializeField] private VREventCallbackVector2 m_Touch0Up;

    [SerializeField] private VREventCallbackVector2 m_Touch1Down;
    [SerializeField] private VREventCallbackVector2 m_Touch1Move;
    [SerializeField] private VREventCallbackVector2 m_Touch1Up;
}
