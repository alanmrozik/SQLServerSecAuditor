/*
Description:
The	sa account	is	a	widely	known	and	often	widely	used	SQL	Server	account	with	sysadmin	
privileges.	This	is	the	original	login	created	during	installation	and	always	has	the	
principal_id=1 and	sid=0x01.
*/
SELECT 
    name as [Name], 
    CASE 
    WHEN is_disabled = 1 THEN 'Disabled'
    WHEN is_disabled = 0 THEN 'Enabled'
    END as [Status]
    FROM sys.server_principals
    WHERE sid = 0x01;
    /*
    Rationale:
Enforcing	this	control	reduces	the	probability	of	an	attacker	executing	brute	force	attacks	
against	a	well-known	principal.
*/