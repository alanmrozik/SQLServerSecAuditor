/*
Description:
Default trace zapewnia rejestrowanie działań w bazie danych na potrzeby audytu, w tym tworzenia kont, podnoszenia uprawnień oraz wykonywania poleceń DBCC.
*/
SELECT 
    name, 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'default trace enabled';

    /*Rationale:
Default	trace	provides	valuable	audit	information	regarding	security-related	activities	on	
the	server.
*/
/*Fix:
EXECUTE sp_configure 'show advanced options', 1; 
RECONFIGURE; 
EXECUTE sp_configure 'default trace enabled', 1; 
RECONFIGURE; 
GO 
EXECUTE sp_configure 'show advanced options', 0; 
RECONFIGURE; 
*/