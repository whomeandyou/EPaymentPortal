/****** Object:  StoredProcedure [dbo].[spGet_BankList]    Script Date: 31-Jan-20 10:33:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGet_BankList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGet_BankList] AS' 
END
GO

ALTER procedure [dbo].[spGet_BankList]
as
begin

select * from BankList;

end;
GO
