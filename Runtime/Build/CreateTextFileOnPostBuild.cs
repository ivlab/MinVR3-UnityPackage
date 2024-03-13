#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Creates a text file (e.g., a shell script to run the application) whenever
    /// Unity does a Build.
    /// 
    /// IMPORTANT: This script will not work and is not even compiled outside of the
    /// UNITY EDITOR, so the GameObject it is attached to must be tagged with EditorOnly.
    /// If not, an error or warning will occur when building because the script will be
    /// referenced by the GameObject but will not be able to be found.
    /// 
    /// </summary>
    [ExecuteInEditMode]
    public class CreateTextFileOnPostBuild : MonoBehaviour, IPostprocessBuildWithReport
    {
        [System.Serializable]
        public class CreateTextFileOnPostBuildSettings
        {
            public string fileName;

            [TextArea(10, 20)]
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

        private static string SettingsFilePrefix = typeof(CreateTextFileOnPostBuildSettings).Name + "_";

        private string SettingsPath { get => Path.Combine(Application.persistentDataPath, SettingsFilePrefix + this.name + ".json"); }

        void OnEnable()
        {
            // Read settings from file (this is so we can persist data between regular editor mode and when the build is happening)
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
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            // See what settings files exist
            string[] settingsFiles = Directory.GetFiles(Application.persistentDataPath, SettingsFilePrefix + "*.json");

            Debug.Log("CreateTextFileOnPostBuild: Found files:\n- " + string.Join("\n- ", settingsFiles));

            foreach (string settingsFile in settingsFiles)
            {
                // Load individual settings file
                string settingsPath = Path.Combine(Application.persistentDataPath, settingsFile);
                using (StreamReader reader = new StreamReader(settingsPath))
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
}

#endif