-----------------------------------------------------------------------------------------------------------------------------------------------
-- Procedure	GENERATE HIERARCHY ID
--
-- Purpose	Takes the nested set hierarchy and converts it to a HierarchyID format.
--
-- History      Jeremy Caney	10202017  Created initial version
-----------------------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[topics_GenerateHierarchyID]
AS

-----------------------------------------------------------------------------------------------------------------------------------------------
-- CREATE TEMPORARY TABLES
-----------------------------------------------------------------------------------------------------------------------------------------------
CREATE TABLE
#Topics (
  TopicName     varchar(255),
  TopicID	int,
  ParentID	int,
  Ranking	int,
  RangeLeft	int
)

CREATE TABLE
#NewOrg (
  OrgNode hierarchyid,
  TopicID int,
  ParentID int
)

-----------------------------------------------------------------------------------------------------------------------------------------------
-- CREATE ADJACENCY LIST
-----------------------------------------------------------------------------------------------------------------------------------------------
INSERT
INTO		#Topics (
		  TopicName,
		  TopicID,
		  ParentID,
		  Ranking,
		  RangeLeft
		)
SELECT		DISTINCT
		CONCAT(Topics.Indent, Attributes.TopicKey) AS TopicName,
		Topics.TopicID,
		Attributes.ParentID,
		ROW_NUMBER() OVER (PARTITION BY Attributes.ParentID ORDER BY Attributes.ParentID, Topics.RangeLeft) AS Ranking,
		RangeLeft
FROM (
    SELECT	P1.TopicID,
		REPLICATE(' ', (COUNT(P2.TopicID)-1)) AS Indent,
		COUNT(P2.TopicID) AS indentation,
		P1.RangeLeft
    FROM	topics_Topics	AS P1
    JOIN	topics_Topics	AS P2
    ON		P1.RangeLeft
    BETWEEN	P2.RangeLeft
    AND		P2.RangeRight
    WHERE	P1.RangeLeft >= 15
    GROUP BY	P1.TopicID,
		P1.RangeLeft
    ) AS Topics
JOIN		topics_TopicIndex Attributes
ON		Topics.TopicID = Attributes.TopicID
ORDER BY	Topics.RangeLeft;

-----------------------------------------------------------------------------------------------------------------------------------------------
-- RETURN ADJACENCY LIST
-----------------------------------------------------------------------------------------------------------------------------------------------
select		*
from		#Topics;

-----------------------------------------------------------------------------------------------------------------------------------------------
-- CREATE HIERARCHYID PATHS
-----------------------------------------------------------------------------------------------------------------------------------------------
WITH paths(path, TopicID) AS (
  SELECT	hierarchyid::GetRoot() AS OrgNode,
		TopicID
  FROM		#Topics AS C
  WHERE		ParentID IS NULL

  UNION ALL

  SELECT	CAST(p.path.ToString() + CAST(C.Ranking AS varchar(30)) + '/' AS hierarchyid),
		C.TopicID
  FROM		#Topics AS C
  JOIN		paths AS p
  ON		C.ParentID = P.TopicID
)
INSERT		#NewOrg (
		  OrgNode,
		  O.TopicID,
		  O.ParentID
		)
SELECT		P.path,
		O.TopicID,
		O.ParentID
FROM		#Topics AS O
JOIN		paths AS P
ON		O.TopicID = P.TopicID;

-----------------------------------------------------------------------------------------------------------------------------------------------
-- RETURN HIERARCHYID PATHS
-----------------------------------------------------------------------------------------------------------------------------------------------
Select		OrgNode.ToString(),
		TopicID,
		ParentID
from		#NewOrg

-----------------------------------------------------------------------------------------------------------------------------------------------
-- DROP TEMPORARY TABLES
-----------------------------------------------------------------------------------------------------------------------------------------------
drop table #Topics
drop table #NewOrg