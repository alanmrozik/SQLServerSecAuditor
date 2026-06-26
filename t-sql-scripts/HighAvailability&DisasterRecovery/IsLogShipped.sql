/*primary*/
SELECT *
FROM msdb.dbo.log_shipping_primary_databases;

/*secondary*/
SELECT *
FROM msdb.dbo.log_shipping_secondary_databases;

EXEC master.dbo.sp_help_log_shipping_monitor;