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
    [AddComponentMenu("MinVR/Interaction/FSM (State Machine)")]
    public class FSM : MonoBehaviour, IVREventReceiver
    {
        private void Reset()
        {
            // state data table
            m_StateNames = new List<string>();
            m_StateEnterCBs = new List<UnityEvent>();
            m_StateUpdateCBs = new List<UnityEvent>();
            m_StateExitCBs = new List<UnityEvent>();

            // arc data table
            m_ArcFromIDs = new List<int>();
            m_ArcToIDs = new List<int>();
            m_ArcTriggerCBs = new List<VREventCallback>();
            m_ArcRequireTokens = new List<Token>();
            m_ArcReleaseTokens = new List<Token>();

            AddState("START");
            m_StartState = 0;
        }

        public int AddState(string name)
        {
            return AddState(name, null, null, null);
        }

        public int AddState(string name, UnityEvent onEnterCallback, UnityEvent onUpdateCallback, UnityEvent onExitCallback)
        {
            m_StateNames.Add(name);
            if (onEnterCallback == null) {
                m_StateEnterCBs.Add(new UnityEvent());
            } else {
                m_StateEnterCBs.Add(onEnterCallback);
            }
            if (onUpdateCallback == null) {
                m_StateUpdateCBs.Add(new UnityEvent());
            } else {
                m_StateUpdateCBs.Add(onUpdateCallback);
            }
            if (onExitCallback == null) {
                m_StateExitCBs.Add(new UnityEvent());
            } else {
                m_StateExitCBs.Add(onExitCallback);
            }
            return m_StateNames.Count - 1;
        }

        public void RemoveState(int id)
        {
            Debug.Assert(id != m_StartState, "Cannot remove the FSM's Start State.");
            Debug.Assert(id != m_CurrentState, "Cannot remove the FSM's Current State.");
            
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
            return AddArc(-1, -1, null, null, null);
        }

        public int AddArc(string fromState, string toState, VREventCallback triggerCallback, Token requireToTransition, Token releaseOnTransition)
        {
            return AddArc(GetStateID(fromState), GetStateID(toState), triggerCallback, requireToTransition, releaseOnTransition);
        }

        public int AddArc(int fromStateID, int toStateID, VREventCallback triggerCallback, Token requireToTransition, Token releaseOnTransition)
        {
            m_ArcFromIDs.Add(fromStateID);
            m_ArcToIDs.Add(toStateID);
            if (triggerCallback == null) {
                m_ArcTriggerCBs.Add(new VREventCallback());
            } else {
                m_ArcTriggerCBs.Add(triggerCallback);
            }
            m_ArcRequireTokens.Add(requireToTransition);
            m_ArcReleaseTokens.Add(releaseOnTransition);
            return m_ArcFromIDs.Count - 1;
        }

        public void RemoveArc(int id)
        {
            m_ArcFromIDs.RemoveAt(id);
            m_ArcToIDs.RemoveAt(id);
            m_ArcTriggerCBs.RemoveAt(id);
            m_ArcRequireTokens.RemoveAt(id);
            m_ArcReleaseTokens.RemoveAt(id);
        }

        public int NumArcs()
        {
            return m_ArcFromIDs.Count;
        }


        public int currentStateID {
            get => m_CurrentState;
        }

        public int startStateID {
            get => m_StartState;
            set => m_StartState = value;
        }


        private void OnEnable()
        {
            VREngine.main.eventManager.AddEventReceiver(this);
        }

        private void OnDisable()
        {
            VREngine.main.eventManager.RemoveEventReceiver(this);
        }

        void Start()
        {
            m_CurrentState = startStateID;
        }

        void Update()
        {
            m_StateUpdateCBs[m_CurrentState].Invoke();
        }

        public void OnVREvent(VREventInstance vrEvent)
        {
            if (enabled) {

                for (int i = 0; i < NumArcs(); i++) {
                    // if the arc originates in the current state and the event matches the arcs trigger
                    if ((m_ArcFromIDs[i] == m_CurrentState) && (m_ArcTriggerCBs[i].listeningFor.name == vrEvent.name)) {
                        if (m_Debug) {
                            Debug.Log("Input " + vrEvent.name + " matches trigger for arc: " + ArcToString(i));
                        }

                        // check to see if the arc is guarded by a token
                        bool canTraverse = true;
                        if (m_ArcRequireTokens[i] != null) {
                            canTraverse = m_ArcRequireTokens[i].RequestToken(this);
                            if (m_Debug) {
                                if (canTraverse) {
                                    Debug.Log("Arc Guard Condition met; token acquired.");
                                } else {
                                    Debug.Log("Arc Guard Condition not met; token could not be acquired.");
                                }
                            }
                        }

                        if (canTraverse) {
                            // if the arc releases a token when traversed, go ahead and do that first
                            if (m_ArcReleaseTokens[i] != null) {
                                m_ArcReleaseTokens[i].ReleaseToken(this);
                            }

                            // now traverse the arc, changing state and calling appropriate callbacks as needed

                            // case 1: this arc stays within the same state, only call the arc's callback
                            if (m_ArcFromIDs[i] == m_ArcToIDs[i]) {
                                if (m_Debug) {
                                    Debug.Log("Calling OnTrigger callback(s): " + m_ArcTriggerCBs[i].ToString());
                                }
                                m_ArcTriggerCBs[i].Invoke(vrEvent);
                            }

                            // case 2: this arc causes the FSM to change states, call state exit/enter callbacks as well
                            else {
                                if (m_Debug) {
                                    Debug.Log("Calling OnExit callback(s): " + m_StateExitCBs[m_CurrentState].ToString());
                                }
                                m_StateExitCBs[m_CurrentState].Invoke();

                                if (m_Debug) {
                                    Debug.Log("Calling OnTrigger callback(s): " + m_ArcTriggerCBs[i].ToString());
                                }
                                m_ArcTriggerCBs[i].Invoke(vrEvent);
                                m_CurrentState = m_ArcToIDs[i];

                                if (m_Debug) {
                                    Debug.Log("Calling OnEnter callback(s): " + m_StateEnterCBs[m_CurrentState].ToString());
                                }
                                m_StateEnterCBs[m_CurrentState].Invoke();
                            }
                        }
                    }
                }
            }
        }


        public string StateToString(int id) {
            return m_StateNames[id];
        }

        public string ArcToString(int id) {
            return m_StateNames[m_ArcFromIDs[id]] + "-->" + m_StateNames[m_ArcToIDs[id]];
        }

        // id of the state to start in
        [SerializeField] private int m_StartState = 0;

        // state data table
        [SerializeField] private List<string> m_StateNames = new List<string>();
        [SerializeField] private List<UnityEvent> m_StateEnterCBs = new List<UnityEvent>();
        [SerializeField] private List<UnityEvent> m_StateUpdateCBs = new List<UnityEvent>();
        [SerializeField] private List<UnityEvent> m_StateExitCBs = new List<UnityEvent>();
        
        // arc data table
        [SerializeField] private List<int> m_ArcFromIDs = new List<int>();
        [SerializeField] private List<int> m_ArcToIDs = new List<int>();
        [SerializeField] private List<VREventCallback> m_ArcTriggerCBs = new List<VREventCallback>();
        [SerializeField] private List<Token> m_ArcRequireTokens = new List<Token>();
        [SerializeField] private List<Token> m_ArcReleaseTokens = new List<Token>();


        // logs OnEnter(), OnTrigger(), and OnExit() calls 
        [SerializeField] private bool m_Debug = false;


        // runtime only, change only through the API
        private int m_CurrentState;
    }

}
