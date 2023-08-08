# API Overview

This page gives an overview of the most important classes in MinVR3.


## VR Engine

| Class | Purpose |
| --- | --- |
| @IVLab.MinVR3.VREngine | Main MinVR3 class responsible for starting up and shutting down the VR scene. The VREngine is a <xref:IVLab.MinVR3.Singleton%601>, meaning there is only ONE instance of the VREngine object in the VR app at any given time and it persists through scene loads. The VREngine contains instance variables for the @IVLab.MinVR3.VREventManager and @IVLab.MinVR3.VRConfigManager. |


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

Input devices are a special type of event producer that gets polled by the
@IVLab.MinVR3.VREventManager every frame. You can implement the
@IVLab.MinVR3.IPolledInputDevice interface to create a new input device for use
in MinVR3.

| Interface | Purpose |
| --- | --- |
| @IVLab.MinVR3.IPolledInputDevice | Implement this interface to create a new input device recognized by MinVR3. |


### Advanced VREvent Queue Manipulation

There are further operations that can be done with VREvents each frame to
support advanced functionality such as modifying events mid-frame, or cloning
and renaming them.

| Topic | Purpose |
| --- | --- |
| @IVLab.MinVR3.IVREventFilter | Processes every event every frame and can modify and/or produce more events from an event. |
| @IVLab.MinVR3.VREventAlias | Can rename or rename-clone an event. Useful for separating out "raw" input devices from "application-level" input. |
| @IVLab.MinVR3.VREventManager.GetEventQueue | Get the current event queue for this frame |
| @IVLab.MinVR3.VREventManager.QueueEvent(IVLab.MinVR3.VREvent) | Insert an event in the queue |
| @IVLab.MinVR3.VREventManager.InsertInQueue(IVLab.MinVR3.VREvent) | Insert a derived event in the queue |



## VR Configs

| Class | Purpose |
| --- | --- |
| @IVLab.MinVR3.VRConfigManager | Manages VR configs in the scene and allows the developer to choose which one is active on startup. Handles command line parameter `-vrconfig` to switch VRConfigs after an executable is built. |
| @IVLab.MinVR3.VRConfig | Individual VRConfig script. Attach to a GameObject that should be turned on/off with a particular VRConfig setup. |
| @IVLab.MinVR3.VRConfigMask | Enables or disables a particular GameObject if one of the selected VRConfigs are active. |


## Remote Connections

MinVR3 can also handle connections to remote Unity clients (and even other languages!).

| Class | Purpose |
| --- | --- |
| @IVLab.MinVR3.IVREventConnection | Implement this interface for sending / receiving events across a particular connection. The connection details are up to the programmer. |
| @IVLab.MinVR3.VREventConnectionReceiver | This class translates events received by a @IVLab.MinVR3.IVREventConnection into @IVLab.MinVR3.VREvent objects and inserts them into MinVR3's Event Queue. |
| @IVLab.MinVR3.VREventConnectionSender | This sends events from MinVR3's event queue to a @IVLab.MinVR3.IVREventConnection |

There are other language clients available as well, see the root of this package:

- MinVR3.js~
- MinVR3.cpp~
- MinVR3.py~


## Cluster Connections

Cluster connections are a special case of remote connections that require a
higher level of synchronization between devices. An example of a clustered
application would be a multi-window app running in the IV/LAB CAVE, where the
graphics on each wall are a separate Unity app.

Clustered setups need to have tight, frame-by-frame synchronization in order to
work correctly, and as such there's a "cluster mode" built into MinVR3. The
cluster mode in MinVR3 supports ONE server and MANY clients. In
general, the process for each frame in a clustered app is:

1. Get events from input devices
2. Synchronize all events across the cluster (generally, the cluster server will simply produce all events and send them to the clients)
3. Wait for all other nodes to be done processing events
4. End the frame

This process ensures that frame timings across the cluster are in lock step (i.e., one app cannot run "faster" than another in the cluster.)

Useful topics for working with clustered apps are below:


| Topic | Purpose |
| --- | --- |
| @IVLab.MinVR3.IClusterNode | Generic node in the cluster (can be server or client) |
| @IVLab.MinVR3.ClusterServer | Cluster server (ONE) |
| @IVLab.MinVR3.ClusterClient | Cluster client (MANY) |
| @IVLab.MinVR3.VREngine.DeltaTimeEventPrototype | Delta time event for the cluster server; useful for implementing frame-based animation accurately accross nodes. |

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


## Extension Methods

Unity's built-in methods are sometimes lackluster (especially for the @UnityEngine.Transform class, for example). We have implemented a bunch of useful methods for the following builtin classes. Check out:

- @IVLab.MinVR3.GameObjectExtensions
- @IVLab.MinVR3.TransformExtensions
- @IVLab.MinVR3.Matrix4x4Extensions


Note that here we are taking the approach that MinVR extension methods are the "surpreme" extensions methods - i.e., they are at the top-level of the `IVLab.MinVR3` namespace. This ensures that the extensions methods are available whenever you do `using IVLab.MinVR3`. Before implementing or downloading your own extension methods for these classes, check if MinVR3 has one first!