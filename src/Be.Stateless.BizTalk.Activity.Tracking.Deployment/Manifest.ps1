#region Copyright & License

# Copyright © 2012 - 2021 François Chabot
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.u
# See the License for the specific language governing permissions and
# limitations under the License.

#endregion

[CmdletBinding()]
[OutputType([hashtable])]
param(
   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $BizTalkApplicationUserGroup = 'BizTalk Application Users',

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $Domain = $env:COMPUTERNAME,

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $ManagementServer = $env:COMPUTERNAME,

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $ProcessingServer = $env:COMPUTERNAME,

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $MonitoringServer = $env:COMPUTERNAME,

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $BizTalkServerOperatorEmail = "biztalk.factory@stateless.be",

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $ClaimStoreCheckOutDirectory = "C:\Files\Drops\BizTalk.Factory\CheckOut",

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [int]
   $BamArchiveWindowTimeLength = 30,

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [int]
   $BamOnlineWindowTimeLength = 15
)

Set-StrictMode -Version Latest

ApplicationManifest -Name BizTalk.Activity.Tracking -Description 'BizTalk.Factory''s activity model and tracking API for general purpose BizTalk Server development.' -Build {
   Assembly -Path (Get-ResourceItem -Name Be.Stateless.BizTalk.Activity.Tracking)
   BamActivityModel -Path (Get-ResourceItem -Name ActivityModel -Extensions .xml)
   BamIndex -Activity Process -Name BeginTime, InterchangeID, ProcessName, Value1, Value2, Value3
   BamIndex -Activity ProcessMessagingStep -Name MessagingStepActivityID, ProcessActivityID
   BamIndex -Activity MessagingStep -Name InterchangeID, Time, Value1, Value2, Value3
   Map -Path (Get-ResourceItem -Name Be.Stateless.BizTalk.Claim.Check.Maps)
   Schema -Path (Get-ResourceItem -Name Be.Stateless.BizTalk.Claim.Check.Schemas)
   SqlDeploymentScript -Path (Get-ResourceItem -Extensions .sql -Name TurnOffGlobalTracking, CreateMonitoringObjects) -Server $ManagementServer
   SqlUndeploymentScript -Path (Get-ResourceItem -Extensions .sql -Name DropMonitoringObjects) -Server $ManagementServer
   SqlDeploymentScript -Path (Get-ResourceItem -Extensions .sql -Name CreateClaimCheckObjects) -Server $ProcessingServer
   SqlUndeploymentScript -Path (Get-ResourceItem -Extensions .sql -Name DropClaimCheckObjects) -Server $ProcessingServer
   SqlDeploymentScript -Path (Get-ResourceItem -Extensions .sql -Name CreateBAMPrimaryImportObjects) -Server $MonitoringServer -Variables @{
      BizTalkApplicationUserGroup = "$Domain\$BizTalkApplicationUserGroup"
   }
   SqlUndeploymentScript -Path (Get-ResourceItem -Extensions .sql -Name DropBAMPrimaryImportObjects) -Server $MonitoringServer -Variables @{
      BizTalkApplicationUserGroup = "$Domain\$BizTalkApplicationUserGroup"
   }
   SqlDeploymentScript -Path (Get-ResourceItem -Extensions .sql -Name CreateBizTalkServerOperator) -Server $ManagementServer -Variables @{
      BizTalkServerOperatorEmail = $BizTalkServerOperatorEmail
   }
   SqlUndeploymentScript -Path (Get-ResourceItem -Extensions .sql -Name DropBizTalkServerOperator) -Server $ManagementServer
   SqlDeploymentScript -Path (Get-ResourceItem -Extensions .sql -Name CreateBamTrackingActivitiesMaintenanceJob) -Server $MonitoringServer -Variables @{
      BamArchiveWindowTimeLength  = $BamArchiveWindowTimeLength
      BamOnlineWindowTimeLength   = $BamOnlineWindowTimeLength
      ClaimStoreCheckOutDirectory = $ClaimStoreCheckOutDirectory
      MonitoringDatabaseServer    = $MonitoringServer
   }
   SqlUndeploymentScript -Path (Get-ResourceItem -Extensions .sql -Name DropBamTrackingActivitiesMaintenanceJob) -Server $MonitoringServer
}
