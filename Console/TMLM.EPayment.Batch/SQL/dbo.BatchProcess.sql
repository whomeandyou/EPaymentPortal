CREATE TABLE [dbo].[BatchProcess](
	[BatchID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[FileName] [nvarchar](100) NOT NULL,
	[Status] [nvarchar](100) NOT NULL,
	[StatusDescription] [nvarchar](1000) NULL,

	[CreateDate] [datetime] NOT NULL DEFAULT(GETDATE()),
	[LastUpdated] [datetime] NOT NULL DEFAULT(GETDATE()),
)