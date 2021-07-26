using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace IVLab.MinVR3
{
    [AddComponentMenu("MinVR/Finite State Machine (FSM)")]
    public class FSM : MonoBehaviour
    {
        public FSM()
        {
            AddState("START");
            m_StartState = 0;
        }

        public int AddState(string name)
        {
            m_StateNames.Add(name);
            m_StateEnterCBs.Add(new FSMStateCallback());
            m_StateUpdateCBs.Add(new FSMStateCallback());
            m_StateExitCBs.Add(new FSMStateCallback());
            if (m_Debug) {
                IsValid();
            }
            return m_StateNames.Count - 1;
        }

        public void RemoveState(int id)
        {
            if (id == m_StartState) {
                Log("Cannot remove the FSM's Start State.", true);
                return;
            }
            if (id == m_CurrentState) {
                Log("Cannot remove the FSM's Current State.", true);
                return;
            }
            m_StateNames.RemoveAt(id);
            m_StateEnterCBs.RemoveAt(id);
            m_StateUpdateCBs.RemoveAt(id);
            m_StateExitCBs.RemoveAt(id);

            for (int i=0; i< m_ArcFromIDs.Count; i++) {
                if (m_ArcFromIDs[i] == id) {
                    m_ArcFromIDs[i] = -1;
                }
            }
            for (int i = 0; i < m_ArcToIDs.Count; i++) {
                if (m_ArcToIDs[i] == id) {
                    m_ArcToIDs[i] = -1;
                }
            }
            if (m_Debug) {
                IsValid();
            }
        }

        public int NumStates()
        {
            return m_StateNames.Count;
        }

        public int GetStateID(string name)
        {
            return m_StateNames.IndexOf(name);
        }

        public bool StateExists(string name)
        {
            return m_StateNames.IndexOf(name) != -1;
        }

        public List<string> stateNames
        {
            get => m_StateNames;
        }


        public int AddArc()
        {
            m_ArcFromIDs.Add(-1);
            m_ArcToIDs.Add(-1);
            m_ArcTriggers.Add(new VRActionReference());
            m_ArcTriggerCBs.Add(new FSMArcCallback());
            if (m_Debug) {
                IsValid();
            }
            return m_ArcFromIDs.Count - 1;
        }

        public void RemoveArc(int id)
        {
            m_ArcFromIDs.RemoveAt(id);
            m_ArcToIDs.RemoveAt(id);
            m_ArcTriggers.RemoveAt(id);
            m_ArcTriggerCBs.RemoveAt(id);
            if (m_Debug) {
                IsValid();
            }
        }

        public int NumArcs()
        {
            return m_ArcFromIDs.Count;
        }


        public int currentState {
            get => m_CurrentState;
        }

        public int startState {
            get => m_StartState;
            set {
                m_StartState = value;
                if (m_Debug) {
                    IsValid();
                }
            }
        }
        

        void Start()
        {
            m_CurrentState = startState;
            MinVR.mainInput.actionTriggered.AddListener(OnInputAction);
        }

        void Update()
        {
            m_StateUpdateCBs[m_CurrentState].Invoke();
        }

        void OnInputAction(InputAction.CallbackContext context)
        {
            string fullname = VRInput.ActionToString(context.action, false);

            for (int i = 0; i < NumArcs(); i++) {
                // if the arc originates in the current state, check to see if the action+phase pair sent to this
                // function is a match with the arc's trigger.
                if ((m_ArcFromIDs[i] == m_CurrentState) && (m_ArcTriggers[i].Matches(context)))
                {
                    if (m_Debug) {
                        Log("Input " + fullname + "-" + context.phase + " matches trigger for arc: " + ArcToString(i), false);
                    }
                    if (m_ArcFromIDs[i] == m_ArcToIDs[i]) {
                        // this arc stays within the same state, only call the arc's callback
                        if (m_Debug) {
                            Log("Calling OnTrigger callback(s): " + m_ArcTriggerCBs[i].ToString(), false);
                        }
                        m_ArcTriggerCBs[i].Invoke(context);
                    } else {
                        // this arc causes the FSM to change states, call state exit/enter callbacks as well
                        if (m_Debug) {
                            Log("Calling OnExit callback(s): " + m_StateExitCBs[m_CurrentState].ToString(), false);
                        }
                        m_StateExitCBs[m_CurrentState].Invoke();
                        if (m_Debug) {
                            Log("Calling OnTrigger callback(s): " + m_ArcTriggerCBs[i].ToString(), false);
                        }
                        m_ArcTriggerCBs[i].Invoke(context);
                        m_CurrentState = m_ArcToIDs[i];
                        if (m_Debug) {
                            Log("Calling OnEnter callback(s): " + m_StateEnterCBs[m_CurrentState].ToString(), false);
                        }
                        m_StateEnterCBs[m_CurrentState].Invoke();
                    }
                }
            }
        }


        public bool IsValid() {
            bool valid = true;
            if (NumStates() < 1) {
                Log("Must contain at least one state.", Application.isPlaying);
                valid = false;
            }
            if ((m_StartState < 0) || (m_StartState >= NumStates())) {
                Log("Invalid start state ID: " + m_StartState, Application.isPlaying);
                valid = false;
            }
            if ((m_CurrentState < 0) || (m_CurrentState >= NumStates())) {
                Log("Invalid current state ID: " + m_CurrentState, Application.isPlaying);
                valid = false;
            }

            for (int i = 0; i < NumStates(); i++) {
                string nameA = m_StateNames[i];
                for (int j = i + 1; j < NumStates(); j++) {
                    string nameB = m_StateNames[j];
                    if (nameA == nameB) {
                        Log("StateMachine: State #s" + i + " and " + j + " both have the same name: " + nameA, Application.isPlaying);
                        valid = false;
                    }
                }
            }

            for (int i = 0; i < NumArcs(); i++) {
                if ((m_ArcFromIDs[i] < 0) || (m_ArcFromIDs[i] >= NumStates())) {
                    Log("Arc #" + i + " has an invalid FROM state: " + m_ArcFromIDs[i], Application.isPlaying);
                    valid = false;
                }
                if ((m_ArcToIDs[i] < 0) || (m_ArcToIDs[i] >= NumStates())) {
                    Log("Arc #" + i + " has an invalid TO state: " + m_ArcToIDs[i], Application.isPlaying);
                    valid = false;
                }
                /*
                bool found = false;
                List<string> actionNames = MinVR.mainInput.GetAllActionNames();
                foreach (string actionName in actionNames) {
                    if (actionName == m_ArcTriggerActionNames[i]) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    Log("MinVR.mainInput does not contain an action for Arc #" + i + "'s trigger: " + m_ArcTriggerActionNames[i], Application.isPlaying);
                    valid = false;
                }
                */
            }
            return valid;
        }



        public string StateToString(int id) {
            return m_StateNames[id];
        }

        public string ArcToString(int id) {
            return m_StateNames[m_ArcFromIDs[id]] + "-->" + m_StateNames[m_ArcToIDs[id]];
        }

        private void Log(string message, bool error)
        {
            if (error) {
                Debug.LogError("[FSM:" + StateToString(m_CurrentState) + "] ERROR: " + message);
            } else {
                Debug.Log("[FSM:" + StateToString(m_CurrentState) + "] " + message);
            }
        }
        
        // id of the state to start in
        [SerializeField] private int m_StartState;

        // state data table
        [SerializeField] private List<string> m_StateNames = new List<string>();
        [SerializeField] private List<FSMStateCallback> m_StateEnterCBs = new List<FSMStateCallback>();
        [SerializeField] private List<FSMStateCallback> m_StateUpdateCBs = new List<FSMStateCallback>();
        [SerializeField] private List<FSMStateCallback> m_StateExitCBs = new List<FSMStateCallback>();

        // arc data table
        [SerializeField] private List<int> m_ArcFromIDs = new List<int>();
        [SerializeField] private List<int> m_ArcToIDs = new List<int>();
        [SerializeField] private List<VRActionReference> m_ArcTriggers = new List<VRActionReference>();
        [SerializeField] private List<FSMArcCallback> m_ArcTriggerCBs = new List<FSMArcCallback>();

        // runtime only, change only through the API
        private int m_CurrentState;

        // logs OnEnter(), OnTrigger(), and OnExit() calls 
        public bool m_Debug = false;
    }

}
