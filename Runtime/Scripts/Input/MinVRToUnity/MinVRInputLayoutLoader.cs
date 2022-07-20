#if XR_INTERACTION_TOOLKIT_PRESENT

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Scripting;

namespace IVLab.MinVR3
{
    /// <summary>
    /// This class automatically registers control layouts used by the <see cref="XRDeviceSimulator"/>.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve]
    public static class MinVRInputLayoutLoader
    {
        [Preserve]
        static MinVRInputLayoutLoader()
        {
            RegisterInputLayouts();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad), Preserve]
        public static void Initialize()
        {
            // Will execute the static constructor as a side effect.
        }

        static void RegisterInputLayouts()
        {
            //InputSystem.InputSystem.RegisterLayout<MinVRHMD>();
            UnityEngine.InputSystem.InputSystem.RegisterLayout<MinVRControllerDevice>();
        }
    }
}

#endif
