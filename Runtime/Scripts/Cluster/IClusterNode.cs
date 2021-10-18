using System.Collections;
using System.Collections.Generic;

namespace IVLab.MinVR3 {


    /** Servers and clients both implement this interface, but perform different tasks for each function. */
    public interface IClusterNode {

        void Initialize();

        void SynchronizeInputEventsAcrossAllNodes(ref List<VREvent> inputEvents);

        void SynchronizeSwapBuffersAcrossAllNodes();

        void Shutdown();

    }

}
