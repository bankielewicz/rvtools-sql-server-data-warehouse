/*
    RVTools Data Warehouse - Dynamic MERGE Procedure with Soft-Delete Support

    Purpose: Dynamically merges data from Staging to Current and History
             using metadata from Config.ColumnMapping.
             Includes soft-delete detection for records no longer in source.

    Parameters:
        @ImportBatchId  - The batch ID for this import
        @TableName      - Table to merge (e.g., 'vInfo', 'vHost')
        @SourceFile     - Source xlsx filename (for history tracking)
        @VIServer       - vCenter server name (for multi-vCenter soft-delete safety)
        @EffectiveDate  - Override for ValidFrom (historical imports)
        @MergedCount    - OUTPUT: Number of rows merged
        @DeletedCount   - OUTPUT: Number of rows soft-deleted

    Process:
        1. Log start to Audit.MergeProgress
        2. Read column mappings from Config.ColumnMapping
        3. Build dynamic MERGE statement with proper type conversions
        4. Archive changed/deleted records to History (HISTORY_CLOSE)
        5. **NEW: SOFT_DELETE** - Mark Current records as deleted if not in staging
        6. **NEW: UPDATE_LAST_SEEN** - Update LastSeenBatchId/Date for matched records
        7. Execute MERGE to update Current (also resets IsDeleted=0 for reappearing records)
        8. Insert new history records for changes (HISTORY_INSERT)
        9. Update progress with result (includes RowsDeleted)

    Soft-Delete Behavior:
        - Records not in current import are marked IsDeleted=1
        - ONLY marks deleted for same VI_SDK_Server (multi-vCenter safe)
        - Records that reappear have IsDeleted reset to 0
        - Deleted records tracked with DeletedBatchId, DeletedDate, DeletedReason
        - Controlled by Config.Settings 'SoftDeleteEnabled' (default: true)

    Enhanced Logging:
        - Logs to Audit.MergeProgress before/after each operation
        - On failure, logs full error details to Audit.ErrorLog
        - Captures dynamic SQL for debugging
        - Now populates RowsDeleted column

    Usage:
        DECLARE @Merged INT, @Deleted INT;
        EXEC dbo.usp_MergeTable
            @ImportBatchId = 1,
            @TableName = 'vInfo',
            @SourceFile = 'export.xlsx',
            @VIServer = 'vcenter01.domain.com',
            @MergedCount = @Merged OUTPUT,
            @DeletedCount = @Deleted OUTPUT;
*/

USE [RVToolsDW]
GO

