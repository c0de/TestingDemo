/**
 * Function that calculates the number of days since a user account was created
 * @version 1.0
 */
CREATE OR ALTER FUNCTION dbo.[GetUserAccountAge]
(
    @CreatedAt DATETIME2
)
RETURNS INT
AS
BEGIN
    DECLARE @AccountAge INT
    
    -- Calculate the difference in days from creation date to current date
    IF @CreatedAt IS NULL
        SET @AccountAge = 0
    ELSE
        SET @AccountAge = DATEDIFF(DAY, @CreatedAt, GETDATE())
    
    -- Ensure we don't return negative values for future dates
    IF @AccountAge < 0
        SET @AccountAge = 0
    
    RETURN @AccountAge
END
