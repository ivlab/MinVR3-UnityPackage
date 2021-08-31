using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("MinVR/Interaction/Token")]
public class Token : MonoBehaviour
{
    public MonoBehaviour currentOwner {
        get => m_CurrentOwner;
    }

    public bool RequestToken(MonoBehaviour requester)
    {
        if (m_CurrentOwner == null) {
            m_CurrentOwner = requester;
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

    private MonoBehaviour m_CurrentOwner;
}
