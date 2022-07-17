# MinVR3 Package

## Installation:

### Install via the Unity Package Manager
1. In Unity, open Window -> Package Manager.
2. Click the ```+``` button
3. Select ```Add package from git URL```
4. Paste ```https://github.umn.edu/ivlab-cs/MinVR3-UnityPackage``` for the latest package
  - If you want to access a particular release or branch, you can append ```#<tag or branch>``` at the end, e.g. ```https://github.umn.edu/ivlab-cs/MinVR3-UnityPackage#v0.0.1```
5. To later switch to a new version or branch, just repeat the above steps.


### To Make Changes to the Code
Note: You can do this in the same project where you have previously installed MinVR using the Package Manager steps above.  Placing a local copy of the package in your Assets/ folder essentially disables the version in the Packages/ folder.

1. Open your terminal or Git tool and navigate to your Unity project's main folder.
2. Clone this repository into the Assets folder using this command: ```git clone https://github.umn.edu/ivlab-cs/MinVR3-UnityPackage Assets/MinVR3```


## MinVR's Event System

### Guiding Principles and Observations:

1.  Unity does not support and/or we can do a better job than Unity at supporting: a) Scientific research (rather than gaming industry) XR devices and displays like the Cave, custom input devices with optical tracking and input from hand-wired sensors, etc.  b) Programmers writing sophisticated / innovative / multi-modal 3D user interactions for research where the goal is to explore XR novel interactions rather than employ a toolkit of existing interactions to create a new game or app.
2. We do not want to give up and/or we cannot do a better job than Unity at: a) rapid development using existing assets and packages contributed by the community, b) publishing online tutorials and examples to introduce XR programming in Unity to new programmers, c) rewriting all of the existing UI support in Unity to "do it our way".
3. The landscape of VR software is rapidly changing; stay flexible.

### Resulting Design Decisions:

1. Develop our own event-based VR interaction toolkit, drawing upon code and VR interaction techniques from >20 years of experience.
2. When possible, leverage Unity's built-in support for XR devices, touchscreens, etc. to get input into our system, so we do not need to write new device drivers for keyboard, mouse, new headsets, etc.  Simply, convert the input data Unity can already receive from these devices to our VREvents.  This should make it possible for any input data Unity knows about to be used with MinVR.
3. For devices not supported by Unity, write new drivers and use device drivers we have written in the past, VRPN, etc. to get their input data into our system as VREvents.
4. In general, code our innovative research-style user interfaces using VREvents and our lessons learned over the years rather than what we commonly see with Unity (e.g., XR Toolkit style interfaces).
5. However, make it possible for our system to co-exist with the packages Unity developers continue to improve, like Unity UI and Unity XR Toolkit, so we do not miss out on those developments when they are useful.  A programmer may wish, for example, to code "more traditional" parts of an applications user interface using Unity's XR Interaction Toolkit or to include menus created using Unity's Canvas and UI support, and to mix this with "more research" parts of the application that they prefer to write using MinVR's VREvent system.
6. To use Unity's UI capabilities while still supporting input from custom devices that Unity does not support, use virtual input devices (e.g., MinVR XRController) to feed input from MinVR into Unity's UI system.  This should make it possible for any VREvents MinVR knows about to also be used with native Unity UI widgets (e.g., canvases) and techniques (e.g., XRTK teleport).

### How To:

#### Generating VREvents from the input devices Unity already supports
XR Tracking and Controller Input:
- [Runtime/Scripts/Input/UnityToMinVR/InputActionsToVREvents.cs](InputActionsToVREvents) - Unity XR apps created in the past year or so will use the New Input System with InputActions.  MinVR can translate any InputAction into a corresponding VREvent.
- [Runtime/Scripts/Input/UnityToMinVR/UnityXR.cs](UnityXR) - VREvents can also be created by reading directly from devices supported by the UnityXR system.

Touch Input:
- (Not yet supported by Unity) [Runtime/Scripts/Input/UnityToMinVR/InputActionsToVREvents.cs](New Input System via Input Actions) - Unity's new input system does not yet work for touch on mobile platforms, when it does, the InputActionsToVREvents component should work fine with it.
- [Runtime/Scripts/Input/UnityToMinVR/TouchBuiltin.cs](Legacy Input Module) - Can translate any touch events UnityEngine.Input can read into VREvents.

Input from Phone and Tablet Sensors:
- (Not yet supported by Unity) [Runtime/Scripts/Input/UnityToMinVR/InputActionsToVREvents.cs](New Input System via Input Actions) - Unity's new input system does not yet work for sensor data on mobile platforms, when it does, the InputActionsToVREvents component should work fine with it.
- [Runtime/Scripts/Input/UnityToMinVR/MobileSensors.cs](Legacy Input Module) - Can translate mobile sensor data accessed via UnityEngine.Input into VREvents.

Input from Mouse, Keyboard, Joysticks, Controllers 
- [Runtime/Scripts/Input/UnityToMinVR/InputActionsToVREvents.cs](InputActionsToVREvents) - If already using the New Input System to access tracking or other events, it's easy to also use it to capture keyboard, mouse, and any other input from Unity.
- [Runtime/Scripts/Input/UnityToMinVR/MouseAndKeyboard.cs](Legacy Input Module) - If you don't plan to use Unity's New Input System, you can use this to get mouse and keyboard VREvents from the legacy input module.


#### Generating VREvents from other sources
Look in the [Runtime/Scripts/Input/Devices](Runtime/Scripts/Input/Devices) folder for the latest list of MinVR input device drivers.  These include support for networked VR input using VRPN and TUIO as well as custom drivers for touchscreens, zSpace, and custom sensors.


#### Sending VREvents (back) into Unity's Input System
This should only be done for input from devices that Unity does not already support.  One useful example is to combine VRPN tracking and button events (which cannot be read by Unity's input system) in a virtual, Unity-style XRController.  This makes it possible to use custom VR devices, like those used in the CAVE, with Unity's XR Interaction Toolkit, including it's built-in support for interacting with 2D UIs displayed on Unity world-space Canvases.  Support for this piping of VREvents (backwards) into the Unity Input System is only planned to be supported for the New Input System with the XR Interaction Toolkit.  Technically it is possible with the Legacy Input Module, but it is a fair amount of work, and it may be best to upgrade applications instead of diving into that work. 

- Add Unity's XRInteraction Toolkit package to your project.  This defines the XRController base class.
- Create a new GameObject and add a [Runtime/Scripts/Input/MinVRToUnity/MinVRController.cs](MinVRController) to it.  Adjust its properties to drive the position, rotation, and buttons of the controller using VREvents.
- Use input from this controller to drive interactions (grabbing, teleporting, etc.) supported out of the box with the XR Interaction Toolkit
- And/or, use it to drive interaction with Unity UI (canvases, physics raycasting).  For this, you need to add Unity's XRUIInputModule to a gameobject somewhere in your scene AND you need to add a TrackedDeviceRaycaster component to each Canvas that you wish to interact with.


