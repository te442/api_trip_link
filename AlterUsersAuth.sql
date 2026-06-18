-- הוספת שדות אימות למשתמשים
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Email')
BEGIN
    ALTER TABLE Users ADD Email NVARCHAR(256) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE Users ADD PasswordHash NVARCHAR(500) NULL;
END
GO
