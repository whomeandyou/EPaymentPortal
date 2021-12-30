
CREATE TABLE [dbo].[PaymentRequestLog](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[BatchId] [int] NOT NULL,
	[Status] [nvarchar](100) NOT NULL,
	[RequestUrl] [nvarchar](1000) NULL,
	[RequestBody] [nvarchar](MAX) NULL,
	[ResponseBody] [nvarchar](MAX) NULL,

	[CreateDate] [datetime] NOT NULL DEFAULT(GETDATE()),
	[LastUpdated] [datetime] NOT NULL DEFAULT(GETDATE()),
)