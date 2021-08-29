using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.InputSystem;

namespace IVLab.MinVR3
{
    [CustomEditor(typeof(FSM))]
    public class FSMEditor : Editor
    {

        public void OnEnable()
        {
            m_StateMachine = (FSM)target;

            m_StartStateProp = serializedObject.FindProperty("m_StartState");
            m_DebugProp = serializedObject.FindProperty("m_Debug");

            m_StateNamesProp = serializedObject.FindProperty("m_StateNames");
            m_StateEnterCBsProp = serializedObject.FindProperty("m_StateEnterCBs");
            m_StateUpdateCBsProp = serializedObject.FindProperty("m_StateUpdateCBs");
            m_StateExitCBsProp = serializedObject.FindProperty("m_StateExitCBs");

            m_ArcFromIDsProp = serializedObject.FindProperty("m_ArcFromIDs");
            m_ArcToIDsProp = serializedObject.FindProperty("m_ArcToIDs");
            m_ArcTriggerCBsProp = serializedObject.FindProperty("m_ArcTriggerCBs");
            m_ArcRequireTokensProp = serializedObject.FindProperty("m_ArcRequireTokens");
            m_ArcReleaseTokensProp = serializedObject.FindProperty("m_ArcReleaseTokens");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIContent[] stateDisplayNames = m_StateMachine.stateNames.Select(n => new GUIContent(n)).ToArray();
            int[] stateIDs = new int[m_StateMachine.stateNames.Count];
            for (int i = 0; i < stateIDs.Length; i++) stateIDs[i] = i;


            // GENERAL 

            m_StartStateProp.intValue = EditorGUILayout.IntPopup(new GUIContent("Start State", "One state must be identified as the default/initial state"), m_StartStateProp.intValue, stateDisplayNames, stateIDs);

            EditorGUILayout.PropertyField(m_DebugProp, new GUIContent("Debug Log", "Logs state transitions and OnEnter(), OnTrigger(), and OnExit() functions called"));


            // STATES

            EditorGUILayout.LabelField("States", EditorStyles.boldLabel);


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

                if ((i < m_StateExpanded.Count) && (m_StateExpanded[i])) {
                    EditorGUILayout.PropertyField(nameProp, new GUIContent("Name", "There must be at least one state, and each state must have a unique name"));
                    
                    SerializedProperty enterCBProp = m_StateEnterCBsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(enterCBProp, new GUIContent("On Enter", "Add GameObject function(s) to call whenever the FSM enters this state from another state"));

                    SerializedProperty updateCBProp = m_StateUpdateCBsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(updateCBProp, new GUIContent("On Update", "Add GameObject function(s) to call once per frame whenever this state is active"));

                    SerializedProperty exitCBProp = m_StateExitCBsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(exitCBProp, new GUIContent("On Exit", "Add GameObject function(s) to call whenever the FSM leaves this state to enter another one"));
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
                if ((fromIDProp.intValue >= 0) && (fromIDProp.intValue < stateDisplayNames.Length)) {
                    from = stateDisplayNames[fromIDProp.intValue].text;
                }
                string to = "(null)";
                if ((toIDProp.intValue >= 0) && (toIDProp.intValue < stateDisplayNames.Length)) {
                    to = stateDisplayNames[toIDProp.intValue].text;
                }
                string arcName = "Arc #" + i + ":  " + from + " --> " + to;

                EditorGUILayout.BeginHorizontal();
                m_ArcExpanded[i] = EditorGUILayout.BeginFoldoutHeaderGroup(m_ArcExpanded[i], arcName);
                if (GUILayout.Button(new GUIContent("-", "Delete this arc"), EditorStyles.miniButton, GUILayout.Width(20f))) {
                    m_StateMachine.RemoveArc(i);
                    m_ArcExpanded.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();

                if ((i < m_ArcExpanded.Count) && (m_ArcExpanded[i])) {
                    fromIDProp.intValue = EditorGUILayout.IntPopup(new GUIContent("From State", "The arc starts at the FROM state and goes to the TO state"), fromIDProp.intValue, stateDisplayNames, stateIDs);
                    toIDProp.intValue = EditorGUILayout.IntPopup(new GUIContent("To State", "The arc ends at this TO state, which can be the same as the FROM state if the arc should not cause a state transition"), toIDProp.intValue, stateDisplayNames, stateIDs);

                    SerializedProperty triggerCBProp = m_ArcTriggerCBsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(triggerCBProp, new GUIContent("Trigger", "The VREvent that triggers this arc transition and optionally a callback to receive when the arc is traversed."));

                    SerializedProperty requireTokenProp = m_ArcRequireTokensProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(requireTokenProp, new GUIContent("Require", "[Optional] Add a token to place a conditional guard on this arc; the transition will only occur if the token can be successfully acquired."));

                    SerializedProperty releaseTokenProp = m_ArcReleaseTokensProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(releaseTokenProp, new GUIContent("Release", "[Optional] Release a token previously acquired by the FSM when the arc transitions."));
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if (GUILayout.Button("Add Arc (+)")) {
                m_StateMachine.AddArc();
            }
            
            EditorGUILayout.EndVertical();
 
            serializedObject.ApplyModifiedProperties();
        }
 
        private FSM m_StateMachine;

        private SerializedProperty m_StartStateProp;

        private SerializedProperty m_StateNamesProp;
        private SerializedProperty m_StateEnterCBsProp;
        private SerializedProperty m_StateUpdateCBsProp;
        private SerializedProperty m_StateExitCBsProp;

        private SerializedProperty m_ArcFromIDsProp;
        private SerializedProperty m_ArcToIDsProp;
        private SerializedProperty m_ArcTriggerCBsProp;
        private SerializedProperty m_ArcRequireTokensProp;
        private SerializedProperty m_ArcReleaseTokensProp;

        private SerializedProperty m_DebugProp;

        private List<bool> m_StateExpanded = new List<bool>();
        private List<bool> m_ArcExpanded = new List<bool>();
    }

} // namespace
