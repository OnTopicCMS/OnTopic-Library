--------------------------------------------------------------------------------------------------------------------------------
-- DELETE ORPHANED LAST MODIFIED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
-- Current versions of the OnTopic Library evaluate whether or not an attribute value has changed since the previous version,
-- and doesn't create a new attribute version if it has. This process doesn't work for <c>LastModified</c>, however, as that
-- value changes every time a value is saved—at least via the OnTopic Editor. If you save a topic five times in the editor, it
-- will generate five <c>LastModified</c> values, <i>even if no other attribute values changed</i>. Over time, this can create a
-- lot of clutter in the database, and potentially slow down some queries. This script identifies <c>LastModified</c> attributes
-- which don't correspond to any other updates of <i>indexed</i> attributes, and deletes them.
--------------------------------------------------------------------------------------------------------------------------------
-- NOTE: There are legitimate scenarios where a topic is updated but it doesn't show up in other attribute values, such as when
-- relationships are updated. These situations are the exception, however, and it's usually not an issue to lose that level of
-- granularity.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[DeleteOrphanedLastModifiedAttributes]
AS

SET NOCOUNT ON;

--------------------------------------------------------------------------------------------------------------------------------
-- CHECK INITIAL VALUES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@Count	INT

SELECT	@Count	= Count(TopicID)
FROM	Attributes
WHERE	AttributeKey	= 'LastModified'

Print('Initial Count: ' + CAST(@Count AS VARCHAR) + ' LastModified records in the database.');

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE ORPHANED LAST MODIFIED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
DELETE	Attributes
FROM	Attributes
LEFT JOIN	Attributes Unmatched
  ON	Attributes.TopicID = Unmatched.TopicID
  AND	Attributes.Version = Unmatched.Version
  AND	Attributes.AttributeKey != Unmatched.AttributeKey
LEFT JOIN	ExtendedAttributes UnmatchedExtended
  ON	Attributes.TopicID = UnmatchedExtended.TopicID
  AND	Attributes.Version = UnmatchedExtended.Version
WHERE	Unmatched.AttributeKey is null
  AND	UnmatchedExtended.TopicID is null
  AND	Attributes.AttributeKey = 'LastModified'

--------------------------------------------------------------------------------------------------------------------------------
-- CHECK FINAL VALUES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	@Count	= @Count - Count(TopicID)
FROM	Attributes
WHERE	AttributeKey	= 'LastModified'

Print('Final Count: ' + CAST(@Count AS VARCHAR) + ' orphaned LastModified records were identified and deleted.')