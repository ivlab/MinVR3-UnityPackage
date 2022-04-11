using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Provides a base set of serialization utilities for turning VREvents into JSON and back
    /// </summary>
    public static class VREventSerialization
    {
        /// <summary>
        /// Type map to look up VREvents for JSON serialization/deserialization
        /// (this could also be achieved by using System.Reflection, but direct
        /// lookup should be significantly faster).
        /// </summary>
        /// <remarks>
        /// This typemap needs to be updated whenever a new VREvent type is added!!
        /// </remarks>
        public static Dictionary<string, System.Type> VREventTypes = new Dictionary<string, System.Type>
        {
            { "VREventFloat", typeof(VREventFloat) },
            { "VREventInt", typeof(VREventInt) },
            { "VREventVector2", typeof(VREventVector2) },
            { "VREventVector3", typeof(VREventVector3) },
            { "VREventQuaternion", typeof(VREventQuaternion) },
        };

        /// <summary>
        /// Serialize a VREvent of any type to JSON. Serialized JSON takes the following form (for example):
        /// <pre> <code>
        /// {
        ///     "eventType": "VREventVector2",
        ///     "eventData": "{\"m_Name\": \"Testing\", \"m_Data\": { \"x\": 0.1, \"y\": 0.2}
        /// }
        /// </code></pre>
        /// </summary>
        /// <remarks>
        /// Unfortunately, this necessates double-serialization because of the types.
        /// </remarks>
        public static string ToJson(in VREvent evt)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Formatting = Formatting.None;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            JsonConvert.DefaultSettings = () => settings;

            string json = JsonConvert.SerializeObject(evt, settings);
            Debug.Log(json);
            return "";
                // BinaryFormatter bf = new BinaryFormatter();
                // using (MemoryStream ms = new MemoryStream()) {
                //     List<VREvent> events = (List<VREvent>)bf.Deserialize(ms);
                //     inputEvents.AddRange(events);
                // }
                
            // VREventWithType evtT = new VREventWithType()
            // {
            //     eventType = evt.GetType().Name,
            //     eventData = JsonUtility.ToJson(evt)
            // };
            // return JsonUtility.ToJson(evtT);
        }

        /// <summary>
        /// Deserialize a VREvent of any type from JSON. Serialized JSON takes the following form (for example):
        /// <pre> <code>
        /// {
        ///     "eventType": "VREventVector2",
        ///     "eventData": "{\"m_Name\": \"Testing\", \"m_Data\": { \"x\": 0.1, \"y\": 0.2}
        /// }
        /// </code></pre>
        /// </summary>
        /// <remarks>
        /// Unfortunately, this necessates double-deserialization because of the types.
        /// </remarks>
        public static VREvent FromJson(string evtStr)
        {
            // VREventVector2 evt = JObject.DeserializeObject<VREventVector2>(evtStr);
            JObject jo = JObject.Parse(evtStr);
            VREventVector2 evt = jo.ToObject<VREventVector2>();
            Debug.Log(evt.name);
            Debug.Log(evt.GetData<Vector2>());
            return null;
            // VREventWithType evtT = JsonUtility.FromJson<VREventWithType>(evtStr);
            // System.Type t = VREventTypes[evtT.eventType];
            // // VREvent evt = JsonUtility.FromJson<VREvent>(evtT.eventData);
            // Debug.Log("NAME: " + evt.name);
            // Debug.Log("DATA: " + evt.GetData<Vector2>());
            // Debug.Log(evt.);
            // Debug.Log(t.GetGenericTypeDefinition());
            // var ctor = t.GetConstructor(new Type[] { typeof(string), t.GetGenericTypeDefinition() });
            // try
            // {
            //     // Get the first constructor (assume this is the one that has a string "type" and the object value)
            //     var ctor = t.GetConstructors()[0];
            //     // ctor.Invoke(new object[] {  })
            //     return default;
            // }
            // catch (IndexOutOfRangeException)
            // {
            //     // Type is not valid
            //     return default;
            // }
        }

        /// <summary>
        /// Packages up a VREvent with its correct type to be serialized/deserialized as
        /// </summary>
        [System.Serializable]
        private class VREventWithType
        {
            [SerializeField]
            public string eventType;
            [SerializeField]
            public string eventData; 
        }
    }
}