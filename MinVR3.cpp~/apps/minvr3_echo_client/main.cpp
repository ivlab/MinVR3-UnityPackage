
#include <chrono>
#include <iostream>
#include <string>
#include <thread>


#include <minvr3.h>

int main(int argc, char** argv) {
    
    // default MinVR3 Connection server
    std::string ip = "localhost";
    int port = 9034;
    
    // optionally, override defaults with command line options
    if (argc > 1) {
        std::string arg = argv[1];
        if ((arg == "help") || (arg == "-h") || (arg == "-help") || (arg == "--help")) {
            std::cout << "Usage: minvr3_echo_client [ip-address] [port]" << std::endl;
            std::cout << "  * Outputs all events received from a MinVR3 Connection server to stdout" << std::endl;
            std::cout << "  * ip-address defaults to " << ip << std::endl;
            std::cout << "  * port defaults to " << port << std::endl;
            std::cout << "  * Quits if an event named 'Shutdown' is received, or press Ctrl-C" << std::endl;
            exit(0);
        }
        ip = arg;
    }
    if (argc > 2) {
        port = std::stoi(argv[2]);
    }
    
    
    MinVR3Net::Init();
    
    SOCKET server_fd;
    if (MinNet::ConnectTo(ip, port, &server_fd)) {
        bool done = false;
        while (!done) {
            if (MinVR3Net::IsReadyToRead(&server_fd)) {
                VREvent* e = MinVR3Net::ReceiveVREvent(&server_fd);
                std::cout << *e << std::endl;
                if (e->get_name() == "Shutdown") {
                    done = true;
                }
                delete e;
            }
            std::this_thread::sleep_for(std::chrono::milliseconds(100));
        }
        MinNet::CloseSocket(&server_fd);
    }
        
    MinVR3Net::Shutdown();
    return 0;
}
