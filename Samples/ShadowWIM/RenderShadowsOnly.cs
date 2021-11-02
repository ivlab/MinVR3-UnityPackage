using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RenderShadowsOnly : MonoBehaviour
{
    public GameObject m_MeshRoot;
    private MeshRenderer[] m_Renderers;

    void Start()
    {
        m_Renderers = m_MeshRoot.GetComponentsInChildren<MeshRenderer>();
    }

    void OnPreRender()
    {
        foreach (MeshRenderer r in m_Renderers) {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }

    void OnPostRender()
    {
        foreach (MeshRenderer r in m_Renderers) {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
}
