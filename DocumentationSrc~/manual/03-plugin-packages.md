# MinVR3 Plugin Packages

These packages integrate with the main MinVR3 package to provide additional functionality, typically to support input or display devices that require external dependencies.  All of these plugins are installed via the Unity Package Manager.  The installation instructions are the same as for the main MinVR3 package, only the name of the package git URL changes:

    1. In Unity, open Window -> Package Manager.
    2. Click the ```+``` button
    3. Select ```Add package from git URL```
    4. Paste ```git@github.umn.edu:ivlab-cs/PackageName-UnityPackage.git```, where "PackageName" will change for each package listed below.

## Available Packages

| Package Name | Functionality / Notes | git repo |
|--------------|-----------------------|----------|
| VRPN | Provides bindings for Virtual Reality Peripheral Network (VRPN) connections (Trackers, Buttons, and Analogs). This is useful for connecting with input devices across the network; for example, an external tracking system like OptiTrack or input buttons like the Wixel. | `git@github.umn.edu:ivlab-cs/MinVR3Plugin-VRPN` |
| TUIO11       | Reads touch data sent over a network connection via the TUIO protocol.  [TUIO servers and simulators are available for many touch devices and platforms.](https://www.tuio.org/?software) | ```git@github.umn.edu:ivlab-cs/MinVR3Plugin-TUIO11.git``` |
| WebSocket    | Makes it possible to use the scripts in Scripts/Connection to make a MinVR Unity program talk (send/receive VREvents) with a webpage. | ```git@github.umn.edu:ivlab-cs/MinVR3Plugin-WebSocket.git``` |
| XR Interaction Toolkit | Makes it possible to use Unity's XR Interaction Toolkit and New Event System side-by-side with MinVR.  VREvents can be combined into a MinVRController that implements the XRController interface that the XR Interaction Toolkit expects.  AND, input from any XRControllers that Unity knows about can also be converted into VREvents. | `git@github.umn.edu:ivlab-cs/MinVR3Plugin-XRIToolkit` |
| Sensel       | Reads data from a [Sensel Morph](https://morph.sensel.com/) pressure-sensitive multi-touch devices and translates input to VREvents. | ```git@github.umn.edu:ivlab-cs/MinVR3Plugin-Sensel.git``` |
| zCore6       | Provides support for zSpace input and display devices via their zCore 6.0 API. | ```git@github.umn.edu:ivlab-cs/MinVR3Plugin-zCore6.git``` |
| NDI | Enables graphics streaming via the NDI protocol (https://ndi.com) | `git@github.umn.edu:ivlab-cs/MinVR3Plugin-NDI` |
| DigiStar | Provides a means to render Unity content into the [DigiStar](https://www.es.com/digistar/) software used at the Bell Museum planetarium. THIS PACKAGE CONTAINS PROPRIETARY CODE, DO NOT SHARE OUTSIDE THE LAB. | `git@github.umn.edu:ivlab-cs/MinVR3Plugin-DigiStar` |


## Under the Hood

TODO 2023-07-10: This section is out of date but still a cool feature in Unity

Some key logic for making these packages "optional" is contained in the MinVR3/Runtime/IVLab.MinVR3.Runtime.asmdef file, which you can see/edit within the Unity editor by clicking on the filename in the Project view.  Notice that each of the packages is listed as what we will call a "soft-dependency" of the main Runtime package.  Also, notice the rules toward the bottom of the asmdef file.  When Unity finds that a the package is available, it sets a #define of the form PACKAGENAME_PRESENT.  If the package is not found, PACKAGENAME_PRESENT is left undefined.  Code inside the main MinVR3 package can then be compiled conditionally depending upon the presence of a certain package by placing it inside an #if PACKAGENAME_PRESENT ... #endif block.  In this way, the main MinVR package compiles regardless of whether any of the optional packages are present, but functionality is limited to the features supported by installed packages.
