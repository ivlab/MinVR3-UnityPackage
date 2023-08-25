using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections;

namespace IVLab.MinVR3 {

    /// <summary>
    /// This class is the main manager for any MinVR3 application. It's Script Priority is maxed out
    /// to make sure it is the first class that starts up when your application starts.  On startup,
    /// the first thing it will do is enable the VRConfig that you have selected as the "Startup
    /// VRConfig" and disable all other VRConfigs it finds in the scene.  You can set the Startup
    /// VRConfig via the Inspector by modifying its value under the VRConfigManager component that
    /// is attached to the VREngine GameObject.  OR, you can set the Startup VRConfig using command
    /// line arguments.  This is especially useful when running in a cluster mode, when you may have
    /// a run script that starts several instances of the same application but each with a different
    /// VRConfig.
    ///
    /// While your application is running, VREngine's main job is to handle VREvents.
    /// `VREngine.instance.eventManager` acts as the event manager for the whole application.  If
    /// running in cluster mode, VREngine will make sure its eventManager is synched up each frame
    /// with the eventManagers on all of cluster nodes.  In cluster mode, the VREngine also
    /// synchronizes the graphics "SwapBuffers" calls across all nodes so that each node displays
    /// the same frame at exactly the same time.
    /// 
    /// VREngine is implmented using a Singleton that makes sure there is only ever one instance of
    /// the VREngine that exists at a time and that this instance persists across scene loads and
    /// unloads and is not destroyed until the application quits.  You should access this single
    /// instance of the VREngine class via the static `instance` variable, like this:
    /// ```
    /// VREngine.instance
    /// ```
    /// From there, you can access the key classes that VREngine uses to manage the application.  For
    /// example:
    /// ```
    /// VREngine.instance.eventManager
    /// VREngine.instance.configManager
    /// VREngine.instance.roomSpaceOrigin
    /// ```
    /// An important implementation detail is that these key classes are not themselves Singletons,
    /// but since we only ever access them through `VREngine.instance`, they act a bit like singletons
    /// in practice.
    ///
    /// MinVR is designed so that VREngine is the one singleton in the MinVR3 codebase, and the one
    /// `VREngine.instance` that it provides includes critical services like an eventManger,
    /// configManger, etc.  While, technically speaking, one could add additional VREventMangers
    /// or VRConfigManagers to a scene, we can't (today) imagine a situation where that would be
    /// useful.  Generally, when you want to access to a VREventManager, you will want to access the
    /// one being used by the VREngine.
    ///
    /// This design is an example of a Singleton that uses a "Service Provider" pattern.  Singletons
    /// can be super useful, but they are like "bringing out the big guns".  It's a really big design
    /// decision to decide a class should be a Singleton because they are difficult to work with in
    /// Unity's editor, and if the assumption that there will only ever be one instance of the class
    /// ever changes, it usually requires a major refactoring of the code to "undo" the original
    /// decision to make the class a Singleton.  We think MinVR3's approach of using just one VREngine
    /// singleton that provides access to key services implemented in other "regular" classes is a
    /// good "best of both worlds" approach.
    /// </summary>
    [RequireComponent(typeof(VRConfigManager))]
    [RequireComponent(typeof(VREventManager))]
    [DefaultExecutionOrder(VREngine.ScriptPriority)] 
    public class VREngine : Singleton<VREngine>, IVREventProducer
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

        public VRConfigManager configManager {
            get {
                if (m_ConfigManager == null) {
                    m_ConfigManager = GetComponent<VRConfigManager>();
                }
                return m_ConfigManager;
            }
        }

        public RoomSpaceOrigin roomSpaceOrigin {
            get {
                if (m_RoomSpaceOrigin == null) {
                    m_RoomSpaceOrigin = FindObjectOfType<RoomSpaceOrigin>();
                }
                return m_RoomSpaceOrigin;
            }
        }

        private const string DeltaTimeEventName = "MinVR3/DeltaTime";
        /// <summary>
        /// Time event for delta time synchronization across nodes in a network.
        /// Useful for implementing animations on a clustered setup where
        /// `Time.deltaTime` may be different on individual nodes and it's
        /// useful to have a "ground truth" timing event.
        ///
        /// By default, this gets sent in <see cref="VREngine.Update"/> for both
        /// cluster and non-cluster setups.
        /// </summary>
        private static VREventFloat DeltaTimeEvent { get => new VREventFloat(DeltaTimeEventName, Time.deltaTime); }

        /// <summary>
        /// Time event for delta time synchronization across nodes in a network.
        /// Useful for implementing animations on a clustered setup where
        /// `Time.deltaTime` may be different on individual nodes and it's
        /// useful to have a "ground truth" timing event.
        ///
        /// By default, this gets sent in <see cref="VREngine.Update"/> for both
        /// cluster and non-cluster setups.
        /// </summary>
        public static VREventPrototypeFloat DeltaTimeEventPrototype { get => VREventPrototypeFloat.Create(DeltaTimeEventName); }

        private const string ShutdownEventName = "MinVR3/Shutdown";
        /// <summary>
        /// Shutdown event that gets sent when Unity is quitting. This event
        /// gets sent when the user has pressed the play button to "unplay" the
        /// app in editor, or quits the built application. See <see
        /// cref="VREngine.SendShutdownEvent"/>.
        /// </summary>
        private static VREvent ShutdownEvent { get => new VREvent(ShutdownEventName); }
        
        /// <summary>
        /// Shutdown event that gets sent when Unity is quitting. This event
        /// gets sent when the user has pressed the play button to "unplay" the
        /// app in editor, or quits the built application.
        /// </summary>
        public static VREventPrototype ShutdownEventPrototype { get => VREventPrototype.Create(ShutdownEventName); }

