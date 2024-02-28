#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor;


/// <summary>
/// Make a tarball of a selected packages and output them to the Packages folder
/// </summary>
[ExecuteAlways]
public class MakeTgzPackage : MonoBehaviour
{
    [Header("STEPS TO BUILD PACKAGES:")]
    [Header("1. Define which packages to build (using their package name -")]
    [Header("  find it in Package Manager or in Packages/manifest.json)")]
    [Header("2. Check the 'Build' checkbox to build all packages")]
    [Header("3. Verify that all .tgz files build correctly and are now in your Packages folder")]
    [Header("4. Add these tgz files as 'tarball packages' in the Package Manager")]

    [Tooltip("Add package names you want to build here")]
    public List<string> packageNamesToBuild = new List<string>();

    [Tooltip("Press this 'button' to build your packages")]
    public bool build;

    private string buildFolder;

    // adds a GameObject to the scene which enables user to build tarball
    // packages
    [MenuItem("MinVR3/Build Package(s) to Tarball tgz...")]
    public static void BuildPackagesToTgz()
    {
        GameObject go = new GameObject("Tarball Package Builder");
        go.AddComponent<MakeTgzPackage>();
        Selection.activeGameObject = go;
    }

    void Update()
    {
        if (build && !Application.isPlaying)
        {
            buildFolder = Path.Combine(Application.dataPath, "..", "Packages");
            List();
            build = false;
        }
    }

    static ListRequest Request;

    void List()
    {
        Request = Client.List();    // List packages installed for the project
        EditorApplication.update += Progress;
    }

    void Progress()
    {
        if (Request.IsCompleted)
        {
            if (Request.Status == StatusCode.Success)
            {
                int n = 0;
                foreach (var package in Request.Result)
                {
                    if (packageNamesToBuild.Contains(package.name))
                    {
                        Client.Pack(package.resolvedPath, buildFolder);
                        Debug.Log("Built package: " + package.name);
                        n += 1;
                    }
                }
                Debug.Log($"Built {n} packages to {buildFolder}");
            }
            else if (Request.Status >= StatusCode.Failure)
                Debug.Log(Request.Error.message);

            EditorApplication.update -= Progress;
        }
    }
}

#endif