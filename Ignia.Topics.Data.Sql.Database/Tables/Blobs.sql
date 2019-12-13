--------------------------------------------------------------------------------------------------------------------------------
-- BLOBS (TABLE)
--------------------------------------------------------------------------------------------------------------------------------
-- Provides a storage blob (in XML format) for attribute key/value pairs. Attributes can also be stored in the
-- Attributes table. The difference is that the latter limits attribute values to 255 characters per AttributeValue,
-- whereas the blob offers virtually unlimited storage capacity (at least in practical terms).
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TABLE	[dbo].[Blobs] (
	  [TopicID]		INT	NOT NULL,
	  [Blob]		XML	NOT NULL,
	  [DateModified]	DATETIME
  CONSTRAINT	  [DF_Blobs_DateModified]	DEFAULT (
			  GetDate()
			)	NOT NULL,
	  [Version]		DATETIME
  CONSTRAINT	  [DF_Blobs_Version]	DEFAULT (
			  GetDate()
			)	NOT NULL,
  CONSTRAINT	  [PK_Blobs]		PRIMARY KEY	CLUSTERED (
	    [TopicID]		ASC,
	    [Version]		DESC
	  ),
  CONSTRAINT	  [FK_Blobs_Topics]
  FOREIGN KEY (	    [TopicID]
	)
  REFERENCES	  [dbo].[Topics] (
	    [TopicID]
	)
);