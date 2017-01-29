import mysql.connector as db_connector
import sys

class DbHandler:
	'''
		A class that represent a data base, including methods of DB manipulations
	'''
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

	def __del__(self):
		print("Closing DB cursor")
		self.cursor.close()