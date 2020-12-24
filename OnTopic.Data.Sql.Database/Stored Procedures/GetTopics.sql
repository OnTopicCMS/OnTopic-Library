--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPICS
--------------------------------------------------------------------------------------------------------------------------------
-- Gets the tree of current topics rooted FROM the provided TopicID.  If no TopicID is provided then the sproc returns
-- everything under the topic with the lowest id.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[GetTopics]
	@TopicID		int	= -1,
	@DeepLoad		bit	= 1,
	@UniqueKey		nvarchar(255)	= null
AS

--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC ID IF UNKNOWN.
--------------------------------------------------------------------------------------------------------------------------------
IF @UniqueKey IS NOT NULL
  BEGIN
    SET	@TopicID		= dbo.GetTopicID(@UniqueKey)
  END

IF @TopicID < 0
  BEGIN
    SET	@TopicID		= dbo.GetTopicID('Root')
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
        AND	T2.RangeRight
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
SELECT	Topics.TopicID,
  	Topics.ContentType,
  	Topics.ParentID,
  	Topics.TopicKey,
  	Storage.SortOrder
FROM	Topics		AS Topics
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Topics.TopicID
ORDER BY	SortOrder

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Attributes.TopicID,
	Attributes.AttributeKey,
	Attributes.AttributeValue,
	Attributes.Version
FROM	AttributeIndex		Attributes
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Attributes.TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT EXTENDED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Attributes.TopicID,
	Attributes.AttributesXml,
	Attributes.Version
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