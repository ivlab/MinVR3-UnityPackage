import socket
import sys
import time
import minvr3.vr_event
import select

BUFFER_SIZE = 1024

def connect_to(ip: str, port: int) -> socket.socket:
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM, socket.IPPROTO_TCP)

    s.connect((ip, port))

    # Disable Nagle's Algorithm
    s.setsockopt(socket.IPPROTO_IP, socket.TCP_NODELAY, 1)
    return s

def create_listener(ip: str, port: int, backlog: int=10) -> socket.socket:
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM, socket.IPPROTO_TCP)

    s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

    s.bind((ip, port))

    s.listen(backlog)
    return s


def try_accept_connection(listener: socket.socket) -> socket.socket:
    client_socket, client_addr = listener.accept()

    # disable Nagle's algorithm on the client socket
    client_socket.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)

    print('minvr3_net.try_accept_connection(): Accepted connection from ' + str(client_addr))

    return client_socket


def send_bytes(s: socket.socket, buf: bytes, timeout_ms: float=0):
    if timeout_ms != 0:
        start = time.time()

    bytes_sent = 0
    n = 0
    length = len(buf)
    while bytes_sent < length:
        n = s.send(buf[bytes_sent:])
        bytes_sent += n

        if timeout_ms != 0:
            now = time.time()
            elapsed_ms = (now - start) * 1000
            if elapsed_ms > timeout_ms:
                raise TimeoutError('minvr3_net.send_bytes(): Send timed out')

def receive_bytes(s: socket.socket, length: int, timeout_ms: float=0) -> bytes:
    if timeout_ms != 0:
        start = time.time()

    buf = bytes()
    bytes_recd = 0
    while bytes_recd < length:
        buf += s.recv(min(length - bytes_recd, BUFFER_SIZE))
        bytes_recd += len(buf)

        if timeout_ms != 0:
            now = time.time()
            elapsed_ms = (now - start) * 1000
            if elapsed_ms > timeout_ms:
                raise TimeoutError('minvr3_net.receive_bytes(): Receive timed out')
    return buf


def send_uint32(s: socket.socket, value: int, timeout_ms: float=0):
    msg_bytes = value.to_bytes(4, sys.byteorder)
    send_bytes(s, msg_bytes, timeout_ms)

def receive_uint32(s: socket.socket, timeout_ms: float=0) -> int:
    msg_bytes = receive_bytes(s, 4, timeout_ms)
    return int.from_bytes(msg_bytes, sys.byteorder)

def send_string(s: socket.socket, value: str, timeout_ms: float=0):
    send_uint32(s, len(value), timeout_ms)
    send_bytes(s, value.encode(), timeout_ms)

def receive_string(s: socket.socket, timeout_ms: float=0) -> str:
    length = receive_uint32(s, timeout_ms)
    msg_bytes = receive_bytes(s, length, timeout_ms)
    return msg_bytes.decode()

def send_vr_event(s: socket.socket, evt: minvr3.vr_event.VREvent, timeout_ms: float=0):
    js = evt.to_json()
    send_string(s, js, timeout_ms)

def receive_vr_event(s: socket.socket, timeout_ms: float=0) -> minvr3.vr_event.VREvent:
    js = receive_string(s, timeout_ms)
    if js is not None:
        return minvr3.vr_event.VREvent.from_json(js)


def close_socket(s: socket.socket):
    s.close()


def is_ready_to_read(s: socket.socket) -> bool:
    reading_socks, _, _ = select.select([s], [], [], 0)
    return len(reading_socks) > 0

def is_ready_to_write(s: socket.socket) -> bool:
    writing_socks, _, _ = select.select([], [s], [], 0)
    return len(writing_socks) > 0

def select_ready_to_read(sockets: list) -> list:
    reading_socks, _, _ = select.select(sockets, [], [], 0)
    return reading_socks