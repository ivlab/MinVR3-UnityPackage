# JavaScript MinVR3 library

This package provides communication bindings for MinVR3 in JavaScript. The
biggest use case for JS MinVR3 bindings is communicating with web browsers, but
other examples exist (Node.js servers, other apps written in React Native,
custom JavaScript implementations like that used in Digistar planetarium
software, etc.).


## Prerequisites

If you wish to communicate between Unity and a browser, you will need to install
the [MinVR3 WebSocket Plugin](https://github.umn.edu/ivlab-cs/MinVR3Plugin-WebSocket).



## Getting started

There are two parts to setting up communication between MinVR3 in Unity and the JavaScript client:

1. Set up Unity
2. Set up the webpage


### 1. Set up Unity for remote communications


To get started on the Unity side, use the following steps in your Unity project:

1. Create a new empty GameObject and name it `HTTPListener`.
2. Add a "HTTP Web Socket VR Event Connection" component to the `HTTPListener` GameObject.
3. Set the "Web Server Folder" on the VREventConnection to `web`. (we will create this directory later)
4. (optional) change the Host and Port (use host `0.0.0.0` to make the server
accessible to other computers on the network)



### 2. Set up webpage to communicate with Unity

To get started on the Browser side, use the following steps:

1. In the Assets folder of your project, create a new folder called `web`.
2. Inside the `web` folder, create a new file `index.html` and paste the following contents:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>MinVR3 WebSocket Example</title>
</head>
<body>
    This page is being served by Unity.
    <script type="module">
        // import the MinVR3.js browser script
        import * as minvr3 from 'https://esm.run/minvr3.js@latest';

        // connect to the websocket (await call returns after websocket connected)
        const ws = await minvr3.connect(window.location.host);

        // add a VR event listener
        ws.onmessage = (msg) => {
            console.log("got message from Unity: " + VREvent.fromJson(msg));
        }

        // send a VR event
        let msg = new minvr3.VREventString('HelloEventName', 'Hello from the browser');
        ws.send(msg.toJson());

        // send another VR event
        msg = new minvr3.VREventVector3('Position', new minvr3.Vector3(1, 2, 3));
        ws.send(msg.toJson());
    </script>
</body>
</html>
```

3. To check that it's working, press Play in Unity, then go to
http://localhost:8000 in a browser on the same computer as Unity is running on.
you should see the text "This page is being served by Unity."


**For more advanced usage such as sending/receiving VR events, please see the
manual page "Remote Connections".**


## Building and deploying to NPM

NPM module design inspired by https://adrianmanduc.medium.com/how-to-create-a-js-library-and-publish-it-to-npm-6e6351971984

To rebuild and publish the package, do:

```
npm install
npm run build
npm publish
```