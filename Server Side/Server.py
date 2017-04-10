from JsonDataParser import JsonDataParser
from datetime import timedelta
from DbHandler import DbHandler
from passlib.apps import custom_app_context as pwd_context
from socket import *
import os
import ssl
import sys

from flask import Flask, request, session, g, redirect, url_for, abort, render_template, flash

SUCCESS = "SUCCESS"
AUTH_FAILED = "AUTH_FAILED"

dbHandler = DbHandler()

working_dir = os.path.dirname(os.path.abspath(__file__))
ctx = ssl.SSLContext(ssl.PROTOCOL_SSLv23)
ctx.load_cert_chain(os.path.join(working_dir, 'resources\lds.dev.crt'),
					os.path.join(working_dir, 'resources\lds.dev.key'))

app = Flask(__name__)

# Load default config and override config from an environment variable
app.config.update(dict(
    DEBUG=True,
    SECRET_KEY='development key',
    USERNAME='admin',
    PASSWORD='default',
	ssl_context=ctx
))
app.config.from_envvar('FLASKR_SETTINGS', silent=True)

def verify_user_session():
	print(str({'logged_in': session.get('logged_in') is True, 'user_id': session.get('user_id')}))
	if not session.get('logged_in'):
		abort(401, "You are not logged in or your session has expired, please login")


@app.before_request
def make_session_permanent():
	session.permanent = True
	app.permanent_session_lifetime = timedelta(seconds=10)

@app.route('/user', methods=['GET', 'POST', 'DELETE'])
def handle_user_request():
	verify_user_session()

	if request.method == 'GET':
		# here we want to get the value of user (i.e. ?user=some-value)
		username = request.args.get('username')
		print("username is: ", username)
		if username is not None:
			users = dbHandler.get_users({'username': username})
		else:
			users = dbHandler.get_users()
		return str([u for u in users])

	elif request.method == 'POST':
		# TODO: verificate the users

		unitId = dbHandler.get_units()[0]['_id']
		password = request.form.get('password')
		hashed_password = pwd_context.encrypt(password)
		new_object_id = dbHandler.add_user(request.form.get('fullname'), request.form.get('username') , hashed_password , unitId)
		if new_object_id is None:
			abort(500)
		return str(new_object_id)

	elif request.method == 'DELETE':
		username = request.form.get('username')
		print("delete username is: ", username)

		deleted_user_id = dbHandler.delete_user({'username': username})

		return "Deleted " + str(deleted_user_id.deleted_count) + " users"

	abort(400)

@app.route('/login', methods=['GET', 'POST'])
def handle_login_request():
	if request.method == 'POST':
		username = request.form['username']
		print("authenticating username: " + username)
		users = dbHandler.get_users({'username': username})
		print("found {} users".format(users.count()))
		if users.count() == 1:
			password = request.form['password']
			if pwd_context.verify(password, users[0]['password']):
				print("user is authorized")
				session['logged_in'] = True
				session['user_id'] = str(users[0]['_id'])
				flash('You were logged in')
				return SUCCESS

		abort(401, "Authentication failed")
	else:
		return str({'logged_in': session.get('logged_in') is True, 'user_id': session.get('user_id')})

@app.route('/detection', methods=['GET', 'POST'])
def handle_detection_request():
	verify_user_session()
	if request.method == 'GET':
		# here we want to get the value of user (i.e. ?user=some-value)
		material_id = request.args.get('material_id')
		print("material_id is: ", material_id)
		if material_id is not None:
			detections = dbHandler.get_detections({'material_id': material_id})
		else:
			detections = dbHandler.get_detections()
		return str([d for d in detections])

	elif request.method == 'POST':
		# TODO: verificate the incomming data

# user_id, gscan_id, area_id, material_id, raman_output, date_time, plate_number, suspect_id, location
		new_object_id = dbHandler.add_detection(request.form.get('fullname'), request.form.get('username'),
										   request.form.get('password'))
		if new_object_id is None:
			abort(500)
		return str(new_object_id)

	abort(400)



