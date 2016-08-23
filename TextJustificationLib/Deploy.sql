-- First, drop everything in backwards order
IF  EXISTS (SELECT * FROM sys.procedures procs WHERE procs.name = N'GetJustifiedText')
	DROP PROCEDURE [dbo].[GetJustifiedText]
GO
GO
 
IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'TextJustificationLib')
    DROP ASSEMBLY [TextJustificationLib]
GO
IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'FSCore')
    DROP ASSEMBLY [FSCore]
GO
 
-- Now, create everything in forwards order
CREATE ASSEMBLY FSCore FROM 'C:\Users\mark.atkinson\Source\Repos\TextJustificationLib\TextJustificationLib\bin\Debug\FSharp.Core.dll' WITH PERMISSION_SET = UNSAFE
GO
CREATE ASSEMBLY SqlClr FROM 'C:\Users\mark.atkinson\Source\Repos\TextJustificationLib\TextJustificationLib\bin\Debug\TextJustificationLib.dll'
GO
 
-- External name is [SqlAssemblyName].[Full typename].[Method name]
CREATE PROCEDURE [dbo].[GetJustifiedText]
	@text [nvarchar](max),
	@line_width [int]
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [TextJustificationLib].[TextJustificationLib.SqlClrQuery].[GetJustifiedText]
GO