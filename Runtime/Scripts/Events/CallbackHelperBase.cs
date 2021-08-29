using System;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Abstract base class for zero and one-argument callback functions
    /// </summary>
    [Serializable]
    public abstract class CallbackHelperBase
    {
        /// <summary>
        /// Returns typeof(data).Name or "" if the callback does not take a data parameter.
        /// </summary>
        public abstract string GetDataType();
        public abstract void Reset();
        public abstract void Invoke(VREventInstance e);
    }


} // namespace