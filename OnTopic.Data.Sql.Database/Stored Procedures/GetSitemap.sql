--------------------------------------------------------------------------------------------------------------------------------
-- GET SITEMAP
--------------------------------------------------------------------------------------------------------------------------------
-- Returns the minimal data the sitemap renders: The topic rows, then the handful of indexed attributes the sitemap evaluates.
-- SqlDataReaderExtensions.LoadTopicGraph stitches these two sets and passed over the result sets it would otherwise read
-- (extended attributes, relationships, references, history), which this sproc simply does not return. Deliberately omits the
-- nested-set descent, extended-attribute blobs, relationships, references, and version history.
--
-- HasChildren and HasExtendedAttributes must be present so LoadTopicGraph's reader contract is satisfied (it reads both columns
-- unconditionally), but their values don't need to be computed: This sproc returns the entire flattened tree in one pass, so
-- every topic's children are already present in the graph regardless of the flag, and the graph is not subject to lazy-loading
-- (i.e., no ITopicLazyLoader stamped), so LoadState is never consulted to trigger a fill regardless. NULL leaves both
-- boundaries at their default of LoadState.Loaded (KeyedTopicCollection's default), matching the resolver-free invariant.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[GetSitemap]
AS

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPICS
--------------------------------------------------------------------------------------------------------------------------------
SELECT		Topics.TopicID,
		Topics.ContentType,
		Topics.ParentID,
		Topics.TopicKey,
		HasChildren		= CAST(NULL AS BIT),
		HasExtendedAttributes	= CAST(NULL AS BIT)
FROM		Topics		AS Topics
ORDER BY		Topics.RangeLeft

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
-- Filtered to exactly the keys AddTopic evaluates plus LastModified; this IN list is coupled to the controller's inclusion
-- logic and must grow with it.
SELECT		Attributes.TopicID,
		Attributes.AttributeKey,
		Attributes.AttributeValue,
		Attributes.Version
FROM		AttributeIndex		AS Attributes
WHERE		Attributes.AttributeKey 	IN (
                                  'IsPrivateBranch',
                                  'NoIndex',
                                  'IsDisabled',
                                  'Url',
                                  'LastModified'
                                )