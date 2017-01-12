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
	
	def sendString(self, stringToSend):
		self.socket.send(stringToSend)
		print("String transmition ended naturally")

	def sendFile(self):
		readByte = open(self.fileName, "rb")
		data = readByte.read()
		readByte.close()
		print("Sending file: " + self.fileName)
		self.socket.send(data)
		self.socket.shutdown(SHUT_WR)
		self.socket.close()
		print("File transmition ended naturally")
	
if len(sys.argv) != 4:
	print("usage: Client.py <host> <port> <file name>")
	exit()

#the file name will contain a JSON object
#fileNameJSON = sys.argv[3]
stringJSON = "example: 5"
client = Client(sys.argv[1], int(sys.argv[2]))
client.sendString(stringJSON)
#self.sendFile()
