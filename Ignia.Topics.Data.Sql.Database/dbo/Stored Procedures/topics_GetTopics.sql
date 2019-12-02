-----------------------------------------------------------------------------------------------------------------------------------------------
-- Procedure	GET TOPICS
--
-- Purpose	Gets the tree of current topics rooted FROM the provided TopicID.  If no TopicID is provided then the sproc returns everything
--		under the topic with the lowest id.
--
-- History      Casey Margell		04062009  Initial Creation based on code FROM Celko's SQL For Smarties.
--		Jeremy Caney		07192009  Added support for AttributeID lookup.
--		Jeremy Caney		05282010  Reformatted code AND refactored identifiers for improved readability.
--		Jeremy Caney		06072010  Added support for blob fields.
--		Hedley Robertson	07062010  Added support for related items.
--		Jeremy Cane		09272013  Removed dependency on Attributes, in favor of Oroboros Configuration.
--		Jeremy Caney		09282013  Injected separate data set for key attribute values.
--		Katherine Trunkey	08072014  Added ROW_NUMBER functionality to (indexed) Attributes selection and TOP 1 selection to Blob
--					to support versioning of Topic Attributes.
--		Jeremy Caney		07232017  Restricted blob results to latest version, and only non-empty results
--		Jeremy Caney		07232017  Restricted versions to last five versions
--		Jeremy Caney		07242017  Removed unnecessary sorts; added indexes to temporary table
--		Jeremy Caney		07252017  Removed support for depth and versioning; versions should be loaded via new loadVersion sproc
-----------------------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[topics_GetTopics]
		@TopicID	int			= -1,
		@DeepLoad	bit			= 1,
		@TopicKey	nvarchar(255)		= null
AS

-----------------------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND DEFINE VARIABLES
-----------------------------------------------------------------------------------------------------------------------------------------------

-----------------------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC ID IF UNKNOWN.
-----------------------------------------------------------------------------------------------------------------------------------------------
IF @TopicKey IS NOT NULL
  BEGIN
    SELECT	@TopicID = TopicID
    FROM	topics_TopicAttributes
    WHERE	AttributeKey = 'Key'
      AND	AttributeValue like @TopicKey
  END

IF @TopicID < 0
  BEGIN
    SELECT	TOP 1
		@TopicID = TopicID
    FROM	topics_Topics
    ORDER BY	TopicID ASC
  END

-----------------------------------------------------------------------------------------------------------------------------------------------
-- CREATE TEMP TABLES
-----------------------------------------------------------------------------------------------------------------------------------------------
CREATE TABLE
#Topics (
  TopicID	int,
  SortOrder	int
)

CREATE CLUSTERED INDEX IDX_C_Topics_SortOrder ON #Topics(SortOrder)
CREATE INDEX IDX_Topics_TopicID ON #Topics(TopicID, SortOrder)

-----------------------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC AND DESCENDENTS
-----------------------------------------------------------------------------------------------------------------------------------------------
IF @DeepLoad = 1
  BEGIN
    INSERT
    INTO	#Topics (
		  TopicID,
		  SortOrder
		)
    SELECT	T1.TopicID,
		T1.RangeLeft
    FROM	topics_Topics		AS T1
    INNER JOIN	topics_Topics		AS T2
    ON		T1.RangeLeft
      BETWEEN	T2.RangeLeft
        AND	ISNULL(T2.RangeRight, 0)
      AND	T2.TopicID		= @TopicID
    ORDER BY	T1.RangeLeft
    OPTION	(OPTIMIZE FOR		(@TopicID = 1))
  END

-----------------------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC ONLY
-----------------------------------------------------------------------------------------------------------------------------------------------
ELSE
  BEGIN
    INSERT
    INTO	#Topics (
		  TopicID,
		  SortOrder
		)
    SELECT	TopicID,
		1
    FROM	topics_Topics
    WHERE	TopicID			= @TopicID
    OPTION	(OPTIMIZE FOR		(@TopicID UNKNOWN))
  END

-----------------------------------------------------------------------------------------------------------------------------------------------
-- SELECT KEY ATTRIBUTES
-----------------------------------------------------------------------------------------------------------------------------------------------
SELECT		TopicIndex.TopicID,
  		TopicIndex.ContentType,
  		TopicIndex.ParentID,
  		TopicIndex.TopicKey,
  		Storage.SortOrder
FROM		topics_TopicIndex AS TopicIndex
JOIN		#Topics AS Storage
  ON		Storage.TopicID = TopicIndex.TopicID
ORDER BY	SortOrder

-----------------------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC ATTRIBUTES
-----------------------------------------------------------------------------------------------------------------------------------------------
SELECT		TopicAttributes.TopicID,
		TopicAttributes.AttributeKey,
		TopicAttributes.AttributeValue
FROM		topics_TopicAttributeIndex TopicAttributes
JOIN		#Topics AS Storage
  ON		Storage.TopicID = TopicAttributes.TopicID

-----------------------------------------------------------------------------------------------------------------------------------------------
-- SELECT BLOB
-----------------------------------------------------------------------------------------------------------------------------------------------
SELECT		TopicBlob.TopicID,
		TopicBlob.Blob
FROM		topics_BlobIndex AS TopicBlob
JOIN		#Topics AS Storage
  ON		Storage.TopicID = TopicBlob.TopicID
--AND		TopicBlob.Blob.exist('/attributes/attribute') >= 1
--ORDER BY	TopicBlob.TopicID ASC

-----------------------------------------------------------------------------------------------------------------------------------------------
-- SELECT RELATIONSHIPS
-----------------------------------------------------------------------------------------------------------------------------------------------
;SELECT		Relationships.RelationshipTypeID,
		Relationships.Source_TopicID,
		Relationships.Target_TopicID
FROM		topics_Relationships Relationships
JOIN		#Topics AS Storage
  ON		Storage.TopicID = Relationships.Source_TopicID

-----------------------------------------------------------------------------------------------------------------------------------------------
-- SELECT HISTORY
-----------------------------------------------------------------------------------------------------------------------------------------------
;WITH TopicVersions AS (
  SELECT	Distinct
			Attributes.TopicID,
		Attributes.Version,
		RowNumber = ROW_NUMBER() OVER (
		  Partition by Attributes.TopicID
		  ORDER BY	Version DESC
		)
  FROM		topics_TopicAttributes Attributes
  JOIN		#Topics AS Storage
    ON		Storage.TopicID = Attributes.TopicID
  Group by	Attributes.TopicID, Attributes.Version
)
SELECT	TopicId,
		Version
FROM		TopicVersions
WHERE		RowNumber <= 6