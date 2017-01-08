from socket import *
import sys

class Client:
	host = ''
	fileName = ''
	port = -1
	socket = socket(AF_INET, SOCK_STREAM)

	def __init__(self, host, port, fileName):
		self.port = port
		self.host = host
		self.fileName = fileName
		self.connect()

	def connect(self):
		self.socket.connect((self.host, self.port))
#		self.sendFileName()
		self.sendFile()
	
#	def sendFileName(self):
#        self.socket.send("name: " +self.file)
		
	def sendFile(self):
		readByte = open(self.fileName, "rb")
		data = readByte.read()
		readByte.close()
		print("Sending file: " + self.fileName)
		self.socket.send(data)
		self.socket.shutdown(SHUT_WR)
		self.socket.close()
		print("File transfer ended naturally")
	
if len(sys.argv) != 4:
	print("usage: Client.py <host> <port> <file name>")
	exit()
	
client= Client(sys.argv[1], int(sys.argv[2]), sys.argv[3])
