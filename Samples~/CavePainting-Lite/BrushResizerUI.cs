using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushResizerUI : MonoBehaviour
{
    public void ScaleBrush_OnEnter()
    {
        m_LastHandPos = handCursor.transform.position;
        m_LastBrushPos = brushCursor.transform.position;
    }

    public void ScaleBrush_OnUpdate()
    {
        Vector3 handPosWorld = handCursor.transform.position;
        Vector3 brushPosWorld = brushCursor.transform.position;
        Vector3 curSpan = handPosWorld - brushPosWorld;
        Vector3 lastSpan = m_LastHandPos - m_LastBrushPos;

        float deltaScale = curSpan.magnitude / lastSpan.magnitude;
        brushCursor.transform.ScaleAroundLocalOrigin(deltaScale);

        // clamp to make sure the brush never gets so small it disappears
        Vector3 s = brushCursor.transform.localScale;
        s.x = Mathf.Clamp(s.x, minBrushScale, maxBrushScale);
        s.y = Mathf.Clamp(s.y, minBrushScale, maxBrushScale);
        s.z = Mathf.Clamp(s.z, minBrushScale, maxBrushScale);
        brushCursor.transform.localScale = s;

        m_LastHandPos = handPosWorld;
        m_LastBrushPos = brushPosWorld;
    }

    public GameObject brushCursor;
    public GameObject handCursor;
    public float minBrushScale = 0.01f;
    public float maxBrushScale = 0.5f;

    private Vector3 m_LastBrushPos;
    private Vector3 m_LastHandPos;
}
