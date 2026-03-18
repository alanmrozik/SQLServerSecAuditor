/*
Description:
Applies	the	same	password	complexity	policy	used	in	Windows	to	passwords	used	inside	
SQL	Server.
Rationale:
Ensure	SQL	authenticated	login	passwords	comply	with	the	secure	password	policy	applied	
by	the	Windows Server Benchmark so that they cannot be easily compromised via brute force attack.
*/
SELECT name, is_disabled
FROM sys.sql_logins
WHERE is_policy_checked = 0;