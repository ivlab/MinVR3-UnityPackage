
@rem This is untested, but something like this or very close should work

@rem Start the server
START "Driver" ./Build/TcpConnectionTest.exe -vrconfig VRConfig_Driver

SLEEP 2

@rem Start the client(s)
START "Viewer" ./Build/TcpConnectionTest.exe -vrconfig VRConfig_Viewer
START "Viewer" ./Build/TcpConnectionTest.exe -vrconfig VRConfig_Viewer
