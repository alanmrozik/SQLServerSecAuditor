/*
Description:
This setting records failed SQL Server login authentication attempts in the error log.
It has historically been available in every SQL Server version and edition. Before SQL Server Audit, it was the only available mechanism for recording successful and failed logins.
*/
EXEC xp_loginconfig 'audit level'; 
/*Rationale:
Capturing	failed	logins	provides	key	information	that	can	be	used	to	detect\confirm	
password	guessing	attacks.	Capturing	successful	login	attempts	can	be	used	to	confirm	
server	access	during	forensic	investigations,	but	using	this	audit	level	setting	to	also	
capture	successful	logins	creates	excessive noise	in	the	SQL	Server	Errorlog which	can	
hamper	a	DBA	trying	to	troubleshoot	problems.	Elsewhere	in	this	benchmark,	we	
recommend	using	the	newer	lightwieght SQL Server Audit feature to capture both succesful and failed logins.
*/
/*Fix:
EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', 
N'Software\Microsoft\MSSQLServer\MSSQLServer', N'AuditLevel', 
REG_DWORD, 2 
*/
