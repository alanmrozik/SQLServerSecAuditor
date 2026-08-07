/*
Description:
Wyłączenie opcji Database Mail XPs ogranicza powierzchnię ataku na serwer SQL, eliminuje wektor ataku typu DoS oraz kanał służący do eksfiltracji danych z serwera bazy danych na host zdalny.

*/
SELECT 
    name as [Name], 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'Database Mail XPs';
    /*Rationale:
Disabling	the	Database Mail XPs option	reduces	the	SQL	Server	surface,	eliminates	a	DOS	
attack	vector	and	channel	to	exfiltrate	data	from	the	database	server	to	a	remote host*/