/****** Object:  StoredProcedure [dbo].[spGet_ApplicationAccount_By_Id]    Script Date: 31-Jan-20 10:33:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGet_ApplicationAccount_By_Id]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGet_ApplicationAccount_By_Id] AS' 
END
GO


ALTER PROCEDURE [dbo].[spGet_ApplicationAccount_By_Id]
	@Id int
AS
	begin
		select * from [ApplicationAccount]
		where Id=@Id;
	end

GO
