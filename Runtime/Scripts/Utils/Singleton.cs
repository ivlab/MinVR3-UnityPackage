using System.Threading;
using UnityEngine;


namespace IVLab.MinVR3
{
    /// <summary>
    /// Inherit from this base class to create a singleton.
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
            if (m_ShuttingDown)
            {
                // Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                //     "' already destroyed. Returning null.");
                return null;
            }

            lock (m_Lock)
            {
                if (m_Instance == null && mainThread == Thread.CurrentThread)
                {

                    // Search for existing instance.
                    try
                    {
                        m_Instance = (T)FindObjectOfType(typeof(T));
                    }
                    catch (UnityEngine.UnityException)
                    {
                        Debug.LogError("Make sure you call " + typeof(T).Name + "." + nameof(GetInstance) + "()" + " in some  Unity Thread method, e.g. Awake() or Start()");
                        return default;
                    }

                    // Create new instance if one doesn't already exist.
                    if (m_Instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        m_Instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";

                        // Make instance persistent.
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return m_Instance;
            }
        }
        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static T Instance
        {
            get
            {
                return GetInstance();
            }
        }

        protected virtual void Awake()
        {
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
