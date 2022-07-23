# VREvent System

## Events, Prototypes, and Aliases

- Types of VREvents
- Accessing data carried by events
- VREventPrototypes
- VREventAliases


## Listening for VREvents

- VREventCallbacks
- SimpleEventListener


## Advanced Topics

### Event Filters


### Injecting "Derived Events" into the Queue
- example VREventType_GameObject


### Sending VREvents (back) to Unity's Input System
This should only be done for input from devices that Unity does not already support.  One useful example is to combine VRPN tracking and button events (which cannot be read by Unity's input system) in a virtual, Unity-style XRController.  This makes it possible to use custom VR devices, like those used in the CAVE, with Unity's XR Interaction Toolkit, including it's built-in support for interacting with 2D UIs displayed on Unity world-space Canvases.  Support for this piping of VREvents (backwards) into the Unity Input System is only planned to be supported for the New Input System with the XR Interaction Toolkit.  Technically it is possible with the Legacy Input Module, but it is a fair amount of work, and it may be best to upgrade applications instead of diving into that work.

- Add Unity's XRInteraction Toolkit package to your project.  This defines the XRController base class.
- Create a new GameObject and add a Runtime/Scripts/Input/MinVRToUnity/MinVRController.cs MinVRController  to it.  Adjust its properties to drive the position, rotation, and buttons of the controller using VREvents.
- Use input from this controller to drive interactions (grabbing, teleporting, etc.) supported out of the box with the XR Interaction Toolkit
- And/or, use it to drive interaction with Unity UI (canvases, physics raycasting).  For this, you need to add Unity's XRUIInputModule to a gameobject somewhere in your scene AND you need to add a TrackedDeviceRaycaster component to each Canvas that you wish to interact with.

### Adding a VREvent type that carries a new type of Data
