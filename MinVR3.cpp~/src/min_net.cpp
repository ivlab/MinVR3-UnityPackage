
#include "min_net.h"

#include <chrono>
#include <iostream>

#ifdef WIN32
#include <winsock2.h>
#include <windows.h>
#include <stdint.h>
#include <ws2tcpip.h>
#pragma comment (lib, "Ws2_32.lib")
#pragma comment (lib, "Mswsock.lib")
#pragma comment (lib, "AdvApi32.lib")
#else
#define SOCKET int
#include "stdint.h"
#include <unistd.h>
#include <errno.h>
#include <string.h>
#include <netdb.h>
#include <sys/types.h>
#include <netinet/in.h>
#include <netinet/tcp.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#endif


bool MinNet::Init() {
#ifdef WIN32
    WSADATA wsaData;
    int iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (iResult != 0) {
        std::cerr << "WSAStartup failed with error: " << iResult << std::endl;
        return false;
    }
#endif
    return true;
}


bool MinNet::Shutdown() {
#ifdef WIN32
    WSACleanup();
#endif
    return true;
}


bool MinNet::ConnectTo(const std::string &ip, int port, SOCKET *socket_fd) {
    std::string port_str = std::to_string(port);
    *socket_fd = INVALID_SOCKET;

    struct addrinfo hints;
    memset(&hints, 0, sizeof hints);
    hints.ai_family = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;

    // getaddrinfo returns a linked list of results.
    struct addrinfo *server_addresses;
    int err = getaddrinfo(ip.c_str(), port_str.c_str(), &hints, &server_addresses);
    if (err != 0) {
        std::cerr << "MinNet::ConnectTo() Error: Could not obtain server addrinfo, error code: " << err << std::endl;
        return false;
    }
    
    // calc the number of addresses returned, should be > 0 since the getaddrinfo above reported success
    struct addrinfo *cur_addr = server_addresses;
    int n_addr = 0;
    while (cur_addr != NULL) {
        cur_addr = cur_addr->ai_next;
        n_addr++;
    }
    
    // go to the front of the list and try to connect to the server, if the connection fails, try the
    // next address, and so on.
    cur_addr = server_addresses;
    int n_tried = 0;
    bool connected = false;
    do {
        n_tried++;
        *socket_fd = socket(cur_addr->ai_family, cur_addr->ai_socktype, cur_addr->ai_protocol);
        if (*socket_fd != INVALID_SOCKET) {
            err = connect(*socket_fd, cur_addr->ai_addr, (int)cur_addr->ai_addrlen);
            if (err == 0) {
                connected = true;
            }
            else {
                CloseSocket(socket_fd);
                if (n_tried < n_addr) {
                    //std::cerr << "MinNet::ConnectTo() Warning: Connect refused with error code: " << err
                    //<< ". Will try another server address." << std::endl;
                    cur_addr = cur_addr->ai_next;
                }
                else {
                    std::cerr << "MinNet::ConnectTo() Error: Connect refused with error code: " << err
                    << std::endl;
                    freeaddrinfo(server_addresses);
                    return false;
                }
            }
        }
        else if (n_tried < n_addr) {
            //std::cerr << "MinNet::ConnectTo() Warning: Could not create socket. Will try " <<
            //"another server address." << std::endl;
            cur_addr = cur_addr->ai_next;
        }
    } while ((!connected) && (cur_addr != NULL));
    freeaddrinfo(server_addresses);
    
    if ((*socket_fd == INVALID_SOCKET) || (!connected)) {
        std::cerr << "MinNet::ConnectTo() Error: could not create and connect socket." << std::endl;
        return false;
    }
        
    // Disable Nagle's algorithm
    char value = 1;
    setsockopt(*socket_fd, IPPROTO_TCP, TCP_NODELAY, &value, sizeof(value));

    std::cout << "MinNet::OpenSocket() Connected to " << MinNet::GetAddressAndPort(*socket_fd) << std::endl;
    return true;
}


