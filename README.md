# MinVR3 Package



# To install MinVR3 in a Unity Project

## Prereqs: SSH access to github.umn.edu and being a member of the IV/LAB Organization on github.umn.edu.
1. Create a [GitHub SSH key](https://docs.github.com/en/github-ae@latest/github/authenticating-to-github/connecting-to-github-with-ssh/generating-a-new-ssh-key-and-adding-it-to-the-ssh-agent) for your UMN GitHub account on your development machine.  Unity has trouble sshing with passwords; just leave the password for this key blank.
2. If you cannot see the [IV/LAB Organization on github.umn.edu](https://github.umn.edu/ivlab-cs), then ask the [Current Lab GitHub and Software Development Czar](https://docs.google.com/document/d/1p3N2YOQLKyyNpSSTtALgtXoB3Tchy4BVgEEbAG6KYfg/edit?skip_itp2_check=true&pli=1) to please add you to the org.

## To use the package in a read-only mode, the same way you would for packages downloaded directly from Unity
1. In Unity, open Window -> Package Manager.
2. Click the ```+``` button
3. Select ```Add package from git URL```
4. Paste ```git@github.umn.edu:ivlab-cs/MinVR3-UnityPackage.git``` for the latest package
5. Repeat steps 2-4 for each of these additional dependencies:
  - REQUIRED: ```git@github.umn.edu:ivlab-cs/IVLab-Utilities-UnityPackage.git```
  - (Optional, only needed if WebSocket support is desired) ```git@github.umn.edu:ivlab-cs/WebSocket-UnityPackage.git```
  - (Optional, only needed if Sensel support is desired) ```git@github.umn.edu:ivlab-cs/Sensel-UnityPackage.git```
  - (Optional, only needed if zSpace support is desired) ```git@github.umn.edu:ivlab-cs/zCore6-UnityPackage.git```
  - (Optional, only needed if TUIO support is desired) ```git@github.umn.edu:ivlab-cs/TUIO11-UnityPackage.git```

## To switch to development mode so you can edit code within the package
Note: Collectively, the lab now recommends a development process where you start by adding the package to your project in read-only mode, as described above.  This way, your Unity project files will always maintain a link to download the latest version of the package from git whenever the project is loaded, and all users of the package will be including it the same way.  If/when you have a need to edit the package, the process is then to "temporarily" switch into development mode by cloning a temporary copy of the package, then edit the source as needed, test your edits for as long as you like, etc.  Finally, when you get to a good stopping point, commit and push the changes to github.  Once the latest version of your package is on github, you can then switch out of development mode.  This will cause Unity to revert to using the read-only version of the package, and since Unity knows where to access this on github, it is easy to tell Unity to use the latest available version.

0. Follow the read-only mode steps above.
1. Navigate your terminal or Git tool into your Unity project's main folder and clone this repository into the packages folder, e.g., ```cd Packages; git clone git@github.umn.edu:ivlab-cs/MinVR3-UnityPackage.git MinVR3```.  This will create a MinVR3 folder that contains all the sourcecode in the package.
2. Go for it.  Edit the source you just checked out; add files, etc.  However, BE VERY CAREFUL NOT TO ADD THE MinVR3-UnityPackage FOLDER TO YOUR PROJECT'S GIT REPO.  We are essentially cloning one git repo inside another here, but we do not want to add the package repo as a submodule or subdirectory of the project's repo, we just want to temporarily work with the source.
3. When you are ready to commit and push changes to the package repo, go for it.  JUST MAKE SURE YOU DO THIS FROM WITHIN THE Packages/MinVR3-UnityPackage DIRECTORY!  
4. Once these changes are up on github, you can switch out of "development mode" by simply deleting the MinVR3-UnityPackage directory.  The presence of that directory is like a temporary override.  Once it is gone, Unity will revert back to using the cached version of MinVR3 that it originally downloaded from git.
5. The final step is to force a refresh of the package cache so that you can pull in the new version of the package you just saved to github.  To do this, simply delete the [package-lock.json](https://docs.unity3d.com/Manual/upm-conflicts-auto.html) file inside your project's Packages folder.




# MinVR's Event System

## Guiding Principles and Observations:

1.  Unity does not support and/or we can do a better job than Unity at supporting: a) Scientific research (rather than gaming industry) XR devices and displays like the Cave, custom input devices with optical tracking and input from hand-wired sensors, etc.  b) Programmers writing sophisticated / innovative / multi-modal 3D user interactions for research where the goal is to explore XR novel interactions rather than employ a toolkit of existing interactions to create a new game or app.
2. We do not want to give up and/or we cannot do a better job than Unity at: a) rapid development using existing assets and packages contributed by the community, b) publishing online tutorials and examples to introduce XR programming in Unity to new programmers, c) rewriting all of the existing UI support in Unity to "do it our way".
3. The landscape of VR software is rapidly changing; stay flexible.

