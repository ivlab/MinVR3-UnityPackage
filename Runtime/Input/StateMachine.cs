using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace IVLab.Minteract
{

    public class StateMachine : MonoBehaviour
    {
        public StateMachine()
        {
            AddState("START");
            m_StartState = 0;
        }

        public int AddState(string name)
        {
            m_StateNames.Add(name);
            m_StateEnterCBs.Add(new FSMCallback());
            m_StateUpdateCBs.Add(new FSMCallback());
            m_StateExitCBs.Add(new FSMCallback());
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
            m_ArcTriggerActions.Add(new InputActionReference());
            m_ArcTriggerActionPhases.Add(InputActionPhase.Performed);
            m_ArcTriggerCBs.Add(new FSMDataCallback());
            if (m_Debug) {
                IsValid();
            }
            return m_ArcFromIDs.Count - 1;
        }

        public void RemoveArc(int id)
        {
            m_ArcFromIDs.RemoveAt(id);
            m_ArcToIDs.RemoveAt(id);
            m_ArcTriggerActions.RemoveAt(id);
            m_ArcTriggerActionPhases.RemoveAt(id);
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

        public InputActionAsset inputActionAsset {
            get => m_InputActionAsset;
            set {
                m_InputActionAsset = value;
                if (m_Debug) {
                    IsValid();
                }
            }
        }


        void Start()
        {
            m_CurrentState = startState;
        }

        void Update()
        {
            if (m_VerboseDebug) {
                //Log("Calling " + StateToString(m_CurrentState) + ".Update()", false);
            }
            m_StateUpdateCBs[m_CurrentState].Invoke();
        }

        void OnInputAction(InputAction.CallbackContext context)
        {
            if (m_VerboseDebug) {
                Log("Received input: " + context.action.name + "-" + context.phase, false);
            }
            for (int i = 0; i < NumArcs(); i++) {
                // if the arc originates in the current state, check to see if the action+phase pair sent to this
                // function is a match with the arc's trigger.
                if ((m_ArcFromIDs[i] == m_CurrentState) &&
                    (m_ArcTriggerActions[i].action.name == context.action.name) &&
                    (m_ArcTriggerActionPhases[i] == context.action.phase))
                {
                    if (m_Debug) {
                        Log("Input " + context.action.name + "-" + context.phase + " matches trigger for arc: " + ArcToString(i), false);
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
                        m_CurrentState = m_ArcToIDs[m_CurrentState];
                        if (m_Debug) {
                            Log("Calling OnEnter callback(s): " + m_StateEnterCBs[m_CurrentState].ToString(), false);
                        }
                        m_StateEnterCBs[m_CurrentState].Invoke();
                    }
                }
            }
        }


        void OnEnable()
        {
            if (m_InputActionAsset) {
                if (m_Debug) {
                    Log("Starting to listen for InputActions...", false);
                }
                // start listening for actions and make OnInputAction the single callback func to use for all actions
                // in the InputActions asset.
                // TODO?: we could enable/disable actionmaps based on the current state, that would give the ActionMap
                // functionality a purpose, but not sure it is needed -- right now, just listen for everything
                m_InputActionAsset.Enable();
                foreach (var actionMap in m_InputActionAsset.actionMaps) {
                    actionMap.actionTriggered += OnInputAction;
                }
            } else {
                if (m_Debug) {
                    Log("No connected InputActionsAsset", Application.isPlaying);
                }
            }
        }

        void OnDisable()
        {
            if (m_InputActionAsset) {
                if (m_Debug) {
                    Log("Stopping listening for InputActions", false);
                }
                foreach (var actionMap in m_InputActionAsset.actionMaps) {
                    actionMap.actionTriggered -= OnInputAction;
                }
                m_InputActionAsset.Disable();
            }
        }

        
        bool IsValid() {
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
                if (m_InputActionAsset != null) {
                    bool found = false;
                    foreach (var actionMap in m_InputActionAsset.actionMaps) {
                        foreach (var action in actionMap.actions) {
                            if (action.name == m_ArcTriggerActions[i].action.name) {
                                found = true;
                                break;
                            }
                        }
                        if (found) {
                            break;
                        }
                    }
                    if (!found) {
                        Log("The attached InputActionAsset does not contain an action for Arc #" + i + "'s trigger: " + m_ArcTriggerActions[i].action.name, Application.isPlaying);
                        valid = false;
                    }
                }
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
                Debug.LogWarning("[FSM:" + StateToString(m_CurrentState) + "] " + message);
            }
        }

        // where the input events (technically input actions and phases)
        [SerializeField] private InputActionAsset m_InputActionAsset;

        // id of the state to start in
        [SerializeField] private int m_StartState;

        // state data table
        [SerializeField] private List<string> m_StateNames = new List<string>();
        [SerializeField] private List<FSMCallback> m_StateEnterCBs = new List<FSMCallback>();
        [SerializeField] private List<FSMCallback> m_StateUpdateCBs = new List<FSMCallback>();
        [SerializeField] private List<FSMCallback> m_StateExitCBs = new List<FSMCallback>();

        // arc data table
        [SerializeField] private List<int> m_ArcFromIDs = new List<int>();
        [SerializeField] private List<int> m_ArcToIDs = new List<int>();
        [SerializeField] private List<InputActionReference> m_ArcTriggerActions = new List<InputActionReference>();
        [SerializeField] private List<InputActionPhase> m_ArcTriggerActionPhases = new List<InputActionPhase>();
        [SerializeField] private List<FSMDataCallback> m_ArcTriggerCBs = new List<FSMDataCallback>();

        // runtime only, change only through the API
        private int m_CurrentState;

        // logs OnEnter(), OnTrigger(), and OnExit() calls 
        public bool m_Debug = false;

        // logs all input events received and all OnUpdate() calls, which happen once per frame
        public bool m_VerboseDebug = false;
    }

}
