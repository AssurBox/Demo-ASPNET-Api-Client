CREATE TABLE [Technical].[Logs]
(
	[Id] INT NOT NULL PRIMARY KEY identity,
	[Title] nvarchar(max) null,
	[Content] nvarchar(max) null,
	[LogDate] datetime not null default(getutcdate())
)
