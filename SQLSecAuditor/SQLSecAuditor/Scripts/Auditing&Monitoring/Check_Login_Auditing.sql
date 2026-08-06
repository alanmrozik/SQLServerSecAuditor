/*
Description:
To ustawienie powoduje rejestrowanie nieudanych prób uwierzytelniania dla logowań do programu SQL Server w Errorlog.
Ustawienie to było historycznie dostępne we wszystkich wersjach i edycjach programu SQL Server. Przed wprowadzeniem funkcji SQL Server Audit był to jedyny dostępny mechanizm rejestrowania logowań (zarówno udanych, jak i nieudanych).
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