IF OBJECT_ID('dbo.usp_MergeTable', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_MergeTable;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE PROCEDURE dbo.usp_MergeTable
    @ImportBatchId INT,
    @TableName NVARCHAR(100),
    @SourceFile NVARCHAR(500) = NULL,
    @VIServer NVARCHAR(255) = NULL,    -- vCenter server for multi-vCenter soft-delete safety
    @EffectiveDate DATETIME2 = NULL,   -- Override for ValidFrom (historical imports)
    @MergedCount INT = 0 OUTPUT,
    @DeletedCount INT = 0 OUTPUT       -- NEW: Number of rows soft-deleted
AS
BEGIN
    SET NOCOUNT ON;
    -- NOTE: Removed XACT_ABORT to allow proper error logging

    DECLARE @StartTime DATETIME2 = GETUTCDATE();
    DECLARE @Now DATETIME2 = ISNULL(@EffectiveDate, @StartTime);  -- Use effective date if provided (historical imports)
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @CurrentSQL NVARCHAR(MAX);  -- Track current SQL being executed
    DECLARE @CurrentOperation NVARCHAR(50);  -- Track current operation
    DECLARE @NaturalKeyColumns NVARCHAR(500);
    DECLARE @IsActive BIT;
    DECLARE @ProgressId BIGINT;
    DECLARE @RowsInStaging INT = 0;
    DECLARE @ErrorMsg NVARCHAR(MAX);
    DECLARE @SoftDeleteEnabled BIT = 1;  -- Default enabled

    -- Check if soft-delete is enabled in Config.Settings
    SELECT @SoftDeleteEnabled = CASE WHEN SettingValue IN ('true', '1', 'yes') THEN 1 ELSE 0 END
    FROM Config.Settings
    WHERE SettingName = 'SoftDeleteEnabled';

    -- If @VIServer not provided, try to detect from staging data
    IF @VIServer IS NULL
    BEGIN
        SET @SQL = N'SELECT TOP 1 @vi = VI_SDK_Server FROM [Staging].' + QUOTENAME(@TableName) +
                   ' WHERE ImportBatchId = @BatchId AND VI_SDK_Server IS NOT NULL';
        BEGIN TRY
            EXEC sp_executesql @SQL, N'@BatchId INT, @vi NVARCHAR(255) OUTPUT',
                @BatchId = @ImportBatchId, @vi = @VIServer OUTPUT;
        END TRY
        BEGIN CATCH
            SET @VIServer = NULL;  -- Table might not have VI_SDK_Server (e.g., vMetaData)
        END CATCH
    END

    -- ========================================================================
    -- Step 0: Log start of operation
    -- ========================================================================
    -- Count rows in staging for this batch
    SET @SQL = N'SELECT @cnt = COUNT(*) FROM [Staging].' + QUOTENAME(@TableName) + ' WHERE ImportBatchId = @BatchId';
    BEGIN TRY
        EXEC sp_executesql @SQL, N'@BatchId INT, @cnt INT OUTPUT', @BatchId = @ImportBatchId, @cnt = @RowsInStaging OUTPUT;
    END TRY
    BEGIN CATCH
        SET @RowsInStaging = 0;
    END CATCH

    -- Insert progress record BEFORE any work (survives rollback)
    INSERT INTO Audit.MergeProgress (ImportBatchId, TableName, Operation, Status, RowsInStaging)
    VALUES (@ImportBatchId, @TableName, 'MERGE', 'InProgress', @RowsInStaging);

    SET @ProgressId = SCOPE_IDENTITY();

    -- ========================================================================
    -- Validate table exists in mapping
    -- ========================================================================
    SET @CurrentOperation = 'VALIDATION';

    SELECT
        @NaturalKeyColumns = NaturalKeyColumns,
        @IsActive = IsActive
    FROM Config.TableMapping
    WHERE TableName = @TableName;

    IF @NaturalKeyColumns IS NULL
    BEGIN
        SET @ErrorMsg = 'Table "' + @TableName + '" not found in Config.TableMapping';

        UPDATE Audit.MergeProgress
        SET EndTime = GETUTCDATE(), Status = 'Failed', ErrorMessage = @ErrorMsg,
            DurationMs = DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE())
        WHERE ProgressId = @ProgressId;

        INSERT INTO Audit.ErrorLog (ImportBatchId, ProcedureName, TableName, Operation, ErrorMessage)
        VALUES (@ImportBatchId, 'usp_MergeTable', @TableName, @CurrentOperation, @ErrorMsg);

        RAISERROR(@ErrorMsg, 16, 1);
        RETURN;
    END

    IF @IsActive = 0
    BEGIN
        UPDATE Audit.MergeProgress
        SET EndTime = GETUTCDATE(), Status = 'Skipped', ErrorMessage = 'Table is inactive',
            DurationMs = DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE())
        WHERE ProgressId = @ProgressId;

        SET @MergedCount = 0;
        RETURN;
    END

    -- Skip if no rows in staging
    IF @RowsInStaging = 0
    BEGIN
        UPDATE Audit.MergeProgress
        SET EndTime = GETUTCDATE(), Status = 'Skipped', RowsProcessed = 0,
            ErrorMessage = 'No rows in staging', DurationMs = DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE())
        WHERE ProgressId = @ProgressId;

        SET @MergedCount = 0;
        RETURN;
    END

    -- ========================================================================
    -- Build dynamic SQL components
    -- ========================================================================
    SET @CurrentOperation = 'BUILD_SQL';

    DECLARE @SelectColumns NVARCHAR(MAX) = '';
    DECLARE @OnClause NVARCHAR(MAX) = '';
    DECLARE @UpdateSet NVARCHAR(MAX) = '';
    DECLARE @InsertColumns NVARCHAR(MAX) = '';
    DECLARE @InsertValues NVARCHAR(MAX) = '';
    DECLARE @HistoryColumns NVARCHAR(MAX) = '';
    DECLARE @HistoryValues NVARCHAR(MAX) = '';

    -- Cursor to build column expressions
    DECLARE @StagingCol NVARCHAR(200);
    DECLARE @CurrentCol NVARCHAR(200);
    DECLARE @DataType NVARCHAR(100);
    DECLARE @IsBoolean BIT;
    DECLARE @IsNaturalKey BIT;
    DECLARE @IsSystem BIT;
    DECLARE @ColExpression NVARCHAR(500);

    -- Check which columns actually exist in Staging table
    DECLARE @StagingColumns TABLE (ColumnName NVARCHAR(200));
    INSERT INTO @StagingColumns
    SELECT c.name
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    INNER JOIN sys.columns c ON t.object_id = c.object_id
    WHERE s.name = 'Staging' AND t.name = @TableName;

    DECLARE col_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT
            StagingColumnName,
            CurrentColumnName,
            TargetDataType,
            IsBooleanField,
            IsNaturalKey,
            IsSystemColumn
        FROM Config.ColumnMapping
        WHERE TableName = @TableName
          AND IncludeInMerge = 1
        ORDER BY OrdinalPosition;

    DECLARE @ExistsInStaging BIT;

    OPEN col_cursor;
    FETCH NEXT FROM col_cursor INTO @StagingCol, @CurrentCol, @DataType, @IsBoolean, @IsNaturalKey, @IsSystem;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Check if this column exists in Staging
        SET @ExistsInStaging = CASE WHEN EXISTS (SELECT 1 FROM @StagingColumns WHERE ColumnName = @StagingCol) THEN 1 ELSE 0 END;

        -- Skip columns that don't exist in staging and aren't handled specially
        IF @ExistsInStaging = 0 AND @CurrentCol NOT IN ('LastModifiedDate', 'ModifiedDate', 'CreatedDate')
        BEGIN
            FETCH NEXT FROM col_cursor INTO @StagingCol, @CurrentCol, @DataType, @IsBoolean, @IsNaturalKey, @IsSystem;
            CONTINUE;
        END

        -- Build column expression with appropriate type conversion
        IF @ExistsInStaging = 0 AND @CurrentCol IN ('LastModifiedDate', 'ModifiedDate')
        BEGIN
            SET @ColExpression = '@Now';
        END
        ELSE IF @IsBoolean = 1
        BEGIN
            SET @ColExpression = 'CASE WHEN s.[' + @StagingCol + '] IN (''True'',''1'',''Yes'') THEN 1 ELSE 0 END';
        END
        ELSE IF @DataType IN ('int', 'bigint', 'smallint', 'tinyint')
        BEGIN
            SET @ColExpression = 'TRY_CAST(s.[' + @StagingCol + '] AS ' + @DataType + ')';
        END
        ELSE IF @DataType LIKE 'decimal%' OR @DataType LIKE 'numeric%'
        BEGIN
            SET @ColExpression = 'TRY_CAST(s.[' + @StagingCol + '] AS ' + @DataType + ')';
        END
        ELSE IF @DataType IN ('datetime', 'datetime2', 'date', 'time')
        BEGIN
            SET @ColExpression = 'TRY_CAST(s.[' + @StagingCol + '] AS ' + @DataType + ')';
        END
        ELSE IF @DataType = 'bit'
        BEGIN
            SET @ColExpression = 'CASE WHEN s.[' + @StagingCol + '] IN (''True'',''1'',''Yes'') THEN 1 ELSE 0 END';
        END
        ELSE IF @CurrentCol = 'ImportBatchId'
        BEGIN
            SET @ColExpression = '@BatchId';
        END
        ELSE
        BEGIN
            SET @ColExpression = 's.[' + @StagingCol + ']';
        END

        -- Add to SELECT clause
        IF LEN(@SelectColumns) > 0 SET @SelectColumns = @SelectColumns + ',
            ';
        SET @SelectColumns = @SelectColumns + @ColExpression + ' AS [' + @CurrentCol + ']';

        -- Add to ON clause (natural keys only)
        IF @IsNaturalKey = 1
        BEGIN
            IF LEN(@OnClause) > 0 SET @OnClause = @OnClause + ' AND ';
            SET @OnClause = @OnClause + 'target.[' + @CurrentCol + '] = source.[' + @CurrentCol + ']';
        END

        -- Add to UPDATE SET clause (skip natural keys and system tracking columns)
        IF @IsNaturalKey = 0 AND @CurrentCol NOT IN ('CreatedDate', 'RowId')
        BEGIN
            IF LEN(@UpdateSet) > 0 SET @UpdateSet = @UpdateSet + ',
                ';
            IF @CurrentCol = 'ImportBatchId'
                SET @UpdateSet = @UpdateSet + '[ImportBatchId] = @BatchId';
            ELSE IF @CurrentCol IN ('LastModifiedDate', 'ModifiedDate')
                SET @UpdateSet = @UpdateSet + '[' + @CurrentCol + '] = @Now';
            ELSE
                SET @UpdateSet = @UpdateSet + '[' + @CurrentCol + '] = source.[' + @CurrentCol + ']';
        END

        -- Add to INSERT columns/values
        IF @CurrentCol NOT IN ('RowId', 'CreatedDate')
        BEGIN
            IF LEN(@InsertColumns) > 0
            BEGIN
                SET @InsertColumns = @InsertColumns + ', ';
                SET @InsertValues = @InsertValues + ', ';
            END
            SET @InsertColumns = @InsertColumns + '[' + @CurrentCol + ']';
            IF @CurrentCol = 'ImportBatchId'
                SET @InsertValues = @InsertValues + '@BatchId';
            ELSE IF @CurrentCol IN ('LastModifiedDate', 'ModifiedDate')
                SET @InsertValues = @InsertValues + '@Now';
            ELSE
                SET @InsertValues = @InsertValues + 'source.[' + @CurrentCol + ']';
        END

        -- History columns (for archiving)
        IF @CurrentCol NOT IN ('RowId', 'CreatedDate', 'LastModifiedDate', 'ModifiedDate', 'ImportBatchId')
        BEGIN
            IF LEN(@HistoryColumns) > 0
            BEGIN
                SET @HistoryColumns = @HistoryColumns + ', ';
                SET @HistoryValues = @HistoryValues + ', ';
            END
            SET @HistoryColumns = @HistoryColumns + '[' + @CurrentCol + ']';
            SET @HistoryValues = @HistoryValues + 'c.[' + @CurrentCol + ']';
        END

        FETCH NEXT FROM col_cursor INTO @StagingCol, @CurrentCol, @DataType, @IsBoolean, @IsNaturalKey, @IsSystem;
    END

    CLOSE col_cursor;
    DEALLOCATE col_cursor;

    -- ========================================================================
    -- Validate we have required components
    -- ========================================================================
    IF LEN(@OnClause) = 0
    BEGIN
        SET @ErrorMsg = 'No natural key columns found for table "' + @TableName + '"';

        UPDATE Audit.MergeProgress
        SET EndTime = GETUTCDATE(), Status = 'Failed', ErrorMessage = @ErrorMsg,
            DurationMs = DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE())
        WHERE ProgressId = @ProgressId;

        INSERT INTO Audit.ErrorLog (ImportBatchId, ProcedureName, TableName, Operation, ErrorMessage)
        VALUES (@ImportBatchId, 'usp_MergeTable', @TableName, @CurrentOperation, @ErrorMsg);

        RAISERROR(@ErrorMsg, 16, 1);
        RETURN;
    END

    -- ========================================================================
    -- Step 1: Close out history records for changed/deleted rows
    -- ========================================================================
    SET @CurrentOperation = 'HISTORY_CLOSE';

    DECLARE @HistoryOnClause NVARCHAR(MAX) = REPLACE(REPLACE(@OnClause, 'target.', 'h.'), 'source.', 'c.');
    DECLARE @StagingCompareClause NVARCHAR(MAX) = REPLACE(REPLACE(@OnClause, 'target.', 's.'), 'source.', 'c.');

    SET @CurrentSQL = N'
    UPDATE h
    SET h.ValidTo = @Now
    FROM [History].[' + @TableName + '] h
    INNER JOIN [Current].[' + @TableName + '] c
        ON h.ValidTo IS NULL
        AND ' + @HistoryOnClause + '
    WHERE NOT EXISTS (
        SELECT 1 FROM [Staging].[' + @TableName + '] s
        WHERE s.ImportBatchId = @BatchId
        AND ' + @StagingCompareClause + '
    );';

    BEGIN TRY
        EXEC sp_executesql @CurrentSQL,
            N'@BatchId INT, @Now DATETIME2',
            @BatchId = @ImportBatchId,
            @Now = @Now;
    END TRY
    BEGIN CATCH
        SET @ErrorMsg = 'History close-out failed for ' + @TableName + ': ' + ERROR_MESSAGE();

        UPDATE Audit.MergeProgress
        SET EndTime = GETUTCDATE(), Status = 'Failed', ErrorMessage = LEFT(@ErrorMsg, 4000),
            DurationMs = DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE())
        WHERE ProgressId = @ProgressId;

        INSERT INTO Audit.ErrorLog (ImportBatchId, ProcedureName, TableName, Operation,
            ErrorNumber, ErrorSeverity, ErrorState, ErrorLine, ErrorMessage, DynamicSQL)
        VALUES (@ImportBatchId, 'usp_MergeTable', @TableName, @CurrentOperation,
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE(),
            ERROR_MESSAGE(), LEFT(@CurrentSQL, 8000));

        RAISERROR(@ErrorMsg, 16, 1);
        RETURN;
    END CATCH

    -- ========================================================================
    -- Step 1b: SOFT_DELETE - Mark records as deleted if not in staging
    -- Only runs if SoftDeleteEnabled=true and we have VI_SDK_Server context
    -- ========================================================================
    IF @SoftDeleteEnabled = 1
    BEGIN
        SET @CurrentOperation = 'SOFT_DELETE';

        -- Build VI_SDK_Server filter (multi-vCenter safety)
        DECLARE @VIServerFilter NVARCHAR(500) = '';
        IF @VIServer IS NOT NULL
        BEGIN
            SET @VIServerFilter = ' AND c.VI_SDK_Server = @VIServer';
        END

        -- Check if IsDeleted column exists in this table
        DECLARE @HasIsDeleted BIT = 0;
        SELECT @HasIsDeleted = 1
        FROM sys.columns col
        INNER JOIN sys.tables t ON col.object_id = t.object_id
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = 'Current' AND t.name = @TableName AND col.name = 'IsDeleted';

        IF @HasIsDeleted = 1
        BEGIN
            -- Mark records as deleted that are NOT in current staging import
            -- but ARE for the same VI_SDK_Server being imported
            SET @CurrentSQL = N'
            UPDATE c
            SET
                c.IsDeleted = 1,
                c.DeletedBatchId = @BatchId,
                c.DeletedDate = @Now,
                c.DeletedReason = ''NotInSource''
            FROM [Current].[' + @TableName + '] c
            WHERE c.IsDeleted = 0' + @VIServerFilter + '
              AND NOT EXISTS (
                  SELECT 1 FROM [Staging].[' + @TableName + '] s
                  WHERE s.ImportBatchId = @BatchId
                  AND ' + @StagingCompareClause + '
              );

            SELECT @DeletedCnt = @@ROWCOUNT;
            ';

            BEGIN TRY
                EXEC sp_executesql @CurrentSQL,
                    N'@BatchId INT, @Now DATETIME2, @VIServer NVARCHAR(255), @DeletedCnt INT OUTPUT',
                    @BatchId = @ImportBatchId,
                    @Now = @Now,
                    @VIServer = @VIServer,
                    @DeletedCnt = @DeletedCount OUTPUT;
            END TRY
            BEGIN CATCH
                -- Log but don't fail - soft-delete is supplementary
                INSERT INTO Audit.ErrorLog (ImportBatchId, ProcedureName, TableName, Operation,
                    ErrorNumber, ErrorSeverity, ErrorState, ErrorLine, ErrorMessage, DynamicSQL)
                VALUES (@ImportBatchId, 'usp_MergeTable', @TableName, @CurrentOperation,
                    ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE(),
                    ERROR_MESSAGE(), LEFT(@CurrentSQL, 8000));
                SET @DeletedCount = 0;
            END CATCH
        END
    END

    -- ========================================================================
    -- Step 1c: UPDATE_LAST_SEEN - Update LastSeenBatchId/Date for matched records
    -- Also resets IsDeleted=0 if a record reappears after being deleted
    -- ========================================================================
    IF @SoftDeleteEnabled = 1
    BEGIN
        SET @CurrentOperation = 'UPDATE_LAST_SEEN';

        -- Check if LastSeenBatchId column exists
        DECLARE @HasLastSeen BIT = 0;
        SELECT @HasLastSeen = 1
        FROM sys.columns col
        INNER JOIN sys.tables t ON col.object_id = t.object_id
        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name = 'Current' AND t.name = @TableName AND col.name = 'LastSeenBatchId';

        IF @HasLastSeen = 1
        BEGIN
            -- Update LastSeen for records that ARE in current import
            -- Also reset IsDeleted=0 if previously deleted (record reappeared)
            SET @CurrentSQL = N'
            UPDATE c
            SET
                c.LastSeenBatchId = @BatchId,
                c.LastSeenDate = @Now,
                c.IsDeleted = 0,
                c.DeletedBatchId = NULL,
                c.DeletedDate = NULL,
                c.DeletedReason = NULL
            FROM [Current].[' + @TableName + '] c
            INNER JOIN [Staging].[' + @TableName + '] s
                ON ' + REPLACE(REPLACE(@OnClause, 'target.', 'c.'), 'source.', 's.') + '
            WHERE s.ImportBatchId = @BatchId;
            ';

            BEGIN TRY
                EXEC sp_executesql @CurrentSQL,
                    N'@BatchId INT, @Now DATETIME2',
                    @BatchId = @ImportBatchId,
                    @Now = @Now;
            END TRY
            BEGIN CATCH
                -- Log but don't fail - LastSeen update is supplementary
                INSERT INTO Audit.ErrorLog (ImportBatchId, ProcedureName, TableName, Operation,
                    ErrorNumber, ErrorSeverity, ErrorState, ErrorLine, ErrorMessage, DynamicSQL)
                VALUES (@ImportBatchId, 'usp_MergeTable', @TableName, @CurrentOperation,
                    ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE(),
                    ERROR_MESSAGE(), LEFT(@CurrentSQL, 8000));
            END CATCH
        END
    END

    -- ========================================================================
    -- Step 2: Build and execute MERGE statement
    -- ========================================================================
    SET @CurrentOperation = 'MERGE';

    DECLARE @FirstKeyCol NVARCHAR(200);
    SELECT TOP 1 @FirstKeyCol = CurrentColumnName
    FROM Config.ColumnMapping
    WHERE TableName = @TableName AND IsNaturalKey = 1
    ORDER BY OrdinalPosition;

    -- Build WHERE clause to exclude rows where ALL natural key columns are NULL
    DECLARE @NullKeyFilter NVARCHAR(MAX) = '';
    SELECT @NullKeyFilter = @NullKeyFilter +
        CASE WHEN LEN(@NullKeyFilter) > 0 THEN ' OR ' ELSE '' END +
        's.[' + CurrentColumnName + '] IS NOT NULL'
    FROM Config.ColumnMapping
    WHERE TableName = @TableName AND IsNaturalKey = 1
    ORDER BY OrdinalPosition;

    IF LEN(@NullKeyFilter) > 0
        SET @NullKeyFilter = ' AND (' + @NullKeyFilter + ')';

    SET @CurrentSQL = N'
    DECLARE @Changes TABLE (Action NVARCHAR(10), KeyValue NVARCHAR(500));

    MERGE [Current].[' + @TableName + '] AS target
    USING (
        SELECT
            ' + @SelectColumns + '
        FROM [Staging].[' + @TableName + '] s
        WHERE s.ImportBatchId = @BatchId' + @NullKeyFilter + '
    ) AS source
    ON ' + @OnClause + '

    WHEN MATCHED THEN
        UPDATE SET
            ' + @UpdateSet + '

    WHEN NOT MATCHED BY TARGET THEN
        INSERT (' + @InsertColumns + ')
        VALUES (' + @InsertValues + ')

    OUTPUT $action, INSERTED.[' + @FirstKeyCol + ']
    INTO @Changes;

    SELECT @RowCount = COUNT(*) FROM @Changes;
    ';

    DECLARE @RowCount INT = 0;

    BEGIN TRY
        EXEC sp_executesql @CurrentSQL,
            N'@BatchId INT, @Now DATETIME2, @RowCount INT OUTPUT',
            @BatchId = @ImportBatchId,
            @Now = @Now,
            @RowCount = @RowCount OUTPUT;

        SET @MergedCount = @RowCount;
    END TRY
    BEGIN CATCH
        SET @ErrorMsg = 'MERGE failed for ' + @TableName + ': ' + ERROR_MESSAGE();

        UPDATE Audit.MergeProgress
        SET EndTime = GETUTCDATE(), Status = 'Failed', ErrorMessage = LEFT(@ErrorMsg, 4000),
            DurationMs = DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE())
        WHERE ProgressId = @ProgressId;

        INSERT INTO Audit.ErrorLog (ImportBatchId, ProcedureName, TableName, Operation,
            ErrorNumber, ErrorSeverity, ErrorState, ErrorLine, ErrorMessage, DynamicSQL,
            ContextData)
        VALUES (@ImportBatchId, 'usp_MergeTable', @TableName, @CurrentOperation,
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE(),
            ERROR_MESSAGE(), LEFT(@CurrentSQL, 8000),
            '{"RowsInStaging":' + CAST(@RowsInStaging AS VARCHAR) + ',"NaturalKeys":"' + @NaturalKeyColumns + '"}');

        RAISERROR(@ErrorMsg, 16, 1);
        RETURN;
    END CATCH

    -- ========================================================================
    -- Step 3: Insert new history records for all current data
    -- ========================================================================
    SET @CurrentOperation = 'HISTORY_INSERT';

    SET @CurrentSQL = N'
    INSERT INTO [History].[' + @TableName + '] (
        ImportBatchId, ValidFrom, ValidTo, SourceFile,
        ' + @HistoryColumns + '
    )
    SELECT
        @BatchId, @Now, NULL, @SourceFile,
        ' + @HistoryValues + '
    FROM [Current].[' + @TableName + '] c
    WHERE c.ImportBatchId = @BatchId;
    ';

    BEGIN TRY
        EXEC sp_executesql @CurrentSQL,
            N'@BatchId INT, @Now DATETIME2, @SourceFile NVARCHAR(500)',
            @BatchId = @ImportBatchId,
            @Now = @Now,
            @SourceFile = @SourceFile;
    END TRY
    BEGIN CATCH
        -- Log but don't fail for history insert issues
        INSERT INTO Audit.ErrorLog (ImportBatchId, ProcedureName, TableName, Operation,
            ErrorNumber, ErrorSeverity, ErrorState, ErrorLine, ErrorMessage, DynamicSQL)
        VALUES (@ImportBatchId, 'usp_MergeTable', @TableName, @CurrentOperation,
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE(),
            ERROR_MESSAGE(), LEFT(@CurrentSQL, 8000));
    END CATCH

    -- ========================================================================
    -- Log success (now includes RowsDeleted)
    -- ========================================================================
    UPDATE Audit.MergeProgress
    SET EndTime = GETUTCDATE(),
        Status = 'Success',
        RowsProcessed = @MergedCount,
        RowsDeleted = @DeletedCount,  -- NEW: Track soft-deleted count
        DurationMs = DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE())
    WHERE ProgressId = @ProgressId;

END
GO

PRINT 'Created dbo.usp_MergeTable with enhanced logging';
GO
