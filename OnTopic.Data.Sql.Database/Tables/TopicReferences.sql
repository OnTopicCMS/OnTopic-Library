--------------------------------------------------------------------------------------------------------------------------------
-- TOPIC REFERENCES (TABLE)
--------------------------------------------------------------------------------------------------------------------------------
-- Represents 1:1 relationship between topics, grouped together by namespaces ("ReferenceKey").
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TABLE	[dbo].[TopicReferences] (
	  [Source_TopicID]	INT	NOT NULL,
	  [ReferenceKey]	VARCHAR(128)	NOT NULL,
	  [Target_TopicID]	INT	NULL,
	  [Version]		DATETIME2(7)	NOT NULL	DEFAULT GETUTCDATE()
  CONSTRAINT	  [PK_TopicReferences]	PRIMARY KEY
  CLUSTERED (	    [Source_TopicID]	ASC,
	    [ReferenceKey]	ASC,
	    [Version]		DESC
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