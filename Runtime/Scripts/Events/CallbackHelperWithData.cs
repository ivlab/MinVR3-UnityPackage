using UnityEngine.Events;
using System;

namespace IVLab.MinVR3
{

    [Serializable]
    public class CallbackHelperWithData<T> : CallbackHelperBase
    {
        public UnityEvent<T> onVREvent;

        public override void Reset()
        {
            onVREvent = null;
        }

        public override string GetDataType()
        {
            return typeof(T).Name;
        }

        public override void Invoke(VREventInstance e)
        {
            onVREvent?.Invoke(((VREventInstance<T>)e).data);
        }
    }

} // namespace
