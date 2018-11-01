CREATE TABLE [AssurBox].[CarGreenCardRequest]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[RequestDate] datetime not null,
	[RequestId] nvarchar(max) not null,
	[RawRequest] nvarchar(max) not null,
	[RequestRespondDate] datetime null,
	[ResponseInfo] nvarchar(max) null

)
