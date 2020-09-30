--------------------------------------------------------------------------------------------------------------------------------
-- CONSOLIDATE VERSIONS
--------------------------------------------------------------------------------------------------------------------------------
-- Given a start date and an end date, will consolidate all versions within that range into a single, new version. This has the
-- benefit of reducing the number of versions in the database, reducing the database size, and making some database queries
-- faster. If the end date parameter is not specified, it defaults to 2000-01-01, in which case ALL historical data will be
-- collapsed prior to the start date.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [Utilities].[ConsolidateVersions]
	@StartDate		DATETIME	= NULL,
	@EndDate		DATETIME	= NULL
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
    SET @StartDate = CONVERT(DATETIME, '2000-01-01')
  END

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC ATTRIBUTES
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
-- SELECT EXTENDED ATTRIBUTES
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
-- DELETE CONSOLIDATED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
DELETE
FROM	ExtendedAttributes
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