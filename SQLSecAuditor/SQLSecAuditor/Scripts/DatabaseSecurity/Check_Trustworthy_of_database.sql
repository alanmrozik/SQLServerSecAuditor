/*
Description:
The TRUSTWORTHY database option allows database objects to access objects in other databases under specific circumstances.
*/
IF EXISTS
(
    SELECT 1
    FROM sys.databases
    WHERE is_trustworthy_on = 1
      AND name <> 'msdb'
)
BEGIN
    SELECT 
        name AS [Name],
        CASE
            WHEN is_trustworthy_on = 1 THEN 'Enabled'
            WHEN is_trustworthy_on = 0 THEN 'Disabled'
        END AS [Status]
    FROM sys.databases
    WHERE is_trustworthy_on = 1
      AND name <> 'msdb';
END
ELSE
BEGIN
    SELECT 'TRUSTWORTHY is disabled for all databases' AS [Status];
END;
/*Rationale:
Provides	protection	from	malicious	CLR	assemblies	or	extended	procedures.
*/
/*Fix:
ALTER DATABASE [<database_name>] SET TRUSTWORTHY OFF;
*/
