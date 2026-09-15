/*
Description:
Checking Microsoft Distributed Transaction Coordinator configuration is important for features that require communication between servers, such as linked servers using distributed transactions.
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
    SELECT 'xp_cmdshell is disabled' AS [Status];
END;