bool MinNet::CreateListener(int port, SOCKET* socket_fd, int backlog)
{
    std::string port_str = std::to_string(port);
    *socket_fd = INVALID_SOCKET;

    struct addrinfo hints;
    memset(&hints, 0, sizeof hints);
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;
    hints.ai_flags = AI_PASSIVE;

    // getaddrinfo returns a linked list of results
    // node=NULL and ai_flags=AI_PASSIVE will return addresses appropriate for binding
    struct addrinfo *listener_addresses;
    int err = getaddrinfo(NULL, port_str.c_str(), &hints, &listener_addresses);
    if (err != 0) {
        std::cerr << "MinNet::CreateListener() Error: Could not obtain listener addrinfo, error code: " << err << std::endl;
        return false;
    }
    
    // calc the number of addresses returned, should be > 0 since the getaddrinfo above reported success
    struct addrinfo *cur_addr = listener_addresses;
    int n_addr = 0;
    while (cur_addr != NULL) {
        cur_addr = cur_addr->ai_next;
        n_addr++;
    }
    
    // go to the front of the list and try to create and bind a socket, if it fails, try the next address.
    cur_addr = listener_addresses;
    int n_tried = 0;
    bool bound = false;
    do {
        n_tried++;
        *socket_fd = socket(cur_addr->ai_family, cur_addr->ai_socktype, cur_addr->ai_protocol);
        if (*socket_fd != INVALID_SOCKET) {
            const char value = 1;
            setsockopt(*socket_fd, SOL_SOCKET, SO_REUSEADDR, &value, sizeof(value));
            
            err = bind(*socket_fd, cur_addr->ai_addr, (int)cur_addr->ai_addrlen);
            if (err == 0) {
                bound = true;
            }
            else {
                CloseSocket(socket_fd);
                if (n_tried < n_addr) {
                    //std::cerr << "MinNet::CreateListener() Warning: Binding refused with error code: " << err
                    //<< ". Will try another address." << std::endl;
                    cur_addr = cur_addr->ai_next;
                }
                else {
                    std::cerr << "MinNet::CreateListener() Error: Binding refused with error code: " << err
                    << std::endl;
                    freeaddrinfo(listener_addresses);
                    return false;
                }
            }
        }
        else if (n_tried < n_addr) {
            //std::cerr << "MinNet::CreateListener() Warning: Could not create socket. Will try " <<
            //"another server address." << std::endl;
            cur_addr = cur_addr->ai_next;
        }
    } while ((!bound) && (cur_addr != NULL));

    freeaddrinfo(listener_addresses);
    
    if ((*socket_fd == INVALID_SOCKET) || (!bound)) {
        std::cerr << "MinNet::CreateListener() Error: could not create and bind socket." << std::endl;
        return false;
    }
    
    err = listen(*socket_fd, backlog);
    if (err != 0) {
        std::cerr << "MinNet::CreateListener() Error: Could not listen on socket, error code: " << err << std::endl;
        return false;
    }
    
    std::cout << "MinNet::CreateListener() Listening on " << MinNet::GetAddressAndPort(*socket_fd) << std::endl;
    return true;
}


bool MinNet::TryAcceptConnection(const SOCKET listener_fd, SOCKET* client_fd) {
    struct sockaddr_in client_addr;
    socklen_t client_len = sizeof(client_addr);
    *client_fd = accept(listener_fd, (struct sockaddr *) &client_addr, &client_len);
    if (*client_fd == INVALID_SOCKET) {
        std::cerr << "MinNet::TryAcceptConnection() Accept failed." << std::endl;
        return false;
    }
            
    // Disable Nagle's algorithm on the client's socket
    char value = 1;
    setsockopt(*client_fd, IPPROTO_TCP, TCP_NODELAY, &value, sizeof(value));

    std::cout << "MinNet::TryAcceptConnection() Accepted connection from "
        << MinNet::GetAddressAndPort(*client_fd) << std::endl;
    return true;
}


bool MinNet::SendBytes(SOCKET* socket_fd, uint8_t* buf, int len, double timeout_ms) {
    std::chrono::time_point<std::chrono::system_clock> start_time;
    if (timeout_ms != 0) {
        start_time = std::chrono::system_clock::now();
    }
    
    int total = 0;        // how many bytes we've sent
    int bytesleft = len;  // how many we have left to send
    int n = 0;
    while (total < len) {
#ifdef WIN32
        n = send(*socket_fd, (const char*)(buf + total), bytesleft, 0);
#else
        n = send(*socket_fd, (void*)(buf + total), bytesleft, 0);
#endif
        if (n == SOCKET_ERROR) {
            return false;
        }
        total += n;
        bytesleft -= n;
        
        if (timeout_ms != 0) {
            std::chrono::time_point<std::chrono::system_clock> now = std::chrono::system_clock::now();
            double elapsed_ms = std::chrono::duration_cast<std::chrono::milliseconds>(now - start_time).count();
            if (elapsed_ms > timeout_ms) {
                return false;
            }
        }
    }
    return true;
}


bool MinNet::ReceiveBytes(SOCKET* socket_fd, uint8_t* buf, int len, double timeout_ms) {
    std::chrono::time_point<std::chrono::system_clock> start_time;
    if (timeout_ms != 0) {
        start_time = std::chrono::system_clock::now();
    }
    
    int total = 0;        // how many bytes we've received
    int bytesleft = len;  // how many we have left to receive
    int n = 0;
    while (total < len) {
#ifdef WIN32
        n = recv(*socket_fd, (char*)(buf + total), bytesleft, 0);
#else
        n = recv(*socket_fd, (void*)(buf + total), bytesleft, 0);
#endif
        if (n == SOCKET_ERROR) { 
            return false; 
        }
        total += n;
        bytesleft -= n;
        
        if (timeout_ms != 0) {
            std::chrono::time_point<std::chrono::system_clock> now = std::chrono::system_clock::now();
            double elapsed_ms = std::chrono::duration_cast<std::chrono::milliseconds>(now - start_time).count();
            if (elapsed_ms > timeout_ms) {
                return false;
            }
        }
    }
    return true;
}


