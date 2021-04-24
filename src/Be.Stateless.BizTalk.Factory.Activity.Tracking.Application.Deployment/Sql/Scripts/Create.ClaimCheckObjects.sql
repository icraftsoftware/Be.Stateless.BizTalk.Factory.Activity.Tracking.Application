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

USE [BizTalkFactoryTransientStateDb]
GO

IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_claim_Tokens_Available]') AND parent_object_id = OBJECT_ID(N'[dbo].[claim_Tokens]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_claim_Tokens_Available]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[claim_Tokens] DROP CONSTRAINT [DF_claim_Tokens_Available]
END


End
GO
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_claim_Tokens_Timestamp]') AND parent_object_id = OBJECT_ID(N'[dbo].[claim_Tokens]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_claim_Tokens_Timestamp]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[claim_Tokens] DROP CONSTRAINT [DF_claim_Tokens_Timestamp]
END


End
GO
/****** Object:  Table [dbo].[claim_Tokens] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[claim_Tokens]') AND type in (N'U'))
DROP TABLE [dbo].[claim_Tokens]
GO
/****** Object:  Table [dbo].[claim_Tokens] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[claim_Tokens]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[claim_Tokens](
   [Url] [nvarchar](50) NOT NULL,
   [Available] [bit] NOT NULL,
   [CorrelationToken] [nvarchar](256) NULL,
   [EnvironmentTag] [nvarchar](256) NULL,
   [MessageType] [nvarchar](256) NULL,
   [OutboundTransportLocation] [nvarchar](256) NULL,
   [ProcessActivityId] [nvarchar](32) NULL,
   [ReceiverName] [nvarchar](256) NULL,
   [SenderName] [nvarchar](256) NULL,
   [Any] [xml] NULL,
   [Timestamp] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_claim_Tokens] PRIMARY KEY CLUSTERED
(
   [Url] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Default [DF_claim_Tokens_Available] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_claim_Tokens_Available]') AND parent_object_id = OBJECT_ID(N'[dbo].[claim_Tokens]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_claim_Tokens_Available]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[claim_Tokens] ADD  CONSTRAINT [DF_claim_Tokens_Available]  DEFAULT ((0)) FOR [Available]
END


End
GO
/****** Object:  Default [DF_claim_Tokens_Timestamp] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_claim_Tokens_Timestamp]') AND parent_object_id = OBJECT_ID(N'[dbo].[claim_Tokens]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_claim_Tokens_Timestamp]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[claim_Tokens] ADD  CONSTRAINT [DF_claim_Tokens_Timestamp]  DEFAULT (sysutcdatetime()) FOR [Timestamp]
END


End
GO

/****** Object:  View [dbo].[vw_claim_AvailableTokens] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_claim_AvailableTokens]'))
DROP VIEW [dbo].[vw_claim_AvailableTokens]
GO

/****** Object:  View [dbo].[vw_claim_AvailableTokens] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_claim_AvailableTokens]
AS
   SELECT Url
   -- important to skip past the locked rows instead of blocking current transaction until locks are released
   FROM claim_Tokens WITH (READPAST)
   WHERE Available = 1
GO

/****** Object:  StoredProcedure [dbo].[usp_claim_CheckIn] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_claim_CheckIn]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_claim_CheckIn]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =================================================================================================
-- Author:      François Chabot
-- Create date: 12/04/2017
-- Description: Check in a new claim token.
-- =================================================================================================
CREATE PROCEDURE [dbo].[usp_claim_CheckIn]
   @correlationToken nvarchar(256) = NULL,
   @environmentTag nvarchar(256) = NULL,
   @messageType nvarchar(256) = NULL,
   @outboundTransportLocation nvarchar(256) = NULL,
   @processActivityId nvarchar(32) = NULL,
   @receiverName nvarchar(256) = NULL,
   @senderName nvarchar(256) = NULL,
   @url nvarchar(50),
   @any [xml] = NULL
AS
BEGIN
   -- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
   SET NOCOUNT ON;

   INSERT INTO claim_Tokens (Url, CorrelationToken, EnvironmentTag, MessageType, OutboundTransportLocation, ProcessActivityId, ReceiverName, SenderName, [Any])
      VALUES (@url, @correlationToken, @environmentTag, @messageType, @outboundTransportLocation, @processActivityId, @receiverName, @senderName, @any);
END
GO
GRANT EXECUTE ON [dbo].[usp_claim_CheckIn] TO [BTS_USERS] AS [dbo]
GO

/****** Object:  StoredProcedure [dbo].[usp_claim_Release] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_claim_Release]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_claim_Release]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =================================================================================================
-- Author:      François Chabot
-- Create date: 22/04/2013
-- Description: Makes a claim token available for check out.
-- =================================================================================================
CREATE PROCEDURE [dbo].[usp_claim_Release]
   @url nvarchar(50)
AS
BEGIN
   -- SET NOCOUNT OFF so that SQL Server returns the number of affected rows to .NET
   SET NOCOUNT OFF;

   -- set XLOCK on row to prevent concurrent execution
   UPDATE claim_Tokens WITH (ROWLOCK, XLOCK)
      SET Available = 1
      WHERE Url = @url;
END
GO
GRANT EXECUTE ON [dbo].[usp_claim_Release] TO [BTS_USERS] AS [dbo]
GO

/****** Object:  StoredProcedure [dbo].[usp_claim_CheckOut] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_claim_CheckOut]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_claim_CheckOut]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =================================================================================================
-- Author:      François Chabot
-- Create date: 12/04/2017
-- Description: Check out available claim tokens.
--              The output, which has to match the Be.Stateless.BizTalk.Schemas.Xml.Claim.Tokens
--              schema, will be as follows:
--              <env:Envelope xmlns:env="urn:schemas.stateless.be:biztalk:envelope:2013:07" xmlns:clm="urn:schemas.stateless.be:biztalk:claim:2017:04">
--                <clm:CheckOut>
--                  <clm:CorrelationToken>...</clm:CorrelationToken>
--                  <clm:EnvironmentTag>...</clm:EnvironmentTag>
--                  <clm:MessageType>...</clm:MessageType>
--                  <clm:OutboundTransportLocation>...</clm:OutboundTransportLocation>
--                  <clm:ProcessActivityId>...</clm:ProcessActivityId>
--                  <clm:ReceiverName>...</clm:ReceiverName>
--                  <clm:SenderName>...</clm:SenderName>
--                  <clm:Url>...</clm:Url>
--                  <Any>...</Any>
--                </clm:CheckOut>
--                <clm:CheckOut>
--                  <clm:CorrelationToken>...</clm:CorrelationToken>
--                  <clm:ReceiverName>...</clm:ReceiverName>
--                  <clm:Url>...</clm:Url>
--                  <Any>...</Any>
--                </clm:CheckOut>
--                <clm:CheckOut>
--                  <clm:Url>...</clm:Url>
--                </clm:CheckOut>
--              </env:Envelope>
-- =================================================================================================
CREATE PROCEDURE [dbo].[usp_claim_CheckOut]
AS
BEGIN
   -- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
   SET NOCOUNT ON;

   DECLARE @AvailableTokens TABLE (Url nvarchar(50) NOT NULL);
   INSERT INTO @AvailableTokens (Url)
      SELECT CT.Url
      -- set XLOCK on row to prevent concurrent execution
      FROM claim_Tokens CT WITH (ROWLOCK, XLOCK)
         INNER JOIN vw_claim_AvailableTokens AT ON CT.Url = AT.Url;

   WITH XMLNAMESPACES (
      'urn:schemas.stateless.be:biztalk:envelope:2013:07' AS env,
      'urn:schemas.stateless.be:biztalk:claim:2017:04' AS clm
   )
   SELECT CT.[CorrelationToken] AS 'clm:CorrelationToken',
      CT.[EnvironmentTag] AS 'clm:EnvironmentTag',
      CT.[MessageType] AS 'clm:MessageType',
      CT.[OutboundTransportLocation] AS 'clm:OutboundTransportLocation',
      CT.[ProcessActivityId] AS 'clm:ProcessActivityId',
      CT.[ReceiverName] AS 'clm:ReceiverName',
      CT.[SenderName] AS 'clm:SenderName',
      CT.[Url] AS 'clm:Url',
      CONVERT(XML, CT.[Any])
   FROM claim_Tokens CT
      INNER JOIN @AvailableTokens AT ON CT.Url = AT.Url
   FOR XML PATH ('clm:CheckOut'), ROOT('env:Envelope');

   DELETE CT FROM claim_Tokens CT WITH (TABLOCK, XLOCK)
      INNER JOIN @AvailableTokens AT ON CT.Url = AT.Url;
END
GO
GRANT EXECUTE ON [dbo].[usp_claim_CheckOut] TO [BTS_USERS] AS [dbo]
GO
