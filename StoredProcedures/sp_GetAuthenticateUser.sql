-- =========================================================================
-- Author:      Antigravity (Clean Architecture Restructuring)
-- Create date: 2026-06-02
-- Description: Authenticates user by looking up username and company ID.
-- =========================================================================
CREATE PROCEDURE [dbo].[sp_GetAuthenticateUser]
    @UserName NVARCHAR(100),
    @CompanyID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 
        UserID,
        UserName,
        FirstName,
        LastName,
        Title,
        UserTypeId,
        CompanyID,
        ProfileImage
    FROM TabUserMaster WITH (NOLOCK)
    WHERE UserName = @UserName
      AND CompanyID = @CompanyID;
END
GO
