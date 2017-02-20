import json as jsonLib
from pymongo import MongoClient

class DbHandler:
	"""
		A class that represent a data base, including methods of DB manipulations
	"""
	def __init__(self):

		self.client = MongoClient('mongodb://buko:buko1234@ds149700.mlab.com:49700/bukoharam')
		self.db = self.client.bukoharam

	def add_user(self, fullname, username, password):
		doc = {"fullname": fullname,
			"username": username,
			"password": password}

		self.db.Users.update(doc, doc, upsert=True)

	def get_users(self, json_filter=""):
		return self.db.Users.find()

	def delete_user(self, json_filter):
		return self.db.Users.delete_many(json_filter)

	def __del__(self):
		print("Closing DB cursor")
