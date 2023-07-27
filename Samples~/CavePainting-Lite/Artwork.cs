using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artwork : MonoBehaviour
{
    public void Clear()
    {
        for (int i = 0; i < transform.childCount; i++) {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }
}
