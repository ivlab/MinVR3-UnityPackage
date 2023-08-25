import test_consts
import minvr3

print('connecting to server')

server = minvr3.minvr3_net.connect_to(test_consts.HOST, test_consts.PORT)

# test basic send/receive
# recd = 0
# while recd < 4:
#     buf = server.recv(4)
#     recd += len(buf)
# print(buf, int.from_bytes(buf, 'little'))
# exit(0)

# test send/receive uint32
# for count in range(test_consts.SEND_EVENT_COUNT):
#     c = minvr3.minvr3_net.receive_uint32(server)
#     print('got', c, 'expected', count)
# exit(0)

# test send/receive string
# for count in range(test_consts.SEND_EVENT_COUNT):
#     c = minvr3.minvr3_net.receive_string(server)
#     print('got', c, 'expected', test_consts.STRING_CONST + str(count))
# exit(0)

counter = 0
while counter < test_consts.SEND_EVENT_COUNT - 1:
    e_int = minvr3.minvr3_net.receive_vr_event(server)
    e_str = minvr3.minvr3_net.receive_vr_event(server)
    print('received: ', e_int, e_str)
    counter = e_int.data
    e_str2 = minvr3.vr_event.VREventString('MY_STRING', 'Hello server!')
    print('sending', e_int, e_str2)
    minvr3.minvr3_net.send_vr_event(server, e_int)
    minvr3.minvr3_net.send_vr_event(server, e_str2)

minvr3.minvr3_net.close_socket(server)
