--------------------------------------------------------------------------------------------------------------------------------
-- DELETE CONSECUTIVE ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
-- Current versions of the OnTopic Library evaluate whether or not the composite XML for the extended attribute values has
-- changed since the previous version, and only creates a new version if it has. This wasn't true in previous versions, however.
-- As a result, there are some cases, and especially in older databases, where unnecessary duplicates occur for attribute
-- values. These dramatically increase the size of the database and can slow down the processing time of certain queries. This
-- procedure will detect concurrent duplicates and remove them from the database. This reduces the size of the database, without
-- interfering with the data integrity.
--------------------------------------------------------------------------------------------------------------------------------
-- NOTE: Because this query must cast the XML values as VARCHAR in order to compare them, it takes a LONG time to run. Please
-- be patient!
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [Utilities].[DeleteConsecutiveExtendedAttributes]
AS

SET NOCOUNT ON;

--------------------------------------------------------------------------------------------------------------------------------
-- CHECK INITIAL VALUES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@Count	INT

SELECT	@Count	= COUNT(TopicID)
FROM	ExtendedAttributes

Print('Initial Count: ' + CAST(@Count AS VARCHAR) + ' Extended Attributes in the database.');

--------------------------------------------------------------------------------------------------------------------------------
-- IDENTIFY GROUPS OF CONCURRENT DUPLICATES
--------------------------------------------------------------------------------------------------------------------------------
WITH GroupedValues AS (
  SELECT	TopicID,
	AttributesXml,
	Version,
	ValueGroup = ROW_NUMBER() OVER(PARTITION BY TopicID ORDER BY TopicID, Version)
	- ROW_NUMBER() OVER(PARTITION BY TopicID, CAST(AttributesXml AS NVARCHAR(MAX)) ORDER BY TopicID, Version)
   FROM	ExtendedAttributes
),

--------------------------------------------------------------------------------------------------------------------------------
-- RANK DUPLICATES BY DATE
--------------------------------------------------------------------------------------------------------------------------------
RankedValues AS (
  SELECT	TopicID,
	AttributesXml,
	Version,
	ValueGroup,
	ValueRank = ROW_NUMBER() OVER(PARTITION BY ValueGroup, TopicID, CAST(AttributesXml AS NVARCHAR(MAX)) ORDER BY TopicID, Version)
  FROM	GroupedValues
)

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE NEWER DUPLICATES
--------------------------------------------------------------------------------------------------------------------------------
DELETE
FROM	RankedValues
WHERE	ValueRank > 1;

PRINT('Concurrent duplicates have been deleted.')

--------------------------------------------------------------------------------------------------------------------------------
-- CHECK FINAL VALUES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	@Count	= @Count - COUNT(TopicID)
FROM	ExtendedAttributes

Print('Final Count: ' + CAST(@Count AS VARCHAR) + ' duplicate Extended Attributes were identified and deleted.')
