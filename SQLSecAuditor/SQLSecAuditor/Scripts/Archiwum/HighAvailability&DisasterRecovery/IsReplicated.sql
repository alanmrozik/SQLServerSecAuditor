IF EXISTS (
    SELECT 1
    FROM sys.databases
    WHERE
        is_published = 1
        OR is_subscribed = 1
        OR is_merge_published = 1
        OR is_distributor = 1
)
BEGIN
    SELECT
        name,
        is_published,
        is_subscribed,
        is_merge_published,
        is_distributor
    FROM sys.databases
    WHERE
        is_published = 1
        OR is_subscribed = 1
        OR is_merge_published = 1
        OR is_distributor = 1;
END
ELSE
BEGIN
    SELECT 'Brak replikacji' AS [Status];
END;