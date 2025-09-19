/**
 * Function that returns the full name of a user by combining first and last name
 * @version 1.0
 */
CREATE OR ALTER FUNCTION dbo.[GetUserFullName]
(
    @FirstName NVARCHAR(255),
    @LastName NVARCHAR(255)
)
RETURNS NVARCHAR(511)
AS
BEGIN
    DECLARE @FullName NVARCHAR(511)
    
    -- Handle null values and trim whitespace
    SET @FirstName = LTRIM(RTRIM(ISNULL(@FirstName, '')))
    SET @LastName = LTRIM(RTRIM(ISNULL(@LastName, '')))
    
    -- Combine names with appropriate spacing
    IF @FirstName = '' AND @LastName = ''
        SET @FullName = ''
    ELSE IF @FirstName = ''
        SET @FullName = @LastName
    ELSE IF @LastName = ''
        SET @FullName = @FirstName
    ELSE
        SET @FullName = @FirstName + ' ' + @LastName
    
    RETURN @FullName
END
