SELECT    
    SERVERPROPERTY('MachineName') AS [Server Name],  
    CASE /*SQL Server Instance Name*/  
    WHEN SERVERPROPERTY('InstanceName') IS NULL THEN 'MSSQLSERVER'  
    ELSE SERVERPROPERTY('InstanceName')  
    END AS  [SQL Server Instance Name],   
    SERVERPROPERTY('Edition') AS [Edition],  
    SERVERPROPERTY('ProductVersion') AS [Product Version],    
    SERVERPROPERTY('ProductLevel') AS [Product Level],
    create_date AS [Uptime],  
    SERVERPROPERTY('ResourceLastUpdateDateTime') AS [Approximately Last Update Date] /*If >2months, check for updates*/
    FROM sys.databases where name = 'tempdb'; /*instance restart = create tempdb*/
GO
