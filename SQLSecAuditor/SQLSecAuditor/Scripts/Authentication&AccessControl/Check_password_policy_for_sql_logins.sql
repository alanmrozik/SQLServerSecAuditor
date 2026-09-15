/*
Description:
SQL Server passwords should follow the same password complexity policy used by Windows.
*/
IF EXISTS
(
    SELECT 1
    FROM sys.sql_logins
    WHERE is_policy_checked = 0
)
BEGIN
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
END
ELSE
BEGIN
    SELECT 'All SQL accounts enforce the system password policy' AS [Status];
END;
    /*Rationale:
Ensure	SQL	authenticated	login	passwords	comply	with	the	secure	password	policy	applied	
by	the	Windows Server Benchmark so that they cannot be easily compromised via brute force attack.
*/
