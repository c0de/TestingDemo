/**
 * Table-valued function that returns user statistics by role
 * @version 1.0
 */
CREATE OR ALTER FUNCTION dbo.[GetUserStatsByRole]
()
RETURNS TABLE
AS
RETURN
(
    SELECT 
        [Role],
        COUNT(*) AS TotalUsers,
        COUNT(CASE WHEN DeletedAt IS NULL THEN 1 END) AS ActiveUsers,
        COUNT(CASE WHEN DeletedAt IS NOT NULL THEN 1 END) AS InactiveUsers,
        MIN(CreatedAt) AS EarliestUserCreated,
        MAX(CreatedAt) AS LatestUserCreated,
        AVG(DATEDIFF(DAY, CreatedAt, GETDATE())) AS AvgAccountAgeDays
    FROM dbo.Users
    GROUP BY [Role]
)
