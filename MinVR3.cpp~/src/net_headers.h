
#ifndef MINVR3_NET_HEADERS_H
#define MINVR3_NET_HEADERS_H

#ifdef WIN32

// windows is very picky about the include order, winsock2 must come before windows.h
#include <winsock2.h>
#include <windows.h>

#else // linux, osx, ...

#define INVALID_SOCKET -1
#define SOCKET_ERROR -1
typedef int SOCKET;

#endif


#endif
