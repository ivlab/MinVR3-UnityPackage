Demonstrates how MinVR events can drive Unity's notion of a XRController.  This
makes it possible to use input coming through the MinVR event system to drive
Unity XRI Toolkit user interfaces.  One feature of the XRI is making it possible
for XRControllers to interact with the traditional 2D Unity UI system with
buttons, sliders, etc. on Canvases.  This sample demonstrates how to do that.

The steps are roughly:
- Make sure there is a XRUIInputModule in the scene.  This is from Unity XRI;
it translates XRController input into something the Unity UI system can
understand. The VRConfig_DesktopVRSimulatorWithXRI prefab includes one of these.
- Add a TrackedDeviceGraphicsRaycaster object to any Canvas that you want to be
able to receive input from XRControllers.  This is also part of the XRI Toolkit.
So far, these steps would be the same if you were wanted to do this without
MinVR.
- To get MinVR to drive a XRController, add a MinVRController component to
a gameobject in the scene.  The VRConfig_DesktopVRSimulatorWithXRI prefab
actually includes two of these.  One for the left hand and one for the right.
You can use this the same way you would use a Unity XRController.  The difference
is that if it were a native Unity controller, the input would come through
Unity's InputActions abstraction or their earlier XRNode abstraction of VR
devices, which only supports the major players (Vive, Quest, etc.).  Here, the
input comes from VREvents, which means the XRController can be driven by any
of the custom devices in the Cave or anything new we think up in the lab.
