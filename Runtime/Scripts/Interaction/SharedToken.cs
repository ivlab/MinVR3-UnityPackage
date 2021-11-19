using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements a binary semaphor or lock that can be acquired by GameObjects.  In a 3DUI
/// system with multiple widgets, this can be used to control which widget has the current
/// input focus.
/// </summary>
[AddComponentMenu("MinVR/Interaction/Shared Token")]
public class SharedToken : MonoBehaviour
{
    public MonoBehaviour currentOwner {
        get => m_CurrentOwner;
    }

    public string tokenName {
        get => m_TokenName;
        set => m_TokenName = value;
    }

    private void Reset()
    {
        m_TokenName = name;
    }

    public bool RequestToken(MonoBehaviour requester)
    {
        if (m_CurrentOwner == null) {
            m_CurrentOwner = requester;
            return true;
        } else if (m_CurrentOwner == requester) {
            return true;
        } else {
            return false;
        }
    }

    public bool ReleaseToken(MonoBehaviour owner)
    {
        if (m_CurrentOwner == owner) {
            m_CurrentOwner = null;
            return true;
        } else {
            Debug.LogWarning("Trying to release a token that is not actually owned by the calling object.");
            return false;
        }
    }

    public bool HasToken(MonoBehaviour possibleOwner)
    {
        return m_CurrentOwner == possibleOwner;
    }

    [SerializeField] private string m_TokenName = "Shared Token";
    private MonoBehaviour m_CurrentOwner;
}
