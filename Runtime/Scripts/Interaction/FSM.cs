using System.Collections.Generic;
using UnityEngine;
using System;


namespace IVLab.MinVR3
{
    [AddComponentMenu("MinVR/Interaction/FSM (State Machine)")]
    public class FSM : MonoBehaviour, IVREventListener
    {
        private void Reset()
        {
            // state data table
            m_StateNames = new List<string>();
            m_StateEnterCBs = new List<VRCallback>();
            m_StateUpdateCBs = new List<VRCallback>();
            m_StateExitCBs = new List<VRCallback>();

            // arc data table
            m_ArcFromIDs = new List<int>();
            m_ArcToIDs = new List<int>();
            m_ArcListeners = new List<VREventCallbackAny>();
            m_ArcRequireTokens = new List<SharedToken>();
            m_ArcReleaseTokens = new List<SharedToken>();
            m_ArcGuards = new List<Condition>();

            AddState("START");
            m_StartState = 0;
            m_Debug = false;
        }

        private void OnEnable()
        {
            m_CurrentState = startStateID;
            StartListening();
        }

        private void OnDisable()
        {
            StopListening();
        }

        public int AddState(string name, VRCallback onEnterCallback = null, VRCallback onUpdateCallback = null, VRCallback onExitCallback = null)
        {
            m_StateNames.Add(name);
            if (onEnterCallback == null) {
                m_StateEnterCBs.Add(new VRCallback());
            } else {
                m_StateEnterCBs.Add(onEnterCallback);
            }
            if (onUpdateCallback == null) {
                m_StateUpdateCBs.Add(new VRCallback());
            } else {
                m_StateUpdateCBs.Add(onUpdateCallback);
            }
            if (onExitCallback == null) {
                m_StateExitCBs.Add(new VRCallback());
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
            return AddArc(-1, -1, new VREventCallbackAny());
        }

        public int AddArc(string fromState, string toState, VREventCallbackAny callback,
            SharedToken requireToTransition = null,
            SharedToken releaseOnTransition = null,
            Condition guard = null)
        {
            return AddArc(GetStateID(fromState), GetStateID(toState), callback,
                requireToTransition, releaseOnTransition, guard);
        }

        public int AddArc(int fromStateID, int toStateID, VREventCallbackAny callback,
            SharedToken requireToTransition = null,
            SharedToken releaseOnTransition = null,
            Condition guard = null)
        {
            m_ArcFromIDs.Add(fromStateID);
            m_ArcToIDs.Add(toStateID);
            if (callback == null) {
                callback = new VREventCallbackAny();
            }
            m_ArcListeners.Add(callback);            
            m_ArcRequireTokens.Add(requireToTransition);
            m_ArcReleaseTokens.Add(releaseOnTransition);
            m_ArcGuards.Add(guard);
            return m_ArcFromIDs.Count - 1;
        }

        public void RemoveArc(int id)
        {
            m_ArcFromIDs.RemoveAt(id);
            m_ArcToIDs.RemoveAt(id);
            m_ArcListeners.RemoveAt(id);
            m_ArcRequireTokens.RemoveAt(id);
            m_ArcReleaseTokens.RemoveAt(id);
            m_ArcGuards.RemoveAt(id);
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


        public void OnVREvent(VREvent vrEvent)
        {
            if (enabled) {

                for (int i = 0; i < NumArcs(); i++) {
                    // if the arc originates in the current state and the event matches the arcs trigger
                    if ((m_ArcFromIDs[i] == m_CurrentState) && (vrEvent.Matches(m_ArcListeners[i]))) {
                        if (m_Debug) {
                            Debug.Log(this.name + " in state " + m_StateNames[m_CurrentState] + " received input " + vrEvent.name + ", which matches the trigger for arc: " + ArcToString(i));
                        }

                        bool canTraverse = true;
                        // if the arc is guarded by a condition, make sure the condition is true
                        if (m_ArcGuards[i] != null) {
                            canTraverse = m_ArcGuards[i].isTrue;
                            if (m_Debug) {
                                if (canTraverse) {
                                    Debug.Log("The arc guard condition " + m_ArcGuards[i].name + " was met.");
                                } else {
                                    Debug.Log("The arc guard condition " + m_ArcGuards[i].name + " was not met.");
                                }
                            }
                        }

                        // if a token is required in order to proceed, then request it and only
                        // proceed if the token is acquired.
                        if ((canTraverse) && (m_ArcRequireTokens[i] != null)) {
                            canTraverse = m_ArcRequireTokens[i].RequestToken(this);
                            if (m_Debug) {
                                if (canTraverse) {
                                    Debug.Log("The required token " + m_ArcRequireTokens[i].name + " was acquired.");
                                } else {
                                    Debug.Log("The required token " + m_ArcRequireTokens[i].name + " could not be acquired.");
                                }
                            }
                        }

                        

                        if (canTraverse) {
                            // if the arc releases a token when traversed, go ahead and do that first
                            if (m_ArcReleaseTokens[i] != null) {
                                if (m_Debug) {
                                    Debug.Log("Releasing token " + m_ArcReleaseTokens[i].name);
                                }
                                m_ArcReleaseTokens[i].ReleaseToken(this);
                            }

                            // now traverse the arc, changing state and calling appropriate callbacks as needed

                            // case 1: this arc stays within the same state, only call the arc's callback
                            if (m_ArcFromIDs[i] == m_ArcToIDs[i]) {
                                if (m_Debug) {
                                    Debug.Log("Traversing arc " + ArcToString(i) + " and calling OnTrigger callback(s)");
                                }
                                m_ArcListeners[i].InvokeWithVREvent(vrEvent);
                            }

                            // case 2: this arc causes the FSM to change states, call state exit/enter callbacks as well
                            else {
                                if (m_Debug) {
                                    Debug.Log("Exiting state " + m_StateNames[m_CurrentState] + " and calling OnExit callback(s)");
                                }
                                m_StateExitCBs[m_CurrentState].Invoke();

                                if (m_Debug) {
                                    Debug.Log("Traversing arc " + ArcToString(i) + " and calling OnTrigger callback(s)");
                                }
                                m_ArcListeners[i].InvokeWithVREvent(vrEvent);
                                m_CurrentState = m_ArcToIDs[i];

                                if (m_Debug) {
                                    Debug.Log("Entering state " + m_StateNames[m_CurrentState] + " and calling OnEnter callback(s)");
                                }
                                m_StateEnterCBs[m_CurrentState].Invoke();
                            }
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (enabled) {
                m_StateUpdateCBs[m_CurrentState].Invoke();
            }
        }

        public string StateToString(int id) {
            return m_StateNames[id];
        }

        public string ArcToString(int id) {
            return m_StateNames[m_ArcFromIDs[id]] + "-->" + m_StateNames[m_ArcToIDs[id]];
        }

        public void StartListening()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        public void StopListening()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }


        // CONFIGURABLE VIA THE EDITOR

        // id of the state to start in
        [SerializeField] private int m_StartState = 0;

        // state data table
        [SerializeField] private List<string> m_StateNames = new List<string>();
        [SerializeField] private List<VRCallback> m_StateEnterCBs = new List<VRCallback>();
        [SerializeField] private List<VRCallback> m_StateUpdateCBs = new List<VRCallback>();
        [SerializeField] private List<VRCallback> m_StateExitCBs = new List<VRCallback>();
        
        // arc data table
        [SerializeField] private List<int> m_ArcFromIDs = new List<int>();
        [SerializeField] private List<int> m_ArcToIDs = new List<int>();
        [SerializeField] private List<VREventCallbackAny> m_ArcListeners = new List<VREventCallbackAny>();
        [SerializeField] private List<SharedToken> m_ArcRequireTokens = new List<SharedToken>();
        [SerializeField] private List<SharedToken> m_ArcReleaseTokens = new List<SharedToken>();
        [SerializeField] private List<Condition> m_ArcGuards = new List<Condition>();

        // logs OnEnter(), OnTrigger(), and OnExit() calls 
        [SerializeField] private bool m_Debug = false;
        public bool DebugMode { get => m_Debug; set => m_Debug = value; }


        // RUNTIME ONLY
        private int m_CurrentState;
    }

} // end namespace
