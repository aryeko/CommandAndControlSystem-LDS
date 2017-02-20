import json
from Detection import Detection

class JsonDataParser:
	"""
		A class that parse a stringified JSON into an object
	"""
	@classmethod
	def extract_json_object(self, data):
		json_array = json.loads(data)
		return Detection(json_array)