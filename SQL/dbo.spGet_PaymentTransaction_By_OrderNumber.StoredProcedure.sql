USE [TMLM_Online]
GO
/****** Object:  StoredProcedure [dbo].[spGet_PaymentTransaction_By_OrderNumber]    Script Date: 21/2/2020 11:16:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGet_PaymentTransaction_By_OrderNumber]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGet_PaymentTransaction_By_OrderNumber] AS' 
END
GO


ALTER PROCEDURE [dbo].[spGet_PaymentTransaction_By_OrderNumber]
	@OrderNumber nvarchar(50)
AS
	begin
		select top 1 * from [PaymentTransaction]
		where OrderNumber=@OrderNumber
		order by CreatedOn desc;
	end

