using UnityEditor;
using UnityEngine;

public class MinVRGameObjectMenu : MonoBehaviour
{
    [MenuItem("GameObject/MinVR/MinVR Root (Room Space)", false, 10)]
    static void CreateMinVRRoot(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, "t:Prefab MinVRRoot");
    }

    [MenuItem("GameObject/MinVR/VRConfig/ClipboardVR", false, 10)]
    static void CreateClipboardVR(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, "t:Prefab VRConfig_ClipboardVR");
    }

    [MenuItem("GameObject/MinVR/VRConfig/ClipboardVR Simulator", false, 10)]
    static void CreateClipboardVRSim(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, "t:Prefab VRConfig_ClipboardVRSimulator");
    }

    [MenuItem("GameObject/MinVR/VRConfig/Desktop VR Simulator", false, 10)]
    static void CreateDesktopVRSim(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, "t:Prefab VRConfig_DesktopVRSimulator");
    }

    [MenuItem("GameObject/MinVR/VRConfig/Tablet", false, 10)]
    static void CreateTablet(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, "t:Prefab VRConfig_Tablet");
    }

    [MenuItem("GameObject/MinVR/VRConfig/UnityXR (Oculus, Magic Leap, Hololens, ...)", false, 10)]
    static void CreateUnityXR(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, "t:Prefab VRConfig_UnityXR");
    }

    [MenuItem("GameObject/MinVR/VRConfig/WorkbenchVR Simulator", false, 10)]
    static void CreateWorkbenchVRSim(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, "t:Prefab VRConfig_WorkbenchVRSimulator");
    }

    [MenuItem("GameObject/MinVR/VRConfig/zSpace (Unity 2019.x Only)", false, 10)]
    static void CreateZSpace(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, "t:Prefab VRConfig_zSpace");
    }


    static void InstatiatePrefabFromAsset(MenuCommand command, string searchStr)
    {
        Object prefabAsset = null;
        string[] guids = AssetDatabase.FindAssets(searchStr);
        if (guids.Length > 0) {
            string fullPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            prefabAsset = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
        }

        Debug.Assert(prefabAsset != null, "Cannot find requested prefab in the AssetDatabase using search string '" +  searchStr + "'.");
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
}
