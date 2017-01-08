from socket import *
import sys

class Server:
	port = -1
	fileName = ''
	host = '127.0.0.1'
	socket = socket(AF_INET, SOCK_STREAM)   
	
	def __init__(self, port):
		self.port = port
		self.socket.bind((self.host, self.port))  
		self.listen()

	def listen(self):
		self.socket.listen(10)
		while True:
			print("Listening for connections, PORT: ", self.port)
			client, address = self.socket.accept()
			print("Client sends data")
			#self.receiveFileName(client)
			self.receiveFile(client)

#	def receiveFileName(self, client):
#		data = client.recv(1024)
#		while data:
#			self.fileName += data
#			data = client.recv(1024)
			
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