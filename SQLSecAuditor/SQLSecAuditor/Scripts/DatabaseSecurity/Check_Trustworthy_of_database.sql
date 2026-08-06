/*
Description:
Opcja bazy danych TRUSTWORTHY umożliwia obiektom bazy danych uzyskiwanie dostępu do obiektów w innych bazach danych w określonych okolicznościach.
*/
SELECT 
	name as [Name],
	CASE
	WHEN is_trustworthy_on = 1 THEN 'Enabled'
	WHEN is_trustworthy_on = 0 THEN 'Disabled'
	END AS [Status]
	FROM sys.databases
	WHERE is_trustworthy_on = 1
	AND name != 'msdb';
/*Rationale:
Provides	protection	from	malicious	CLR	assemblies	or	extended	procedures.
*/