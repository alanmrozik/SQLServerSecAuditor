/*
Description:
Prior	to	SQL	Server	2008,	the	BUILTIN\Administrators group	was	added	as	a	SQL	Server	login	with	sysadmin	privileges	during	installation	by	default.	Best	practices	promote	creating	an	Active	Directory	level	group	containing	approved	DBA	staff	accounts	and	using	
this	controlled	AD	group	as	the	login	with	sysadmin	privileges.	The	AD	group	should	be	
specified	during	SQL	Server	installation	and	the	BUILTIN\Administrators group	would	
therefore	have	no	need	to	be	a	login.
*/
SELECT pr.[name] as [Name],
 pe.[permission_name] as [Permission name],
  pe.[state_desc] as [Status]
FROM sys.server_principals pr
JOIN sys.server_permissions pe
ON pr.principal_id = pe.grantee_principal_id
WHERE pr.name like 'BUILTIN%';
/*Rationale:
The	BUILTIN groups	(Administrators,	Everyone,	Authenticated	Users,	Guests,	etc.)	generally	
contain	very	broad	memberships	which	would	not	meet	the	best	practice	of	ensuring	only	
the	necessary	users	have	been	granted	access	to	a	SQL	Server	instance.	These	groups	
should	not	be	used	for	any	level	of	access	into	a	SQL	Server	Database	Engine	instance.
*/