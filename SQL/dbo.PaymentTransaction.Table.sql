/****** Object:  Table [dbo].[PaymentTransaction]    Script Date: 31-Jan-20 10:33:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentTransaction]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PaymentTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TransactionNumber] [nvarchar](50) NULL,
	[BuyerEmail] [nvarchar](500) NULL,
	[OrderNumber] [nvarchar](50) NOT NULL,
	[Amount] [decimal](10, 2) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastModifiedOn] [datetime] NULL,
	[ApplicationAccountId] [int] NOT NULL,
	[PaymentReferenceNumber] [nvarchar](50) NULL,
	[ReturnUrl] [nvarchar](1000) NOT NULL,
	[Bank] [nvarchar](500) NULL,
	[ResponsePayload] [text] NULL,
	[AuthorizationCode] [nvarchar](50) NULL,
	[MerchantId] [nvarchar](50) NULL,
	[AdditionalInfo] [nvarchar](1000) NULL,
	[PaymentProviderCode] [nvarchar](50) NOT NULL,
	[SessionId] [nvarchar](50) NULL,
	[SecureId] [nvarchar](50) NULL,
	[ProposalId] [nvarchar](50) NULL,
	[AppId] [nvarchar](50) NULL,
	[ResponseCode] [nvarchar](max) NULL,
	[ErrorMessage] [nvarchar](max) NULL,
	[CardType] [nvarchar](50) NULL,
	[CardMethod] [nvarchar](50) NULL,
	[AuthorizationNumber] [nvarchar](50) NULL,
	[IsEnrolment] [bit] NULL,
	[IsInitialPayment] [bit] NULL,
	[IsDifferentRenewalMethod] [bit] NULL,
	[Currency] [nvarchar](50) NULL,
	[CardNumber] [nvarchar](500) NULL,
	[ExpiryMonth] [nvarchar](10) NULL,
	[ExpiryYear] [nvarchar](10) NULL,
	[TransactionStatusId] [int] NOT NULL,
	[MsgToken] [nvarchar(10)] NOT NULL,
 CONSTRAINT [PK_PaymentTransaction1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_PaymentTransaction_TransactionStatusId]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[PaymentTransaction] ADD  CONSTRAINT [DF_PaymentTransaction_TransactionStatusId]  DEFAULT ((0)) FOR [TransactionStatusId]
END
GO
