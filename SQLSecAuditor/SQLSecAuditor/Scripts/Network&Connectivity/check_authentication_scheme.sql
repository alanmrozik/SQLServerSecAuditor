/*
Description:
Authentication scheme
*/
SELECT auth_scheme as [Authentication scheme]
FROM sys.dm_exec_connections 
WHERE session_id = @@SPID;
--output
--KERBEROS -ok
--NTLM -not ok, almost deprecated

/*deeper check but you have to enable xp_cmdshell
DECLARE @query VARCHAR(8000); --query variable  
DECLARE @EngineServiceAccount VARCHAR(256);
SELECT @EngineServiceAccount =         
SUBSTRING(service_account, CHARINDEX('\',service_account)+1,LEN(service_account)) /*get login without domain name*/
FROM sys.dm_server_services 
WHERE filename LIKE '%sqlservr.exe%'          

/*check registrated spns*/        
SET @query = 'setspn -l ' + @EngineServiceAccount;
EXEC xp_cmdshell @query;/*get info about service account from AD*/
SET @query = 'powershell "Import-Module ActiveDirectory; Get-ADUser '+@EngineServiceAccount+' -Properties TrustedForDelegation,TrustedToAuthForDelegation"'
EXEC xp_cmdshell @query;  

/*get info about DTC*/ 
SET @query = 'powershell "Get-DtcNetworkSetting"'EXEC xp_cmdshell @query;  

/*check if tcp is enabled*/
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
    */