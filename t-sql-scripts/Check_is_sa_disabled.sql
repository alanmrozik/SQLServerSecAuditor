/*
Description:
The	sa account	is	a	widely	known	and	often	widely	used	SQL	Server	account	with	sysadmin	
privileges.	This	is	the	original	login	created	during	installation	and	always	has	the	
principal_id=1 and	sid=0x01.
Rationale:
Enforcing	this	control	reduces	the	probability	of	an	attacker	executing	brute	force	attacks	
against	a	well-known	principal.
*/
SELECT name, is_disabled
FROM sys.server_principals
WHERE sid = 0x01
AND is_disabled = 0;