using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace IVLab.MinVR3
{

    [Serializable]
    public class FSMArcCallback
    {
        [Serializable]
        public enum DataType
        {
            Void,
            Bool,
            Int,
            Float,
            Vector2,
            Vector3,
            Quaternion,
            InputActionCallbackContext
        }
        public DataType callbackDataType = DataType.Void;

        public UnityEvent callbackVoid;
        public UnityEvent<bool> callbackBool;
        public UnityEvent<int> callbackInt;
        public UnityEvent<float> callbackFloat;
        public UnityEvent<Vector2> callbackVector2;
        public UnityEvent<Vector3> callbackVector3;
        public UnityEvent<Quaternion> callbackQuaternion;
        public UnityEvent<InputAction.CallbackContext> callbackContext;

        public void Invoke(InputAction.CallbackContext context)
        {
            if (callbackDataType == DataType.Void) {
                callbackVoid.Invoke();
            } else if (callbackDataType == DataType.Bool) {
                callbackBool.Invoke(context.ReadValue<bool>());
            } else if (callbackDataType == DataType.Int) {
                callbackInt.Invoke(context.ReadValue<int>());
            } else if (callbackDataType == DataType.Float) {
                callbackFloat.Invoke(context.ReadValue<float>());
            } else if (callbackDataType == DataType.Vector2) {
                callbackVector2.Invoke(context.ReadValue<Vector2>());
            } else if (callbackDataType == DataType.Vector3) {
                callbackVector3.Invoke(context.ReadValue<Vector3>());
            } else if (callbackDataType == DataType.Quaternion) {
                callbackQuaternion.Invoke(context.ReadValue<Quaternion>());
            } else if (callbackDataType == DataType.InputActionCallbackContext) {
                callbackContext.Invoke(context);
            } else {
                Debug.LogError("Invoking FSMDataCallback with unrecognized data type");
            }
        }

        public override string ToString()
        {
            string s = "";

            if (callbackDataType == DataType.Void) {
                if (callbackVoid.GetPersistentEventCount() == 0) {
                    s = "(null)";
                } else {
                    for (int i = 0; i < callbackVoid.GetPersistentEventCount(); i++) {
                        s += callbackVoid.GetPersistentTarget(i).name + "." + callbackVoid.GetPersistentMethodName(i) + "(); ";
                    }
                }
            } else if (callbackDataType == DataType.Bool) {
                if (callbackBool.GetPersistentEventCount() == 0) {
                    s = "(null)";
                } else {
                    for (int i = 0; i < callbackBool.GetPersistentEventCount(); i++) {
                        s += callbackBool.GetPersistentTarget(i).name + "." + callbackBool.GetPersistentMethodName(i) + "(bool); ";
                    }
                }
            } else if (callbackDataType == DataType.Int) {
                if (callbackInt.GetPersistentEventCount() == 0) {
                    s = "(null)";
                } else {
                    for (int i = 0; i < callbackInt.GetPersistentEventCount(); i++) {
                        s += callbackInt.GetPersistentTarget(i).name + "." + callbackInt.GetPersistentMethodName(i) + "(int); ";
                    }
                }
            } else if (callbackDataType == DataType.Float) {
                if (callbackFloat.GetPersistentEventCount() == 0) {
                    s = "(null)";
                } else {
                    for (int i = 0; i < callbackFloat.GetPersistentEventCount(); i++) {
                        s += callbackFloat.GetPersistentTarget(i).name + "." + callbackFloat.GetPersistentMethodName(i) + "(float); ";
                    }
                }
            } else if (callbackDataType == DataType.Vector2) {
                if (callbackVector2.GetPersistentEventCount() == 0) {
                    s = "(null)";
                } else {
                    for (int i = 0; i < callbackVector2.GetPersistentEventCount(); i++) {
                        s += callbackVector2.GetPersistentTarget(i).name + "." + callbackVector2.GetPersistentMethodName(i) + "(Vector2); ";
                    }
                }
            } else if (callbackDataType == DataType.Vector3) {
                if (callbackVector3.GetPersistentEventCount() == 0) {
                    s = "(null)";
                } else {
                    for (int i = 0; i < callbackVector3.GetPersistentEventCount(); i++) {
                        s += callbackVector3.GetPersistentTarget(i).name + "." + callbackVector3.GetPersistentMethodName(i) + "(Vector3); ";
                    }
                }
            } else if (callbackDataType == DataType.Quaternion) {
                if (callbackQuaternion.GetPersistentEventCount() == 0) {
                    s = "(null)";
                } else {
                    for (int i = 0; i < callbackQuaternion.GetPersistentEventCount(); i++) {
                        s += callbackQuaternion.GetPersistentTarget(i).name + "." + callbackQuaternion.GetPersistentMethodName(i) + "(Quaternion); ";
                    }
                }
            } else if (callbackDataType == DataType.InputActionCallbackContext) {
                if (callbackContext.GetPersistentEventCount() == 0) {
                    s = "(null)";
                } else {
                    for (int i = 0; i < callbackContext.GetPersistentEventCount(); i++) {
                        s += callbackContext.GetPersistentTarget(i).name + "." + callbackContext.GetPersistentMethodName(i) + "(InputAction.CallbackContext); ";
                    }
                }
            }

            return s;
        }

    }

} // namespace