if __name__ == '__main__':
	app.run(ssl_context=ctx)

	#unitId = dbHandler.get_units()[0]['_id']
	#print("Adding Arye")
	#dbHandler.add_user("Arye Kogan", "aryeko", "1234", unit_id)
	#app.run()
	#To serve multiple clients:
	#app.run(threaded=True)
	#Alternately
	#app.run(processes=3)
'''

@app.route('/user', methods=['POST'])
def add_entry():
    #if not session.get('logged_in'):
    #    abort(401)
    #db = get_db()
    #db.execute('insert into entries (title, text) values (?, ?)',
    #           [request.form['title'], request.form['text']])
    #db.commit()
    #flash('New entry was successfully posted')
	#dbHandler.add_user(request.from['fuulname'],)
    #return redirect(url_for('show_entries'))


@app.route('/login', methods=['GET', 'POST'])
def login():
    error = None
    if request.method == 'POST':
        if request.form['username'] != app.config['USERNAME']:
            error = 'Invalid username'
        elif request.form['password'] != app.config['PASSWORD']:
            error = 'Invalid password'
        else:
            session['logged_in'] = True
            flash('You were logged in')
            return redirect(url_for('show_entries'))
    return render_template('login.html', error=error)


@app.route('/logout')
def logout():
    session.pop('logged_in', None)
    flash('You were logged out')
    return redirect(url_for('show_entries'))

'''


'''
#SESSION USAGE EXAMPLE

@app.route('/')
def index():
	if 'username' in session:
		username = session['username']
		return 'Logged in as ' + username + '<br>' + \
         "<b><a href = '/logout'>click here to log out</a></b>"
	return "You are not logged in <br><a href = '/login'></b>" + \
      "click here to log in</b></a>"


@app.route('/login', methods=['GET', 'POST'])
def login():
	if request.method == 'POST':
		session['username'] = request.form['username']
		return redirect(url_for('index'))
	return """

   <form action = "" method = "post">
      <p><input type = text name = username /></p>
      <p><input type = submit value = Login /></p>
   </form>

   """

@app.route('/logout')
def logout():
   # remove the username from the session if it is there
   session.pop('username', None)
   return redirect(url_for('index'))

'''


def print_iterator(iterator):
	print("count: " + str(iterator.count()))
	for item in iterator:
		print(str(item))

def bdTest():

	dbHandler = DbHandler()

	'''
	print("delete all users")
	dbHandler.delete_user({"username":"aryeko"})
	dbHandler.delete_user({"username":"tomerac"})
	'''


	print("current units")
	print_iterator(dbHandler.get_units())

	print("Adding Super Unit")
	unit_id = dbHandler.add_unit("Super Unit")

	print("current units")
	print_iterator(dbHandler.get_units())


	print("current users")
	print_iterator(dbHandler.get_users())


	print("Adding Arye")
	dbHandler.add_user("Arye Kogan", "aryeko", "1234", unit_id)

	print("Adding Tomer")
	dbHandler.add_user("Tomer Achdut", "tomerac", "1234", unit_id)

	print("current users")
	print_iterator(dbHandler.get_users())



	print("current materials")
	print_iterator(dbHandler.get_materials())


	print("Adding Cocaine")
	dbHandler.add_material("Cocaine", "narcotic", 12)

	print("Adding XTZ")
	dbHandler.add_material("XTZ", "narcotic", 55)

	print("current materials")
	print_iterator(dbHandler.get_materials())



	print("current G-Scans")
	print_iterator(dbHandler.get_gscans())


	print("Adding G-Scan 1")
	dbHandler.add_gscan("1111", unit_id)


	print("current G-Scans")
	print_iterator(dbHandler.get_gscans())


	print("current areas")
	print_iterator(dbHandler.get_areas())


	print("Adding area")
	dbHandler.add_area("Road block", (1122, 3344), 100)

	print("current areas")
	print_iterator(dbHandler.get_areas())