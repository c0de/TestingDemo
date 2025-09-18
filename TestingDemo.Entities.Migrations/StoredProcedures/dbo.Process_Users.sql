/**
 * Procedure to process users.
 * @version 1.0
 */
CREATE OR ALTER PROCEDURE dbo.[Process_Users]
    @JobDetailId INT
AS
BEGIN

-- Sleep for 5 seconds for testing purposes
WAITFOR DELAY '00:00:05'

-- Additional processing could go here after the delay
PRINT 'User processing completed after 5-second delay'

END
