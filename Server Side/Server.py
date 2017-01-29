from socket import *
import mysql.connector as db_connector
import cgi
import json
import sys


class Server:
    
	def __init__(self, port, host="127.0.0.1"):
		self.host = host
		self.port = port
		self.dbHandler = DbHandler()
		self.socket = socket(AF_INET, SOCK_STREAM)
		self.socket.bind((self.host, self.port))
		self.socket.listen(10)
		self.accept_connections()

	def accept_connections(self):
		while True:
			print("Listening for connections, PORT: ", self.port)
			client, address = self.socket.accept()

			print("*********Client sends data*********")
			print("Loading...")
			data = self.get_data(client)
			print("*****Recieving ended naturally*****")

			self.update_db(data)
		self.dbHandler.closeCursor()

	def get_data(self, client, type_of_data="JSON"):
		if type_of_data == "JSON":
			string_json = self.receive_string(client)
			return JsonDataParser.extract_json_object(string_json)
		else:
			print("File handling")
			#return self.receiveFile(client)
		return -1

	@staticmethod
	def receive_string(client):
		data = client.recv(1024)
		data_as_string = ''
		while data:
			data_as_string += data.decode()
			data = client.recv(1024)
		return data_as_string

	@staticmethod
	def receive_file(client):
		#TODO: get the file name and extention
		new_file = open("new_file.png", "wb")
		data = client.recv(1024)
		while data:
			new_file.write(data)
			data = client.recv(1024)
		new_file.close()

	def update_db(self, data_to_update):
		print("Updating DB...")
		self.dbHandler.insert_into_db(data_to_update)

class DbHandler:
	def __init__(self):
		self.db = db_connector.connect(host='localhost', user='root', passwd='')
		self.cursor = self.db.cursor()
		self.create_db()
		self.cursor.execute("use LDS")
		self.create_entities()

	# Create new DB
	def create_db(self):
		sql = "create database if not exists LDS"
		self.cursor.execute(sql)
		self.db.commit()

	# Create a new table into the DB
	def create_entities(self):
		sql = '''create table if not exists material
				(materialID int not null auto_increment,
				material varchar(30) not null,
				primary key(materialID))'''
		self.cursor.execute(sql)
		self.db.commit()
		sql = '''create table if not exists person
				(personID varchar(10) not null,
				name varchar(30) not null,
				primary key(personID))'''
		self.cursor.execute(sql)
		self.db.commit()

	# insert values into the DB
	def insert_into_db(self, data):
		sql = "insert into material(material) values('{0}')".format(data.material)
		self.cursor.execute(sql)
		self.db.commit()
		sql = "insert into person(personID, name) values('{0}', 'Tomer Achdut')".format(data.suspectId)
		self.cursor.execute(sql)
		self.db.commit()

	def closeCursor(self):
		self.cursor.close()

class JsonDataParser:
	'''
		A class that parse a stringified JSON into an object
	'''
	@classmethod
	def extract_json_object(self, data):
		json_array = json.loads(data)
		return Detection(json_array)

class Detection:
	def __init__(self, json_array):
		self.dateOfDetection = json_array['dateOfDetection']
		self.material = json_array['material']
		self.position = json_array['position']
		self.suspectId = json_array['suspectId']
		self.suspectPlateId = json_array['suspectPlateId']
		self.gunId = json_array['gunId']
		self.ramenGraph = json_array['ramenGraph']

if len(sys.argv) != 2:
    print("usage: Server.py <port>")
    exit()

server = Server(int(sys.argv[1]))
