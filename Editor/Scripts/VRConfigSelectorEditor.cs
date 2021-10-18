using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{

    [CustomEditor(typeof(VRConfigSelector))]
    public class VRConfigSelectorEditor : Editor
    {

        void OnEnable()
        {
            m_ParentObj = ((VRConfigSelector)target).gameObject;
        }


        public override void OnInspectorGUI()
        {
            //serializedObject.Update();

            EditorGUILayout.HelpBox("This component acts as a switch, activating one and only one of the child " +
                "GameObjects and deactivating all the rest.  Arrange components that should only be active when " +
                "running in the Cave, under a child GameObject named 'Cave'.  Add others that apply only to the " +
                "desktop under a 'Desktop' child, etc.  Before building your application for a particular platform, " +
                "you can select the desired configuration here in the editor.  OR, you can set the active config " +
                "dynamically by specifying the config name on the command line using '-vrconfig <name>', where " +
                "<name> is the name of one of the child GameObjects of this GameObject.", MessageType.Info);

            GameObject[] childObjs = new GameObject[m_ParentObj.transform.childCount];
            string[] childNames = new string[m_ParentObj.transform.childCount];

            int selected = -1;
            for (int i = 0; i < childObjs.Length; i++) {
                childObjs[i] = m_ParentObj.transform.GetChild(i).gameObject;
                childNames[i] = childObjs[i].name;
                if (childObjs[i].activeInHierarchy) {
                    selected = i;
                }
            }

            EditorGUILayout.LabelField("Active Config");
            EditorGUI.BeginChangeCheck();
            selected = EditorGUILayout.Popup(selected, childNames);
            if (EditorGUI.EndChangeCheck()) {
                for (int i = 0; i < childObjs.Length; i++) {
                    childObjs[i].SetActive(selected == i);
                }
            }

            //serializedObject.ApplyModifiedProperties();
        }

        GameObject m_ParentObj;
    }

} // namespace
