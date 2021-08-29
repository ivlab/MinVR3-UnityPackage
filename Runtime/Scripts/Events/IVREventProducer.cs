
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{
    public interface IVREventProducer
    {
        public Dictionary<string, string> GetEventNamesAndTypes();
    }
}