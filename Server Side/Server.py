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
			#self.getData(client)
			self.receiveJsonAsString(client)

	def getData(self, client):
		print("*********Client sends data*********")
		self.receiveJsonAsString(client)
		self.receiveFile(client)

	def receiveJsonAsString(self, client):
		print("*********Client sends data*********")
		data = client.recv(1024)
		stringJSON = ''
		while data:
			stringJSON += data.decode()
			data = client.recv(1024)	

		print("Data Recieved: " + stringJSON)
		print("*****Recieving ended naturally*****")
		self.extacrtJsonObject(stringJSON)
	
	def receiveFile(self, client):
		newFile = open("newFile.png", "wb")
		data = client.recv(1024)
		while data:
			newFile.write(data)
			data = client.recv(1024)
	
		print("*****Recieving ended naturally*****")
		newFile.close()

	def extacrtJsonObject(self, stringJSON):
		print("*******Creating JSON Object*******")
		
class JsonObject:

	def __init__(self, dateOfDetection, material, position, suspectId, suspectPlateId, gunId, ramenGraph):
		self.dateOfDetection = dateOfDetection
		self.material = material
		self.position = position
		self.suspectId = suspectId
		self.suspectPlateId = suspectPlateId
		self.gunId = gunId
		self.ramenGraph = ramenGraph

if len(sys.argv) != 2:
	print("usage: Server.py <port>")
	exit()
	
server = Server(int(sys.argv[1]))