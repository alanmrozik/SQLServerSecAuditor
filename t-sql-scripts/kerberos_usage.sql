SELECT auth_scheme 
FROM sys.dm_exec_connections 
WHERE session_id = @@SPID;
--output
--KERBEROS -ok
--NTLM -not ok, almost deprecated