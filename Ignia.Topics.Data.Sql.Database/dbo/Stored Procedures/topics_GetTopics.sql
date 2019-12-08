--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPICS
--------------------------------------------------------------------------------------------------------------------------------
-- Gets the tree of current topics rooted FROM the provided TopicID.  If no TopicID is provided then the sproc returns everything
--under the topic with the lowest id.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_GetTopics]
	@TopicID		int	= -1,
	@DeepLoad		bit	= 1,
	@TopicKey		nvarchar(255)	= null
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND DEFINE VARIABLES
--------------------------------------------------------------------------------------------------------------------------------

--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC ID IF UNKNOWN.
--------------------------------------------------------------------------------------------------------------------------------
IF @TopicKey IS NOT NULL
  BEGIN
    SELECT	@TopicID		= TopicID
    FROM	topics_TopicAttributes
    WHERE	AttributeKey		= 'Key'
      AND	AttributeValue		like @TopicKey
  END

IF @TopicID < 0
  BEGIN
    SELECT	TOP 1
	@TopicID		= TopicID
    FROM	topics_Topics
    ORDER BY	TopicID		ASC
  END

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE TEMP TABLES
--------------------------------------------------------------------------------------------------------------------------------
CREATE TABLE
#Topics (
  TopicID	int,
  SortOrder	int
)

CREATE CLUSTERED INDEX IDX_C_Topics_SortOrder ON #Topics(SortOrder)
CREATE INDEX IDX_Topics_TopicID ON #Topics(TopicID, SortOrder)

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC AND DESCENDENTS
--------------------------------------------------------------------------------------------------------------------------------
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
    ON	T1.RangeLeft
      BETWEEN	T2.RangeLeft
        AND	ISNULL(T2.RangeRight, 0)
      AND	T2.TopicID		= @TopicID
    ORDER BY	T1.RangeLeft
    OPTION	(OPTIMIZE FOR		(@TopicID = 1))
  END

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC ONLY
--------------------------------------------------------------------------------------------------------------------------------
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
    WHERE	TopicID		= @TopicID
    OPTION (
      OPTIMIZE
      FOR (
	@TopicID		UNKNOWN
      )
    )
  END

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT KEY ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	TopicIndex.TopicID,
  	TopicIndex.ContentType,
  	TopicIndex.ParentID,
  	TopicIndex.TopicKey,
  	Storage.SortOrder
FROM	topics_TopicIndex	AS TopicIndex
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= TopicIndex.TopicID
ORDER BY	SortOrder

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	TopicAttributes.TopicID,
	TopicAttributes.AttributeKey,
	TopicAttributes.AttributeValue
FROM	topics_TopicAttributeIndex	TopicAttributes
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= TopicAttributes.TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT BLOB
--------------------------------------------------------------------------------------------------------------------------------
SELECT	TopicBlob.TopicID,
	TopicBlob.Blob
FROM	topics_BlobIndex	AS TopicBlob
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= TopicBlob.TopicID
--AND	TopicBlob.Blob.exist('/attributes/attribute') >= 1
--ORDER BY	TopicBlob.TopicID ASC

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT RELATIONSHIPS
--------------------------------------------------------------------------------------------------------------------------------
;SELECT	Relationships.RelationshipTypeID,
	Relationships.Source_TopicID,
	Relationships.Target_TopicID
FROM	topics_Relationships	Relationships
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Relationships.Source_TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT HISTORY
--------------------------------------------------------------------------------------------------------------------------------
;WITH	TopicVersions
AS (
  SELECT	Distinct
	Attributes.TopicID,
	Attributes.Version,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		Attributes.TopicID
	  ORDER BY		Version DESC
	)
  FROM	topics_TopicAttributes	Attributes
  JOIN	#Topics		AS Storage
    ON	Storage.TopicID		= Attributes.TopicID
  GROUP BY	Attributes.TopicID,
	Attributes.Version
)
SELECT	TopicId,
	Version
FROM	TopicVersions
WHERE	RowNumber		<= 6