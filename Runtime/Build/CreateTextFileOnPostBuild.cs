#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Copy files to a destination on build
    /// </summary>
    [ExecuteInEditMode]
    public class CreateTextFileOnPostBuild : MonoBehaviour, IPostprocessBuildWithReport
    {
        [System.Serializable]
        public class CreateTextFileOnPostBuildSettings
        {
            public string fileName;

            [TextArea(10, 10)]
            public string fileText;

            public PostBuildCopyLocation copyLocation = PostBuildCopyLocation.BuildFolder;
        }

        public enum PostBuildCopyLocation
        {
            BuildFolder,
            StreamingAssetsFolder,
            PersistentDataFolder
        }

        public CreateTextFileOnPostBuildSettings settings;

        public int callbackOrder { get { return 0; } }

        private string SettingsPath { get => Path.Combine(Application.persistentDataPath, typeof(CreateTextFileOnPostBuildSettings).Name + ".json"); }

        void OnEnable()
        {
            // Read settings from file
            if (File.Exists(SettingsPath))
            {
                using (StreamReader reader = new StreamReader(SettingsPath))
                {
                    string settingsJson = reader.ReadToEnd();
                    settings = JsonUtility.FromJson<CreateTextFileOnPostBuildSettings>(settingsJson);
                }
            }
            else
            {
                settings = new CreateTextFileOnPostBuildSettings();
            }
        }

        void Update()
        {
#if UNITY_EDITOR
            // only write if we're not in play mode...
            if (!Application.isPlaying)
            {
                // Write settings to file
                string settingsJson = JsonUtility.ToJson(settings);
                using (StreamWriter writer = new StreamWriter(SettingsPath))
                {
                    writer.Write(settingsJson);
                }
            }
#endif
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            // Read settings from file
            using (StreamReader reader = new StreamReader(SettingsPath))
            {
                string settingsJson = reader.ReadToEnd();
                CreateTextFileOnPostBuildSettings settingsInstance = JsonUtility.FromJson<CreateTextFileOnPostBuildSettings>(settingsJson);

                string buildFolder = Directory.GetParent(report.summary.outputPath).FullName;
                // DirectoryInfo streamingAssetsFolder = new DirectoryInfo(Application.streamingAssetsPath);
                // DirectoryInfo persistentDataFolder = new DirectoryInfo(Application.persistentDataPath);

                // DirectoryInfo destinationFolder;
                string destinationFolder;
                switch (settingsInstance.copyLocation)
                {
                    case PostBuildCopyLocation.StreamingAssetsFolder:
                        destinationFolder = Application.streamingAssetsPath;
                        break;
                    case PostBuildCopyLocation.PersistentDataFolder:
                        destinationFolder = Application.persistentDataPath;
                        break;
                    case PostBuildCopyLocation.BuildFolder:
                    default:
                        destinationFolder = buildFolder;
                        break;
                }


                string outPath = Path.Combine(destinationFolder, settingsInstance.fileName);

                // File.Copy(assetPath, outPath);
                File.WriteAllText(outPath, settingsInstance.fileText);
                Debug.Log("CopyFileOnPostBuild.cs: wrote file to " + outPath);
            }

        }
    }
}
#endif