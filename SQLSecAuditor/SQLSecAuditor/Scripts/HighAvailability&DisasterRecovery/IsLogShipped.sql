/*Description:
Warto jest mieć AG
*/
IF EXISTS (
    SELECT 1
    FROM msdb.dbo.log_shipping_primary_databases
)
OR EXISTS (
    SELECT 1
    FROM msdb.dbo.log_shipping_secondary_databases
)
BEGIN
    /* Primary */
    SELECT *
    FROM msdb.dbo.log_shipping_primary_databases;

    /* Secondary */
    SELECT *
    FROM msdb.dbo.log_shipping_secondary_databases;

    EXEC master.dbo.sp_help_log_shipping_monitor;
END
ELSE
BEGIN
    SELECT 'Brak Log shipping' AS [Status];
END;