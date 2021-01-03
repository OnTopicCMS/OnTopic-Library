--------------------------------------------------------------------------------------------------------------------------------
-- TOPIC REFERENCES (TABLE)
--------------------------------------------------------------------------------------------------------------------------------
-- Represents 1:1 relationship between topics, grouped together by namespaces ("ReferenceKey").
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TABLE	[dbo].[TopicReferences] (
	  [Source_TopicID]	INT	NOT NULL,
	  [ReferenceKey]	VARCHAR(128)	NOT NULL,
	  [Target_TopicID]	INT	NOT NULL,
  CONSTRAINT	  [PK_TopicReferences]	PRIMARY KEY
  CLUSTERED (	    [Source_TopicID]	ASC,
	    [ReferenceKey]	ASC
  ),
  CONSTRAINT	  [FK_TopicReferences_Source]
  FOREIGN KEY (	    [Source_TopicID]
  )
  REFERENCES	  [dbo].[Topics] (
	    [TopicID]
  ),
  CONSTRAINT	  [FK_TopicReferences_Target]
  FOREIGN KEY (	    [Target_TopicID]
  )
  REFERENCES	  [dbo].[Topics] (
	    [TopicID]
	  )
);