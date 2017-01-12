from socket import *
import sys

class Server:
	
	def __init__(self, port):
	   
		self.host = '127.0.0.1'
		self.port = port
		self.socket = socket(AF_INET, SOCK_STREAM)
		self.socket.bind((self.host, self.port))
		self.socket.listen(10)
		self.acceptConnections()

	def acceptConnections(self):
		while True:
			print("Listening for connections, PORT: ", self.port)
			client, address = self.socket.accept()
			print("Client sends data")
			self.getData()

	def getData(self):
		self.receiveJsonAsString(client)
		#self.receiveFile(client)

	def receiveJsonAsString(self, client):
		data = client.recv(1024)
		while data:
			self.fileName += data
			data = client.recv(1024)

		print(data)
		print("Recieving ended naturally")
	
	def receiveFile(self, client):
		newFile = open("newFile.png", "wb")
		data = client.recv(1024)
		while data:
			newFile.write(data)
			data = client.recv(1024)
	
		print("Recieving ended naturally")
		newFile.close()
		
if len(sys.argv) != 2:
	print("usage: Server.py <port>")
	exit()
	
server = Server(int(sys.argv[1]))