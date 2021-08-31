using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{
    /*
    [CustomEditor(typeof(VREngine))]
    public class VREngineTagManager
    {
        [InitializeOnLoadMethod]
        private static void VerifyMainVREngineTagExists()
        {
            // reference: https://answers.unity.com/questions/33597/is-it-possible-to-create-a-tag-programmatically.html
            UnityEngine.Object[] tagMgrAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((tagMgrAssets == null) || (tagMgrAssets.Length == 0)) {
                Debug.LogError("Cannot load Unity's built-in TagManager.asset");
            } else {
                SerializedObject so = new SerializedObject(tagMgrAssets[0]);
                SerializedProperty tags = so.FindProperty("tags");
                for (int i = 0; i < tags.arraySize; i++) {
                    if (tags.GetArrayElementAtIndex(i).stringValue == VREngine.VRENGINE_MAIN_TAG) {
                        return;     // Tag already present, nothing to do.
                    }
                }
                tags.InsertArrayElementAtIndex(0);
                tags.GetArrayElementAtIndex(0).stringValue = VREngine.VRENGINE_MAIN_TAG;
                so.ApplyModifiedProperties();
                so.Update();
            }
        }
    }
    */

} // namespace
