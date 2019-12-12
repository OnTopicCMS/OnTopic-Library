CREATE
TABLE	[dbo].[topics_Relationships] (
	  [Target_TopicID]	INT	NOT NULL,
	  [Source_TopicID]	INT	NOT NULL,
	  [RelationshipTypeID]	VARCHAR(255)	NOT NULL,
  CONSTRAINT	  [PK_Relationships_1]	PRIMARY KEY	CLUSTERED ([Target_TopicID] ASC, [Source_TopicID] ASC, [RelationshipTypeID] ASC),
  CONSTRAINT	  [FK_Relationships_Source]
  FOREIGN KEY (	    [Source_TopicID]
  )
  REFERENCES	  [dbo].[topics_Topics] (
	    [TopicID]
  ),
  CONSTRAINT	  [FK_Relationships_Target]
  FOREIGN KEY (	    [Target_TopicID]
  )
  REFERENCES	  [dbo].[topics_Topics] (
	    [TopicID]
	  )
);