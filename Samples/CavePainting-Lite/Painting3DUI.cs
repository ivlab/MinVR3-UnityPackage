using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Painting3DUI : MonoBehaviour
{

    public void Painting_OnUpdate()
    {
        GameObject paintSplat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paintSplat.transform.parent = artworkParent.transform;
        paintSplat.transform.position = brushCursor.transform.position;
        paintSplat.transform.rotation = brushCursor.transform.rotation;
        paintSplat.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
    }

    public void Reframing_OnEnter()
    {
        m_LastHandPos = handCursor.transform.position;
        m_LastHandRot = handCursor.transform.rotation;
    }

    public void Reframing_OnUpdate()
    {
        Vector3 deltaPos = handCursor.transform.position - m_LastHandPos;
        Quaternion deltaRot = handCursor.transform.rotation * Quaternion.Inverse(m_LastHandRot);

        if (artworkParent != null) {
            artworkParent.transform.localPosition += deltaPos;
            artworkParent.transform.localRotation *= deltaRot;
        }

        m_LastHandPos = handCursor.transform.position;
        m_LastHandRot = handCursor.transform.rotation;
    }


    [Tooltip("Parent GameObject for any 3D geometry produced by painting")]
    public GameObject artworkParent;

    public GameObject brushCursor;
    public GameObject handCursor;

    private Vector3 m_LastHandPos;
    private Quaternion m_LastHandRot;
}
