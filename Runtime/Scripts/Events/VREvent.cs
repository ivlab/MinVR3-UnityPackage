using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace IVLab.MinVR3
{
    [System.Serializable]
    public class VREvent : ISerializable
    {
        public VREvent(string eventName) : this()
        {
            m_Name = eventName;
        }

        public VREvent()
        {
            m_DataTypeName = ""; // by default, there is no data in a VREvent
        }

        public string name {
            get => m_Name;
            set => m_Name = value;
        }

        public string GetName()
        {
            return m_Name;
        }

        public string GetDataTypeName()
        {
            return m_DataTypeName;
        }

        public T GetData<T>()
        {
            return (this as VREventT<T>).data;
        }

        public bool Matches(IVREventPrototype eventPrototype)
        {
            return (GetName() == eventPrototype.GetEventName()) &&
                (GetDataTypeName() == eventPrototype.GetEventDataTypeName());
        }

        /// <summary>
        /// Does a deep copy of the VREvent, subclasses override this to preserve the correct type of event.
        /// </summary>
        /// <returns></returns>
        public virtual VREvent Clone()
        {
            return new VREvent(m_Name);
        }


        public static VREvent CreateFromJson(string eventJson)
        {
            try
            {
                // Serialize once to get base fields of VREvent, including the type
                VREvent evt = JsonUtility.FromJson<VREvent>(eventJson);

                // PLACE 2 OF 2 TO MODIFY WHEN ADDING A NEW DATA TYPE
                // Convert to the actual type
                // There may be a better way to do this, but at least a switch is fast.
                // Alternative would be to use reflection to convert the type
                // using the s_AvailableDataTypes dictionary.
                switch (evt.GetDataTypeName())
                {
                    case "Vector2":
                        evt = JsonUtility.FromJson<VREventVector2>(eventJson);
                        break;
                    case "Vector3":
                        evt = JsonUtility.FromJson<VREventVector3>(eventJson);
                        break;
                    case "Vector4":
                        evt = JsonUtility.FromJson<VREventVector4>(eventJson);
                        break;
                    case "Quaternion":
                        evt = JsonUtility.FromJson<VREventQuaternion>(eventJson);
                        break;
                    case "GameObject":
                        evt = JsonUtility.FromJson<VREventGameObject>(eventJson);
                        break;
                    case "float":
                        evt = JsonUtility.FromJson<VREventFloat>(eventJson);
                        break;
                    case "int":
                        evt = JsonUtility.FromJson<VREventInt>(eventJson);
                        break;
                    case "string":
                        evt = JsonUtility.FromJson<VREventString>(eventJson);
                        break;
                    default:
                        break;
                }

                return evt;
            }
            catch (System.Exception exc)
            {
                Debug.LogError("Unable to deserialize JSON VREvent message:\n" + exc);
                return null;
            }
        }

        public override string ToString()
        {
            return $"{GetName()} ()";
        }

        /// <summary>
        /// True for raw VREvents created from input devices, trackers, etc.  These need to be sent
        /// across the network to synchronize nodes when running in cluster mode so that all nodes
        /// receive the same input at the same time.  It is also possible to use VREvents for message
        /// passing within an application, and in that case, the data might reference some local object
        /// like a GameObject.  These are considered secondary (i.e., application-specific) events,
        /// and they should not be sent across the network because the reference to a local object
        /// will not transfer to another cluster node running a separate copy of the application.
        /// Instead, secondary events should be regenerated from the primary events by running the same
        /// secondary-event-producing script on each node in the cluster.
        /// </summary>
        public virtual bool IsClusterSafe()
        {
            return true;
        }

        protected VREvent(SerializationInfo info, StreamingContext context) : this()
        {
            m_Name = info.GetString("name");
            m_DataTypeName = info.GetString("dataTypeName");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", m_Name);
            info.AddValue("dataTypeName", m_DataTypeName);
        }

        [SerializeField]
        protected string m_Name;

        [SerializeField]
        protected string m_DataTypeName;

        // PLACE 1 OF 2 TO MODIFY WHEN ADDING A NEW DATA TYPE
        public static Dictionary<string, Type> AvailableDataTypes { get; } = new Dictionary<string, Type>()
        {
            { "", typeof(VREvent) },
            { typeof(Vector2).Name, typeof(VREventVector2) },
            { typeof(Vector3).Name, typeof(VREventVector3) },
            { typeof(Vector4).Name, typeof(VREventVector4) },
            { typeof(Quaternion).Name, typeof(VREventQuaternion) },
            { typeof(float).Name, typeof(VREventFloat) },
            { typeof(int).Name, typeof(VREventInt) },
            { typeof(string).Name, typeof(VREventString) },
            { typeof(GameObject).Name, typeof(GameObject) },
        };
    }

} // namespace
