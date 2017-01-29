class Detection:
	'''
		A class that represent a detection by all its attributes
	'''
	def __init__(self, json_array):
		self.dateOfDetection = json_array['dateOfDetection']
		self.material = json_array['material']
		self.position = json_array['position']
		self.suspectId = json_array['suspectId']
		self.suspectPlateId = json_array['suspectPlateId']
		self.gunId = json_array['gunId']
		self.ramenGraph = json_array['ramenGraph']