/*
Description:
Zaleca się stosowanie do haseł używanych w programie SQL Server tych samych zasad złożoności haseł, które są stosowane w systemie Windows.
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
    SELECT 'Wszystkie konta SQL mają wdrożone systemowe polityki haseł' AS [Status];
END;
    /*Rationale:
Ensure	SQL	authenticated	login	passwords	comply	with	the	secure	password	policy	applied	
by	the	Windows Server Benchmark so that they cannot be easily compromised via brute force attack.
*/