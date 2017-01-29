from socket import *
import sys


class Client:
    def __init__(self, host, port):
        self.socket = socket(AF_INET, SOCK_STREAM)
        self.port = port
        self.host = host
        self.connect()

    def connect(self):
        self.socket.connect((self.host, self.port))

    def send_string(self, stringToSend):
        self.socket.send(stringToSend.encode())
        print("String transmition ended naturally")

    def send_file(self):
        read_byte = open(self.fileName, "rb")
        data = read_byte.read()
        read_byte.close()
        print("Sending file: " + self.fileName)
        self.socket.send(data)
        self.socket.shutdown(SHUT_WR)
        self.socket.close()
        print("File transmition ended naturally")


if len(sys.argv) != 3:
    # TODO: return to 4 arguments when trying to load file name and add to usage <file name>
    print("usage: Client.py <host> <port>")
    exit()

# the file name will contain a JSON object
# fileNameJSON = sys.argv[3]
stringJSON = '{"dateOfDetection":"1.1.17", "material":"Cocaine", "position":"Gaza", "suspectId":"123123", ' \
             '"suspectPlateId":"36-034-98", "gunId":"222", "ramenGraph":"none"} '
client = Client(sys.argv[1], int(sys.argv[2]))
client.send_string(stringJSON)
# self.sendFile()
