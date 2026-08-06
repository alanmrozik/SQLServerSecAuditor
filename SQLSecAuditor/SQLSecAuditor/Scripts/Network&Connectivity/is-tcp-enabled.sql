/*
*/
DECLARE @tcpEnabled INT;
EXEC master.dbo.xp_instance_regread    
N'HKEY_LOCAL_MACHINE',    
N'SOFTWARE\Microsoft\Microsoft SQL Server\MSSQLServer\SuperSocketNetLib\Tcp',    
N'Enabled',    
@tcpEnabled OUTPUT;
SELECT         
    'Is TCP\IP Enabled?',    
    CASE        
    WHEN @tcpEnabled = 1 THEN 'YES'        
    WHEN @tcpEnabled = 0 THEN 'NO'        
    ELSE 'IDK'    
    END AS 'Answer'