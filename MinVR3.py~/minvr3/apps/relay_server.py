'''
MinVR3 VREvent Relay Server This simple app acts as a MinVR3 Connection server.
It will accept connections from an unlimited number of clients.  Whenever a
VREvent is received from a client, the event is relayed out to all attached
clients.  By default, this includes relaying the event "back to" the source
client who sent the event.  However, this behavior can be turned off with the
relay-to-source-client command line option.  The port number and inner-loop
sleep milliseconds can also be set on the command line.
'''

import sys
import time

from minvr3 import vr_event, minvr3_net

def main():
    port = 9034
    relay_to_source_client = True
    read_write_timeout_ms = 500
    sleep_ms = 10

    if len(sys.argv) > 1:
        if 'help' in sys.argv or '-h' in sys.argv or '-help' in sys.argv or '--help' in sys.argv:
            print("Usage: relay_server.py [port] [relay-to-source-client: true/false] [read-write-timeout-ms] [sleep-ms] ")
            print("  * Relays all VREvents received to all connected clients.")
            print("")
            print("  * port defaults to ")
            print("  * relay-to-source-client defaults to ")
            print("  * read-write-timeout-ms defaults to ")
            print("  * sleep-ms defaults to ")
            print("  * Quits if an event named 'Shutdown' is received, or press Ctrl-C")
            exit(0)
        else:
            port = int(sys.argv[1])
    elif len(sys.argv) > 2:
        relay_to_source_client = bool(sys.argv[2])
    elif len(sys.argv) > 3:
        read_write_timeout_ms = float(sys.argv[3])
    elif len(sys.argv) > 4:
        sleep_ms = float(sys.argv[4])

    print('MinVR3 Relay Server')

    listener = minvr3_net.create_listener('0.0.0.0', port)
    clients = []
    shutdown = False
    while not shutdown:
        disconnected_clients = []

        # accept new connections from any clients trying to connect
        while minvr3_net.is_ready_to_read(listener):
            client = minvr3_net.try_accept_connection(listener)
            if client is not None:
                print('New connection from', client.getpeername())
                clients.append(client)

        # see if any clients have sent messages
        if len(clients) > 0:
            ready_clients = minvr3_net.select_ready_to_read(clients)
            # read one event from every client that is ready to read
            for c in ready_clients:
                # receive the incoming event
                try:
                    evt = minvr3_net.receive_vr_event(c, read_write_timeout_ms)
                except Exception as e:
                    # print('Receive error:', e)
                    evt = None
                if evt is not None:
                    # relay event to all clients
                    # print('Relaying', evt)
                    for out_client in clients:
                        if out_client != c or relay_to_source_client:
                            try:
                                minvr3_net.send_vr_event(out_client, evt, read_write_timeout_ms)
                            except Exception as e:
                                # print('Send error:', e)
                                # if there was a problem sending, assume client disconnected
                                disconnected_clients.append(out_client)
                else:
                    # if there was a problem receiving, assume client disconnected
                    disconnected_clients.append(c)
            
            # remove any disconnected clients
            while len(disconnected_clients) > 0:
                c = disconnected_clients.pop()
                print('Dropped connection from', c.getpeername())
                minvr3_net.close_socket(c)
                clients.remove(c)
        else:
            time.sleep(sleep_ms * 0.001)

    

if __name__ == '__main__':
    main()