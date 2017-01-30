class SqlQueries:
	CREATE_DB_LDS = "create database if not exists LDS"
	USE_DB_LDS = "use LDS"
	CREATE_TABLE_MATERIAL = '''create table if not exists material
					(materialID int not null auto_increment,
					material varchar(30) not null,
					primary key(materialID))'''
	CREATE_TABLE_PERSON = '''create table if not exists person
					(personID varchar(10) not null,
					name varchar(30) not null,
					primary key(personID))'''
	INSERT_MATERIAL = "insert into material(material)"
	INSERT_PERSON = "insert into person(personID, name)"