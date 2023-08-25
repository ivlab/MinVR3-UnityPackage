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
