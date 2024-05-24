
#ifndef MINVR3_MINVR3_NET_H
#define MINVR3_MINVR3_NET_H

#include "min_net.h"
#include "vr_event.h"


/** Extends the MinNet class to send/receive VREvent types.
 */
class MinVR3Net : public MinNet {
public:
    /// use this function to send all types of vrevents
    static bool SendVREvent(SOCKET* socket_fd, const VREvent &e, double timeout_ms=0);

    /// this function can receive any type of vrevent but you will need to cast the event created to the appropriate type if
    /// the event has a data payload and you want to access its data
    static VREvent* ReceiveVREvent(SOCKET* socket_fd, double timeout_ms=0);
    /// these functions receive a particular type of vrevent, so there is no need to cast the return type yourself
    static VREventInt* ReceiveVREventInt(SOCKET* socket_fd, double timeout_ms=0);
    static VREventFloat* ReceiveVREventFloat(SOCKET* socket_fd, double timeout_ms=0);
    static VREventVector2* ReceiveVREventVector2(SOCKET* socket_fd, double timeout_ms=0);
    static VREventVector3* ReceiveVREventVector3(SOCKET* socket_fd, double timeout_ms=0);
    static VREventVector4* ReceiveVREventVector4(SOCKET* socket_fd, double timeout_ms=0);
    static VREventQuaternion* ReceiveVREventQuaternion(SOCKET* socket_fd, double timeout_ms=0);
    static VREventString* ReceiveVREventString(SOCKET* socket_fd, double timeout_ms=0);
};

#endif
