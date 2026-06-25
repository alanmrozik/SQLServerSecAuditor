/*
Description:
Applies	the	same	password	expiration	policy	used	in	Windows	to	passwords	used	inside	
SQL	Server.
Rationale:
Ensuring	SQL	logins	comply	with	the	secure	password	policy	applied	by	the	Windows	
Server	Benchmark	will	ensure	the	passwords	for	SQL	logins	with	sysadmin privileges	are	
changed	on	a	frequent	basis	to	help	prevent	compromise	via	a	brute	force	attack.	CONTROL 
SERVER is	an	equivalent	permission	to	sysadmi and logins with that permission should also be required to have expiring passwords.
*/
SELECT l.[name], 'sysadmin membership' AS 'Access_Method',l.is_expiration_checked
FROM sys.sql_logins AS l
WHERE IS_SRVROLEMEMBER('sysadmin',name) = 1
AND l.is_expiration_checked <> 1
UNION ALL
SELECT l.[name], 'CONTROL SERVER' AS 'Access_Method',l.is_expiration_checked
FROM sys.sql_logins AS l
JOIN sys.server_permissions AS p
ON l.principal_id = p.grantee_principal_id
WHERE p.type = 'CL' AND p.state IN ('G', 'W')
AND l.is_expiration_checked <> 1;