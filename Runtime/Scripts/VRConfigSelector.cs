using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IVLab.MinVR3 {

    /*  You should define one or more VRDevices in your application for MinVR to use by attaching a VRDevice
     *  component to one or more GameObjects.  When initialized, the VREngine will activate exactly one of these
     *  VRDevices and deactivate all others.  This acts like a switch, so you can attach anything that should only
     *  run on one VRDevice to that device.  You can tell VREngine which VRDevice to activate with a command line
     *  argument, a critical feature for starting multiple instances of the same app for each wall of a Cave, for
     *  example.  You can also provide a default VRDevice by tagging its GameObject with the tag "DefaultVRDevice",
     *  however, if "-vrdevice DeviceName" is specified on the command line, it will override this.
     *  
     *  A recommend strategy is to create a hierarchy of GameObjects that looks like something like this:
     *
     *  + VRDevices: (parent empty GameObject used only to keep things organized)
     *    + Desktop (attach a VRDevice component to all of these GameObjects)
     *    + 3DTV
     *    + Powerwall
     *    + CaveLeftWall
     *    + CaveRightWall
     *    + CaveFrontWall
     *    + CaveFloor
     *    ...
     *
     *  Attach a VRDevice component to each one and adjust the settings as needed.  Also, add additional scripts
     *  needed for each device, like FakeTrackerInput for testing with the Desktop mode, TrackedProjectionScreen
     *  for the various walls, etc.  When your app starts up, just one of the devices will be activated, like a
     *  switch.
     */
    [AddComponentMenu("MinVR/VRConfig Selector")]
    public class VRConfigSelector : MonoBehaviour {

        public string[] GetConfigNames()
        {
            string[] childNames = new string[transform.childCount];
            for (int i = 0; i < transform.childCount; i++) {
                childNames[i] = transform.GetChild(i).gameObject.name;
            }
            return childNames;
        }

        public void SelectConfig(string name)
        {
            int count = 0;
            for (int i = 0; i < transform.childCount; i++) {
                if (name == transform.GetChild(i).gameObject.name) {
                    count++;
                    transform.GetChild(i).gameObject.SetActive(true);
                } else {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            if (count == 0) {
                throw new System.Exception($"Config named {name} could not be found.");
            } else if (count > 1) {
                throw new System.Exception($"More than one config named {name} was found.");
            }
        }

    }

} // namespace
