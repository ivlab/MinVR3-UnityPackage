#if XR_INTERACTION_TOOLKIT_PRESENT || BUILDING_DOCS

using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;

namespace IVLab.MinVR3
{
    // Virtual input device for Unity's input system to describe a basic VR wand
    [InputControlLayout(stateType = typeof(MinVRControllerState), commonUsages = new[] { "LeftHand", "RightHand" }, isGenericTypeOfDevice = false, displayName = "MinVR Controller"), UnityEngine.Scripting.Preserve]
    public class MinVRControllerDevice : UnityEngine.InputSystem.XR.XRController
    {
        /// The primary touchpad or joystick on a device.
        public Vector2Control primary2DAxis { get; private set; }

        /// An analog (axis) control, like a pressure or force sensitive switch, trigger, grip...
        public AxisControl primaryAxis { get; private set; }

        public ButtonControl primaryButton { get; private set; }

        public ButtonControl secondaryButton { get; private set; }


        protected override void FinishSetup()
        {
            base.FinishSetup();
            primary2DAxis = GetChildControl<Vector2Control>(nameof(primary2DAxis));
            primaryAxis = GetChildControl<AxisControl>(nameof(primaryAxis));
            primaryButton = GetChildControl<ButtonControl>(nameof(primaryButton));
            secondaryButton = GetChildControl<ButtonControl>(nameof(secondaryButton));
        }
    }
}

#endif
