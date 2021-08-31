# MinVR3 Package

## Installation:

### Non-development (read-only) Package use
1. In Unity, open Window -> Package Manager.
2. Click the ```+``` button
3. Select ```Add package from git URL```
4. Paste ```https://github.umn.edu/ivlab-cs/MinVR3-UnityPackage``` for the latest package
  - If you want to access a particular release or branch, you can append ```#<tag or branch>``` at the end, e.g. ```https://github.umn.edu/ivlab-cs/MinVR3-UnityPackage#v0.0.1```

To switch to a new version or branch, just repeat the above steps.

### Development use in a git-managed project
1. Navigate your terminal or Git tool into your version-controlled Unity project's main folder.
2. Add this repository as a submodule: ```git submodule add https://github.umn.edu/ivlab-cs/MinVR3-UnityPackage Assets/IVLab-MinVR3; git submodule update --init --recursive```
3. See https://git-scm.com/book/en/v2/Git-Tools-Submodules for more details on working with Submodules.

### Development use in a non git-managed project
1. Navigate your terminal or Git tool into your non version-controlled Unity project's main folder.
2. Clone this repository into the Assets folder: ```git clone https://github.umn.edu/ivlab-cs/MinVR3-UnityPackage Assets/IVLab-MinVR3```



# TODO

- implement CavePainting-Lite sample
  - color changing bubble paint

- reincorporate VRDevice and move appropriate VRMain stuff over to the VREngine
- add a ClipboardVR display, maybe it's a prefab
- figure out how best to work with UnityXR, maybe it is simply one type of VRDevice

- implement a "looking out" sample that works well for ClipboardVR

