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

/****** Object:  StoredProcedure [dbo].[usp_claim_CheckOut] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_claim_CheckOut]') AND type in (N'P', N'PC'))
   DROP PROCEDURE [dbo].[usp_claim_CheckOut]
GO

/****** Object:  StoredProcedure [dbo].[usp_claim_Release] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_claim_Release]') AND type in (N'P', N'PC'))
   DROP PROCEDURE [dbo].[usp_claim_Release]
GO

/****** Object:  StoredProcedure [dbo].[usp_claim_CheckIn] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_claim_CheckIn]') AND type in (N'P', N'PC'))
   DROP PROCEDURE [dbo].[usp_claim_CheckIn]
GO

/****** Object:  View [dbo].[vw_claim_AvailableTokens] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_claim_AvailableTokens]'))
   DROP VIEW [dbo].[vw_claim_AvailableTokens]
GO

/****** Object:  Table [dbo].[claim_Tokens] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[claim_Tokens]') AND type in (N'U'))
   DROP TABLE [dbo].[claim_Tokens]
GO
