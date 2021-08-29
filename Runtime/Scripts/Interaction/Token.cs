using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("MinVR/Interaction/Token")]
public class Token : MonoBehaviour
{
    public string tokenName {
        get => m_TokenName;
        set => m_TokenName = value;
    }

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

    [Tooltip("[Optional] Assign a unique name to this token")]
    [SerializeField] private string m_TokenName;

    [Tooltip("[Optional] Typically the current owner is set/updated at runtime only, but you can assign an initial " +
        "owner in the editor if you wish.")]
    [SerializeField] private MonoBehaviour m_CurrentOwner;
}
