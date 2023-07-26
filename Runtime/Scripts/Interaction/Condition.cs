
using UnityEngine;

namespace IVLab.MinVR3
{

    /// <summary>
    /// Conditions can be used to programmatically restrict the FSM's ability to
    /// transition along an arc unless some condition is met. Subclass from this
    /// class and implement the condition inside this abstract function.  Then, attach
    /// your new class to the desired arc(s).  Then, the arc will only be traversed
    /// when it is triggered AND the condition is true.
    ///
    /// Note that if you want a simple arc guard that does not depend on any custom code
    /// this might be better implmented using the SharedToken scriptable object, which is
    /// useful, for example, to coordinate between FSMs to determine which should have
    /// focus.
    /// </summary>
    [AddComponentMenu("MinVR Interaction/Building Blocks/Condition")]
    public class Condition : MonoBehaviour
    {
        public bool isTrue {
            get => m_IsTrue;
            set => m_IsTrue = value;
        }

        [SerializeField] bool m_IsTrue = false;
    }

} // namespace
