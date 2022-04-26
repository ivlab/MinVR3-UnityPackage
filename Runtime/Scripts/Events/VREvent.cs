using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
            m_DataTypeName = null; // by default, there is no data in a VREvent
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
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", m_Name);
        }

        [SerializeField]
        protected string m_Name;

        [SerializeField]
        protected string m_DataTypeName;

        // Add a mapping or factory for creating VREventXXXX from XXXX data type
        // have a Create(name, ?data) method in this class?
        //  - register factory w/VREventFactory (subfactories)
        //  - possibly also bring in the VREventPrototypeAny type mapping dict here.
        //  - unity serialization :(
    }

} // namespace
