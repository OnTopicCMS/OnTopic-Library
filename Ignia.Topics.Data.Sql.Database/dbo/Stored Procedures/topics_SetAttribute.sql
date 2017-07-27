-----------------------------------------------------------------------------------------------------------------------------------------------
-- Procedure	topics_CreateAttribute
--
-- Purpose	Creates a new attribute.
--
-- History:	John Mulhausen		06232009  Created initial version.
--		Jeremy Caney		05282009  Added additional metadata properties.
-----------------------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_SetAttribute]
		@AttributeKey		NVARCHAR(124),
		@DisplayName		NVARCHAR(124),
		@DisplayGroup		NVARCHAR(124),
		@Description		NVARCHAR(255),
		@AttributeTypeID	INT
AS

SET NOCOUNT ON;

-----------------------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES
-----------------------------------------------------------------------------------------------------------------------------------------------
DECLARE		@AttributeID		INT

-----------------------------------------------------------------------------------------------------------------------------------------------
-- DEFINE ATTRIBUTE ID
-----------------------------------------------------------------------------------------------------------------------------------------------
SELECT		@AttributeID		= AttributeID
FROM		topics_Attributes
WHERE		AttributeKey		= @AttributeKey

-----------------------------------------------------------------------------------------------------------------------------------------------	
-- PROVIDE UPDATE.
-----------------------------------------------------------------------------------------------------------------------------------------------	
IF @AttributeID is null 
  BEGIN
    UPDATE	topics_Attributes
      SET	AttributeKey		= @AttributeKey		,
		DisplayName		= @DisplayName		,
		DisplayGroup		= @DisplayGroup		,
		Description		= @Description,
		AttributeTypeID		= @AttributeTypeID
    WHERE	AttributeID		= @AttributeID
  END

-----------------------------------------------------------------------------------------------------------------------------------------------	
-- PROVIDE INSERT.
-----------------------------------------------------------------------------------------------------------------------------------------------	
INSERT INTO	topics_Attributes (
		  AttributeKey		,
		  DisplayName		,
		  DisplayGroup		,
		  Description		,
		  AttributeTypeID
		)
VALUES (	@AttributeKey		, 
		@DisplayName		, 
		@DisplayGroup		,
		@Description		,
		@AttributeTypeID
)