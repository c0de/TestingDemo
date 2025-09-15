CREATE OR ALTER VIEW dbo.[ActiveUsers]
WITH SCHEMABINDING
AS
SELECT
    Id,
	FirstName,
	LastName,
	Email,
    [Role],
	CreatedAt,
	DeletedAt
FROM
    dbo.Users
WHERE
    DeletedAt IS NULL
