

/*
Description:
Egzekwowanie tego mechanizmu kontroli ogranicza ryzyko wykorzystania tych zasobów przez podmiot do celów szkodliwych.
Rationale:
Enforcing	this	control	reduces	the	threat	of	an	entity	leveraging	these	facilities	for	
malicious	purposes.
*/
SELECT 
    name as [Name], 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'scan for startup procs';
        /*


*/