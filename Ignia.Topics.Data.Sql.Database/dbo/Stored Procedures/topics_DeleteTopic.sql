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
IF (@@TRANCOUNT = 0)
  BEGIN
    SET @IsNestedTransaction = 0;
    BEGIN TRANSACTION;
  END
ELSE
  BEGIN
    SET @IsNestedTransaction = 1;
  END

--------------------------------------------------------------------------------------------------------------------------------
-- DEFINE RANGE TO DELETE
--------------------------------------------------------------------------------------------------------------------------------
SELECT	@RangeLeft		= RangeLeft,
	@RangeRight		= RangeRight,
	@RangeWidth		= RangeRight - RangeLeft + 1
FROM	topics_Topics
WHERE	TopicID		= @TopicID;

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