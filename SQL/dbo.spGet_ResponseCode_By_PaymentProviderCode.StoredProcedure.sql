/****** Object:  StoredProcedure [dbo].[spGet_ResponseCode_By_PaymentProviderCode]    Script Date: 31-Jan-20 10:33:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGet_ResponseCode_By_PaymentProviderCode]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGet_ResponseCode_By_PaymentProviderCode] AS' 
END
GO


ALTER PROCEDURE [dbo].[spGet_ResponseCode_By_PaymentProviderCode]
	@PaymentProvider nvarchar(50),
	@Code nvarchar(100)
AS
	begin
		select * from [ResponseCode]
		where Code=@Code
		and PaymentProvider=@PaymentProvider;
	end

GO