## Resulting Design Decisions:

1. Develop our own event-based VR interaction toolkit, drawing upon code and VR interaction techniques from >20 years of experience.
2. When possible, leverage Unity's built-in support for XR devices, touchscreens, etc. to get input into our system, so we do not need to write new device drivers for keyboard, mouse, new headsets, etc.  Simply, convert the input data Unity can already receive from these devices to our VREvents.  This should make it possible for any input data Unity knows about to be used with MinVR.
3. For devices not supported by Unity, write new drivers and use device drivers we have written in the past, VRPN, etc. to get their input data into our system as VREvents.
4. In general, code our innovative research-style user interfaces using VREvents and our lessons learned over the years rather than what we commonly see with Unity (e.g., XR Toolkit style interfaces).
5. However, make it possible for our system to co-exist with the packages Unity developers continue to improve, like Unity UI and Unity XR Toolkit, so we do not miss out on those developments when they are useful.  A programmer may wish, for example, to code "more traditional" parts of an applications user interface using Unity's XR Interaction Toolkit or to include menus created using Unity's Canvas and UI support, and to mix this with "more research" parts of the application that they prefer to write using MinVR's VREvent system.
6. To use Unity's UI capabilities while still supporting input from custom devices that Unity does not support, use virtual input devices (e.g., MinVR XRController) to feed input from MinVR into Unity's UI system.  This should make it possible for any VREvents MinVR knows about to also be used with native Unity UI widgets (e.g., canvases) and techniques (e.g., XRTK teleport).

## How To:

### Generate VREvents from the input devices Unity already supports
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


### Generate VREvents from other sources
Look in the [Runtime/Scripts/Input/Devices](Runtime/Scripts/Input/Devices) folder for the latest list of MinVR input device drivers.  These include support for networked VR input using VRPN and TUIO as well as custom drivers for touchscreens, zSpace, and custom sensors.


### Send VREvents (back) into Unity's Input System
This should only be done for input from devices that Unity does not already support.  One useful example is to combine VRPN tracking and button events (which cannot be read by Unity's input system) in a virtual, Unity-style XRController.  This makes it possible to use custom VR devices, like those used in the CAVE, with Unity's XR Interaction Toolkit, including it's built-in support for interacting with 2D UIs displayed on Unity world-space Canvases.  Support for this piping of VREvents (backwards) into the Unity Input System is only planned to be supported for the New Input System with the XR Interaction Toolkit.  Technically it is possible with the Legacy Input Module, but it is a fair amount of work, and it may be best to upgrade applications instead of diving into that work. 

- Add Unity's XRInteraction Toolkit package to your project.  This defines the XRController base class.
- Create a new GameObject and add a [Runtime/Scripts/Input/MinVRToUnity/MinVRController.cs](MinVRController) to it.  Adjust its properties to drive the position, rotation, and buttons of the controller using VREvents.
- Use input from this controller to drive interactions (grabbing, teleporting, etc.) supported out of the box with the XR Interaction Toolkit
- And/or, use it to drive interaction with Unity UI (canvases, physics raycasting).  For this, you need to add Unity's XRUIInputModule to a gameobject somewhere in your scene AND you need to add a TrackedDeviceRaycaster component to each Canvas that you wish to interact with.


