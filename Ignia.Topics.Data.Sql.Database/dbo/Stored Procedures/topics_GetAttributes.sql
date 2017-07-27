-----------------------------------------------------------------------------------------------------------------------------------------------
-- Procedure	GET ATTRIBUTES
--
-- Purpose	Returns a list of available attributes.
--
-- History	John Mulhausen		04072009  Created initial version.
--		Jeremy Caney		05282010  Reformatted code and refactored identifiers for improved readability. 
--		Jeremy Caney		05292010  Added new attributes to list.
-----------------------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_GetAttributes]
AS

-----------------------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES
-----------------------------------------------------------------------------------------------------------------------------------------------
SET NOCOUNT ON;

-----------------------------------------------------------------------------------------------------------------------------------------------
-- RETURN LIST
-----------------------------------------------------------------------------------------------------------------------------------------------
SELECT		AttributeID				,
		AttributeKey				,
		DisplayName				,
		DisplayGroup				,
		Description				,
		SortOrder				,		
		IsVisible				,
		IsRequired				,
		DefaultValue				,
		StoreInBlob				,
		AttributeTypes.AttributeTypeID		,
		AttributeTypes.AttributeTypeName	,
		AttributeTypes.DefaultConfiguration
FROM		topics_Attributes 
INNER JOIN	topics_AttributeTypes AttributeTypes
  ON		AttributeTypes.AttributeTypeID		= topics_Attributes.AttributeTypeID