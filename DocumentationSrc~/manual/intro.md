# MinVR3 Unity Package
This in the 3rd incarnation of "MinVR", a VR (really XR) open source toolkit designed to support research in XR, spatial user interfaces, pen and touch interfaces, and 3D data visualization.

### High-Level Design Goals
The high-level goals are to:
- Take advantage of recent commercially available hardware and game engine support for XR while also supporting custom VR displays and environments (e.g., CAVE's, PowerWalls, multi-touch stereoscopic tables, 3DTV's, head-mounted displays) and input devices (e.g., 6 degree-of-freedom trackers, multi-touch input devices, haptic devices, home-built devices).
- Facilitate exploration and development of novel spatial user interfaces and input/display devices for XR through coding styles that mirror the way researchers often write about novel spatial interactions (e.g., event-based finite state machines).
- Provide a robust, flexible, cross-platform toolkit for use by researchers.

### History
Some of these goals are shared with [the earlier versions of MinVR](https://github.com/MinVR), which are based on C++, OpenGL graphics.  

What has changed recently is that our research now almost exclusively uses the Unity game engine with C# scripting.  MinVR2 included some support for Unity via a [UnityClient](https://github.com/MinVR/MinVRUnity) that made it possible to combine a graphics application written in Unity with a C++ application, used as an event server for connecting to devices and synchronizing rendering in clusters.  

MinVR3 simply embraces Unity.  It drops explicit support for C++ graphics engines and adds support for native Unity/C# device drivers, networking code, and VR environment configuration.
