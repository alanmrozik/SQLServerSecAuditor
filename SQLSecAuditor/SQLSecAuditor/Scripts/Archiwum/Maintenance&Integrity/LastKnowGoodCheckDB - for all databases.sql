SET NOCOUNT ON;
DBCC TRACEON (3604);

CREATE TABLE #temp (
        Id INT IDENTITY(1,1), 
        ParentObject VARCHAR(255),
        [Object] VARCHAR(255),
        Field VARCHAR(255),
        [Value] VARCHAR(255)
)

CREATE TABLE #Results (
        DBName VARCHAR(255),
        LastGoodDBCC VARCHAR(255)
)

DECLARE @Name VARCHAR(255);

DECLARE looping_cursor CURSOR
FOR

SELECT name
FROM master.dbo.sysdatabases
WHERE CONVERT(varchar(500),databasepropertyex(name, 'Status'),0) = 'ONLINE'

OPEN looping_cursor
FETCH NEXT FROM looping_cursor INTO @Name
WHILE @@FETCH_STATUS = 0
    BEGIN

        INSERT INTO #temp
        EXECUTE('DBCC PAGE (['+@Name+'], 1, 9, 3)WITH TABLERESULTS');

        INSERT INTO #Results
        SELECT @Name,MAX(VALUE) FROM #temp
        WHERE Field = 'dbi_dbccLastKnownGood';

    FETCH NEXT FROM looping_cursor INTO @Name
    END
CLOSE looping_cursor;
DEALLOCATE looping_cursor;

SELECT DBName
    ,ISNULL(LastGoodDBCC,'1900-01-01 00:00:00.000') AS 'LastGoodDBCC'
FROM #Results

DROP TABLE #temp
DROP TABLE #Results