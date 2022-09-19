using UnityEngine;

namespace IVLab.MinVR3
{

    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class InfoBoxAttribute : PropertyAttribute
    {
        public string message;

        public InfoBoxAttribute(string message)
        {
            this.message = message;
        }
    }

}
