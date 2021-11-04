using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zSpace.Core;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Place this script on the camera you would like to use to render on the zSpace
    /// </summary>
    [AddComponentMenu("MinVR/Display/zSpace Display")]
    public class zSpaceDisplay : MonoBehaviour
    {

        void Start()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // zSpace only runs on Windows.  The scripts compile just fine on other platforms
            // no zCore plugin library is provided, so an assertion is fired if the zCore
            // component is included in the scene.  This class provides a wrapper for zSpace
            // and only adds the zCore component if running on Windows.  The remainder of the
            // script should always check to make sure m_zCore != null before accessing it.

            m_zCore = FindObjectOfType<ZCore>();
            if (m_zCore == null) {
                m_zCore = gameObject.AddComponent<ZCore>();
            }            
#endif
        }

        private ZCore m_zCore = null;
    }

} // end namespace
