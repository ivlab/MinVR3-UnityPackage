
This sample program is a simple example of using VREventConnections to share events between
multiple MinVR3 apps.  See the ClusterTest sample for similar functionality but with
strict frame-level synchronization of events and rendering for use with tiled displays.
In contrast, this sample is for when you want to share events between apps that do not
require that tight synchronization.

## Driver and Viewers

This example is based on the idea of a simple approach to a multi-user environment where
one user is designated the "driver" and other users are "viewers".  Each user will have
control over their own head movement, but the DH and NDH events from the "driver" are
sent across a VREventConnection to the viewers.  So, they are not able to directly control
the app themselves.  They simply see what the driver does as if the driver's hands are
their hands.  


## Running the Sample

To run this sample, you need to start at least two instances of the app.  You can start one of those
instances by pressing the Play button in the Unity Editor if you want, but the Unity Editor
will only allow you to open one instance of the same Project at a time.  So, you need to
actually build this sample in order to test it.  The steps are:

- File > Build Settings...
- Optionally adjust any settings on this screen or by clicking the Player Settings... button 
  at the bottom.  For example, under Player Settings > Resolution and Presentation, you will
  probably want to select "Windowed" rather than "Fullscreen" for this test, so that you can
  see both instances of the app at the same time.
- Click Build, set the destination to a new folder named "Build" inside this directory, and
  set the name of the app to "TcpConnectionTest" with whatever the default extension (.app, .exe, or
  nothing) is correct for your platform.
- In this directory, you will find several run scripts:
    ```
    run-win.bat
    run-osx
    run-linux
    ```
- Open up a shell or double-click the appropriate one.  These scripts should start one instance
  of the app that acts as the driver.  Then, wait two seconds.  Then, start some additional
  instance(s) of the app that act as viewers.

## Try it out!

- The sample uses MinVR3's typical simulated trackers, so you should see one of the brush
  or cube cursors move in the scene when you move the mouse and you should be able to
  switch between which cursor you are controlling by pressing `1` or `2` on the keyboard.
- The thing to notice is that when you move the tracker in the "driver" window, it should move in
  the exact same way in the viewer window(s).
- However, if you use the arrow keys on the keyboard to move the head tracker, you should see
  that each "user" has their own head position.

## Running from the Editor

- If you use the run scripts to start the sample, they will automatically start multiple instances,
  but you can easily open up those text files to see the command line to start each instance.
- If you want, you can just run one of those lines to start one instance and start the other
  from the Unity editor.  Be careful to make sure that one instance starts in
  `VRConfig_Driver` mode and the other(s) start in `VRConifg_Viewer` mode.  You will see in
  the scripts how this can be controlled with command line arguments.  In the editor, this
  is controlled by selecting a default VRConfig from the drop-down list in the VREngine's
  VRConfigManager component.  Pro tip: When you set that default in the Editor, it becomes
  the default for the Built app.  So, if you run the built app with no command line arguments
  it will start with whatever config was set as the default in the Editor at the time you
  built the app.

