DROP TABLE [dbo].[MPGSBatchProcessResult]
GO

CREATE TABLE [dbo].[MPGSBatchProcessResult](
	[ProcessID] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[BatchID] INT NOT NULL,
	[merchantID] [nvarchar](50) NOT NULL,
	[RecordType] [nvarchar](25) NOT NULL,
	[MechantNumber] [nvarchar](50) NOT NULL,
	[AccountName] [nvarchar](500) NOT NULL,
	[AccountAddress] [nvarchar](500) NOT NULL,
	[PhoneNumber] [nvarchar](13) NOT NULL,
	[PolicyNumber] [nvarchar](15) NOT NULL,
	[PostCode] [nvarchar](10) NOT NULL,
	[SumAssured] [decimal](18, 0) NOT NULL,
	[OrderID] [nvarchar](50) NOT NULL,
	[TransactionID] [nvarchar](50) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Currency] [nvarchar](3) NOT NULL,
	[BankCard] [nvarchar](30) NOT NULL,
	[BankExpiry] [nvarchar](4) NOT NULL,
	[Result] [nvarchar](10) NOT NULL,
	[ErrorCause] [nvarchar](50) NULL,
	[ErrorDescription] [nvarchar](1000) NULL,
	[GatewayCode] [nvarchar](50) NULL,
	[ApiOperation] [nvarchar](15) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL
) ON [PRIMARY]
GO