bool MinNet::SendUInt32(SOCKET* socket_fd, uint32_t i, double timeout_ms) {
    uint8_t buf[4];
    const uint8_t* p = static_cast<const uint8_t*>(static_cast<const void*>(&i));
    if (!is_little_endian()) {
        buf[0] = p[3];
        buf[1] = p[2];
        buf[2] = p[1];
        buf[3] = p[0];
    } else {
        buf[0] = p[0];
        buf[1] = p[1];
        buf[2] = p[2];
        buf[3] = p[3];
    }
    return SendBytes(socket_fd, buf, 4, timeout_ms);
}


bool MinNet::ReceiveUInt32(SOCKET* socket_fd, uint32_t *i, double timeout_ms) {
    uint8_t buf[4];
    bool ok = ReceiveBytes(socket_fd, buf, 4, timeout_ms);
    if (ok) {
        uint8_t* p = static_cast<uint8_t*>(static_cast<void*>(i));
        if (!is_little_endian()) {
            p[0] = buf[3];
            p[1] = buf[2];
            p[2] = buf[1];
            p[3] = buf[0];
        } else {
            p[0] = buf[0];
            p[1] = buf[1];
            p[2] = buf[2];
            p[3] = buf[3];
        }
    }
    return ok;
}


bool MinNet::SendString(SOCKET* socket_fd, const std::string &s, double timeout_ms) {
    return SendUInt32(socket_fd, (uint32_t)s.size(), timeout_ms) && SendBytes(socket_fd, (uint8_t*)s.c_str(), (int)s.size(), timeout_ms);
}


bool MinNet::ReceiveString(SOCKET* socket_fd, std::string *s, double timeout_ms) {
    uint32_t len = 0;
    bool ok = ReceiveUInt32(socket_fd, &len, timeout_ms);
    if (ok) {
        char* buf = new char[len+1];
        if (ReceiveBytes(socket_fd, (uint8_t*)buf, len, timeout_ms)) {
            buf[len] = '\0';
            *s = std::string(buf);
        }
        delete [] buf;
    }
    return ok;
}


bool MinNet::IsReadyToRead(SOCKET* socket_fd) {
    // create a fd set that contains only socket_fd
    fd_set fds;
    FD_ZERO(&fds);
    FD_SET(*socket_fd, &fds);
    struct timeval  tv;
    tv.tv_sec = 0;
    tv.tv_usec = 0;
    SOCKET max_fd = *socket_fd;
    int err = select((int)max_fd + 1, &fds, NULL, NULL, &tv);
    if (err == SOCKET_ERROR) {
        std::cerr << "MinNet::IsReadyToRead() Error: Select failed." << std::endl;
#ifdef WIN32
        std::cerr << "WSAGetLastError() = " << WSAGetLastError() << std::endl;
#else
        std::cerr << "errno = " << errno << std::endl;
#endif
        return false;
    }
    else {
        // true if socket_fd is still in the set after calling select
        return FD_ISSET(*socket_fd, &fds);
    }
}


std::vector<SOCKET> MinNet::SelectReadyToRead(const std::vector<SOCKET> &test_fds) {
    std::vector<SOCKET> ready_fds;
    if (test_fds.size() > 0) {
        // create a fd set that contains all of the socket fds in fds_to_test,
        // and keep track of the max fd
        fd_set fds;
        FD_ZERO(&fds);
        SOCKET max_fd = test_fds[0];
        for (int i=0; i<test_fds.size(); i++) {
            FD_SET(test_fds[i], &fds);
            if (test_fds[i] > max_fd) {
                max_fd = test_fds[i];
            }
        }

        struct timeval  tv;
        tv.tv_sec = 0;
        tv.tv_usec = 0;
        int err = select((int)max_fd + 1, &fds, NULL, NULL, &tv);
        if (err == SOCKET_ERROR) {
            std::cerr << "MinNet::SelectReadyToRead() Error: Select failed." << std::endl;
#ifdef WIN32
            std::cerr << "WSAGetLastError() = " << WSAGetLastError() << std::endl;
#else
            std::cerr << "errno = " << errno << std::endl;
#endif
            return ready_fds;
        }
        
        for (int i=0; i<test_fds.size(); i++) {
            if (FD_ISSET(test_fds[i], &fds)) {
                ready_fds.push_back(test_fds[i]);
            }
        }
    }
    return ready_fds;
}


bool MinNet::CloseSocket(SOCKET *socket_fd) {
#ifdef WIN32
    closesocket(*socket_fd);
#else
    close(*socket_fd);
#endif
    return true;
}


std::string MinNet::GetAddressAndPort(SOCKET socket_fd) {
    struct sockaddr_in addr;
    memset(&addr, 0, sizeof addr);
    socklen_t len = sizeof(addr);
    getsockname(socket_fd, (struct sockaddr *)&addr, &len);
    char ip_str[INET_ADDRSTRLEN];
    inet_ntop(AF_INET, &addr.sin_addr, ip_str, sizeof(ip_str));
    unsigned int port = ntohs(addr.sin_port);
    return std::string(ip_str) + ":" + std::to_string(port);
}
