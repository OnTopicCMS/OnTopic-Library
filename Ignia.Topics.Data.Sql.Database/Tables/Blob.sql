--------------------------------------------------------------------------------------------------------------------------------
-- BLOB (TABLE)
--------------------------------------------------------------------------------------------------------------------------------
-- Provides a storage blob (in XML format) for attribute key/value pairs. Attributes can also be stored in the
-- TopicAttributes table. The difference is that the latter limits attribute values to 255 characters per AttributeValue,
-- whereas the blob offers virtually unlimited storage capacity (at least in practical terms).
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TABLE	[dbo].[Blob] (
	  [TopicID]		INT	NOT NULL,
	  [Blob]		XML	NOT NULL,
	  [DateModified]	DATETIME
  CONSTRAINT	  [DF_Blob_DateModified]	DEFAULT (
			  GetDate()
			)	NOT NULL,
	  [Version]		DATETIME
  CONSTRAINT	  [DF_Blob_Version]	DEFAULT (
			  GetDate()
			)	NOT NULL,
  CONSTRAINT	  [PK_Blob]		PRIMARY KEY	CLUSTERED (
	    [TopicID]		ASC,
	    [Version]		DESC
	  ),
  CONSTRAINT	  [FK_Blob_Topics]
  FOREIGN KEY (	    [TopicID]
	)
  REFERENCES	  [dbo].[Topics] (
	    [TopicID]
	)
);