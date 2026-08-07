/*
Description:
Funkcjonalność ta może zostać wykorzystana do przeprowadzenia ataku typu Denial-of-Service (DoS) na zdalne serwery poprzez przeniesienie procesu przetwarzania zapytań na system docelowy.
*/
SELECT 
    name as [Name], 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'remote access';
    /*
    Rationale:
Functionality	can	be	abused	to	launch	a	Denial-of-Service	(DoS)	attack	on	remote	servers	
by	off-loading	query	processing	to	a	target.*/