        // Interrupt quit to send a shutdown event
        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Application.wantsToQuit += SendShutdownEvent;
        }

        static bool SendShutdownEvent()
        {
            // send a shutdown message
            VREngine.Instance.eventManager.ProcessEvent(ShutdownEvent);
            return true;
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
                        "-vrconfig [name of the VRConfig component to activate]\n");
                    Application.Quit();
                    return;
                }

                // vrconfig
                else if (args[i] == "-vrconfig") {
                    if (args.Length <= i) {
                        throw new Exception("The command line argument -vrconfig must be followed by the name of the config to activate.");
                    }

                    string vrConfigName = args[i + 1];
                    VRConfig vrConfig = configManager.GetConfigByName(vrConfigName);
                    if (vrConfig != null) {
                        configManager.startupConfig = vrConfig;
                    } else {
                        throw new Exception($"Cannot find a VRConfig named '{vrConfigName}'");
                    }
                }

                i++;
            }


            // This looks at all GameObjects with VRConfig scripts attached and disables all of them but the one
            // current VRConfig.
            configManager.EnableStartupVRConfigAndDisableOthers();


            // Load Config Files
            configManager.ParseConfigFiles();


            // Find the Room Space object and cache a reference to it.  Throw an exception if there
            // is more than one.
            RoomSpaceOrigin[] rso = FindObjectsOfType<RoomSpaceOrigin>();
            if (rso.Length == 0) {
                throw new Exception("The scene must include one object with a RoomSpaceOrigin component " +
                    "attached for MinVR to function properly.");
            } else if (rso.Length > 1) {
                throw new Exception("The scene includes more than one object identified as the " +
                    "RoomSpaceOrigin.");
            } else {
                m_RoomSpaceOrigin = rso[0];
            }

            // check to see if the app should run as a node of a cluster, either a client or a server
            ClusterClient[] clientObjs = GameObject.FindObjectsOfType<ClusterClient>();
            if (clientObjs.Length > 1) {
                throw new Exception("Only one ClusterClient object can be active in the Application.");
            } else if (clientObjs.Length > 0) {
                Debug.Log("Running in Cluster Mode: CLIENT");
                m_ClusterNode = clientObjs[0];
            }

            ClusterServer[] serverObjs = GameObject.FindObjectsOfType<ClusterServer>();
            if (serverObjs.Length > 1) {
                throw new Exception("Only one ClusterServer object can be active in the Application.");
            } else if (serverObjs.Length > 0) {
                if (m_ClusterNode != null) {
                    throw new Exception("The application cannot be both a ClusterClient and a ClusterServer.");
                }
                Debug.Log("Running in Cluster Mode: SERVER");
                m_ClusterNode = serverObjs[0];
            }

            if (m_ClusterNode != null) {
                m_ClusterNode.Initialize();
            }

            m_ClusterComState = ClusterCommunicationState.SyncEventsNext;
        }


        // since the DefaultExecutionOrder makes this script run first, Update() is essentially PreUpdate(),
        // i.e., called before any other script's Update method.
        public void Update()
        {
            // GET NEW INPUT EVENTS FROM POLLED INPUT DEVICES
            eventManager.PollInputDevices();

            // Insert the deltaTime event at the beginning of the queue (if not in cluster mode)
            if (m_ClusterNode == null)
            {
                eventManager.QueueEvent(0, DeltaTimeEvent);
            }

            // SYNCHRONIZE INPUT EVENTS SO EACH NODE WILL PROCESS AN IDENTICAL EVENTQUEUE DURING UPDATE()
            if ((m_ClusterNode != null) && (m_ClusterComState == ClusterCommunicationState.SyncEventsNext))
            {
                List<VREvent> queue = eventManager.GetEventQueue();
                m_ClusterNode.SynchronizeInputEventsAcrossAllNodes(ref queue);

                // In cluster mode, only send delta time event if this node is the server
                if (m_ClusterNode.GetType() == typeof(ClusterServer))
                {
                    queue.Insert(0, DeltaTimeEvent);
                }

                eventManager.SetEventQueue(queue);
                m_ClusterComState = ClusterCommunicationState.SwapBuffersNext;
                StartCoroutine(WaitForEndOfFrame());
            }
        }


        private IEnumerator WaitForEndOfFrame()
        {
            yield return (new WaitForEndOfFrame());
            // BLOCK WAITING FOR  THE SIGNAL THAT ALL CLIENTS ARE ALSO READY, THEN SWAPBUFFERS
            if ((m_ClusterNode != null) && (m_ClusterComState == ClusterCommunicationState.SwapBuffersNext)) {
                m_ClusterNode.SynchronizeSwapBuffersAcrossAllNodes();
                m_ClusterComState = ClusterCommunicationState.SyncEventsNext;
            }
        }


        void OnDestroy()
        {
            if (m_ClusterNode != null) {
                m_ClusterNode.Shutdown();
            }
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            return new List<IVREventPrototype>()
            {
                VREventPrototype.Create(ShutdownEvent.GetName()),
                VREventPrototypeFloat.Create(DeltaTimeEvent.GetName())
            };
        }

        // Can reference either a ClusterClient or a ClusterServer
        private IClusterNode m_ClusterNode;

        // When Unity starts up, Update seems to be called twice before we reach the EndOfFrame callback, so we maintain
        // a state variable here to make sure that we don't request events twice before requesting swapbuffers.
        private enum ClusterCommunicationState { SyncEventsNext, SwapBuffersNext }
        private ClusterCommunicationState m_ClusterComState = ClusterCommunicationState.SyncEventsNext;

        // cached access to other components attached to the gameobject
        private VREventManager m_EventManager;
        private VRConfigManager m_ConfigManager;
        private RoomSpaceOrigin m_RoomSpaceOrigin;
    }

} // namespace
