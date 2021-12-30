/****** Object:  Table [dbo].[BatchProcess]    Script Date: 28/2/2020 5:35:20 PM ******/
DROP TABLE [dbo].[BatchProcess]
GO

CREATE TABLE [dbo].[BatchProcess](
	[BatchID] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[FileName] [nvarchar](100) NOT NULL,
	[Status] [nvarchar](10) NOT NULL,
	[StatusDescription] [nvarchar](1000) NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL
) ON [PRIMARY]
GO


