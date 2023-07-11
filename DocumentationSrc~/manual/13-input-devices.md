# Input Devices

Classes that connect to hardware input devices or their software drivers to capture user input should implement the @IVLab.MinVR3.IVREventProducer interface in order to send @IVLab.MinVR3.VREvent\s to MinVR's @IVLab.MinVR3.VREventManager.

**For devices already supported by Unity**, we recommend simply creating a virtual input device that converts input from Unity to VREvents.  As you see in the list below, these have already been developed for most of the devices handled by Unity, and it is extend these existing scripts or add new ones.

**For devices not supported by Unity**, new support can be added by writing a class that reads data from the device and implements the @IVLab.MinVR3.IVREventProducer interface in order to send @IVLab.MinVR3.VREvent\s to MinVR's @IVLab.MinVR3.VREventManager.  If the device requires an external library or code that will only compile on some platforms, then implement the driver as a [MinVR plugin package](03-plugin-packages.md) so that it does not add a required external dependency to the main MinVR package.


## Devices to Support Various Types of Input

#### Misc
@IVLab.MinVR3.InputActionsToVREvents [Requires: New Input System] - This class converts Unity InputActions to VREvents.  It should work with any input devices that Unity can convert to InputActions, which will eventually be everything Unity supports.  Right now, there are a lot of devices that are not yet supported, like the Quest and mobile phones and tablets.


#### 6-DOF Trackers, XR Controllers
@IVLab.MinVR3.UnityXR - Converts tracking, button, and axis data from devices supported by Unity's XRInputSubsystem into VREvents.

VRPN [Requires VRPN Plugin Package](03-plugin-packages.md) - Connect to Virtual Reality Peripheral Network (VRPN) devices -- trackers, buttons, and analogs.

@IVLab.MinVR3.zSpace [Requires zSpace Plugin Package, which runs on Unity2019 only] - Support for the zSpace Device.  Converts tracking data for the head and tracking and button data for the 3D pen to VREvents.


#### Touch Input
@IVLab.MinVR3.TouchBuiltin - Coverts touch data from devices supported by UnityEngine.Input into VREvents.

@IVLab.MinVR3.TouchTuio [Requires TUIO11 Plugin Package] - Coverts touch data received over the network from a TUIO server into VREvents.

@IVLab.MinVR3.TouchTuio [Requires Sensel Plugin Package] - Coverts touch data from a Sensel Morph device into VREvents.


#### Phone and Tablet Sensors:
@IVLab.MinVR3.MobileSensors - Converts gyro rotation, heading, and acceleration data from devices supported by UnityEngine.Input into VREvents.


#### Mouse and Keyboard
@IVLab.MinVR3.MouseAndKeyboard - Converts mouse and keyboard input to VREvents.  Automatically switches between accessing the data via the New Input System or the Legacy InputModule based on what is enabled in the current project.
