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

/****** Object:  StoredProcedure [dbo].[RemoveDanglingInstances] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RemoveDanglingInstances]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RemoveDanglingInstances]
GO

/****** Object:  View [dbo].[vw_MessagingStepContexts] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_MessagingStepContexts]'))
   DROP VIEW [dbo].[vw_MessagingStepContexts]
GO

/****** Object:  View [dbo].[vw_MessagingStepMessages] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_MessagingStepMessages]'))
   DROP VIEW [dbo].[vw_MessagingStepMessages]
GO

/****** Object:  View [dbo].[vw_LastWeekFailedMessagingSteps] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_LastWeekFailedMessagingSteps]'))
   DROP VIEW [dbo].[vw_LastWeekFailedMessagingSteps]
GO

/****** Object:  View [dbo].[vw_LastWeekFailedProcesses] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_LastWeekFailedProcesses]'))
   DROP VIEW [dbo].[vw_LastWeekFailedProcesses]
GO

/****** Object:  View [dbo].[vw_LastWeekSuccessfulMessagingSteps] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_LastWeekSuccessfulMessagingSteps]'))
   DROP VIEW [dbo].[vw_LastWeekSuccessfulMessagingSteps]
GO

/****** Object:  View [dbo].[vw_LastWeekSuccessfulProcesses] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_LastWeekSuccessfulProcesses]'))
   DROP VIEW [dbo].[vw_LastWeekSuccessfulProcesses]
GO

/****** Revoke BizTalk Application User Groups read access to BamPrimaryImport Database ******/
DECLARE @rowNumber INT = 0
DECLARE @group nvarchar(128)
WHILE (1 = 1)
BEGIN
   WITH Groups AS (
      SELECT ROW_NUMBER() OVER(ORDER BY value ASC) AS RowNumber, value AS [Group]
      FROM STRING_SPLIT(N'$(BizTalkHostUserGroups)', ';')
   )
   SELECT TOP 1 @rowNumber = RowNumber, @group = TRIM([Group])
   FROM Groups
   WHERE RowNumber > @rowNumber

   IF @@ROWCOUNT = 0 BREAK;

   EXEC dbo.sp_droprolemember @rolename=N'db_datareader', @membername=@group
END
GO
