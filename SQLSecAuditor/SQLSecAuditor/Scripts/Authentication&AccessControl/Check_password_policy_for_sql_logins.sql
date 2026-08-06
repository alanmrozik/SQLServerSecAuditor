/*
Description:
Applies	the	same	password	complexity	policy	used	in	Windows	to	passwords	used	inside	
SQL	Server.
*/
SELECT 
    name AS [Login name], 
    CASE
    WHEN is_disabled = 1 THEN 'Login disabled'
    WHEN is_disabled = 0 THEN 'Login enabled'
    END AS [Login status],
    CASE
    WHEN is_policy_checked = 1 THEN 'Policy checked'
    WHEN is_policy_checked = 0 THEN 'Policy not checked'
    END AS [Policy status]
    FROM sys.sql_logins
    WHERE is_policy_checked = 0;
    /*Rationale:
Ensure	SQL	authenticated	login	passwords	comply	with	the	secure	password	policy	applied	
by	the	Windows Server Benchmark so that they cannot be easily compromised via brute force attack.
*/