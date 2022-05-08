using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

namespace IVLab.MinVR3 {

    [RequireComponent(typeof(VREventManager))]
    [DefaultExecutionOrder(VREngine.ScriptPriority)] 
    public class VREngine : Singleton<VREngine>
    {
        // In (almost?) all situations, this script should be the first one to run during each frame's
        // Update() phase.  This script must be run before all other MinVR scripts to make sure that
        // VREvents are processed correctly and, if running in a cluster mode, to make sure the events
        // and swapbuffers actions are synchoronized correctly.
        public const int ScriptPriority = -900;

        public VREventManager eventManager {
            get {
                if (m_EventManager == null) {
                    m_EventManager = GetComponent<VREventManager>();
                }
                return m_EventManager;
            }
        }


        // since the DefaultExecutionOrder makes this script run first, the Initialize function will be called before
        // any other script's Start() function.
        public void Start()
        {
            // If specified, pick the VRConfig to use based on the command line argument.
            string[] args = System.Environment.GetCommandLineArgs();
            int i = 1;
            while (i < args.Length) {

                // help command
                if ((args[i] == "-h") || (args[i] == "-help") || (args[i] == "--help")) {
                    Debug.Log("Command Line Arguments:\n" +
                        "-help\n" +
                        "     Display this message.\n" +
                        "-vrconfig [name of the VRConfig component to activate]\n"
                    );
                    i++;
                }

                // vrconfig
                else if (args[i] == "-vrconfig") {
                    if (args.Length <= i) {
                        throw new Exception("The command line argument -vrconfig must be followed by the name of the config to activate.");
                    }
                    string vrConfigName = args[i + 1];
                    VRConfigSelector[] selectors = GameObject.FindObjectsOfType<VRConfigSelector>();
                    foreach (VRConfigSelector cs in selectors) {
                        cs.SelectConfig(vrConfigName);
                    }
                }

                i++;
            }

            // check to see if the app should run as a node of a cluster, either a client or a server
            ClusterClient[] clientObjs = GameObject.FindObjectsOfType<ClusterClient>();
            if (clientObjs.Length > 1) {
                throw new Exception("Only one ClusterClient object can be active in the Application.");
            } else if (clientObjs.Length > 0) {
                m_ClusterNode = clientObjs[0];
            }

            ClusterServer[] serverObjs = GameObject.FindObjectsOfType<ClusterServer>();
            if (serverObjs.Length > 1) {
                throw new Exception("Only one ClusterServer object can be active in the Application.");
            } else if (serverObjs.Length > 0) {
                if (m_ClusterNode != null) {
                    throw new Exception("The application cannot be both a ClusterClient and a ClusterServer.");
                }
                m_ClusterNode = serverObjs[0];
            }

            if (m_ClusterNode != null) {
                m_ClusterNode.Initialize();
            }
        }


        // since the DefaultExecutionOrder makes this script run first, Update() is essentially PreUpdate(),
        // i.e., called before any other script's Update method.
        public void Update()
        {
            // GET NEW INPUT EVENTS FROM POLLED INPUT DEVICES
            eventManager.PollInputDevices();

            // SYNCHRONIZE INPUT EVENTS SO EACH NODE WILL PROCESS AN IDENTICAL EVENTQUEUE DURING UPDATE()
            if ((m_ClusterNode != null) && (m_ClusterComState == ClusterCommunicationState.PreUpdateNext)) {
                List<VREvent> queue = eventManager.GetEventQueue();
                m_ClusterNode.SynchronizeInputEventsAcrossAllNodes(ref queue);
                eventManager.SetEventQueue(queue);
                m_ClusterComState = ClusterCommunicationState.PostRenderNext;
            }
        }


        public void OnPostRender()
        {
            // BLOCK WAITING FOR THE SIGNAL THAT ALL CLIENTS ARE ALSO READY, THEN SWAPBUFFERS
            if ((m_ClusterNode != null) && (m_ClusterComState == ClusterCommunicationState.PostRenderNext)) {
                m_ClusterNode.SynchronizeSwapBuffersAcrossAllNodes();
                m_ClusterComState = ClusterCommunicationState.PreUpdateNext;
            }
        }


        void OnDestroy()
        {
            if (m_ClusterNode != null) {
                m_ClusterNode.Shutdown();
            }
        }



        // Can reference either a ClusterClient or a ClusterServer
        private IClusterNode m_ClusterNode;

        // When Unity starts up, Update seems to be called twice before we reach the EndOfFrame callback, so we maintain
        // a state variable here to make sure that we don't request events twice before requesting swapbuffers.
        private enum ClusterCommunicationState { PreUpdateNext, PostRenderNext }
        private ClusterCommunicationState m_ClusterComState = ClusterCommunicationState.PreUpdateNext;

        // cached access to other components attached to the gameobject
        private VREventManager m_EventManager;
    }

} // namespace
