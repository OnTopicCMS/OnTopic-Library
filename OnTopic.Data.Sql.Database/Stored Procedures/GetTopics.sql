--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPICS
--------------------------------------------------------------------------------------------------------------------------------
-- Gets the tree of current topics rooted FROM the provided TopicID.  If no TopicID is provided then the sproc returns
-- everything under the topic with the lowest id.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[GetTopics]
	@TopicID		INT	= -1,
	@DeepLoad		BIT	= 1,
	@UniqueKey		NVARCHAR(255)	= NULL
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
-- SELECT TOPIC AND ANCESTOR CHAIN
--------------------------------------------------------------------------------------------------------------------------------
-- Ancestors are rows whose nested-set range contains the requested node's RangeLeft, i.e., the mirror of the descendant query
-- above. This guarantees the full parent chain is always materialized, even on a shallow (non-recursive) load.
--------------------------------------------------------------------------------------------------------------------------------
ELSE
  BEGIN
    INSERT	#Topics (
	  TopicID,
	  SortOrder
	)
    SELECT	T1.TopicID,
	T1.RangeLeft
    FROM	Topics		AS T1
    INNER JOIN	Topics		AS T2
    ON	T2.RangeLeft
      BETWEEN	T1.RangeLeft
        AND	T1.RangeRight
      AND	T2.TopicID		= @TopicID
    ORDER BY	T1.RangeLeft
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
  	ContentType,
  	ParentID,
  	TopicKey,
  	SortOrder
FROM	Topics		AS Topics
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Topics.TopicID
ORDER BY	SortOrder

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Attributes.TopicID,
	AttributeKey,
	AttributeValue,
	Version
FROM	AttributeIndex		AS Attributes
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Attributes.TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT EXTENDED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Attributes.TopicID,
	AttributesXml,
	Version
FROM	ExtendedAttributeIndex	AS Attributes
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Attributes.TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT RELATIONSHIPS
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Source_TopicID,
	RelationshipKey,
	Target_TopicID,
	IsDeleted
FROM	RelationshipIndex	AS Relationships
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Relationships.Source_TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT REFERENCES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Source_TopicID,
	ReferenceKey,
	Target_TopicID
FROM	ReferenceIndex		AS TopicReferences
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= TopicReferences.Source_TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT HISTORY
--------------------------------------------------------------------------------------------------------------------------------
SELECT	History.TopicID,
	Version
FROM	VersionHistoryIndex	AS History
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= History.TopicID;