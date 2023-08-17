'''
Client to send canned responses to a server, and print out any responses we get
from the server.
'''

import threading
import time

from minvr3 import vr_event, minvr3_net

STOP_REQUEST = None
SERVER = None

def receiver():
    while not STOP_REQUEST.is_set():
        try:
            evt = minvr3_net.receive_vr_event(SERVER)
            print('RECEIVED', evt, 'FROM', SERVER.getpeername())
        except Exception as e:
            print()
            pass

def main():
    host = '127.0.0.1'
    port = 9034
    events = [
        vr_event.VREventVector2("Test/Vector2", vr_event.Vector2(1, 2)),
        vr_event.VREventVector3("Test/Vector3", vr_event.Vector3(1, 2, 3)),
        vr_event.VREventVector4("Test/Vector4", vr_event.Vector4(1, 2, 3, 4)),
        vr_event.VREventQuaternion("Test/Quaternion", vr_event.Quaternion(0, 0, 0, 1)),
        vr_event.VREventGameObject("Test/GameObject", vr_event.GameObject(999)),
        vr_event.VREventString("Test/String", 'Hello world'),
        vr_event.VREventInt("Test/Int", 42),
        vr_event.VREventFloat("Test/Float", 3.14159265),
    ]

    global SERVER
    SERVER = minvr3_net.connect_to(host, port)

    global STOP_REQUEST
    STOP_REQUEST = threading.Event()
    receive_thread = threading.Thread(target=receiver)
    receive_thread.start()

    value = 'foo'
    while value != '':
        for i, evt in enumerate(events):
            print(f'{i}.', evt)
        print('<Enter> to quit')
        value = input('Select an event (number) to send: ')
        try:
            index = int(value)
            evt = events[index]
            print('SENDING', evt)
            minvr3_net.send_vr_event(SERVER, evt)
            print()
        except Exception as e:
            print(e)
            pass

        time.sleep(0.01)

    STOP_REQUEST.set()
    # receive_thread.join()

    minvr3_net.close_socket(SERVER)
    

if __name__ == '__main__':
    main()