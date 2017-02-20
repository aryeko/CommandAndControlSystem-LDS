from JsonDataParser import JsonDataParser
from DbHandler import DbHandler
from socket import *
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
		#self.dbHandler.closeCursor()

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

'''
if len(sys.argv) != 3:
    print("usage: Server.py <port> <host>")
    exit()
'''
#server = Server(int(sys.argv[1]), sys.argv[2])


dbHandler = DbHandler()
'''
print("delete all users")
dbHandler.delete_user({"username":"aryeko"})
dbHandler.delete_user({"username":"tomerac"})
users = dbHandler.get_user()
for user in users:
	print(str(user))
'''
print("Adding Arye")
dbHandler.add_user("Arye Kogan", "aryeko", "1234")
users = dbHandler.get_users()
for user in users:
	print(str(user))

print("Adding Tomer")
dbHandler.add_user("Tomer Achdut", "tomerac", "1234")
users = dbHandler.get_users()
for user in users:
	print(str(user))