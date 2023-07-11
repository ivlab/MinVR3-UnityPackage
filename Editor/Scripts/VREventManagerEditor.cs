using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using IVLab.MinVR3.ExtensionMethods;

namespace IVLab.MinVR3
{

    [CustomEditor(typeof(VREventManager))]
    public class VREventManagerEditor : Editor
    {
        private bool vrEventGeneratorToggle = false;
        private Dictionary<GameObject, bool> gameObjectToggles = new Dictionary<GameObject, bool>();
        private string filterText = "";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Basic output
            bool debugMode = serializedObject.FindProperty("m_ShowDebuggingOutput").boolValue;
            debugMode = EditorGUILayout.Toggle("Show Debugging Output", debugMode);
            serializedObject.FindProperty("m_ShowDebuggingOutput").boolValue = debugMode;

            // Show all object that are currently registering VREvent prototypes
#if UNITY_EDITOR
            vrEventGeneratorToggle = EditorGUILayout.BeginFoldoutHeaderGroup(vrEventGeneratorToggle, "VR Event prototypes in this scene");
            if (vrEventGeneratorToggle)
            {
                EditorGUILayout.HelpBox(
                    "This section shows all the GameObjects in the scene that are " +
                    "currently registering VREvent prototypes and what type. " +
                    "NOTE: This only works in Unity Editor. " +
                    "Optionally, you can filter the displayed events by name.",
                    MessageType.Info
                );

                GameObject[] eventProducerObjects = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                    .Where(m => m.GetComponent<IVREventProducer>() != null)
                    .Select(m => m.gameObject).ToArray();

                HashSet<GameObject> eventProducerObjectsUnique = new HashSet<GameObject>(eventProducerObjects);

                // Unique event lists per GameObject
                var objectEventLists = new Dictionary<GameObject, HashSet<IVREventPrototype>>();
                foreach (var producerObject in eventProducerObjectsUnique) {
                    var producers = producerObject.GetComponents<IVREventProducer>();
                    foreach (var producer in producers)
                    {
                        var expectedFromThisSource = producer.GetEventPrototypes();
                        if (objectEventLists.ContainsKey(producerObject.gameObject))
                        {
                            objectEventLists[producerObject.gameObject].UnionWith(expectedFromThisSource);
                        }
                        else
                        {
                            objectEventLists[producerObject.gameObject] = new HashSet<IVREventPrototype>(expectedFromThisSource);
                        }

                        if (!gameObjectToggles.ContainsKey(producerObject.gameObject))
                        {
                            gameObjectToggles[producerObject.gameObject] = false;
                        }
                    }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Filter event names", EditorStyles.miniLabel);
                filterText = EditorGUILayout.TextField(filterText);
                EditorGUILayout.EndHorizontal();

                foreach (var kv in objectEventLists)
                {
                    if (filterText == "" || kv.Value.Any(evt => evt.GetEventName().ToLower().Contains(filterText.ToLower())))
                    {
                        gameObjectToggles[kv.Key] = EditorGUILayout.Foldout(gameObjectToggles[kv.Key], "GameObject: " + kv.Key.GetScenePath());
                        if (gameObjectToggles[kv.Key])
                        {
                            foreach (var evt in kv.Value)
                            {
                                EditorGUILayout.LabelField("    " + evt.GetEventDisplayName(), EditorStyles.miniLabel);
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
#endif

            serializedObject.ApplyModifiedProperties();
        }
    }

} // namespace
