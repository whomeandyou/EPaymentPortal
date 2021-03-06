/****** Object:  StoredProcedure [dbo].[spUpdate_PaymentTransaction_Information]    Script Date: 31-Jan-20 10:33:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spUpdate_PaymentTransaction_Information]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spUpdate_PaymentTransaction_Information] AS' 
END
GO



ALTER PROCEDURE [dbo].[spUpdate_PaymentTransaction_Information]
	@TransactionNumber	nvarchar(50),
	@BuyerEmail	nvarchar(500) = null,
	@SecureId	nvarchar(50) = null,
	@SessionId	nvarchar(50) = null,
	@ProposalId	nvarchar(50) = null,
	@IsDifferentRenewalMethod	bit = null,
	@MsgToken	nvarchar(10) = null
AS
Begin
			update PaymentTransaction set 
			[BuyerEmail]=IsNull(@BuyerEmail, BuyerEmail),
			[SecureId]=IsNull(@SecureId, SecureId),
			[SessionId]=IsNull(@SessionId, SessionId),
			[LastModifiedOn]=GETDATE(),
			[ProposalId]=IsNull(@ProposalId, ProposalId),
			[MsgToken]=IsNull(@MsgToken, MsgToken),
			[IsDifferentRenewalMethod]=IsNull(@IsDifferentRenewalMethod, IsDifferentRenewalMethod)
			where TransactionNumber = @TransactionNumber
			End
GO
