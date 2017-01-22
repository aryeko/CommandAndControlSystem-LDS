from socket import *
import mysql.connector as dbConnector
import cgi
import json
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

			print("*********Client sends data*********")
			print("Loading...")
			data = self.getData(client, "JSON")
			print("*****Recieving ended naturally*****")

			self.updateDB(data)

	def getData(self, client, type):

		if type == "JSON":
			stringJSON = self.receiveJsonAsString(client)
			return self.extacrtJsonObject(stringJSON)
		else:
			print("File handling")
			data = 0; #temp name
			#return self.receiveFile(client)
		return -1

	def receiveJsonAsString(self, client):
		data = client.recv(1024)
		stringJSON = ''
		while data:
			stringJSON += data.decode()
			data = client.recv(1024)	
		
		return stringJSON
	
	def receiveFile(self, client):
		newFile = open("newFile.png", "wb")
		data = client.recv(1024)
		while data:
			newFile.write(data)
			data = client.recv(1024)

		newFile.close()

	def extacrtJsonObject(self, stringJSON):
		jsonArray = json.loads(stringJSON)
		return JsonObject(jsonArray)
		
	def updateDB(self, dataToUpdate):
		print("Updating DB...")
		db, cursor = self.connectDB()
		self.createDB(db, cursor)
		self.createEntities(db, cursor)
		cursor.close()

	#Connect to mySQL DB
	def connectDB(self):
		db = dbConnector.connect(host='localhost', user='root', passwd='')
		cursor = db.cursor()
		return db, cursor

	#Create new DB
	def createDB(self, db, cursor):
		sql = "create database if not exists LDS"
		cursor.execute(sql)
		db.commit()

	#Create a new table into the DB
	def createEntities(self, db, cursor):
		cursor.execute("use LDS")
		sql = '''create table if not exists material
				(materialID int not null auto_increment,
				material varchar(30) not null,
				primary key(materialID))'''
		cursor.execute(sql)
		db.commit()
		sql = '''create table if not exists person
				(personID int not null auto_increment,
				name varchar(30) not null,
				primary key(personID))'''
		cursor.execute(sql)
		db.commit()

class JsonObject:

	def __init__(self, jsonArray): #dateOfDetection, material, position, suspectId, suspectPlateId, gunId, ramenGraph):
		self.dateOfDetection = jsonArray['dateOfDetection']
		self.material = jsonArray['material']
		self.position = jsonArray['position']
		self.suspectId = jsonArray['suspectId']
		self.suspectPlateId = jsonArray['suspectPlateId']
		self.gunId = jsonArray['gunId']
		self.ramenGraph = jsonArray['ramenGraph']

if len(sys.argv) != 2:
	print("usage: Server.py <port>")
	exit()
	
server = Server(int(sys.argv[1]))