
#include <string>
#include <iostream>

#include <minvr3.h>

int main() {
    const std::string ip = "localhost";
    const int port = 9034;
    
    MinVR3Net::Init();
    
    SOCKET server_fd;
    if (MinNet::ConnectTo(ip, port, &server_fd)) {
        int counter = 0;
        std::string msg_str;
        while (counter < 99) {
            VREventInt* e_int = MinVR3Net::ReceiveVREventInt(&server_fd);
            VREventString* e_str = MinVR3Net::ReceiveVREventString(&server_fd);
            std::cout << "received: " << (*e_int) << " " << (*e_str) << std::endl;
            counter = e_int->get_data();
            VREventString e_str2("MY_STRING", "Hello server!");
            std::cout << "sending: " << *e_int << " " << e_str2 << std::endl;
            MinVR3Net::SendVREvent(&server_fd, *e_int);
            MinVR3Net::SendVREvent(&server_fd, e_str2);
            delete e_int;
            delete e_str;
        }
        MinNet::CloseSocket(&server_fd);
    }
        
    MinVR3Net::Shutdown();
    return 0;
}

