using System.Threading;
using UnityEngine;


namespace IVLab.MinVR3
{
    /// <summary>
    /// Inherit from this base class to create a singleton.  Several flavors of singleton are possible to implement in
    /// Unity.  This singleton persists across scene loads/unloads.  It also inherits from MonoBehaviour, so your class
    /// can implememnt the typical Start(), Update(), and other MonoBehaviour methods.  When you access
    /// MyClassName.Instance from a script, the singleton instance of MyClassName will be created "on demand" and
    /// added to the scene hierarchy in the DontDestroyOnLoad section.  Alternatively, if you prefer to have MyClassName
    /// visible in the editor, you can attach your MyClassName script to a GameObject in your scene.  However, you
    /// should only do this in one place in your scene (otherwise it will not be a singleton).  An assertion will fire
    /// if two or more MyClassName objects are found in the scene.  Also, to remain persistent across scene loads, the
    /// Unity imposes a constraing that the GameObject your MyClassName script is attached to must be placed in the
    /// root of the hierarchy.
    ///
    /// This example shows how to create a script that inherets from Singleton.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyClassName : Singleton&lt;MyClassName&gt; { }
    ///
    /// // MyClassName will now be a "Singleton" and can be accessed via .Instance or .GetInstance()
    /// public class Testing : MonoBehaviour
    /// {
    ///     void Start()
    ///     {
    ///         Debug.Log(MyClassName.Instance.GetHashCode());
    ///     }
    /// }
    /// </code>
    /// </example>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Check to see if we're about to be destroyed.
        private static bool m_ShuttingDown = false;
        private static object m_Lock = new object();
        private static T m_Instance;

        private static Thread mainThread = Thread.CurrentThread;

        public static T GetInstance()
        {
            if (m_ShuttingDown) {
                // Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                //     "' already destroyed. Returning null.");
                return null;
            }

            lock (m_Lock) {
                if (m_Instance == null && mainThread == Thread.CurrentThread) {

                    // Search for existing instance.
                    try {
                        m_Instance = (T)FindFirstObjectByType(typeof(T));
                        if (m_Instance) {
                            // Case 1: Found in the scene, so we do not need to create it, but we do need to make sure
                            // the object the singleton is attached to persists across scene loads/unloads so that it
                            // behaves the same way as the case below.
                            DontDestroyOnLoad(m_Instance.gameObject);
                        }
                    } catch (UnityEngine.UnityException) {
                        Debug.LogError("Make sure you call " + typeof(T).Name + "." + nameof(GetInstance) + "()" + " in some  Unity Thread method, e.g. Awake() or Start()");
                        return default;
                    }

                    // Case 2: Singleton not found, create the instance and add it to the scene.
                    if (m_Instance == null) {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        m_Instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";

                        // Make the new object persist across scene loads/unloads.
                        DontDestroyOnLoad(singletonObject);
                    }

                }
                return m_Instance;
            }
        }


        /// <summary>
        /// Access singleton instance through this propriety.  Note: Both uppercase "Instance" and lowercase "instance"
        /// accessors are provided to support projects that follow typical C# style guidelines where properties begin
        /// with an uppercase letter and the style of Unity's API, where all properties begin with lowercase letters.
        /// </summary>
        public static T Instance {
            get {
                return GetInstance();
            }
        }


        /// <summary>
        /// Access singleton instance through this propriety.  Note: Both uppercase "Instance" and lowercase "instance"
        /// accessors are provided to support projects that follow typical C# style guidelines where properties begin
        /// with an uppercase letter and the style of Unity's API, where all properties begin with lowercase letters.
        /// </summary>
        public static T instance {
            get {
                return GetInstance();
            }
        }


        protected virtual void Awake()
        {
            // Since this style of singleton can be added to a GameObject in the editor, there is nothing to stop
            // programmers from adding multiple within the same scene.  This creates a race condition where the first
            // one to call awake will become the singlton instance and others will be in limbo.  This assertion is
            // critical to make sure programmers avoid this.
            Debug.Assert(m_Instance == null || m_Instance == this,
                $"{this.GetType().Name} is a singleton but more than one instance has been instantiated -- make sure that you only include one instance of the {this.GetType().Name} class in your scene!", this);

            // First chance we get, assign the instance
            mainThread = Thread.CurrentThread;
            GetInstance();
        }

        private void OnApplicationQuit()
        {
            m_ShuttingDown = true;
        }


        private void OnDestroy()
        {
            m_ShuttingDown = true;
        }
    }
}
