using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace IVLab.MinVR3 {

    using UnityEngine;


    [AddComponentMenu("MinVR/VR Input")]
    public class VRInput : MonoBehaviour
    {
        /// <summary>
        /// All ActionMaps in these assets are automatically enabled.  These serve as the input sources for
        /// all MinVR inputs.
        /// </summary>
        public List<InputActionAsset> inputActionAssets {
            get => m_InputActionAssets;
            set => m_InputActionAssets = value;
        }

        /// <summary>
        /// Called automatically from MonoBehaviour.OnEnable(); can also call manually together with DisableInput().
        /// </summary>
        public void EnableInput()
        {
            if (m_InputActionAssets != null) {
                foreach (var actionAsset in m_InputActionAssets) {
                    if (actionAsset != null) {
                        foreach (var actionMap in actionAsset.actionMaps) {
                            actionMap.Enable();
                            actionMap.actionTriggered += InternalOnActionTriggered;
                        }
                    }
                }
            }
        }

        protected void OnEnable()
        {
            EnableInput();
        }


        /// <summary>
        /// Called automatically from MonoBehaviour.OnEnable(); can also call manually together with EnableInput().
        /// </summary>
        public void DisableInput()
        {
            if (m_InputActionAssets != null) {
                foreach (var actionAsset in m_InputActionAssets) {
                    if (actionAsset != null) {
                        foreach (var actionMap in actionAsset.actionMaps) {
                            actionMap.Disable();
                            actionMap.actionTriggered -= InternalOnActionTriggered;
                        }
                    }
                }
            }
        }


        protected void OnDisable()
        {
            DisableInput();
        }


        public static string ActionToString(InputAction a, bool includePhase=true)
        {
            string s = "";
            if (a.actionMap != null) {
                s += a.actionMap.name + "/";
            }
            s += a.name;
            if (includePhase) {
                s += " (" + a.phase.ToString() + ")";
            }
            return s;
        }


        // list of the full names of all actions in the attached InputActionAssets
        public List<string> editorActionNames {
            get {
                if (m_EditorActionNames == null) {
                    RefreshEditorArrays();
                }
                return m_EditorActionNames;
            }
        }
        private List<string> m_EditorActionNames;

        // GUI labels for each action
        public GUIContent[] editorGUIContents {
            get {
                if (m_EditorGUIContents == null) {
                    RefreshEditorArrays();
                }
                return m_EditorGUIContents;
            }
        }
        private GUIContent[] m_EditorGUIContents;

        // GUI indices
        public int[] editorActionIndices {
            get {
                if (m_EditorActionIndices == null) {
                    RefreshEditorArrays();
                }
                return m_EditorActionIndices;
            }
        }
        private int[] m_EditorActionIndices;


        public void RefreshEditorArrays() {
            // TODO: include some dirty flag here so that these lists are not regenerated unless needed

            int nActions = 0;
            if (m_InputActionAssets != null) {
                foreach (var actionAsset in m_InputActionAssets) {
                    if (actionAsset != null) {
                        foreach (var actionMap in actionAsset.actionMaps) {
                            nActions += actionMap.actions.Count;
                        }
                    }
                }
            }

            if (nActions == 0) return;

            m_EditorActionNames = new List<string>(nActions);
            m_EditorGUIContents = new GUIContent[nActions];
            m_EditorActionIndices = new int[nActions];

            int i = 0;
            foreach (var actionAsset in m_InputActionAssets) {
                if (actionAsset != null) {
                    foreach (var actionMap in actionAsset.actionMaps) {
                        foreach (var action in actionMap) {
                            m_EditorActionNames.Insert(i, ActionToString(action, false));
                            m_EditorGUIContents[i] = new GUIContent(m_EditorActionNames[i]);
                            m_EditorActionIndices[i] = i;
                            i++;
                        }
                    }
                }
            }
        }
        

        private void InternalOnActionTriggered(InputAction.CallbackContext context)
        {
            if (m_LogAllInput) {
                Debug.Log("MinVRInput Action Triggered: " + ActionToString(context.action));
            }

            // forward to anyone registered with the global actiontriggered action
            actionTriggered.Invoke(context);
        }


        [Tooltip("One or more Unity InputActionAssets that provide user input to the MinVRInput system")]
        [SerializeField] private List<InputActionAsset> m_InputActionAssets;

        [Tooltip("Called whenever any of the actions in the attached InputActionAssets is triggered; read the CallbackContext struct to determine which action was triggered and extract any data from it")]
        public UnityEvent<InputAction.CallbackContext> actionTriggered;

        [Tooltip("For debugging purposes, send all input actions received to Debug.Log()")]
        [SerializeField] private bool m_LogAllInput = false;
    }

} // namespace