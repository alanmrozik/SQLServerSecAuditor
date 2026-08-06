/*
Description:
Konta serwisowe zalogowane do usług SQL Server powinny być kontami domenowymi z wyłączoną możliwością logowania do innych usług.
*/
SELECT 
    servicename as [Service name],
    service_account as [Service account name],
    status_desc as [Status],
    process_id as [Process ID] 
    FROM sys.dm_server_services
