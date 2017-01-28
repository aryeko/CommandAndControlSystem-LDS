from socket import *
import mysql.connector as db_connector
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
        self.accept_connections()

    def accept_connections(self):
        while True:
            print("Listening for connections, PORT: ", self.port)
            client, address = self.socket.accept()

            print("*********Client sends data*********")
            print("Loading...")
            data = self.get_data(client, "JSON")
            print("*****Recieving ended naturally*****")

            self.update_db(data)

    def get_data(self, client, type_of_data):

        if type_of_data == "JSON":
            string_json = self.receive_json_as_string(client)
            return self.extract_json_object(string_json)
        else:
            print("File handling")
            #data = 0;  # temp name
        # return self.receiveFile(client)
        return -1

    @staticmethod
    def receive_json_as_string(client):
        data = client.recv(1024)
        string_json = ''
        while data:
            string_json += data.decode()
            data = client.recv(1024)

        return string_json

    @staticmethod
    def receive_file(client):
        new_file = open("new_file.png", "wb")
        data = client.recv(1024)
        while data:
            new_file.write(data)
            data = client.recv(1024)

        new_file.close()

    @staticmethod
    def extract_json_object(string_json):
        json_array = json.loads(string_json)
        return JsonObject(json_array)

    def update_db(self, data_to_update):
        print("Updating DB...")
        db, cursor = self.connect_db()
        self.create_db(db, cursor)
        self.create_entities(db, cursor)
        self.insert_into_db(db, cursor, data_to_update)
        cursor.close()

    # Connect to mySQL DB
    @staticmethod
    def connect_db():
        db = db_connector.connect(host='localhost', user='root', passwd='')
        cursor = db.cursor()
        return db, cursor

    # Create new DB
    @staticmethod
    def create_db(db, cursor):
        sql = "create database if not exists LDS"
        cursor.execute(sql)
        db.commit()

    # Create a new table into the DB
    @staticmethod
    def create_entities(db, cursor):
        cursor.execute("use LDS")
        sql = '''create table if not exists material
				(materialID int not null auto_increment,
				material varchar(30) not null,
				primary key(materialID))'''
        cursor.execute(sql)
        db.commit()
        sql = '''create table if not exists person
				(personID varchar(10) not null,
				name varchar(30) not null,
				primary key(personID))'''
        cursor.execute(sql)
        db.commit()

    # insert values into the DB
    @staticmethod
    def insert_into_db(db, cursor, data):
        sql = "insert into material(material) values('{0}')".format(data.material)
        cursor.execute(sql)
        db.commit()
        sql = "insert into person(personID, name) values('{0}', 'Tomer Achdut')".format(data.suspectId)
        cursor.execute(sql)
        db.commit()


class JsonObject:
    def __init__(self,
                 json_array):  # dateOfDetection, material, position, suspectId, suspectPlateId, gunId, ramenGraph):
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
