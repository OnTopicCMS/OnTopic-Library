--------------------------------------------------------------------------------------------------------------------------------
-- HIERARCHY (TABLE)
--------------------------------------------------------------------------------------------------------------------------------
-- Represents n:n relationships between topics, grouped together by namespaces ("RelationshipKey").
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TABLE	[dbo].[Relationships] (
	  [Target_TopicID]	INT	NOT NULL,
	  [Source_TopicID]	INT	NOT NULL,
	  [RelationshipKey]	VARCHAR(255)	NOT NULL,
	  [IsDeleted]		BIT	NOT NULL	DEFAULT 0,
	  [Version]		DATETIME2(7)	NOT NULL	DEFAULT GETUTCDATE()
  CONSTRAINT	  [PK_Relationships]	PRIMARY KEY
  CLUSTERED (	    [Source_TopicID]	ASC,
	    [RelationshipKey]	ASC,
	    [Target_TopicID]	ASC,
	    [Version]		DESC
  ),
  CONSTRAINT	  [FK_Relationships_Source]
  FOREIGN KEY (	    [Source_TopicID]
  )
  REFERENCES	  [dbo].[Topics] (
	    [TopicID]
  ),
  CONSTRAINT	  [FK_Relationships_Target]
  FOREIGN KEY (	    [Target_TopicID]
  )
  REFERENCES	  [dbo].[Topics] (
	    [TopicID]
	  )
);