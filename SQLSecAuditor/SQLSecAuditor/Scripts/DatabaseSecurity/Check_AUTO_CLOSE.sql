/*
Description:
Parametr AUTO_CLOSE określa, czy dana baza danych jest zamykana po zakończeniu połączenia. 
Jeśli jest on włączony, kolejne połączenia z tą bazą będą wymagały jej ponownego otwarcia oraz odbudowania odpowiednich pamięci podręcznych procedur.
*/
IF EXISTS
(
    SELECT 1
    FROM sys.databases
    WHERE is_auto_close_on = 1
)
BEGIN
    SELECT 
        name AS [Name], 
        CASE
            WHEN is_auto_close_on = 1 THEN 'Enabled'
            WHEN is_auto_close_on = 0 THEN 'Disabled'
        END AS [Status]
    FROM sys.databases
    WHERE is_auto_close_on = 1;
END
ELSE
BEGIN
    SELECT 'Żadna baza danych nie ma włączonego AUTO_CLOSED' AS [Status];
END;
    /*Rationale:
Because	authentication	of	users	for	contained	databases	occurs	within	the	database	not	at	
the	server\instance	level,	the	database	must	be	opened	every	time	to	authenticate	a	user.	
The	frequent	opening/closing	of	the	database consumes additional server resources and may contribute to a denial of service.
*/