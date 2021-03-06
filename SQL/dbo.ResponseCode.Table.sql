/****** Object:  Table [dbo].[ResponseCode]    Script Date: 31-Jan-20 10:33:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResponseCode]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ResponseCode](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PaymentProvider] [nvarchar](50) NOT NULL,
	[Code] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](1000) NOT NULL,
	[TMLMStatus] [int] NOT NULL,
 CONSTRAINT [PK_ResponseCode] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
