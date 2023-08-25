import time
import minvr3
import test_consts

listener = minvr3.minvr3_net.create_listener(test_consts.HOST, test_consts.PORT, 0)

clients = []

counter = 5
print(f'Waiting {counter} seconds for one or more client(s) to connect...')
while counter > 0:
    print('Waiting', counter)
    if minvr3.minvr3_net.is_ready_to_read(listener):
        client = minvr3.minvr3_net.try_accept_connection(listener)
        if client is not None:
            clients.append(client)
    time.sleep(1.0)
    counter -= 1

if len(clients) == 0:
    print('Nobody connected, exiting.')
    exit(0)

# test basic send/receive
# for c in clients:
#     c.send(int.to_bytes(42, 4, 'little'))
#     c.close()
# exit(0)

# test send/receive uint32
# for c in clients:
#     for count in range(test_consts.SEND_EVENT_COUNT):
#         minvr3.minvr3_net.send_uint32(c, count)
# exit(0)

# test send/receive string
# for c in clients:
#     for count in range(test_consts.SEND_EVENT_COUNT):
#         minvr3.minvr3_net.send_string(c, test_consts.STRING_CONST + str(count))
# exit(0)


counter = 0

while counter < test_consts.SEND_EVENT_COUNT:
    # test sending
    for i in range(len(clients)):
        e_int = minvr3.vr_event.VREventInt('MY_INT', counter)
        e_str = minvr3.vr_event.VREventString('MY_STRING', 'Hello client!')
        print('sending to client', clients[i].getpeername(), e_int, e_str)
        minvr3.minvr3_net.send_vr_event(clients[i], e_int)
        minvr3.minvr3_net.send_vr_event(clients[i], e_str)

    # test receiving
    # this is overkill. for such a simple program we could just loop through the client_fds
    # in order and read from each one. however, the reads are blocking functions, so this
    # approach would only work well if all the clients are running at the same speed.  in
    # other words, if client[0] happened to be really slow, the server would wait, spinning
    # its wheels, to read from client[0] even if messages have already come in from
    # client[1], [2], [3], ...  the right way to handle this situation in network programming
    # is to use a select() call where the OS queries a set of possible fds and returns the
    # subset that are currently ready to read.  this loop demonstrates how to do that in MinNet.
    ready_clients = minvr3.minvr3_net.select_ready_to_read(clients)
    for c in ready_clients:
        e_int = minvr3.minvr3_net.receive_vr_event(c)
        e_str = minvr3.minvr3_net.receive_vr_event(c)
        counter = e_int.data
        print('received: ', e_int, e_str, 'from client', c.getpeername())

    counter += 1

minvr3.minvr3_net.close_socket(listener)

for i in range(len(clients)):
    minvr3.minvr3_net.close_socket(clients[i])