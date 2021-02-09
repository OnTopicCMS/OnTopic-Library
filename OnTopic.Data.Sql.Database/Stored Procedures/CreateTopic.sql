--------------------------------------------------------------------------------------------------------------------------------
-- CREATE TOPIC
--------------------------------------------------------------------------------------------------------------------------------
-- Creates a new topic.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[CreateTopic]
	@Key		VARCHAR(128)		,
	@ContentType		VARCHAR(128)		,
	@ParentID		INT		= NULL,
	@Attributes		AttributeValues		READONLY,
	@ExtendedAttributes 	XML		= NULL,
	@References		TopicReferences		READONLY,
	@Version		DATETIME2(7)		= NULL
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@IsNestedTransaction	BIT;
DECLARE	@TopicID		INT;

BEGIN TRY

--------------------------------------------------------------------------------------------------------------------------------
-- BEGIN TRANSACTION
--------------------------------------------------------------------------------------------------------------------------------
-- ### NOTE JJC20210218: By necessity, this procedure potentially makes a massive number of changes to the Topics table's nested
-- set. During the execution, the nested set hierarchy WILL be in an inconsistent state. Read operations during that time are
-- very likely to be corrupted. As such, it's critical that the updates made as part of this procedure be isolated from other
-- reads being performed on the system. Further, we don't want any writes being made to the Topics table during this time—see
-- notes below regarding TABLOCK. By combining SERIALIZABLE with TABLOCK, we ensure that a) readers get a stable state, while b)
-- writers are prevented from concurrently modifying the table. Fortunately, these types of operations should be pretty
-- uncommon! The nested set model is very much optimized for read performance and presumes a relatively stable data set.
--------------------------------------------------------------------------------------------------------------------------------
IF (@@TRANCOUNT = 0)
  BEGIN
    SET @IsNestedTransaction = 0;
    BEGIN TRANSACTION;
  END
ELSE
  BEGIN
    SET @IsNestedTransaction = 1;
  END

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

--------------------------------------------------------------------------------------------------------------------------------
-- SET DEFAULT VERSION DATETIME
--------------------------------------------------------------------------------------------------------------------------------
IF	@Version		IS NULL
SET	@Version		= SYSUTCDATETIME()

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@RangeRight		INT	--Right Most Sibling
SET	@RangeRight		= 0

--------------------------------------------------------------------------------------------------------------------------------
-- RESERVE SPACE FOR NEW CHILD.
--------------------------------------------------------------------------------------------------------------------------------
IF (@ParentID IS NOT NULL)
  BEGIN
    SELECT	@RangeRight		= RangeRight
    FROM	Topics
    WHERE	TopicID		= @ParentID

    UPDATE	Topics
      SET	RangeLeft		=
        CASE
          WHEN	RangeLeft		> @RangeRight
          THEN	RangeLeft		+ 2
          ELSE	RangeLeft
        END,
	RangeRight		=
        CASE
          WHEN	RangeRight		>= @RangeRight
          THEN	RangeRight		+ 2
          ELSE	RangeRight
        END
    WHERE	RangeRight		>= @RangeRight
  END
ELSE
  BEGIN
    SELECT	@RangeRight		= MAX(RangeRight) + 1
    FROM	Topics
  END

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE NEW TOPIC
--------------------------------------------------------------------------------------------------------------------------------
INSERT INTO	Topics (
	RangeLeft,
	RangeRight,
	TopicKey,
	ContentType,
	ParentID,
	LastModified
)
Values (
	@RangeRight,
	@RangeRight		+ 1,
	@Key,
	@ContentType,
	@ParentID,
	@Version
)

SELECT	@TopicID		= SCOPE_IDENTITY()

--------------------------------------------------------------------------------------------------------------------------------
-- ADD INDEXED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
INSERT INTO	Attributes (
	TopicID		,
	AttributeKey		,
	AttributeValue		,
	Version
)
SELECT	@TopicID,
	AttributeKey,
	AttributeValue,
	@Version
FROM	@Attributes
WHERE	ISNULL(AttributeValue, '')	!= ''

--------------------------------------------------------------------------------------------------------------------------------
-- ADD EXTENDED ATTRIBUTES (XML)
--------------------------------------------------------------------------------------------------------------------------------
IF @ExtendedAttributes IS NOT NULL
  BEGIN
    INSERT
    INTO	ExtendedAttributes (
	TopicID		,
	AttributesXml		,
	Version
    )
    VALUES (	@TopicID		,
	@ExtendedAttributes 	,
	@Version
    )
  END

--------------------------------------------------------------------------------------------------------------------------------
-- ADD REFERENCES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@ReferenceCount		INT
SELECT	@ReferenceCount		= COUNT(ReferenceKey)
FROM	@References

IF @ReferenceCount > 0
  BEGIN
    EXEC	UpdateReferences	@TopicID,
			@References,
			@Version,
			1
  END

--------------------------------------------------------------------------------------------------------------------------------
-- COMMIT TRANSACTION
--------------------------------------------------------------------------------------------------------------------------------
IF (@@TRANCOUNT > 0 AND @IsNestedTransaction = 0)
  BEGIN
    COMMIT
  END
END TRY

--------------------------------------------------------------------------------------------------------------------------------
-- HANDLE ERRORS
--------------------------------------------------------------------------------------------------------------------------------
BEGIN CATCH
  IF (@@TRANCOUNT > 0 AND @IsNestedTransaction = 0)
    BEGIN
      ROLLBACK;
    END;
  THROW
  RETURN;
END CATCH

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID