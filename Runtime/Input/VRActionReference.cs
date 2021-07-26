using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VRActionReference
    {
        public VRActionReference()
        {
            inputActionPhase = InputActionPhase.Performed;
        }

        public VRActionReference(string name, InputActionPhase phase)
        {
            inputActionName = name;
            inputActionPhase = phase;
        }

        public bool Matches(InputAction.CallbackContext context)
        {
            return (VRInput.ActionToString(context.action, false) == inputActionName) &&
                (context.phase == inputActionPhase);
        }

        public string inputActionName;
        public InputActionPhase inputActionPhase;
    }

}
