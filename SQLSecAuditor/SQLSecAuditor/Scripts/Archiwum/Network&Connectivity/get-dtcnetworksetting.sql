DECLARE @query VARCHAR(8000);
SET @query = 'powershell "Get-DtcNetworkSetting"'EXEC xp_cmdshell @query; 