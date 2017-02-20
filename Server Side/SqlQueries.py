class SqlQueries:
	CREATE_DB_LDS = "create database if not exists LDS"

	USE_DB_LDS = "use LDS"

	CREATE_TABLE_MATERIAL = '''create table if not exists Material
					(materialID int not null auto_increment,
					name varchar(30) not null,
					type varchar(20) not null,
					cas varchar(10) not null,
					primary key(materialID))'''

	CREATE_TABLE_USER = '''create table if not exists User
					(userID varchar(10) not null auto_increment,
					username varchar(30) not null,
					password varchar(30) not null,
					primary key(userID))'''

	CREATE_TABLE_USER = '''create table if not exists User
						(userID varchar(10) not null auto_increment,
						username varchar(30) not null,
						password varchar(30) not null,
						primary key(userID))'''

	INSERT_MATERIAL = "insert into material(material)"

	INSERT_PERSON = "insert into person(personID, name)"