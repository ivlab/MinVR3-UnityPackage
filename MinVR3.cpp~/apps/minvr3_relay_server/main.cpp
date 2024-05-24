/** MinVR3 VREvent Relay Server
 This simple app acts as a MinVR3 Connection server.  It will accept connections from an unlimited number of clients.
 Whenever a VREvent is received from a client, the event is relayed out to all attached clients.  By default, this includes
 relaying the event "back to" the source client who sent the event.  However, this behavior can be turned off with the
 relay-to-source-client command line option.  The port number and inner-loop sleep milliseconds can also be set on
 the command line.
*/


#include <chrono>
#include <iostream>
#include <thread>

#include <minvr3.h>


int main(int argc, char** argv) {
    // default settings
    int port = 9034;
    bool relay_to_source_client = true;
    int read_write_timeout_ms = 500;
    int sleep_ms = 10;
    
    // optionally, override defaults with command line options
    if (argc > 1) {
        std::string arg = argv[1];
        if ((arg == "help") || (arg == "-h") || (arg == "-help") || (arg == "--help")) {
            std::cout << "Usage: minvr3_relay_server [port] [relay-to-source-client: true/false] [read-write-timeout-ms] [sleep-ms] " << std::endl;
            std::cout << "  * Relays all VREvents received to all connected clients." << std::endl;
            std::cout << "" << std::endl;
            std::cout << "  * port defaults to " << port << std::endl;
            std::cout << "  * relay-to-source-client defaults to " << relay_to_source_client << std::endl;
            std::cout << "  * read-write-timeout-ms defaults to " << read_write_timeout_ms << std::endl;
            std::cout << "  * sleep-ms defaults to " << sleep_ms << std::endl;
            std::cout << "  * Quits if an event named 'Shutdown' is received, or press Ctrl-C" << std::endl;
            exit(0);
        }
        port = std::stoi(argv[1]);
    }
    if (argc > 2) {
        std::string arg = argv[2];
        relay_to_source_client = ((arg == "1") || (arg == "true") || (arg == "True") || (arg == "TRUE"));
    }
    if (argc > 3) {
        read_write_timeout_ms = std::stoi(argv[3]);
    }
    if (argc > 4) {
        sleep_ms = std::stoi(argv[4]);
    }


    std::cout << "MinVR3 Relay Server" << std::endl;
    MinVR3Net::Init();
    
    SOCKET listener_fd;
    if (!MinVR3Net::CreateListener(port, &listener_fd)) {
        exit(1);
    }
    
    std::vector<SOCKET> client_fds;
    std::vector<std::string> client_descs;
    bool shutdown = false;
    while (!shutdown) {
        std::vector<SOCKET> disconnected_fds;
        
        // Accept new connections from any clients trying to connect
        while (MinVR3Net::IsReadyToRead(&listener_fd)) {
            SOCKET new_client_fd;
            if (MinVR3Net::TryAcceptConnection(listener_fd, &new_client_fd)) {
                client_fds.push_back(new_client_fd);
                client_descs.push_back(MinVR3Net::GetAddressAndPort(new_client_fd));
            }
        }
        
        // See if any clients have sent messages
        if (client_fds.size() > 0) {
            std::vector<SOCKET> ready_to_read = MinNet::SelectReadyToRead(client_fds);
            // Read one event from every socket that is ready for a read.
            for (int i=0; i<ready_to_read.size(); i++) {
                // Receive the incoming event from the client
                VREvent* e = MinVR3Net::ReceiveVREvent(&ready_to_read[i], read_write_timeout_ms);
                if (e == NULL) {
                    // If there was a problem receiving, then assume this client disconnected
                    disconnected_fds.push_back(ready_to_read[i]);
                }
                else {
                    // Relay the event out to all clients.
                    for (int j=0; j<client_fds.size(); j++) {
                        if ((relay_to_source_client) || (client_fds[j] != ready_to_read[i])) {
                            bool success = MinVR3Net::SendVREvent(&client_fds[j], *e, read_write_timeout_ms);
                            
                            //std::cout << *e << std::endl;
                            
                            if (!success) {
                                // If there was a problem sending, then assume this client disconnected
                                disconnected_fds.push_back(client_fds[j]);
                            }
                        }
                    }

                    // If the event happened to be named "Shutdown", then we can also shutdown.
                    shutdown = (e->get_name() == "Shutdown") || (e->get_name() == "SHUTDOWN");
                    
                    // Done with the event
                    delete e;
                }
            }
            
            // Remove any disconnected clients from the list
            for (int i=0; i<disconnected_fds.size(); i++) {
                auto it = std::find(client_fds.begin(), client_fds.end(), disconnected_fds[i]);
                if (it != client_fds.end()) {
                    int index = it - client_fds.begin();
                    std::cout << "Dropped connection from " << client_descs[index] << std::endl;

                    // officially close the socket
                    MinVR3Net::CloseSocket(&disconnected_fds[i]);
                    // remove the client from both lists
                    client_fds.erase(client_fds.begin() + index);
                    client_descs.erase(client_descs.begin() + index);
                }
            }
        }
        else {
            std::this_thread::sleep_for(std::chrono::milliseconds(sleep_ms));
        }
    }
                
    
    MinVR3Net::CloseSocket(&listener_fd);
    for (int i=0; i<client_fds.size(); i++) {
        MinVR3Net::CloseSocket(&client_fds[i]);
    }
    MinVR3Net::Shutdown();
    return 0;
}
