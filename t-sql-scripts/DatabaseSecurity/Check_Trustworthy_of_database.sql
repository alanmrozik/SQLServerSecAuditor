/*
Description:
The	TRUSTWORTHY database	option	allows	database	objects	to	access	objects	in	other	
databases	under	certain	circumstances.
Rationale:
Provides	protection	from	malicious	CLR	assemblies	or	extended	procedures.
*/
SELECT 
	name,
	CASE
	WHEN is_trustworthy_on = 1 THEN 'Enabled'
	WHEN is_trustworthy_on = 0 THEN 'Disabled'
	END AS [Status]
	FROM sys.databases
	WHERE is_trustworthy_on = 1
	AND name != 'msdb';
