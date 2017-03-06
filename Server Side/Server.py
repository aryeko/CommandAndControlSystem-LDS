from JsonDataParser import JsonDataParser
from datetime import timedelta
from DbHandler import DbHandler
from socket import *
import sys

from flask import Flask, request, session, g, redirect, url_for, abort, render_template, flash


dbHandler = DbHandler()


app = Flask(__name__)

# Load default config and override config from an environment variable
app.config.update(dict(
    DEBUG=True,
    SECRET_KEY='development key',
    USERNAME='admin',
    PASSWORD='default'
))
app.config.from_envvar('FLASKR_SETTINGS', silent=True)

@app.before_request
def make_session_permanent():
    session.permanent = True
    app.permanent_session_lifetime = timedelta(minutes=5)


@app.route("/")
def hello():
	return "Hello World!"

@app.route('/user', methods=['POST'])
def add_user():
	#TODO: verificate the users


	unitId = dbHandler.get_units()[0]['_id']
	print(unitId)
	return str(dbHandler.add_user(request.form.get('fullname'), request.form.get('username'), request.form.get('password'), unitId))

@app.route('/login', methods=['GET', 'POST'])
def login():
	if request.method == 'POST':
		username = request.form['username']
		print("authenticating username: " + username)
		users = dbHandler.get_users({'username':username})
		print("found {} users".format(users.count()))
		if users.count() == 1:
			if users[0]['password'] == request.form['password']:
				print("user is authorized")
				session['logged_in'] = True
				flash('You were logged in')
				return "SUCESS"

		return "AUTH_FAILED"
	else:
		return "GET is not supported yet"





if __name__ == '__main__':
	app.run()
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