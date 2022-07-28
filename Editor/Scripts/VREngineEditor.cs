using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{

    [CustomEditor(typeof(VREngine))]
    public class VREngineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            const string msg =
                "VREngine is a singleton that persists across scene loads and unloads. The same VREngine must remain " +
                "active throughout the life of the application in order to properly handle MinVR's cluster support. " +
                "The managers attached to the VREngine and any child GameObjects will similarly persist across scene " +
                "loads and unloads.  This is a good place to add VRConfigs and their input and display devices that " +
                "should remain active throughout the life of the Application.";

            EditorGUILayout.HelpBox(msg, MessageType.Info);
        }
    }

} // namespace
