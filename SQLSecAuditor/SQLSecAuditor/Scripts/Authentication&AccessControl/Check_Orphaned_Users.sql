/*
Description:
Użytkownik bazy danych, dla którego odpowiadający mu login programu SQL Server jest niezdefiniowany lub niepoprawnie zdefiniowany w instancji serwera, nie może zalogować się do tej instancji - jest on określany mianem "użytkownika osieroconego" i powinien zostać usunięty.
*/
--USE <database_name>;
--GO
--EXEC sp_change_users_login @Action='Report';
IF NOT EXISTS
(
    SELECT 1
    FROM sys.databases d
    CROSS APPLY
    (
        SELECT COUNT(*) AS Cnt
        FROM sys.database_principals dp
        LEFT JOIN sys.server_principals sp
            ON dp.sid = sp.sid
        WHERE d.database_id > 4
    ) x
)
BEGIN
    PRINT 'Brak orphaned users';
END
ELSE
BEGIN
    CREATE TABLE #OrphanedUsers
    (
        DatabaseName SYSNAME,
        UserName SYSNAME,
        UserSID VARBINARY(85)
    );

    DECLARE @DatabaseName SYSNAME;
    DECLARE @SQL NVARCHAR(MAX);

    DECLARE db_cursor CURSOR FAST_FORWARD FOR
    SELECT name
    FROM sys.databases
    WHERE name not in ('master','msdb','model','tempdb','distribution')
      AND state_desc = 'ONLINE';

    OPEN db_cursor;

    FETCH NEXT FROM db_cursor INTO @DatabaseName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        CREATE TABLE #Temp
        (
            UserName SYSNAME,
            UserSID VARBINARY(85)
        );

        SET @SQL = '
        USE ' + QUOTENAME(@DatabaseName) + ';

        INSERT INTO #Temp
        EXEC sp_change_users_login @Action = ''Report'';

        INSERT INTO #OrphanedUsers (DatabaseName, UserName, UserSID)
        SELECT ''' + @DatabaseName + ''', UserName, UserSID
        FROM #Temp;
        ';

        EXEC (@SQL);

        DROP TABLE #Temp;

        FETCH NEXT FROM db_cursor INTO @DatabaseName;
    END

    CLOSE db_cursor;
    DEALLOCATE db_cursor;

    IF NOT EXISTS (SELECT 1 FROM #OrphanedUsers)
    BEGIN
        SELECT 'Brak orphaned users' AS Status;
    END
    ELSE
    BEGIN
        SELECT
            DatabaseName,
            UserName AS OrphanedUser
        FROM #OrphanedUsers
        ORDER BY DatabaseName, UserName;
    END

    DROP TABLE #OrphanedUsers;
END
/*Rationale:
Orphan	users	should	be	removed	to	avoid	potential	misuse	of	those	broken	users	in	any	
way.
*/