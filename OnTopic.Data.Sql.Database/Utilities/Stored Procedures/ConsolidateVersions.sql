--------------------------------------------------------------------------------------------------------------------------------
-- CONSOLIDATE VERSIONS
--------------------------------------------------------------------------------------------------------------------------------
-- Given a start date and an end date, will consolidate all versions within that range into a single, new version. This has the
-- benefit of reducing the number of versions in the database, reducing the database size, and making some database queries
-- faster. If the end date parameter is not specified, it defaults to 2000-01-01, in which case ALL historical data will be
-- collapsed prior to the start date.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [Utilities].[ConsolidateVersions]
	@StartDate		DATETIME2(7)	= NULL,
	@EndDate		DATETIME2(7)	= NULL
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
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
-- ESTABLISH IMPLICIT DEFAULT FOR START DATE
--------------------------------------------------------------------------------------------------------------------------------
IF (@StartDate IS NULL)
  BEGIN
    SET @StartDate = CONVERT(DATETIME2(7), '2000-01-01')
  END

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE CONSOLIDATED TOPIC ATTRIBUTE RECORD
--------------------------------------------------------------------------------------------------------------------------------
;WITH	TopicAttributes
AS (
  SELECT	Version,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		TopicID,
			AttributeKey
	  ORDER BY		Version		DESC
	)
  FROM	Attributes
  WHERE	Version		> @StartDate
    AND	Version		<= @EndDate
)
UPDATE	TopicAttributes
SET	Version		= @EndDate
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE CONSOLIDATED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
DELETE
FROM	Attributes
WHERE	Version		> @StartDate
  AND	Version		< @EndDate

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE CONSOLIDATED EXTENDED ATTRIBUTES RECORD
--------------------------------------------------------------------------------------------------------------------------------
;WITH	TopicExtendedAttributes
AS (
  SELECT	Version,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		TopicID
	  ORDER BY		Version		DESC
	)
  FROM	ExtendedAttributes
  WHERE	Version		> @StartDate
    AND	Version		<= @EndDate
)
UPDATE	TopicExtendedAttributes
SET	Version		= @EndDate
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE CONSOLIDATED EXTENDED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
DELETE
FROM	ExtendedAttributes
WHERE	Version		> @StartDate
  AND	Version		< @EndDate

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE CONSOLIDATED RELATIONSHIPS RECORD
--------------------------------------------------------------------------------------------------------------------------------
;WITH	TopicRelationships
AS (
  SELECT	Version,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		Source_TopicID,
			RelationshipKey,
			Target_TopicID
	  ORDER BY		Version		DESC
	)
  FROM	Relationships
  WHERE	Version		> @StartDate
    AND	Version		<= @EndDate
)
UPDATE	TopicRelationships
SET	Version		= @EndDate
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE CONSOLIDATED RELATIONSHIPS
--------------------------------------------------------------------------------------------------------------------------------
DELETE
FROM	Relationships
WHERE	Version		> @StartDate
  AND	Version		< @EndDate

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE CONSOLIDATED TOPIC REFERENCES RECORD
--------------------------------------------------------------------------------------------------------------------------------
;WITH	TopicReferenceVersions
AS (
  SELECT	Version,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		Source_TopicID
	  ORDER BY		Version		DESC
	)
  FROM	TopicReferences
  WHERE	Version		> @StartDate
    AND	Version		<= @EndDate
)
UPDATE	TopicReferenceVersions
SET	Version		= @EndDate
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE CONSOLIDATED TOPIC REFERENCES
--------------------------------------------------------------------------------------------------------------------------------
DELETE
FROM	TopicReferences
WHERE	Version		> @StartDate
  AND	Version		< @EndDate

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