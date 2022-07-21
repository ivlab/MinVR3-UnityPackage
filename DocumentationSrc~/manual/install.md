
# To install MinVR3 in a Unity Project

## Prereqs: SSH access to github.umn.edu and being a member of the IV/LAB Organization on github.umn.edu.
1. Create a [GitHub SSH key](https://docs.github.com/en/github-ae@latest/github/authenticating-to-github/connecting-to-github-with-ssh/generating-a-new-ssh-key-and-adding-it-to-the-ssh-agent) for your UMN GitHub account on your development machine.  Unity has trouble sshing with passwords; just leave the password for this key blank.
2. If you cannot see the [IV/LAB Organization on github.umn.edu](https://github.umn.edu/ivlab-cs), then ask the [Current Lab GitHub and Software Development Czar](https://docs.google.com/document/d/1p3N2YOQLKyyNpSSTtALgtXoB3Tchy4BVgEEbAG6KYfg/edit?skip_itp2_check=true&pli=1) to please add you to the org.

## To use the package in a read-only mode, the same way you would for packages downloaded directly from Unity
1. In Unity, open Window -> Package Manager.
2. Click the ```+``` button
3. Select ```Add package from git URL```
4. Paste ```git@github.umn.edu:ivlab-cs/MinVR3-UnityPackage.git``` for the latest package
5. Repeat steps 2-4 for each of these required dependencies:
  - REQUIRED: ```git@github.umn.edu:ivlab-cs/IVLab-Utilities-UnityPackage.git```


## Optional Packages

6. Repeat steps 2-4 for any of these optional dependencies IF you would like to use the functionality they provide.  Note: The logic for these optional dependencies is contained in the MinVR3/Runtime/IVLab.MinVR3.Runtime.asmdef file, which you can see/edit within the Unity editor by clicking on the filename in the Project view.  If MinVR finds the optional packages within the solution, it sets a #define of the form PACKAGENAME_PRESENT.  All of the MinVR code that relates to the package is written inside #if PACKAGENAME_PRESENT ... #endif blocks.  This is a nice way to implement optional dependencies in that MinVR automatically compiles and works without the optional functionality when the package is not found, and MinVR automatically includes the functionality that depends on the package whenever the package is installed.

| Package Name | Functionality / Notes | git repo |
|--------------|-----------------------|----------|
| Sensel       | Reads data from a [Sensel Morph](https://morph.sensel.com/) pressure-sensitive multi-touch devices and translates input to VREvents. | ```git@github.umn.edu:ivlab-cs/Sensel-UnityPackage.git``` |
| TUIO11       | Reads touch data sent over a network connection via the TUIO protocol.  [TUIO servers and simulators are available for many touch devices and platforms.](https://www.tuio.org/?software) | ```git@github.umn.edu:ivlab-cs/TUIO11-UnityPackage.git``` |
| WebSocket    | Makes it possible to use the scripts in Scripts/Connection to make a MinVR Unity program talk (send/receive VREvents) with a webpage. | ```git@github.umn.edu:ivlab-cs/WebSocket-UnityPackage.git``` |
| XR Interaction Toolkit | Makes it possible to use Unity's XR Interaction Toolkit and New Event System side-by-side with MinVR.  VREvents can be combined into a MinVRController that implements the XRController interface that the XR Interaction Toolkit expects.  AND, input from any XRControllers that Unity knows about can also be converted into VREvents. | Install the XR Interaction Toolkit package from Unity. |
| zCore6       | Provides support for zSpace input and display devices via their zCore 6.0 API.  Note: The API (and hence this support) only works on Unity 2019. | ```git@github.umn.edu:ivlab-cs/zCore6-UnityPackage.git``` |


## To switch to development mode so you can edit code within the package
Note: Collectively, the lab now recommends a development process where you start by adding the package to your project in read-only mode, as described above.  This way, your Unity project files will always maintain a link to download the latest version of the package from git whenever the project is loaded, and all users of the package will be including it the same way.  If/when you have a need to edit the package, the process is then to "temporarily" switch into development mode by cloning a temporary copy of the package, then edit the source as needed, test your edits for as long as you like, etc.  Finally, when you get to a good stopping point, commit and push the changes to github.  Once the latest version of your package is on github, you can then switch out of development mode.  This will cause Unity to revert to using the read-only version of the package, and since Unity knows where to access this on github, it is easy to tell Unity to use the latest available version.

0. Follow the read-only mode steps above.
1. Navigate your terminal or Git tool into your Unity project's main folder and clone this repository into the packages folder, e.g., ```cd Packages; git clone git@github.umn.edu:ivlab-cs/MinVR3-UnityPackage.git MinVR3```.  This will create a MinVR3 folder that contains all the sourcecode in the package.
2. Go for it.  Edit the source you just checked out; add files, etc.  However, BE VERY CAREFUL NOT TO ADD THE MinVR3-UnityPackage FOLDER TO YOUR PROJECT'S GIT REPO.  We are essentially cloning one git repo inside another here, but we do not want to add the package repo as a submodule or subdirectory of the project's repo, we just want to temporarily work with the source.
3. When you are ready to commit and push changes to the package repo, go for it.  JUST MAKE SURE YOU DO THIS FROM WITHIN THE Packages/MinVR3-UnityPackage DIRECTORY!  
4. Once these changes are up on github, you can switch out of "development mode" by simply deleting the MinVR3-UnityPackage directory.  The presence of that directory is like a temporary override.  Once it is gone, Unity will revert back to using the cached version of MinVR3 that it originally downloaded from git.
5. The final step is to force a refresh of the package cache so that you can pull in the new version of the package you just saved to github.  To do this, simply delete the [package-lock.json](https://docs.unity3d.com/Manual/upm-conflicts-auto.html) file inside your project's Packages folder.
