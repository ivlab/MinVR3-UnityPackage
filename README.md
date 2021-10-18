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

- deploy to Android tablet
  - detect/test multi-touch input
  - add a ClipboardVR display, maybe it's just a prefab that has two ProjectionScreen components
  - implement a "looking out" terrain model or other scene that works well for ClipboardVR

- deploy to Cave
  - test cluster framework



What are u trying to accomplish?
- want to be able to create FSMs in code, requries adding callbacks that are UnityAction<T>
- learning more about how to deal with System.Type, might be able to use templates for eventrefs rather than strings -- more robust, efficient
- vreventcallback is pretty messy...
  - looks like you can't avoid having a nodata version and a templated version, but maybe can
    avoid needing to predefine all the possible templates, or use a factory for this.






Notifier

NotifierWithData<T>
  UnityEvent<T>

NotifierWithoutData


eventsource


VREventSignature -- decl
  - Declaration
  - Spec
  - Prototype
  - Pattern
  - Type
VREventInstance -- actual data
VREventNotifier -- UnityEvent<>
  - caller
  - broadcaster
  - transmitter
  - propogator
  - dispatcher
  - 


VREventFactory
VREventPrototype (VREventSignature)
VREvent (VREventInstance)
VREventCallbackList (VREventNotifier)


VREventListener (higher-level, inside Interactions)

