--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPICS
--------------------------------------------------------------------------------------------------------------------------------
-- Gets the tree of current topics rooted FROM the provided TopicID.  If no TopicID is provided then the sproc returns
-- everything under the topic with the lowest id.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[GetTopics]
	@TopicID		INT		= -1,
	@LoadDescendants	BIT		= 1,
	@LoadAscendants		BIT		= 0,
	@IncludeIndexed		BIT		= 1,
	@IncludeExtended	BIT		= 1,
	@IncludeRelationships	BIT		= 1,
	@IncludeReferences	BIT		= 1,
	@IncludeHistory		BIT		= 1,
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
IF @LoadDescendants = 1
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
ELSE IF @LoadAscendants = 1
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
-- SELECT SINGLE TOPIC (NO SCOPE)
--------------------------------------------------------------------------------------------------------------------------------
-- Inserts only the requested topic; used by the lazy-load resolver to fill a single topic's extended attributes without
-- traversing the tree in either direction.
--------------------------------------------------------------------------------------------------------------------------------
ELSE
  BEGIN
    INSERT	#Topics (
	  TopicID,
	  SortOrder
	)
    SELECT	@TopicID,
	0
  END

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT KEY ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Topics.TopicID,
  	ContentType,
  	ParentID,
  	TopicKey,
  	SortOrder,
  	HasExtendedAttributes	=
  	  CASE
  	    WHEN		@IncludeExtended = 0
  	    THEN		CAST(
	      CASE
	        WHEN EXISTS (
  	          SELECT	1
  	          FROM		ExtendedAttributeIndex	AS Extended
  	          WHERE		Extended.TopicID	= Topics.TopicID
  	        )
	        THEN 		1
	        ELSE 		0
	      END 		AS BIT
  	    )
  	    ELSE NULL
  	  END
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
WHERE	@IncludeIndexed		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT EXTENDED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Attributes.TopicID,
	AttributesXml,
	Version
FROM	ExtendedAttributeIndex	AS Attributes
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= Attributes.TopicID
WHERE	@IncludeExtended	= 1

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
WHERE	@IncludeRelationships	= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT REFERENCES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Source_TopicID,
	ReferenceKey,
	Target_TopicID
FROM	ReferenceIndex		AS TopicReferences
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= TopicReferences.Source_TopicID
WHERE	@IncludeReferences	= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT HISTORY
--------------------------------------------------------------------------------------------------------------------------------
SELECT	History.TopicID,
	Version
FROM	VersionHistoryIndex	AS History
JOIN	#Topics		AS Storage
  ON	Storage.TopicID		= History.TopicID
WHERE	@IncludeHistory		= 1;