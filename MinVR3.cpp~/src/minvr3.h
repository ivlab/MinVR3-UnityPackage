/**
  MinVR3_cpp -- This library provides a partial C++ implementation of MinVR3 so that C++ programs can
  send and receive serialized VREvents over a network connection with Unity-based MinVR3 programs.
 */

#ifndef MINVR3_MINVR3_H
#define MINVR3_MINVR3_H

// including net headers first makes sure winsock2.h and windows.h are included in the correct order
#include "net_headers.h"

#include "json/json.h"
#include "config_val.h"
#include "min_net.h"
#include "minvr3_net.h"
#include "minvr3_utils.h"
#include "vr_event.h"

#endif
