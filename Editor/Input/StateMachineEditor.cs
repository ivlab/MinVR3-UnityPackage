using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.InputSystem;


namespace IVLab.Minteract
{
    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : Editor
    {

        public void OnEnable()
        {
            m_StateMachine = (StateMachine)target;

            m_InputActionsProp = serializedObject.FindProperty("m_InputActionAsset");
            m_DebugProp = serializedObject.FindProperty("m_Debug");
            m_VerboseDebugProp = serializedObject.FindProperty("m_VerboseDebug");

            m_StartStateProp = serializedObject.FindProperty("m_StartState");

            m_StateNamesProp = serializedObject.FindProperty("m_StateNames");
            m_StateEnterCBsProp = serializedObject.FindProperty("m_StateEnterCBs");
            m_StateUpdateCBsProp = serializedObject.FindProperty("m_StateUpdateCBs");
            m_StateExitCBsProp = serializedObject.FindProperty("m_StateExitCBs");

            m_ArcFromIDsProp = serializedObject.FindProperty("m_ArcFromIDs");
            m_ArcToIDsProp = serializedObject.FindProperty("m_ArcToIDs");
            m_ArcTriggerActionsProp = serializedObject.FindProperty("m_ArcTriggerActions");
            m_ArcTriggerActionPhasesProp = serializedObject.FindProperty("m_ArcTriggerActionPhases");
            m_ArcTriggerCBsProp = serializedObject.FindProperty("m_ArcTriggerCBs");

            RefreshActionNames();
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIContent[] stateNames = m_StateMachine.stateNames.Select(n => new GUIContent(n)).ToArray();
            int[] stateIDs = new int[m_StateMachine.stateNames.Count];
            for (int i = 0; i < stateIDs.Length; i++) stateIDs[i] = i;


            // STATES

            EditorGUILayout.LabelField("States", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            m_StartStateProp.intValue = EditorGUILayout.IntPopup(new GUIContent("Start State", "One state must be identified as the default/initial state"), m_StartStateProp.intValue, stateNames, stateIDs);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(1.5f * EditorGUIUtility.standardVerticalSpacing);

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            while (m_StateExpanded.Count < m_StateNamesProp.arraySize) {
                m_StateExpanded.Add(true);
            }
            for (int i = 0; i < m_StateNamesProp.arraySize; i++) {
                SerializedProperty nameProp = m_StateNamesProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();
                m_StateExpanded[i] = EditorGUILayout.BeginFoldoutHeaderGroup(m_StateExpanded[i], "State #" + i + ":  " + nameProp.stringValue);
                if (GUILayout.Button(new GUIContent("-", "Delete this state"), EditorStyles.miniButton, GUILayout.Width(20f))) {
                    m_StateMachine.RemoveState(i);
                    m_StateExpanded.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();

                if (m_StateExpanded[i]) {
                    EditorGUILayout.PropertyField(nameProp, new GUIContent("Name", "There must be at least one state, and each state must have a unique name"));
                    
                    //EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

                    SerializedProperty enterCBProp = m_StateEnterCBsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(enterCBProp, new GUIContent("On Enter", "Add GameObject function(s) to call whenever the FSM enters this state from another state"));

                    SerializedProperty updateCBProp = m_StateUpdateCBsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(updateCBProp, new GUIContent("On Update", "Add GameObject function(s) to call once per frame whenever this state is active"));

                    SerializedProperty exitCBProp = m_StateExitCBsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(exitCBProp, new GUIContent("On Exit", "Add GameObject function(s) to call whenever the FSM leaves this state to enter another one"));
                    //EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

            }

            if (GUILayout.Button(new GUIContent("Add State (+)", "Add a new state to the state machine"))) {
                int id = 1;
                while (m_StateMachine.StateExists("State (" + id + ")")) {
                    id++;
                }
                m_StateMachine.AddState("State (" + id + ")");
            }
            EditorGUILayout.EndVertical();



            // ARCS

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Arcs", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

            while (m_ArcExpanded.Count < m_ArcFromIDsProp.arraySize) {
                m_ArcExpanded.Add(true);
            }
            for (int i = 0; i < m_ArcFromIDsProp.arraySize; i++) {
                SerializedProperty fromIDProp = m_ArcFromIDsProp.GetArrayElementAtIndex(i);
                SerializedProperty toIDProp = m_ArcToIDsProp.GetArrayElementAtIndex(i);

                string from = "(null)";
                if ((fromIDProp.intValue >= 0) && (fromIDProp.intValue < stateNames.Length)) {
                    from = stateNames[fromIDProp.intValue].text;
                }
                string to = "(null)";
                if ((toIDProp.intValue >= 0) && (toIDProp.intValue < stateNames.Length)) {
                    to = stateNames[toIDProp.intValue].text;
                }
                string arcName = "Arc #" + i + ":  " + from + " --> " + to;

                EditorGUILayout.BeginHorizontal();
                m_ArcExpanded[i] = EditorGUILayout.BeginFoldoutHeaderGroup(m_ArcExpanded[i], arcName);
                if (GUILayout.Button(new GUIContent("-", "Delete this arc"), EditorStyles.miniButton, GUILayout.Width(20f))) {
                    m_StateMachine.RemoveArc(i);
                    m_ArcExpanded.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();

                if (m_ArcExpanded[i]) {
                    fromIDProp.intValue = EditorGUILayout.IntPopup(new GUIContent("From State", "The arc starts at the FROM state and goes to the TO state"), fromIDProp.intValue, stateNames, stateIDs);
                    toIDProp.intValue = EditorGUILayout.IntPopup(new GUIContent("To State", "The arc ends at this TO state, which can be the same as the FROM state if the arc should not cause a state transition"), toIDProp.intValue, stateNames, stateIDs);

                    EditorGUILayout.Space(1.5f * EditorGUIUtility.standardVerticalSpacing);

                    SerializedProperty triggerActionProp = m_ArcTriggerActionsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(triggerActionProp, new GUIContent("Trigger Action", "In Unity's Input System, each action (e.g., 'draw') has multiple phases (e.g., 'started, performed, cancelled'). To define a trigger, specify both the action AND the particular phase of that action that should trigger the arc"));
                    SerializedProperty triggerActionPhaseProp = m_ArcTriggerActionPhasesProp.GetArrayElementAtIndex(i);
                    triggerActionPhaseProp.enumValueIndex = EditorGUILayout.Popup(new GUIContent("Trigger Phase", "The specific phase of the action to use for the trigger; corresponds to Unity's InputActionPhase"), triggerActionPhaseProp.enumValueIndex, triggerActionPhaseProp.enumDisplayNames);

                    EditorGUILayout.Space(1.5f * EditorGUIUtility.standardVerticalSpacing);

                    SerializedProperty triggerCBProp = m_ArcTriggerCBsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(triggerCBProp, new GUIContent("On Trigger", "Add GameObject function(s) to call whenever this arc is triggered"));
                }
            }

            if (GUILayout.Button("Add Arc (+)")) {
                m_StateMachine.AddArc();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();



            // DEBUG

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(m_DebugProp, new GUIContent("Log Transitions", "Logs state transitions and OnEnter(), OnTrigger(), and OnExit() functions called"));
            EditorGUILayout.PropertyField(m_VerboseDebugProp, new GUIContent("Log Input", "Logs all input recieved (even if it does not match a trigger)"));

            EditorGUI.indentLevel--;


            serializedObject.ApplyModifiedProperties();
        }

        // Call this whenever the attached InputActionsAsset is changed
        public void RefreshActionNames()
        {
            m_AllActionNames = new List<string>();
            if (m_StateMachine.inputActionAsset != null) {
                foreach (var actionMap in m_StateMachine.inputActionAsset.actionMaps) {
                    foreach (var action in actionMap.actions) {
                        m_AllActionNames.Add(action.name);
                    }
                }
            }
            m_AllActionNamesArray = m_AllActionNames.ToArray();
        }

        public List<string> m_AllActionNames;
        public string[] m_AllActionNamesArray;
 
        private StateMachine m_StateMachine;
        private SerializedProperty m_InputActionsProp;

        private SerializedProperty m_StartStateProp;

        private SerializedProperty m_StateNamesProp;
        private SerializedProperty m_StateEnterCBsProp;
        private SerializedProperty m_StateUpdateCBsProp;
        private SerializedProperty m_StateExitCBsProp;

        private SerializedProperty m_ArcFromIDsProp;
        private SerializedProperty m_ArcToIDsProp;
        private SerializedProperty m_ArcTriggerActionsProp;
        private SerializedProperty m_ArcTriggerActionPhasesProp;
        private SerializedProperty m_ArcTriggerCBsProp;

        private SerializedProperty m_DebugProp;
        private SerializedProperty m_VerboseDebugProp;

        private List<bool> m_StateExpanded = new List<bool>();
        private List<bool> m_ArcExpanded = new List<bool>();
    }

}