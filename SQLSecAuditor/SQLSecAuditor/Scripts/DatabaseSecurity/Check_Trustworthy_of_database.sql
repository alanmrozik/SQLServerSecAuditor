/*
Description:
Opcja bazy danych TRUSTWORTHY umożliwia obiektom bazy danych uzyskiwanie dostępu do obiektów w innych bazach danych w określonych okolicznościach.
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
    SELECT 'Żadna baza nie jest TRUSTWORTHY' AS [Status];
END;
/*Rationale:
Provides	protection	from	malicious	CLR	assemblies	or	extended	procedures.
*/
/*Fix:
ALTER DATABASE [<database_name>] SET TRUSTWORTHY OFF;
*/