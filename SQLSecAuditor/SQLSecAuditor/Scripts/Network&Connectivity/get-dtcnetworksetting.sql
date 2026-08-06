/*
Description:
Get-DtcNetworkSetting
*/
IF EXISTS (
    SELECT 1
    FROM sys.configurations
    WHERE name = 'xp_cmdshell'
      AND value_in_use = 1
)
BEGIN
    DECLARE @query VARCHAR(8000);

    SET @query = 'powershell "Get-DtcNetworkSetting"';

    EXEC xp_cmdshell @query;
END
ELSE
BEGIN
    SELECT 'xp_cmdshell jest wyłączony' AS [Status];
END;