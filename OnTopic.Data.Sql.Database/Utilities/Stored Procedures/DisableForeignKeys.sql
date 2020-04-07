CREATE PROCEDURE DisableForeignKeys
	@disable		BIT	= 1
AS

DECLARE
	@sql		VARCHAR(500),
	@tableName		VARCHAR(128),
	@foreignKeyName		VARCHAR(128)

-- A list of all foreign keys and table names
DECLARE	foreignKeyCursor	CURSOR
FOR	SELECT
	  ref.CONSTRAINT_NAME	AS FK_Name,
	  fk.TABLE_NAME		AS FK_Table
FROM	INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS ref
INNER JOIN	INFORMATION_SCHEMA.TABLE_CONSTRAINTS fk
  ON	ref.CONSTRAINT_NAME	= fk.CONSTRAINT_NAME
ORDER BY	fk.TABLE_NAME,
	ref.CONSTRAINT_NAME

OPEN	foreignKeyCursor

FETCH NEXT
FROM	foreignKeyCursor
INTO	@foreignKeyName,
	@tableName

WHILE (	@@FETCH_STATUS = 0
)
BEGIN
  IF	@disable = 1
    SET	@sql = 'ALTER TABLE ['
	+ @tableName + '] NOCHECK CONSTRAINT ['
	+ @foreignKeyName + ']'
  ELSE
    SET	@sql = 'ALTER TABLE ['
	+ @tableName + '] CHECK CONSTRAINT ['
	+ @foreignKeyName + ']'

PRINT	'Executing Statement - ' + @sql

EXECUTE(@sql)

FETCH NEXT
FROM	foreignKeyCursor
INTO	@foreignKeyName,
	@tableName
END

CLOSE	foreignKeyCursor
DEALLOCATE	foreignKeyCursor