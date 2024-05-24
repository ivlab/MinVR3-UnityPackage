
#include <chrono>
#include <iostream>
#include <thread>

#include <minvr3.h>


int main(int argc, char** argv) {
    const int port = 9034;
    std::vector<SOCKET> client_fds;

    MinVR3Net::Init();
    
    SOCKET listener_fd;
    MinVR3Net::CreateListener(port, &listener_fd);
    
    
    std::cout << "Waiting 5 seconds for one or more client(s) to connect..." << std::endl;
    unsigned int counter = 5;
    while (counter > 0) {
        if (MinVR3Net::IsReadyToRead(&listener_fd)) {
            SOCKET new_client_fd;
            if (MinVR3Net::TryAcceptConnection(listener_fd, &new_client_fd)) {
                client_fds.push_back(new_client_fd);
            }
        }
        std::this_thread::sleep_for(std::chrono::milliseconds(1000));
        std::cout << counter << std::endl;
        counter--;
    }
    
    if (client_fds.size() == 0) {
        std::cout << "Nobody connected, exiting." << std::endl;
        return 0;
    }

    counter = 0;
    while (counter < 100) {
        for (int i=0; i<client_fds.size(); i++) {
            VREventInt e_int("MY_INT", counter);
            VREventString e_str("MY_STRING", "Hello client!");
            std::cout << "sending to client " << client_fds[i]
                << " " << e_int << " " << e_str << std::endl;
            MinVR3Net::SendVREvent(&client_fds[i], e_int);
            MinVR3Net::SendVREvent(&client_fds[i], e_str);
        }

        // this is overkill. for such a simple program we could just loop through the client_fds
        // in order and read from each one. however, the reads are blocking functions, so this
        // approach would only work well if all the clients are running at the same speed.  in
        // other words, if client[0] happened to be really slow, the server would wait, spinning
        // its wheels, to read from client[0] even if messages have already come in from
        // client[1], [2], [3], ...  the right way to handle this situation in network programming
        // is to use a select() call where the OS queries a set of possible fds and returns the
        // subset that are currently ready to read.  this loop demonstrates how to do that in MinNet.
        std::vector<SOCKET> read_from = client_fds;
        while (read_from.size() > 0) {
            std::vector<SOCKET> ready_to_read = MinNet::SelectReadyToRead(read_from);
            for (int i=0; i<ready_to_read.size(); i++) {
                VREventInt* e_int = MinVR3Net::ReceiveVREventInt(&ready_to_read[i]);
                VREventString* e_str = MinVR3Net::ReceiveVREventString(&ready_to_read[i]);
                std::cout << "client " << ready_to_read[i] << " replies with " << *e_int << " " << *e_str << std::endl;
                delete e_int;
                delete e_str;
                read_from.erase(std::find(read_from.begin(), read_from.end(), ready_to_read[i]));
            }
        }
                
        counter++;
    }
        
    
    MinVR3Net::CloseSocket(&listener_fd);
    for (int i=0; i<client_fds.size(); i++) {
        MinVR3Net::CloseSocket(&client_fds[i]);
    }
    MinVR3Net::Shutdown();
    return 0;
}
