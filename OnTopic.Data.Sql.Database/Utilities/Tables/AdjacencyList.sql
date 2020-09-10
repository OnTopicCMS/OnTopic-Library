--------------------------------------------------------------------------------------------------------------------------------
-- ADJACENCY LIST (TABLE)
--------------------------------------------------------------------------------------------------------------------------------
-- Provides temporary storage for representing the topic hierarchy as an adjacency list. This isn't the preferred format for
-- representing the topic hierarchy, and is not used in production code. Nevertheless, having this available is useful for
-- processing migrations from other data formats, or rebuilding the nested set hierarchy should it become corrupted.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TABLE	[Utilities].[AdjacencyList] (
	  [TopicID]		INT	NOT NULL,
	  [Parent_TopicID]	INT	NULL,
	  [DateAdded]		DATETIME	NOT NULL,
  [SortOrder] INT NOT NULL, 
    CONSTRAINT	  [PK_Hierarchy]	PRIMARY KEY
  CLUSTERED (	  [TopicID]		ASC
  )
);