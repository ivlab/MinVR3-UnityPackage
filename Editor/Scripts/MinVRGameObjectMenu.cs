using UnityEditor;
using UnityEngine;

public class MinVRGameObjectMenu : MonoBehaviour
{
    // BUG: This might change depending on how the MinVR package is included in the current project
    // TODO: Need a more portable solution for this.
    static string CONFIG_PREFABS_PATH = "Assets/MinVR3/Runtime/Config-Prefabs/";


    
    [MenuItem("GameObject/MinVR/MinVR Root (Room Space)", false, 10)]
    static void CreateMinVRRoot(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, CONFIG_PREFABS_PATH + "MinVR Root (Room Space).prefab");
    }

    [MenuItem("GameObject/MinVR/VRConfig/ClipboardVR", false, 10)]
    static void CreateClipboardVR(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, CONFIG_PREFABS_PATH + "VR Configs/ClipboardVR.prefab");
    }

    [MenuItem("GameObject/MinVR/VRConfig/ClipboardVR Simulator", false, 10)]
    static void CreateClipboardVRSim(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, CONFIG_PREFABS_PATH + "VR Configs/ClipboardVR Simulator.prefab");
    }

    [MenuItem("GameObject/MinVR/VRConfig/Desktop VR Simulator", false, 10)]
    static void CreateDesktopVRSim(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, CONFIG_PREFABS_PATH + "VR Configs/Desktop VR Simulator.prefab");
    }

    [MenuItem("GameObject/MinVR/VRConfig/Tablet", false, 10)]
    static void CreateTablet(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, CONFIG_PREFABS_PATH + "VR Configs/Tablet.prefab");
    }

    [MenuItem("GameObject/MinVR/VRConfig/UnityXR (Oculus, Magic Leap, Hololens, ...)", false, 10)]
    static void CreateUnityXR(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, CONFIG_PREFABS_PATH + "VR Configs/UnityXR (Oculus, Magic Leap, Hololens, ...).prefab");
    }

    [MenuItem("GameObject/MinVR/VRConfig/WorkbenchVR Simulator", false, 10)]
    static void CreateWorkbenchVRSim(MenuCommand command)
    {
        InstatiatePrefabFromAsset(command, CONFIG_PREFABS_PATH + "VR Configs/WorkbenchVR Simulator.prefab");
    }





    static void InstatiatePrefabFromAsset(MenuCommand command, string assetPath)
    {
        Object prefabAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
}
