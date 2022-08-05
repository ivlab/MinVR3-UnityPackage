
This sample program is the simplest example of running MinVR3 in a cluster mode.

## Cluster Server and Clients

In cluster mode, multiple instances of the application are run simultaneously.  One instance
is required to act as a server, and all other instances are clients.

In the sample, there are two VRConfigs, one to start a server, and one to start a client.
Notice the VRNetServer and VRNetClient components attached to these VRConfig objects.

The VRNetServer needs to know how many clients to expect.  In this simple example, it is set
to expect just one client.

The VRNetClient needs to know the IP address for the server.  In this simple example, it is
set to expect the server to be running on the same machine (i.e., 127.0.0.1).

The port listed in the settings for VRNetServer and VRNetClient can be any valid port number,
just be sure it is the same number in both places.


## Running the Sample

To run this sample, you need to start two instances of the app.  You can start one of those
instances by pressing the Play button in the Unity Editor if you want, but the Unity Editor
will only allow you to open one instance of the same Project at a time.  So, you need to
actually build this sample in order to test it.  The steps are:

- File > Build Settings...
- Optionally adjust any settings on this screen or by clicking the Player Settings... button 
  at the bottom.  For example, under Player Settings > Resolution and Presentation, you will
  probably want to select "Windowed" rather than "Fullscreen" for this test, so that you can
  see both instances of the app at the same time.
- Click Build, set the destination to a new folder named "Build" inside this directory, and
  set the name of the app to "ClusterTest" with whatever the default extensin (.app, .exe, or
  nothing) is correct for your platform.
- In this directory, you will find several run scripts:
    ```
    run-win.bat
    run-osx
    run-linux
    ```
- Open up a shell or double-click the appropriate one.  This should start up two instances
  of your newly built application.

## Try it out!

- The sample uses MinVR3's typical simulated trackers, so you should see one of the brush
  or cube cursors move in the scene when you move the mouse and you should be able to
  switch between which cursor you are controlling by pressing `1` or `2` on the keyboard.
- The thing to notice is that when you move the tracker in one window, it should move in
  the exact same way in the other!
- MinVR3 synchronizes every node in the cluster so that they run in lockstep:
  - At the beginning of each frame, all of the VREvents produced from input devices attached
    to *any of* the nodes in the cluster are combined into a single list that is sychronized
    across the entire cluster.  This way, every node in the cluster will process the same
    VREvents each frame.
  - At the end of each frame and *just before that frame is displayed on the screen*, each
    node in the cluster sends a signal saying "I'm ready to display my new graphics".  The
    server coordinates these and waits until every node is ready before sending the signal
    to everyone at once "Ok, go ahead and display the next frame".  This way, if one node
    happens to be displaying a part of the scene that only includes a few triangles and
    another node needs to render a huge model that will take longer to draw, the nodes stay
    in sync and only go as fast as the slowest node.

## Running from the Editor

- If you use the run scripts to start the sample, they will automatically start two instances,
  but you can easily open up those text files to see the command line to start each instance.
- If you want, you can just run one of those lines to start one instance and start the other
  from the Unity editor.  Be careful to make sure that one instance starts in
  `VRConfig_Server` mode and the other starts in `VRConifg_Client` mode.  You will see in
  the scripts how this can be controlled with command line arguments.  In the editor, this
  is controlled by selecting a default VRConfig from the drop-down list in the VREngine's
  VRConfigManager component.  Pro tip: When you set that default in the Editor, it becomes
  the default for the Built app.  So, if you run the built app with no command line arguments
  it will start with whatever config was set as the default in the Editor at the time you
  built the app.

