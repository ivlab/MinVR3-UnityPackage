using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    /// <summary>
    /// Log the frames per second to the terminal
    /// </summary>
    [AddComponentMenu("MinVR/Debug/Print FPS")]
    public class PrintFPS : MonoBehaviour
    {
        public float logFrequency = 1.0f; // seconds
        private float timeSinceLastLog = 0.0f;

        void Update()
        {
            timeSinceLastLog += Time.deltaTime;

            if (timeSinceLastLog > logFrequency)
            {
                float fps = 1.0f / Time.smoothDeltaTime;
                string text = string.Format("MinVR3: {0:0.} fps", fps);

                Debug.Log(text);
                timeSinceLastLog = 0.0f;
            }
        }
    }

} // namespace
