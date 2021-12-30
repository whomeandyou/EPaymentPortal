/****** Object:  Table [dbo].[BankList]    Script Date: 31-Jan-20 10:33:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankList]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[BankList](
	[BankCode] [nvarchar](10) NOT NULL,
	[BankName] [nvarchar](max) NOT NULL,
	[MsgToken] [nvarchar](5) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
