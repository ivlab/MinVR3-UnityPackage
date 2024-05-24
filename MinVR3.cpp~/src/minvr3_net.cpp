
#include "minvr3_net.h"


bool MinVR3Net::SendVREvent(SOCKET* socket_fd, const VREvent &e, double timeout_ms) {
    std::string json = e.ToJson();
    return SendString(socket_fd, json, timeout_ms);
}

VREvent* MinVR3Net::ReceiveVREvent(SOCKET* socket_fd, double timeout_ms) {
    std::string json;
    if (ReceiveString(socket_fd, &json, timeout_ms)) {
        return VREvent::CreateFromJson(json);
    }
    return NULL;
}

VREventInt* MinVR3Net::ReceiveVREventInt(SOCKET* socket_fd, double timeout_ms) {
    std::string json;
    if (ReceiveString(socket_fd, &json, timeout_ms)) {
        return dynamic_cast<VREventInt*>(VREvent::CreateFromJson(json));
    }
    return NULL;
}

VREventFloat* MinVR3Net::ReceiveVREventFloat(SOCKET* socket_fd, double timeout_ms) {
    std::string json;
    if (ReceiveString(socket_fd, &json, timeout_ms)) {
        return dynamic_cast<VREventFloat*>(VREvent::CreateFromJson(json));
    }
    return NULL;
}

VREventVector2* MinVR3Net::ReceiveVREventVector2(SOCKET* socket_fd, double timeout_ms) {
    std::string json;
    if (ReceiveString(socket_fd, &json, timeout_ms)) {
        return dynamic_cast<VREventVector2*>(VREvent::CreateFromJson(json));
    }
    return NULL;
}

VREventVector3* MinVR3Net::ReceiveVREventVector3(SOCKET* socket_fd, double timeout_ms) {
    std::string json;
    if (ReceiveString(socket_fd, &json, timeout_ms)) {
        return dynamic_cast<VREventVector3*>(VREvent::CreateFromJson(json));
    }
    return NULL;
}

VREventVector4* MinVR3Net::ReceiveVREventVector4(SOCKET* socket_fd, double timeout_ms) {
    std::string json;
    if (ReceiveString(socket_fd, &json, timeout_ms)) {
        return dynamic_cast<VREventVector4*>(VREvent::CreateFromJson(json));
    }
    return NULL;
}

VREventQuaternion* MinVR3Net::ReceiveVREventQuaternion(SOCKET* socket_fd, double timeout_ms) {
    std::string json;
    if (ReceiveString(socket_fd, &json, timeout_ms)) {
        return dynamic_cast<VREventQuaternion*>(VREvent::CreateFromJson(json));
    }
    return NULL;
}

VREventString* MinVR3Net::ReceiveVREventString(SOCKET* socket_fd, double timeout_ms) {
    std::string json;
    if (ReceiveString(socket_fd, &json, timeout_ms)) {
        return dynamic_cast<VREventString*>(VREvent::CreateFromJson(json));
    }
    return NULL;
}
