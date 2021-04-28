/*
 Copyright © 2012 - 2021 François Chabot

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
 */

USE [BAMPrimaryImport]
GO

UPDATE dbo.bam_Metadata_Activities
SET OnlineWindowTimeUnit = 'DAY',
    OnlineWindowTimeLength = $(BamOnlineWindowTimeLength)
WHERE ActivityName IN ('Process', 'ProcessingStep', 'ProcessMessagingStep', 'MessagingStep')
GO

/****** Object:  View [dbo].[vw_MessagingStepContexts] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_MessagingStepContexts]'))
   DROP VIEW [dbo].[vw_MessagingStepContexts]
GO

/****** Object:  View [dbo].[vw_MessagingStepContexts] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_MessagingStepContexts]
AS
   SELECT ActivityID AS MessagingStepActivityID,
         LongReferenceData AS EncodedContext
   FROM [bam_MessagingStep_AllRelationships]
   WHERE ReferenceType = 'Ctxt'
GO

/****** Object:  View [dbo].[vw_MessagingStepMessages] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_MessagingStepMessages]'))
   DROP VIEW [dbo].[vw_MessagingStepMessages]
GO

/****** Object:  View [dbo].[vw_MessagingStepMessages] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_MessagingStepMessages]
AS
   SELECT ActivityID AS MessagingStepActivityID,
         ReferenceType AS EncodedBodyType,
         LongReferenceData AS EncodedBody
   FROM [bam_MessagingStep_AllRelationships]
   WHERE ReferenceType = 'Claimed' OR ReferenceType = 'Unclaimed'
GO

/****** Object:  View [dbo].[vw_LastWeekFailedMessagingSteps] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_LastWeekFailedMessagingSteps]'))
   DROP VIEW [dbo].[vw_LastWeekFailedMessagingSteps]
GO

/****** Object:  View [dbo].[vw_LastWeekFailedMessagingSteps] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_LastWeekFailedMessagingSteps]
AS
    WITH DataTable AS (
        SELECT ISNULL(PortName, 'Direct') AS PortName, DATEDIFF(DAY, [Time], SYSUTCDATETIME()) [DaysAgo], COUNT(1) AS FailureCount
          FROM dbo.bam_MessagingStep_CompletedInstances
          WHERE [Status] = 'FailedMessage' AND [Time] > DATEADD(dd, 0, DATEDIFF(dd, 0, DATEADD(day, -6, SYSUTCDATETIME())))
          GROUP BY PortName, DATEDIFF(DAY, [Time], SYSUTCDATETIME()) HAVING COUNT(1) > 0
    ),
    PivotTable AS (
        SELECT PortName,
            ISNULL(CONVERT(VARCHAR, [6]), '') AS [6],
            ISNULL(CONVERT(VARCHAR, [5]), '') AS [5],
            ISNULL(CONVERT(VARCHAR, [4]), '') AS [4],
            ISNULL(CONVERT(VARCHAR, [3]), '') AS [3],
            ISNULL(CONVERT(VARCHAR, [2]), '') AS [2],
            ISNULL(CONVERT(VARCHAR, [1]), '') AS [1],
            ISNULL(CONVERT(VARCHAR, [0]), '') AS [0]
        FROM DataTable
        PIVOT (AVG(FailureCount) FOR [DaysAgo] IN ([6], [5], [4], [3], [2], [1], [0])) PivotTable
    ),
    FormattedTable AS (
        SELECT '_Date_' as [PortName],
            CONVERT(VARCHAR, DATEADD(day, -6, SYSUTCDATETIME()), 102) AS [6],
            CONVERT(VARCHAR, DATEADD(day, -5, SYSUTCDATETIME()), 102) AS [5],
            CONVERT(VARCHAR, DATEADD(day, -4, SYSUTCDATETIME()), 102) AS [4],
            CONVERT(VARCHAR, DATEADD(day, -3, SYSUTCDATETIME()), 102) AS [3],
            CONVERT(VARCHAR, DATEADD(day, -2, SYSUTCDATETIME()), 102) AS [2],
            CONVERT(VARCHAR, DATEADD(day, -1, SYSUTCDATETIME()), 102) AS [1],
            CONVERT(VARCHAR, DATEADD(day, -0, SYSUTCDATETIME()), 102) AS [0]
        UNION ALL
        SELECT * FROM PivotTable
    )
    SELECT PortName,
        [6] AS [6 Days Ago],
        [5] AS [5 Days Ago],
        [4] AS [4 Days Ago],
        [3] AS [3 Days Ago],
        [2] AS [2 Days Ago],
        [1] AS [Yesterday],
        [0] AS [Today]
    FROM FormattedTable
GO

/****** Object:  View [dbo].[vw_LastWeekFailedProcesses] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_LastWeekFailedProcesses]'))
   DROP VIEW [dbo].[vw_LastWeekFailedProcesses]
GO

/****** Object:  View [dbo].[vw_LastWeekFailedProcesses] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_LastWeekFailedProcesses]
AS
    WITH DataTable AS (
        SELECT ProcessName, DATEDIFF(DAY, BeginTime, SYSUTCDATETIME()) [DaysAgo], COUNT(1) AS FailureCount
          FROM dbo.bam_Process_CompletedInstances
          WHERE [Status] = 'Failed' AND BeginTime > DATEADD(dd, 0, DATEDIFF(dd, 0, DATEADD(day, -6, SYSUTCDATETIME())))
          GROUP BY ProcessName, DATEDIFF(DAY, BeginTime, SYSUTCDATETIME()) HAVING COUNT(1) > 0
    ),
    PivotTable AS (
        SELECT ProcessName,
            ISNULL(CONVERT(VARCHAR, [6]), '') AS [6],
            ISNULL(CONVERT(VARCHAR, [5]), '') AS [5],
            ISNULL(CONVERT(VARCHAR, [4]), '') AS [4],
            ISNULL(CONVERT(VARCHAR, [3]), '') AS [3],
            ISNULL(CONVERT(VARCHAR, [2]), '') AS [2],
            ISNULL(CONVERT(VARCHAR, [1]), '') AS [1],
            ISNULL(CONVERT(VARCHAR, [0]), '') AS [0]
        FROM DataTable
        PIVOT (AVG(FailureCount) FOR [DaysAgo] IN ([6], [5], [4], [3], [2], [1], [0])) PivotTable
    ),
    FormattedTable AS (
        SELECT '_Date_' as ProcessName,
            CONVERT(VARCHAR, DATEADD(day, -6, SYSUTCDATETIME()), 102) AS [6],
            CONVERT(VARCHAR, DATEADD(day, -5, SYSUTCDATETIME()), 102) AS [5],
            CONVERT(VARCHAR, DATEADD(day, -4, SYSUTCDATETIME()), 102) AS [4],
            CONVERT(VARCHAR, DATEADD(day, -3, SYSUTCDATETIME()), 102) AS [3],
            CONVERT(VARCHAR, DATEADD(day, -2, SYSUTCDATETIME()), 102) AS [2],
            CONVERT(VARCHAR, DATEADD(day, -1, SYSUTCDATETIME()), 102) AS [1],
            CONVERT(VARCHAR, DATEADD(day, -0, SYSUTCDATETIME()), 102) AS [0]
        UNION ALL
        SELECT * FROM PivotTable
    )
    SELECT ProcessName,
        [6] AS [6 Days Ago],
        [5] AS [5 Days Ago],
        [4] AS [4 Days Ago],
        [3] AS [3 Days Ago],
        [2] AS [2 Days Ago],
        [1] AS [Yesterday],
        [0] AS [Today]
    FROM FormattedTable
GO

/****** Object:  View [dbo].[vw_LastWeekSuccessfulMessagingSteps] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_LastWeekSuccessfulMessagingSteps]'))
   DROP VIEW [dbo].[vw_LastWeekSuccessfulMessagingSteps]
GO

/****** Object:  View [dbo].[vw_LastWeekSuccessfulMessagingSteps] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_LastWeekSuccessfulMessagingSteps]
AS
    WITH DataTable AS (
        SELECT ISNULL(PortName, 'Direct') AS PortName, DATEDIFF(DAY, [Time], SYSUTCDATETIME()) [DaysAgo], COUNT(1) AS FailureCount
          FROM dbo.bam_MessagingStep_CompletedInstances MS
          WHERE [Status] <> 'FailedMessage' AND [Time] > DATEADD(dd, 0, DATEDIFF(dd, 0, DATEADD(day, -6, SYSUTCDATETIME())))
          GROUP BY PortName, DATEDIFF(DAY, [Time], SYSUTCDATETIME()) HAVING COUNT(1) > 0
    ),
    PivotTable AS (
        SELECT PortName,
            ISNULL(CONVERT(VARCHAR, [6]), '') AS [6],
            ISNULL(CONVERT(VARCHAR, [5]), '') AS [5],
            ISNULL(CONVERT(VARCHAR, [4]), '') AS [4],
            ISNULL(CONVERT(VARCHAR, [3]), '') AS [3],
            ISNULL(CONVERT(VARCHAR, [2]), '') AS [2],
            ISNULL(CONVERT(VARCHAR, [1]), '') AS [1],
            ISNULL(CONVERT(VARCHAR, [0]), '') AS [0]
        FROM DataTable
        PIVOT (AVG(FailureCount) FOR [DaysAgo] IN ([6], [5], [4], [3], [2], [1], [0])) PivotTable
    ),
    FormattedTable AS (
        SELECT '_Date_' as [PortName],
            CONVERT(VARCHAR, DATEADD(day, -6, SYSUTCDATETIME()), 102) AS [6],
            CONVERT(VARCHAR, DATEADD(day, -5, SYSUTCDATETIME()), 102) AS [5],
            CONVERT(VARCHAR, DATEADD(day, -4, SYSUTCDATETIME()), 102) AS [4],
            CONVERT(VARCHAR, DATEADD(day, -3, SYSUTCDATETIME()), 102) AS [3],
            CONVERT(VARCHAR, DATEADD(day, -2, SYSUTCDATETIME()), 102) AS [2],
            CONVERT(VARCHAR, DATEADD(day, -1, SYSUTCDATETIME()), 102) AS [1],
            CONVERT(VARCHAR, DATEADD(day, -0, SYSUTCDATETIME()), 102) AS [0]
        UNION ALL
        SELECT * FROM PivotTable
    )
    SELECT PortName,
        [6] AS [6 Days Ago],
        [5] AS [5 Days Ago],
        [4] AS [4 Days Ago],
        [3] AS [3 Days Ago],
        [2] AS [2 Days Ago],
        [1] AS [Yesterday],
        [0] AS [Today]
    FROM FormattedTable
GO

/****** Object:  View [dbo].[vw_LastWeekSuccessfulProcesses] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_LastWeekSuccessfulProcesses]'))
   DROP VIEW [dbo].[vw_LastWeekSuccessfulProcesses]
GO

/****** Object:  View [dbo].[vw_LastWeekSuccessfulProcesses] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_LastWeekSuccessfulProcesses]
AS
    WITH DataTable AS (
        SELECT ProcessName, DATEDIFF(DAY, BeginTime, SYSUTCDATETIME()) [DaysAgo], COUNT(1) AS FailureCount
          FROM dbo.bam_Process_CompletedInstances
          WHERE [Status] <> 'Failed' AND BeginTime > DATEADD(dd, 0, DATEDIFF(dd, 0, DATEADD(day, -6, SYSUTCDATETIME())))
          GROUP BY ProcessName, DATEDIFF(DAY, BeginTime, SYSUTCDATETIME()) HAVING COUNT(1) > 0
    ),
    PivotTable AS (
        SELECT ProcessName,
            ISNULL(CONVERT(VARCHAR, [6]), '') AS [6],
            ISNULL(CONVERT(VARCHAR, [5]), '') AS [5],
            ISNULL(CONVERT(VARCHAR, [4]), '') AS [4],
            ISNULL(CONVERT(VARCHAR, [3]), '') AS [3],
            ISNULL(CONVERT(VARCHAR, [2]), '') AS [2],
            ISNULL(CONVERT(VARCHAR, [1]), '') AS [1],
            ISNULL(CONVERT(VARCHAR, [0]), '') AS [0]
        FROM DataTable
        PIVOT (AVG(FailureCount) FOR [DaysAgo] IN ([6], [5], [4], [3], [2], [1], [0])) PivotTable
    ),
    FormattedTable AS (
        SELECT '_Date_' as ProcessName,
            CONVERT(VARCHAR, DATEADD(day, -6, SYSUTCDATETIME()), 102) AS [6],
            CONVERT(VARCHAR, DATEADD(day, -5, SYSUTCDATETIME()), 102) AS [5],
            CONVERT(VARCHAR, DATEADD(day, -4, SYSUTCDATETIME()), 102) AS [4],
            CONVERT(VARCHAR, DATEADD(day, -3, SYSUTCDATETIME()), 102) AS [3],
            CONVERT(VARCHAR, DATEADD(day, -2, SYSUTCDATETIME()), 102) AS [2],
            CONVERT(VARCHAR, DATEADD(day, -1, SYSUTCDATETIME()), 102) AS [1],
            CONVERT(VARCHAR, DATEADD(day, -0, SYSUTCDATETIME()), 102) AS [0]
        UNION ALL
        SELECT * FROM PivotTable
    )
    SELECT ProcessName,
        [6] AS [6 Days Ago],
        [5] AS [5 Days Ago],
        [4] AS [4 Days Ago],
        [3] AS [3 Days Ago],
        [2] AS [2 Days Ago],
        [1] AS [Yesterday],
        [0] AS [Today]
    FROM FormattedTable
GO

/****** Object:  StoredProcedure [dbo].[RemoveDanglingInstances] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RemoveDanglingInstances]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RemoveDanglingInstances]
GO

/****** Object:  StoredProcedure [dbo].[RemoveDanglingInstances] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Microsoft
-- Description: http://msdn.microsoft.com/en-us/library/aa560758.aspx
-- =============================================
-- TODO : Check latest version
CREATE PROCEDURE [dbo].[RemoveDanglingInstances]
    @ActivityName nvarchar(128),
    @ActivityId nvarchar(128) = NULL,
    @DateThreshold datetime = NULL,
    @NewTableExtension nvarchar(30) = NULL
AS
    DECLARE @QueryString nvarchar(4000)
    DECLARE @ActiveTableName sysname
    DECLARE @ActiveRelationshipsTableName sysname
    DECLARE @ContinuationsTableName sysname
    DECLARE @DanglingActiveTableName sysname
    DECLARE @DanglingActiveRelationshipsTableName sysname
    DECLARE @DanglingContinuationsTableName sysname

    SET @ActiveTableName = 'bam_' + @ActivityName + '_Active'
    SET @ActiveRelationshipsTableName = 'bam_' + @ActivityName + '_ActiveRelationships'
    SET @ContinuationsTableName = 'bam_' + @ActivityName + '_Continuations'

    SET TRANSACTION ISOLATION LEVEL READ COMMITTED
    BEGIN TRAN

    DECLARE @LockActivity nvarchar(128)
    SELECT @LockActivity = ActivityName
    FROM bam_Metadata_Activities WITH (XLOCK)
    WHERE ActivityName = @ActivityName

    EXEC sp_tables @table_name = #DanglingActivities
    IF @@ROWCOUNT > 0 DROP TABLE #DanglingActivities

    CREATE TABLE #DanglingActivities(ActivityID nvarchar(128) PRIMARY KEY)

    SET @QueryString = N'INSERT INTO #DanglingActivities (ActivityID) SELECT ActivityID FROM [bam_' + @ActivityName + '_Active]'

    IF (@DateThreshold is not NULL) OR (@ActivityId is not NULL)
    BEGIN
        SET @QueryString = @QueryString + ' WHERE'
    END

    IF (@DateThreshold is not NULL)
    BEGIN
        SET @QueryString = @QueryString + ' LastModified < N''' + CONVERT(nvarchar(50), @DateThreshold, 109) + ''''
        IF (@ActivityId is not NULL)
        BEGIN
            SET @QueryString = @QueryString + ' AND'
        END
    END

    IF (@ActivityId is not NULL)
    BEGIN
        SET @QueryString = @QueryString + ' ActivityID = N''' + @ActivityId + ''''
    END

    EXEC sp_executesql @QueryString
    SELECT * FROM #DanglingActivities

    SET @QueryString = N''

    -- If the user gave a table extension, the dangling instances will be inserted
    -- into that table.
    IF (isnull(@NewTableExtension, '') <> '')
    BEGIN
        SET @DanglingActiveTableName = @ActiveTableName + '_' + @NewTableExtension
        SET @DanglingActiveRelationshipsTableName = @ActiveRelationshipsTableName + '_' + @NewTableExtension
        SET @DanglingContinuationsTableName = @ContinuationsTableName + '_' + @NewTableExtension

        -- If the table for the dangling instances exists then insert into it
        -- If the table does not exist, then create the dangling instances table
        -- and then insert into it. SELECT INTO will do that.
        EXEC sp_tables @table_name = @DanglingActiveTableName
        IF @@ROWCOUNT > 0
        BEGIN
            SET @QueryString = N'INSERT INTO ' + '[' + @DanglingActiveTableName + '] SELECT active.* FROM [' + @ActiveTableName + '] active INNER JOIN #DanglingActivities dangling ON active.ActivityID = dangling.ActivityID'
            EXEC sp_executesql @QueryString
        END
        ELSE
        BEGIN
            SET @QueryString = N'SELECT active.* INTO [' + @DanglingActiveTableName + '] FROM [' + @ActiveTableName + '] active INNER JOIN #DanglingActivities dangling ON active.ActivityID = dangling.ActivityID'
            EXEC sp_executesql @QueryString
        END

        -- Now do what you did for the Active Instances table for the
        -- ActiveRelationships table
        EXEC sp_tables @table_name = @DanglingActiveRelationshipsTableName
        IF @@ROWCOUNT > 0
        BEGIN
            SET @QueryString = N'INSERT INTO ' + '[' + @DanglingActiveRelationshipsTableName + '] SELECT active.* FROM [' + @ActiveRelationshipsTableName + '] active INNER JOIN #DanglingActivities dangling ON active.ActivityID = dangling.ActivityID'
            EXEC sp_executesql @QueryString
        END
        ELSE
        BEGIN
            SET @QueryString = N'SELECT active.* INTO [' + @DanglingActiveRelationshipsTableName + '] FROM [' + @ActiveRelationshipsTableName + '] active INNER JOIN #DanglingActivities dangling ON active.ActivityID = dangling.ActivityID'
            EXEC sp_executesql @QueryString
        END

        -- And finally for the continuations table
        EXEC sp_tables @table_name = @DanglingContinuationsTableName
        IF @@ROWCOUNT > 0
        BEGIN
            SET @QueryString = N'INSERT INTO ' + '[' + @DanglingContinuationsTableName + '] SELECT active.* FROM [' + @ContinuationsTableName + '] active INNER JOIN #DanglingActivities dangling ON active.ParentActivityID = dangling.ActivityID'
            EXEC sp_executesql @QueryString
        END
        ELSE
        BEGIN
            SET @QueryString = N'SELECT active.* INTO [' + @DanglingContinuationsTableName + '] FROM [' + @ContinuationsTableName + '] active INNER JOIN #DanglingActivities dangling ON active.ParentActivityID = dangling.ActivityID'
            EXEC sp_executesql @QueryString
        END
    END

    -- Remove the dangling instances from the Active Instances Table
    SET @QueryString = 'DELETE FROM [' + @ActiveTableName + '] FROM [' + @ActiveTableName + '] active INNER JOIN #DanglingActivities dangling ON active.ActivityID = dangling.ActivityID '
    EXEC sp_executesql @QueryString

    SET @QueryString = 'DELETE FROM [' + @ActiveRelationshipsTableName + '] FROM [' + @ActiveRelationshipsTableName + '] active INNER JOIN #DanglingActivities dangling ON active.ActivityID = dangling.ActivityID '
    EXEC sp_executesql @QueryString

    SET @QueryString = 'DELETE FROM [' + @ContinuationsTableName + '] FROM [' + @ContinuationsTableName + '] active INNER JOIN #DanglingActivities dangling ON active.ParentActivityID = dangling.ActivityID '
    EXEC sp_executesql @QueryString

    DROP TABLE #DanglingActivities

    COMMIT TRAN
GO

/****** Grant IIS BizTalkActivityMonitoring AppPool identity read access to BamPrimaryImport Database ******/
EXEC dbo.sp_addrolemember @rolename=N'db_datareader', @membername=N'$(BizTalkApplicationUserGroup)'
GO