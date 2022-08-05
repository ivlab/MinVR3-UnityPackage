
@rem Start the server
START "Cluster Server" ./Build/MyMinVR3App.exe -vrconfig VRConfig_Server

@rem Start the client
START "Cluster Client" ./Build/MyMinVR3App.exe -vrconfig VRConfig_Client
