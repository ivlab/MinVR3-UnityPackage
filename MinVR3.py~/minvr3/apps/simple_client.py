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