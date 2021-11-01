using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{

    public interface IVREventProducer
    {
        /// <summary>
        /// All event producers must implement this function to tell MinVR the names and
        /// data types for each possible event that can be produced.
        /// </summary>
        /// <returns>List of prototypes of events that can be produced at runtime.</returns>
        public List<IVREventPrototype> GetEventPrototypes();
    }

} // end namespace
