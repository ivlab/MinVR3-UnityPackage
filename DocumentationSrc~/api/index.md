# API Overview

This page gives an overview of the most important classes in MinVR3.


## VR Engine

| Class | Purpose |
| --- | --- |
| @IVLab.MinVR3.VREngine | Main MinVR3 class responsible for starting up and shutting down the VR scene. The VREngine is a @IVLab.MinVR3.Singleton, meaning there is only ONE instance of the VREngine object in the VR app at any given time and it persists through scene loads. The VREngine contains instance variables for the @IVLab.MinVR3.VREventManager and @IVLab.MinVR3.VRConfigManager. |


## VR Events

### VREventManager

| Class | Purpose |
| --- | --- |
| @IVLab.MinVR3.VREventManager | Maintains the frame-by-frame VR event queue. Scripts can add new VR events so other scripts can use them.


### VR Event Types

Every VREvent has a name, a data type, and optionally data that gets sent along with the event.

| Class | Purpose |
| --- | --- |
| @IVLab.MinVR3.VREvent | VREvent with no data |
| @IVLab.MinVR3.VREventVector2 | VREvent with @UnityEngine.Vector2 data |
| @IVLab.MinVR3.VREventVector3 | VREvent with @UnityEngine.Vector3 data |
| @IVLab.MinVR3.VREventVector4 | VREvent with @UnityEngine.Vector4 data |
| @IVLab.MinVR3.VREventQuaternion | VREvent with @UnityEngine.Quaternion data |
| @IVLab.MinVR3.VREventString | VREvent with @System.String data |
| @IVLab.MinVR3.VREventInt | VREvent with @System.Int32 (int) data |
| @IVLab.MinVR3.VREventFloat | VREvent with @System.Single (float) data |
| @IVLab.MinVR3.VREventGameObject | VREvent with @UnityEngine.GameObject data |


### VREventPrototypes

VREventPrototypes are used when the application is expecting a particular event
but the actual event hasn't been defined yet. For example, an on-screen cursor
might want a @IVLab.MinVR3.VREventVector3 with a name `NDH/Position`. To
"expect" these events, we would create A @IVLab.MinVR3.VREventPrototypeVector3.

| Class | Purpose |
| --- | --- |
| @IVLab.MinVR3.VREventPrototypeAny | Event prototype that can accept ANY type of @IVLab.MinVR3.VREvent |
| @IVLab.MinVR3.VREventPrototype | Event prototype that can accept simple @IVLab.MinVR3.VREvent with no data |
| @IVLab.MinVR3.VREventPrototypeVector2 | Event prototype that can accept @IVLab.MinVR3.VREventVector2 |
| @IVLab.MinVR3.VREventPrototypeVector3 | Event prototype that can accept @IVLab.MinVR3.VREventVector3 |
| @IVLab.MinVR3.VREventPrototypeVector4 | Event prototype that can accept @IVLab.MinVR3.VREventVector4 |
| @IVLab.MinVR3.VREventPrototypeString | Event prototype that can accept @IVLab.MinVR3.VREventString |
| @IVLab.MinVR3.VREventPrototypeInt | Event prototype that can accept @IVLab.MinVR3.VREventInt |
| @IVLab.MinVR3.VREventPrototypeFloat | Event prototype that can accept @IVLab.MinVR3.VREventFloat |
| @IVLab.MinVR3.VREventPrototypeQuaternion | Event prototype that can accept @IVLab.MinVR3.VREventQuaternion |
| @IVLab.MinVR3.VREventPrototypeGameObject | Event prototype that can accept @IVLab.MinVR3.VREventGameObject |


### Event Producers and Listeners

Events can be produced and consumed by any class that uses MinVR3. Implement one
(or both) of the following interfaces:

| Interface | Purpose |
| --- | --- |
| @IVLab.MinVR3.IVREventProducer | Implement this interface to make the events your class produces available to the rest of MinVR3. |
| @IVLab.MinVR3.IVREventListener | Implement this interface to listen for VREvents produced by other classes in MinVR3. |

### Input Devices




## VR Configs

| Class | Purpose |
| --- | --- |
| @IVLab.MinVR3.VRConfigManager | Manages VR configs in the scene and allows the developer to choose which one is active on startup. Handles command line parameter `-vrconfig` to switch VRConfigs after an executable is built. |
| @IVLab.MinVR3.VRConfig | Individual VRConfig script. Attach to a GameObject that should be turned on/off with a particular VRConfig setup. |
| @IVLab.MinVR3.VRConfigMask | Enables or disables a particular GameObject if one of the selected VRConfigs are active. |

## Remote Connections

| Class | Purpose |
| --- | --- |


## Cluster Connections


<!-- "MinVR3"
- VR Coordinate Spaces
  - MinVRRoot (RoomSpace Origin)
- VRConfigs
  - Walkthrough an Example
    - Prefabs combine Display(s) + InputDevices
    - Aliases
- VREvents in Detail
  - VREvents and VREventPrototypes
  - Input Devices and Virtual Input Devices
  - VREvent Processing (Queue, Filters, Aliases)
- Remote Connections
- Cluster Support -->
