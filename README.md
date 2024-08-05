# MinVR3 for Unity
This is the main MinVR3 library for Unity distributed as a Unity package.


# Installation

## Use Unity's package manager to add MinVR3 to your project
1. Open your Unity project.
2. Go to Window -> Package Manager.
3. Click the ```+``` button on the top left
4. Select ```Add package from git URL```
5. Paste ```https://github.com/ivlab/MinVR3-UnityPackage.git``` for the latest package
6. Repeat steps 2-4 for any MinVR3 Plugins IF you would like to use the functionality they provide.

## Additional functionality from plugins
MinVR3 provides support for special input/output devices and other functionality that requires external dependencies with separate add-on libraries that we call "MinVR3Plugins".  Technically, these are just additional Unity packages.  Conceptually, we think of them as plugins because they all depend upon and add functionality to this main MinVR3 library.  

The best way to find the current list of available plugins is to bookmark this URL, which will search github for repos in the IV/LAB organization that include MinVR3 in their name:
[MinVR3 Plugins on GitHub.com](https://github.com/orgs/ivlab/repositories?q=MinVR3).

If you are a member of the IV/LAB, you may also be able to access some non-public MinVR3 plugins via this link, which searches our organization on UMN's Enterprise version of GitHub:
[MinVR3 Plugins on github.umn.edu](https://github.umn.edu/orgs/ivlab-cs/repositories?q=minvr3&type=all&language=&sort=).


# Modifying / Adding to this main MinVR3 library

## Switch to development mode so you can edit code within the package
Important Background: Collectively, the lab now recommends a development process where you start by adding the package to your project in read-only mode, as described above.  This way, your Unity project files will always maintain a link to download the latest version of the package from github whenever the project is loaded, and both users and developers of the package will include it the same way.  If/when you have a need to edit the package, the process is then to "temporarily" switch into development mode by cloning a temporary copy of the package, then edit the source as needed, test your edits for as long as you like, etc.  Finally, when you get to a good stopping point, commit and push the changes to github.  Once the latest version of your package is on github, you can then switch out of development mode.  This will cause Unity to return to using the read-only version of the package available on github, and we can easily tell Unity to re-download the latest version of the package.

After installing the library as described in the Installation section, follow this process:

1. Navigate your terminal or Git tool into your Unity project's main folder and clone this repository into the packages folder, e.g., ```cd Packages; [git clone git@github.umn.edu:ivlab-cs/MinVR3-UnityPackage.git MinVR3](https://github.com/ivlab/MinVR3-UnityPackage.git)```.  This will create a MinVR3-UnityPackage folder that contains all the sourcecode in the package.
2. Go for it.  Edit the source you just checked out; add files, etc.  However, BE VERY CAREFUL NOT TO ADD THE MinVR3-UnityPackage FOLDER TO YOUR PROJECT'S GIT REPO.  We are cloning one git repo inside another here, but we do NOT want to add the package repo as a submodule or subdirectory of the project's repo, we just want to temporarily work with the source.  If you like, you can help prevent yourself from accidentally doing this by editing your project's .gitignore file.  You can add specific directories to ignore or adding this line will tell it to ignore all subdirs of Packages: ```/[Pp]ackages/*/```. 
3. When you are ready to commit and push changes to the package repo, go for it.  JUST MAKE SURE YOU DO THIS FROM WITHIN THE Packages/MinVR3-UnityPackage DIRECTORY!  
4. Once these changes are up on github, you can switch out of "development mode" by simply deleting the MinVR3-UnityPackage directory.  The presence of that directory is like a temporary override.  Once it is gone, Unity will revert back to using the cached version of MinVR3 that it originally downloaded from git.
5. The final step is to force a refresh of the package cache to pull in the new version of the package you just saved to github.  To do this, simply delete the [package-lock.json](https://docs.unity3d.com/Manual/upm-conflicts-auto.html) file from your project's Packages folder, open or return to an already open Unity Editor window, and wait for Unity to compile the new source.


# Modifying / Writing a new MinVR3 Plugin

## When to create a plugin vs. adding to the main library?
The logic here is simple.  Trules for this repository, which is the "main" MinVR3 library are:

1. It should be able to be added to a new Unity project without requiring additional packages to be installed.
2. The code in this repository should be written entirely by MinVR3 project contributors and maintainers for MinVR3, not including portions or copies of other open source projects.
3. The code should work on Unity versions ranging from 2019 to current.

If your contribution would not violate any of these rules, then add it directly to this main MinVR3 library.  If it would violate any of the rules, then create a plugin.

## How to create a plugin?
The IV/LAB has created a [github template repo for creating new UnityPackages](https://github.com/ivlab/Template-UnityPackage).  This main repo and all of the MinVR3 plugins started from this template.  Creating a new repo from this template is probably the best way to start.  Then, you may want to look at or copy example files from other MinVR3 plugins that demonstrate how the plugin can add items to the MinVR3 GameObject and Component menus.  TODO: It would be great to create a second github template repo that is specific for MinVR3 plugins--it would only have minor changes relative to the general unitypackage template, but still useful to setup as a template.

## Is there any way for the main MinVR3 library to know whether a particular plugin has been installed?
Try to avoid this if possible, but if it is absolutely necessary, there is a way to make code in the main MinVR3 library function differently depending on whether a particular plugin is available or not.   To implement this logic, you need to examine the MinVR3/Runtime/IVLab.MinVR3.Runtime.asmdef file, which you can see/edit within the Unity editor by clicking on the filename in the Project view.  Here, you can add identify another ".asm assembly bundle" (the C# code bundle from some other package) as an optional dependency and add a C# compiler definition of the form #define PACKAGENAME_PRESENT whenever that assembly bundle is available.  Then, all of the MinVR code that requires this optional dependency can be added to the main library as long as it is contained entirely within #if PACKAGENAME_PRESENT ... #endif blocks.


# MinVR's Design Philosophy

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



# Tips for Working with VREvents

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


