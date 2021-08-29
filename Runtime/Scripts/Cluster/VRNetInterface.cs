using System.Collections;
using System.Collections.Generic;

namespace IVLab.MinVR3 {


    /** Servers and clients both implement this interface, but perform different tasks for each function. */
    public interface VRNetInterface {

        void SynchronizeInputEventsAcrossAllNodes(ref List<VREventInstance> inputEvents);

        void SynchronizeSwapBuffersAcrossAllNodes();

    }

}
