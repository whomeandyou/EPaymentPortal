/****** Object:  StoredProcedure [dbo].[spInsert_PaymentTransaction]    Script Date: 31-Jan-20 10:33:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spInsert_PaymentTransaction]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spInsert_PaymentTransaction] AS' 
END
GO



ALTER PROCEDURE [dbo].[spInsert_PaymentTransaction]
	@ApplicationAccountId int,
	@Amount decimal(10,2),
	@TransactionNumber nvarchar(50),
	@OrderNumber nvarchar(50),
	@ReturnUrl nvarchar(1000),
	@MerchantId nvarchar(50),
	@PaymentProviderCode nvarchar(50),
	@Currency nvarchar(50),
	@IsEnrolment bit,
	@IsInitialPayment bit

AS
Begin
			insert into PaymentTransaction(
				CreatedOn,
				ApplicationAccountId,
				Amount,
				TransactionNumber,
				ReturnUrl,
				OrderNumber,
				MerchantId,
				PaymentProviderCode,
				Currency,
				IsEnrolment,
				IsInitialPayment
			)
			Values(
				GETDATE(),
				@ApplicationAccountId,
				@Amount,
				@TransactionNumber,
				@ReturnUrl,
				@OrderNumber,
				@MerchantId,
				@PaymentProviderCode,
				@Currency,
				@IsEnrolment,
				@IsInitialPayment
				);
			End
GO
