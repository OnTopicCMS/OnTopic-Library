--------------------------------------------------------------------------------------------------------------------------------
-- UNIQUE KEY (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Generates a view mapping each TopicID to its UniqueKey.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
VIEW	[Utilities].[UniqueKeyIndex]
WITH	SCHEMABINDING
AS

SELECT          Tree.TopicID,
                Path = Replace(Path, '&gt;', ':')
FROM            [dbo].[Topics] Tree
CROSS APPLY (
  SELECT        Path = Stuff((
    SELECT      '>' + AttributeValue
    FROM (
      SELECT    RangeLeft,
                AttributeValue
      FROM      [dbo].[Topics]
      CROSS APPLY (
        SELECT  TOP 1
                AttributeValue
        FROM    [dbo].[Attributes]
        WHERE   Attributes.TopicID = Topics.TopicID
        AND     Attributes.AttributeKey = 'Key'
        ORDER BY Version DESC
      ) AttributeValue
      WHERE     Tree.RangeLeft BETWEEN RangeLeft AND ISNULL(RangeRight, -1)
    ) B1
    ORDER BY    RangeLeft
    FOR XML Path ('')
  )
  ,1,4,'')
) UniqueKeyIndex