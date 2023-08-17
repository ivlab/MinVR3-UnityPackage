# Python MinVR3 library

This package provides communication bindings for MinVR3 in Python. This is
useful because the IV/LAB has historically used Python for many web server
applications (Flask or Django), and it's useful to communicate between Unity and
these servers in real-time.

The implementation here is based off of the MinVR3.cpp~ client and server.


Check the package out on PyPI:

[![MinVR3 Python Library on PyPI](https://badgen.net/pypi/v/minvr3/)](https://pypi.org/project/minvr3)


## Installation

You can install the `minvr3` Python package from the Python package index using the following command:

```
python3 -m pip install --user minvr3
```


## Getting Started

The Python MinVR3 client can function as a client or server. Look below for
simple examples of running a MinVR3 client and server. Check out the `apps`
folder for more complete examples.

Both the server and client implementation are compatible with the
`TcpJsonVREventConnection` provided by the MinVR3-UnityPackage. You can have a
Python server connect with a Unity client, and vice versa.


### Server code

Copy/paste the following code to set up a simple server that sends 10 integer VR Events and listens for 10 integer VR Events:


```py
import time
from minvr3 import minvr3_net

HOST = '127.0.0.1'
PORT = 9034

listener = minvr3_net.create_listener(HOST, PORT, 0)

# wait for 1 connection
while not minvr3_net.is_ready_to_read(listener):
    print('waiting for connection')
    time.sleep(1)

client = minvr3_net.try_accept_connection(listener)

if client is None:
    print('no connection')
    exit(1)

# test send/receive uint32
for count in range(10, 20):
    minvr3_net.send_uint32(client, count)
    print('sent', count)

for count in range(0, 10):
    c = minvr3_net.receive_uint32(client)
    print('got', c, 'expected', count)


minvr3_net.close_socket(client)
```


### Client code


Copy/paste the following code to set up a simple client that sends 10 integer VR Events and listens for 10 integer VR Events:


```py
from minvr3 import minvr3_net

HOST = '127.0.0.1'
PORT = 9034
print('Connecting to server', HOST, PORT)

server = minvr3_net.connect_to(HOST, PORT)

# test send/receive uint32
for count in range(10, 20):
    c = minvr3_net.receive_uint32(server)
    print('got', c, 'expected', count)

for count in range(0, 10):
    minvr3_net.send_uint32(server, count)
    print('sent', count)

minvr3_net.close_socket(server)
```

### Relay server

In addition to example code, a pure Python relay server is also supplied. This server receives VREvents from clients, and relays them to all other clients.

To run the relay server, you can run:

```
python3 -m minvr3.apps.relay_server
```


## Development Installation and Deploying a new package

### Installing the dev version

To install the `minvr3` Python package locally, run the following command in this folder:

```
python3 -m pip install --user --editable .
```

If you're on Windows, probably need to replace `python3` with `py` in the above commands.


### Deploying the package to PyPI

To do this, you will need an account on https://pypi.org and https://test.pypi.org.

Once you've made your changes, build the package, then deploy it to the Python Package Index.

First, make sure you have the build / deploy tools:

```
py -m pip install --user --upgrade build
py -m pip install --user --upgrade twine
```

Build it (run in this folder):

```
py -m build
```

Test deploy it (run in this folder):

```
py -m twine upload --verbose --repository testpypi dist/*
```

verify that it all looks good on the deployed test site: https://test.pypi.org/project/minvr3/

Then, actually deploy it:

```
py -m twine upload --verbose dist/*
```