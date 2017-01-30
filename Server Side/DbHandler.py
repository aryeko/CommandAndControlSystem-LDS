from SqlQueries import SqlQueries
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
		self.cursor.execute(SqlQueries.USE_DB_LDS)
		self.create_entities()

	# Create new DB
	def create_db(self):
		self.cursor.execute(SqlQueries.CREATE_DB_LDS)
		self.db.commit()

	# Create a new table into the DB
	def create_entities(self):
		self.cursor.execute(SqlQueries.CREATE_TABLE_MATERIAL)
		self.db.commit()
		self.cursor.execute(SqlQueries.CREATE_TABLE_PERSON)
		self.db.commit()

	# insert values into the DB
	def insert_into_db(self, data):
		
		sql = SqlQueries.INSERT_MATERIAL + " values('{0}')".format(data.material)
		self.cursor.execute(sql)
		self.db.commit()
		sql = SqlQueries.INSERT_PERSON + " values('{0}', 'Tomer Achdut')".format(data.suspectId)
		self.cursor.execute(sql)
		self.db.commit()

	def __del__(self):
		print("Closing DB cursor")
		self.cursor.close()