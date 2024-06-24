# :computer: Relay Server First Time Setup
- Ensure you have [CMake](https://cmake.org/download/) and [Visual Studio](https://visualstudio.microsoft.com/) or XCode installed
- Clone this repo
- Then on the Terminal or Git Bash, `cd` to this directory first, then run these commands:
```
mkdir build
cd build
cmake-gui .. &
```
- Click **Configure**. Choose the compiler available on your computer, then click **Finish**. Then click **Generate**. Then click **Open Project**
- In the opened Visual Studio window, on the Solution Explorer sidebar, expand the **Apps** entry. Then right click on **minvr_relay_server** and choose **Set as Startup Project**

# Run the Relay Server
- Determine the computer on which to run the Relay Server. It can be the same computer as the one that runs your Unity project.
- Open the **MinVR3.cpp** project in Visual Studio. If you've built the project with CMake following the steps above, the project can be found in **MinVR3.cpp~\build\MinVR3.sln**
- Click Build/Run

**Note:** When running your app to network multiple users, only one computer should run the Relay Server.