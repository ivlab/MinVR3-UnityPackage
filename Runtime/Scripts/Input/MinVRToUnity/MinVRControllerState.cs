#if XR_INTERACTION_TOOLKIT_PRESENT || BUILDING_DOCS

using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine;

namespace IVLab.MinVR3 {


    public enum MinVRControllerButton
    {
        PrimaryButton,
        SecondaryButton,
    }

    /// <summary>
    /// State for input device representing a simple VR wand or handheld/worn 6-DOF tracking device, like the custom
    /// 3D pen devices used in the CAVE.  Not as complete (or complex) as Vive/Occulus-style controllers.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 54)]
    public struct MinVRControllerState : IInputStateTypeInfo
    {
        public static FourCC formatId => new FourCC('M', 'N', 'V', 'R');
        public FourCC format => formatId;

        /// <summary>
        /// The primary touchpad or joystick on a device.
        /// </summary>
        [InputControl(usage = "Primary2DAxis", aliases = new[] { "thumbstick", "joystick" }, offset = 0)]
        [FieldOffset(0)]
        public Vector2 primary2DAxis;

        /// <summary>
        /// A trigger-like control, pressed with the index finger.
        /// </summary>
        [InputControl(usage = "PrimaryAxis", layout = "Axis", offset = 8)]
        [FieldOffset(8)]
        public float primaryAxis;

        /// <summary>
        /// All the buttons on this device.
        /// </summary>
        [InputControl(name = nameof(MinVRControllerDevice.primaryButton), usage = "PrimaryButton", layout = "Button", bit = (uint)MinVRControllerButton.PrimaryButton, offset = 12)]
        [InputControl(name = nameof(MinVRControllerDevice.secondaryButton), usage = "SecondaryButton", layout = "Button", bit = (uint)MinVRControllerButton.SecondaryButton, offset = 12)]
        [FieldOffset(12)]
        public ushort buttons;

        [InputControl(usage = "DevicePosition", offset = 26)]
        [FieldOffset(26)]
        public Vector3 devicePosition;

        [InputControl(usage = "DeviceRotation", offset = 38)]
        [FieldOffset(38)]
        public Quaternion deviceRotation;



        /// <summary>
        /// Set the button mask for the given <paramref name="button"/>.
        /// </summary>
        /// <param name="button">Button whose state to set.</param>
        /// <param name="state">Whether to set the bit on or off.</param>
        /// <returns>The same <see cref="MinVRControllerState"/> with the change applied.</returns>
        /// <seealso cref="buttons"/>
        public MinVRControllerState WithButton(MinVRControllerButton button, bool state = true)
        {
            var bit = 1 << (int)button;
            if (state)
                buttons |= (ushort)bit;
            else
                buttons &= (ushort)~bit;
            return this;
        }

        /// <summary>
        /// Resets the value of all fields to default or the identity rotation.
        /// </summary>
        public void Reset()
        {
            primary2DAxis = default;
            primaryAxis = default;
            buttons = default;
            devicePosition = default;
            deviceRotation = Quaternion.identity;
        }
    }
}

#endif
