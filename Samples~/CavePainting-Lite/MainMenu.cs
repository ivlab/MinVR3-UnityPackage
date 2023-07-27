using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public Artwork m_Artwork;

    public void OnMenuItemSelected(int itemId)
    {
        // clear artwork
        if (itemId == 0) {
            Debug.Assert(m_Artwork != null);
            m_Artwork.Clear();
        }
    }
}
