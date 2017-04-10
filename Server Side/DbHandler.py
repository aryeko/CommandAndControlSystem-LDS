from pymongo import ReturnDocument
from pymongo import MongoClient
from bson.objectid import ObjectId

class DbHandler:
	"""
		A class that represent a data base, including methods of DB manipulations
	"""
	def __init__(self):
		self.client = MongoClient('mongodb://buko:buko1234@ds149700.mlab.com:49700/bukoharam')
		self.db = self.client.bukoharam

	def add_unit(self, unit_name):
		doc = {"name": unit_name}

		affected_doc_id = self.db.Units.find_one_and_update(doc, {'$set': doc}, projection={'_id': True},
															upsert=True, return_document=ReturnDocument.AFTER)

		return affected_doc_id['_id']

	def get_units(self, json_filter = None):
		if json_filter is None:
			return self.db.Units.find()
		return self.db.Units.find(json_filter)

	def delete_unit(self, json_filter):
		return self.db.Units.delete_many(json_filter)

	def add_gscan(self, gscan_sn, owned_unit_id):
		doc = {"gscan_sn": gscan_sn,
			   "owned_unit_id": owned_unit_id}

		affected_doc_id = self.db.Gscans.find_one_and_update({'gscan_sn': gscan_sn}, {'$set': doc}, projection={'_id': True},
															upsert=True, return_document=ReturnDocument.AFTER)
		return ObjectId(affected_doc_id['_id'])

	def get_gscans(self, json_filter = None):
		if json_filter is None:
			return self.db.Gscans.find()
		return self.db.Gscans.find(json_filter)

	def delete_gscan(self, json_filter):
		return self.db.Gscans.delete_many(json_filter)

	def add_user(self, fullname, username, password, unit_id):
		doc = {"fullname": fullname,
			   "username": username,
			   "password": password,
			   "unitId": unit_id}

		affected_doc_id = self.db.Users.find_one_and_update({'username': username}, {'$set': doc}, projection={'_id': True},
															upsert=True, return_document=ReturnDocument.AFTER)
		return ObjectId(affected_doc_id['_id'])

	def get_users(self, json_filter=None):
		if json_filter is None:
			return self.db.Users.find()
		return self.db.Users.find(json_filter)

	def delete_user(self, json_filter):
		return self.db.Users.delete_many(json_filter)

	def add_area(self, area_type, root_location, radius):
		doc = {"area_type": area_type,
			   "root_location": root_location,
			   "radius": radius}

		affected_doc_id = self.db.Areas.find_one_and_update({"root_location": root_location}, {'$set': doc}, projection={'_id': True},
															upsert=True, return_document=ReturnDocument.AFTER)
		return ObjectId(affected_doc_id['_id'])

	def get_areas(self):
		return self.db.Areas.find()

	def delete_area(self, json_filter):
		return self.db.Areas.delete_many(json_filter)

	def add_material(self, name, material_type, cas):
		doc = {"name": name,
			   "type": material_type,
			   "cas": cas}

		affected_doc_id = self.db.Materials.find_one_and_update({"name": name}, {'$set': doc}, projection={'_id': True},
															upsert=True, return_document=ReturnDocument.AFTER)
		return ObjectId(affected_doc_id['_id'])

	def get_materials(self):
		return self.db.Materials.find()

	def delete_material(self, json_filter):
		return self.db.Materials.delete_many(json_filter)

	def add_detection(self, user_id, gscan_id, area_id, material_id, raman_output, date_time, plate_number, suspect_id, location):
		doc = {"user_id": user_id,
			   "gscan_id": gscan_id,
			   "area_id": area_id,
			   "material_id": material_id,
			   "raman_output": raman_output,
			   "date_time": date_time,
			   "plate_number": plate_number,
			   "suspect_id": suspect_id,
			   "location": location}

		filter_by = {"user_id": user_id,
			   "gscan_id": gscan_id,
			   "area_id": area_id,
			   "material_id": material_id}

		affected_doc_id = self.db.Detections.find_one_and_update(filter_by, {'$set': doc}, projection={'_id': True},
															upsert=True, return_document=ReturnDocument.AFTER)
		return ObjectId(affected_doc_id['_id'])

	def get_detections(self, json_filter = None):
		if json_filter is None:
			return self.db.Detections.find()
		return self.db.Detections.find(json_filter)

	def delete_detection(self, json_filter):
		return self.db.Detections.delete_many(json_filter)

	def add_forbidden_combination(self, materials_list):
		doc = {"materials_list": materials_list}

		affected_doc_id = self.db.ForbiddenCombinations.find_one_and_update(doc, {'$set': doc}, projection={'_id': True},
															upsert=True, return_document=ReturnDocument.AFTER)
		return ObjectId(affected_doc_id['_id'])

	def get_materials(self):
		return self.db.ForbiddenCombinations.find()

	def delete_material(self, json_filter):
		return self.db.ForbiddenCombinations.delete_many(json_filter)

	def __del__(self):
		print("Closing DB cursor")
		del self.db
