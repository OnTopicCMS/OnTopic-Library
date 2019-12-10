--------------------------------------------------------------------------------------------------------------------------------
-- DELETE TOPIC
--------------------------------------------------------------------------------------------------------------------------------
-- Deletes a topic in the tree, including all child topics.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_DeleteTopic]
	@TopicID		INT
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DISABLE NOCOUND
--------------------------------------------------------------------------------------------------------------------------------
-- Prevent extra result sets from interfering with SELECT statements
SET NOCOUNT ON;

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@RangeLeft		INT
DECLARE	@RangeRight		INT
DECLARE	@RangeWidth		INT
DECLARE	@Topics		TABLE	(TopicId INT)
DECLARE	@IsNestedTransaction	BIT;

BEGIN TRY

--------------------------------------------------------------------------------------------------------------------------------
-- BEGIN TRANSACTION
--------------------------------------------------------------------------------------------------------------------------------
-- ### NOTE JJC20191208: This application includes a number of read operations that join the topics_Topics table with other
-- tables modified as part of this procedure. Many of those read operations will fail while this operation is happening. For
-- example, common joins between topics_Topics and topics_TopicAttributes may fail since critical AttributeKeys such as Key,
-- ParentId, and ContentType may be missing during this operation. For this reason, we are opting to use an aggressive isolation
-- level—SERIALIZABLE—to ensure that all callers outside of this transcation receive a stable (pre-transaction) state of the
-- data until this transaction is committed.
--------------------------------------------------------------------------------------------------------------------------------
-- ### NOTE JJC20191208: The SERIALIZABLE isolation level also has the benefit (for our needs) of maintaining any holds for the
-- duration of the transaction. This includes any rows within the scope of where clauses within this procedure. Critically, it
-- also includes a TABLOCK established early on, which prevents the nested set hierarchy from being modified until completion.
-- See additional notes below for further explanation of the decision to use a TABLOCK.
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
-- DEFINE RANGE TO DELETE
--------------------------------------------------------------------------------------------------------------------------------
-- ### NOTE JJC20191208: We usually avoid broad hints like TABLOCK. That said, the delete operation requires multiple
-- operations against the topics table which will fail if the topic range shifts. Locking the table helps ensure that data
-- integrity issues aren't introduced by concurrent modification of the nested set.
--------------------------------------------------------------------------------------------------------------------------------
-- ### NOTE JJC20191208: Note that locks are NOT required on the child tables, such as topics_TopicAttributes, topics_Blob, and
-- topics_Relationships. This is because those queries are much narrower in scope, and the standard out-of-the-box row locks
-- that come with the SERIALIZABLE isolation level when those calls are executed will be more than sufficient.
--------------------------------------------------------------------------------------------------------------------------------
SELECT	@RangeLeft		= RangeLeft,
	@RangeRight		= RangeRight,
	@RangeWidth		= RangeRight - RangeLeft + 1
FROM	topics_Topics
WITH (	TABLOCK)
WHERE	TopicID		= @TopicID
;

--------------------------------------------------------------------------------------------------------------------------------
-- STORE RANGE IN TABLE VARIABLE
--------------------------------------------------------------------------------------------------------------------------------
INSERT
INTO	@Topics(TopicId)
SELECT	TopicId
FROM	topics_Topics
WHERE	RangeLeft
  BETWEEN	@RangeLeft
  AND	@RangeRight

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE RELATED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
DELETE	Attributes
FROM	topics_TopicAttributes	Attributes
INNER JOIN	@Topics                         Topics
  ON	Topics.TopicId                  = Attributes.TopicID

DELETE	Blob
FROM	topics_Blob		Blob
INNER JOIN	@Topics                         Topics
  ON	Topics.TopicId                  = Blob.TopicID

DELETE	Relationships
FROM	topics_Relationships	Relationships
INNER JOIN	@Topics                         Topics
  ON	Topics.TopicId                  = Relationships.Source_TopicID

DELETE	Relationships
FROM	topics_Relationships	Relationships
INNER JOIN	@Topics                         Topics
  ON	Topics.TopicId                  = Relationships.Target_TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE RANGE
--------------------------------------------------------------------------------------------------------------------------------
DELETE	TopicsActual
FROM	topics_Topics		TopicsActual
INNER JOIN	@Topics                         Topics
  ON	Topics.TopicId		= TopicsActual.TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- CLOSE LEFT GAP
--------------------------------------------------------------------------------------------------------------------------------
UPDATE	topics_Topics
SET	RangeRight		= RangeRight - @RangeWidth
WHERE	ISNULL(RangeRight, 0)	> @RangeRight

UPDATE	topics_Topics
SET	RangeLeft		= RangeLeft - @RangeWidth
WHERE	RangeLeft		> @RangeRight

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