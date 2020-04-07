--------------------------------------------------------------------------------------------------------------------------------
-- DELETE CONSECUTIVE ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
-- Current versions of the OnTopic Library evaluate whether or not an attribute value has changed since the previous version,
-- and doesn't create a new attribute version if it has. This wasn't true in previous versions, however. As a result, there are
-- some cases, and especially in older databases, where unnecessary duplicates occur for attribute values. This script will
-- detect concurrent duplicates and remove them from the database. This reduces the size of the database, without interfering
-- with the data integrity. If attribute values are not consecutive, the duplicates aren't deleted; e.g., if the value of
-- <c>Title</c> gets changed, then gets reverted, all three versions will be retained in the database.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[DeleteConsecutiveAttributes]
AS

SET NOCOUNT ON;

--------------------------------------------------------------------------------------------------------------------------------
-- CHECK INITIAL VALUES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@Count	INT

SELECT	@Count	= Count(TopicID)
FROM	Attributes

Print('Initial Count: ' + CAST(@Count AS VARCHAR) + ' Attributes in the database.');

--------------------------------------------------------------------------------------------------------------------------------
-- IDENTIFY GROUPS OF CONCURRENT DUPLICATES
--------------------------------------------------------------------------------------------------------------------------------
WITH GroupedValues AS (
  SELECT	TopicID,
	AttributeKey,
	AttributeValue,
	DateModified,
	Version,
	ValueGroup = ROW_NUMBER() OVER(PARTITION BY TopicID, AttributeKey ORDER BY TopicID, AttributeKey, Version)
	- ROW_NUMBER() OVER(PARTITION BY TopicID, AttributeKey, AttributeValue ORDER BY TopicID, AttributeKey, Version)
   FROM	Attributes
),

--------------------------------------------------------------------------------------------------------------------------------
-- RANK DUPLICATES BY DATE
--------------------------------------------------------------------------------------------------------------------------------
RankedValues AS (
  SELECT	TopicID,
	AttributeKey,
	AttributeValue,
	DateModified,
	Version,
	ValueGroup,
	ValueRank = ROW_NUMBER() OVER(PARTITION BY ValueGroup, TopicID, AttributeKey, AttributeValue ORDER BY TopicID, AttributeKey, Version)
  FROM	GroupedValues
)

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE NEWER DUPLICATES
--------------------------------------------------------------------------------------------------------------------------------
DELETE
FROM	RankedValues
WHERE	ValueRank > 1;

--------------------------------------------------------------------------------------------------------------------------------
-- CHECK FINAL VALUES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	@Count	= @Count - Count(TopicID)
FROM	Attributes

Print('Final Count: ' + CAST(@Count AS VARCHAR) + ' Attributes were identified and deleted.')

