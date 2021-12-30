/****** Object:  StoredProcedure [dbo].[spGet_PaymentTransaction_By_TransactionNumber]    Script Date: 31-Jan-20 10:33:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGet_PaymentTransaction_By_TransactionNumber]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGet_PaymentTransaction_By_TransactionNumber] AS' 
END
GO


ALTER PROCEDURE [dbo].[spGet_PaymentTransaction_By_TransactionNumber]
	@TransactionNumber nvarchar(50)
AS
	begin
		select * from [PaymentTransaction]
		where TransactionNumber=@TransactionNumber;
	end

GO
