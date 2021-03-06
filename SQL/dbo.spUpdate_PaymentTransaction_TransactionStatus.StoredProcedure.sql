/****** Object:  StoredProcedure [dbo].[spUpdate_PaymentTransaction_TransactionStatus]    Script Date: 31-Jan-20 10:33:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spUpdate_PaymentTransaction_TransactionStatus]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spUpdate_PaymentTransaction_TransactionStatus] AS' 
END
GO



ALTER PROCEDURE [dbo].[spUpdate_PaymentTransaction_TransactionStatus]
	@TransactionNumber	nvarchar(50),
	@PaymentReferenceNumber	nvarchar(50) = null,
	@Bank	nvarchar(500) = null,
	@TransactionStatusId	int,
	@ResponsePayload	text,
	@AuthorizationCode	nvarchar(50) = null,
	@AuthorizationNumber	nvarchar(50) = null,
	@CardMethod	nvarchar(50) = null,
	@CardType	nvarchar(50) = null,
	@CardNumber	nvarchar(500) = null,
	@ExpiryMonth	nvarchar(10) = null,
	@ExpiryYear	nvarchar(10) = null,
	@AppId	nvarchar(50) = null,
	@ResponseCode	nvarchar(max) = null,
	@ErrorMessage	nvarchar(max) = null
AS
Begin
			update PaymentTransaction set 
			[TransactionStatusId]=@TransactionStatusId,
			[PaymentReferenceNumber]=@PaymentReferenceNumber,
			[Bank]=@Bank,
			[LastModifiedOn]=GETDATE(),
			[ResponsePayload]=@ResponsePayload,
			[AuthorizationCode]=@AuthorizationCode,
			[AuthorizationNumber]=@AuthorizationNumber,
			[CardMethod]=@CardMethod,
			[CardType]=@CardType,
			[CardNumber]=@CardNumber,
			[ExpiryMonth]=@ExpiryMonth,
			[ExpiryYear]=@ExpiryYear,
			[AppId]=@AppId,
			[ResponseCode]=@ResponseCode,
			[ErrorMessage]=@ErrorMessage
			where TransactionNumber = @TransactionNumber
			End
GO
