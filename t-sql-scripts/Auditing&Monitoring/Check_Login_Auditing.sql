/*
Description:
This	setting	will	record	failed	authentication	attempts	for	SQL	Server	logins	to	the	SQL	
Server	Errorlog.	This	is	the	default	setting	for	SQL	Server.
Historically,	this	setting	has	been	available	in	all	versions	and	editions	of	SQL	Server.	Prior	
to	the	availability	of	SQL	Server	Audit,	this	was	the	only	provided	mechanism	for	
capturing	logins	(successful	or	failed).
Rationale:
Capturing	failed	logins	provides	key	information	that	can	be	used	to	detect\confirm	
password	guessing	attacks.	Capturing	successful	login	attempts	can	be	used	to	confirm	
server	access	during	forensic	investigations,	but	using	this	audit	level	setting	to	also	
capture	successful	logins	creates	excessive noise	in	the	SQL	Server	Errorlog which	can	
hamper	a	DBA	trying	to	troubleshoot	problems.	Elsewhere	in	this	benchmark,	we	
recommend	using	the	newer	lightwieght SQL Server Audit feature to capture both succesful and failed logins.
*/
EXEC xp_loginconfig 'audit level'; 