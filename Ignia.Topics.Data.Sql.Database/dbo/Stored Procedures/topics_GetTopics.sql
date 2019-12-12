--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPICS
--------------------------------------------------------------------------------------------------------------------------------
-- Gets the tree of current topics rooted FROM the provided TopicID.  If no TopicID is provided then the sproc returns
-- everything under the topic with the lowest id.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_GetTopics]
	@TopicID		int	= -1,
	@DeepLoad		bit	= 1,
	@TopicKey		nvarchar(255)	= null
AS

--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC ID IF UNKNOWN.
--------------------------------------------------------------------------------------------------------------------------------
IF @TopicKey IS NOT NULL
  BEGIN
    EXEC	@TopicID		= topics_GetTopicID @TopicKey
  END

IF @TopicID < 0
  BEGIN
    EXEC	@TopicID		= topics_GetTopicID 'Root'
  END

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE TEMP TABLES
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TABLE	#Topics (
	  TopicID		INT,
	  SortOrder		INT
)

CREATE
CLUSTERED INDEX	IX_C_Topics_TopicID
  ON	#Topics(
	  TopicID
	)

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC AND DESCENDENTS
--------------------------------------------------------------------------------------------------------------------------------
IF @DeepLoad = 1
  BEGIN
    INSERT	#Topics (
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
    OPTION (
      OPTIMIZE
      FOR (	@TopicID		= 1
      )
    )
  END

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC ONLY
--------------------------------------------------------------------------------------------------------------------------------
ELSE
  BEGIN
    INSERT	#Topics (
	  TopicID,
	  SortOrder
	)
    SELECT	TopicID,
	1
    FROM	topics_Topics
    WHERE	TopicID		= @TopicID
    OPTION (
      OPTIMIZE
      FOR (	@TopicID		UNKNOWN
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

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT RELATIONSHIPS
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Relationships.RelationshipTypeID,
	Relationships.Source_TopicID,
	Relationships.Target_TopicID
FROM	topics_Relationships	Relationships
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Relationships.Source_TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT HISTORY
--------------------------------------------------------------------------------------------------------------------------------
SELECT	VersionHistory.TopicID,
	VersionHistory.Version
FROM	topics_VersionHistoryIndex	VersionHistory
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= VersionHistory.TopicID;