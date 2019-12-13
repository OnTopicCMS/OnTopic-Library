--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPICS
--------------------------------------------------------------------------------------------------------------------------------
-- Gets the tree of current topics rooted FROM the provided TopicID.  If no TopicID is provided then the sproc returns
-- everything under the topic with the lowest id.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[GetTopics]
	@TopicID		int	= -1,
	@DeepLoad		bit	= 1,
	@TopicKey		nvarchar(255)	= null
AS

--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC ID IF UNKNOWN.
--------------------------------------------------------------------------------------------------------------------------------
IF @TopicKey IS NOT NULL
  BEGIN
    EXEC	@TopicID		= GetTopicID @TopicKey
  END

IF @TopicID < 0
  BEGIN
    EXEC	@TopicID		= GetTopicID 'Root'
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
    FROM	Topics		AS T1
    INNER JOIN	Topics		AS T2
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
    FROM	Topics
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
FROM	TopicIndex		AS TopicIndex
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= TopicIndex.TopicID
ORDER BY	SortOrder

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Attributes.TopicID,
	Attributes.AttributeKey,
	Attributes.AttributeValue
FROM	AttributeIndex		Attributes
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Attributes.TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT AttributeXml
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Attributes.TopicID,
	Attributes.AttributesXml
FROM	ExtendedAttributeIndex	AS Attributes
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Attributes.TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT RELATIONSHIPS
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Relationships.Source_TopicID,
	Relationships.RelationshipKey,
	Relationships.Target_TopicID
FROM	Relationships		Relationships
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Relationships.Source_TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT HISTORY
--------------------------------------------------------------------------------------------------------------------------------
SELECT	VersionHistory.TopicID,
	VersionHistory.Version
FROM	VersionHistoryIndex	VersionHistory
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= VersionHistory.TopicID;