using UnityEngine.Events;
using System;

namespace IVLab.MinVR3
{

    [Serializable]
    public class CallbackHelperNoData : CallbackHelperBase
    {
        public UnityEvent onVREvent;

        public override void Reset()
        {
            onVREvent = null;
        }

        public override string GetDataType()
        {
            return "";
        }

        public override void Invoke(VREventInstance e)
        {
            onVREvent?.Invoke();
        }
    }

} // namespace
