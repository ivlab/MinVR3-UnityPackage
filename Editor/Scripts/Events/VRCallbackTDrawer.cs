using UnityEditor;
using UnityEditorInternal;

namespace IVLab.MinVR3
{

    [CustomPropertyDrawer(typeof(VRCallbackT<>))]
    public class VRCallbackTDrawer : UnityEventDrawer
    {
    }

} // end namespace
