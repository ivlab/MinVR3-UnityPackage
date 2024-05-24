/**
  Lightweight cross-platform API for client/server socket-based messaging.  The approach assumes messages
  are serialized in a portable text-based format (e.g., JSON, XML), so just two types of messages are supported:
  1. unsigned integers, 2. strings.  Unsigned ints are good for sending simple codes, e.g., 1 = start routine X,
  2 = start routine Y, 3 = the next message will contain a filename, etc.  Strings are good for sending more
  complex data in a serialized form, e.g., vector3, matrix4, custom data structure.  This class just provides
  a wrapper around the low-level socket programming and leaves the details of which messages to send and
  when up to the application programmer.
 */

#ifndef MINVR3_MINNET_H
#define MINVR3_MINNET_H

#include "net_headers.h"

#include <stdint.h>
#include <string>
#include <vector>


class MinNet {
public:
    // initialize networking -- same for client and server
    static bool Init();

    // server management
    static bool CreateListener(int port, SOCKET* socket_fd, int backlog=10);
    static bool TryAcceptConnection(const SOCKET listener_fd, SOCKET* client_fd);

    // client management
    static bool ConnectTo(const std::string &ip, int port, SOCKET* socket_fd);

    // send messages
    static bool SendUInt32(SOCKET* socket_fd, uint32_t i, double timeout_ms=0);
    static bool SendString(SOCKET* socket_fd, const std::string &s, double timeout_ms=0);

    // receive messages
    static bool IsReadyToRead(SOCKET* socket_fd);
    static std::vector<SOCKET> SelectReadyToRead(const std::vector<SOCKET> &fds_to_test);
    
    static bool ReceiveUInt32(SOCKET* socket_fd, uint32_t* i, double timeout_ms=0);
    static bool ReceiveString(SOCKET* socket_fd, std::string* s, double timeout_ms=0);
    
    // cleanup -- same for client and server
    static bool CloseSocket(SOCKET* socket_fd);
    static bool Shutdown();
    
    static std::string GetAddressAndPort(SOCKET socket_fd);

    // return 0 for big endian, 1 for little endian.
    static inline bool is_little_endian() {
        // http://stackoverflow.com/questions/12791864/c-program-to-check-little-vs-big-endian
        volatile uint32_t i=0x01234567;
        return (*((uint8_t*)(&i))) == 0x67;
    }
    
protected:
    // if timeout_ms == 0, then these routines block and do not return until len bytes have been sent/received.
    // if timeout_ms > 0, then the routine returns true if len bytes are successfully sent/received and false
    // if the operation failed due to either a socket error or taking longer than the timeout.
    static bool SendBytes(SOCKET* socket_fd, uint8_t* buf, int len, double timeout_ms=0);
    static bool ReceiveBytes(SOCKET* socket_fd, uint8_t* buf, int len, double timeout_ms=0);
};

#endif
