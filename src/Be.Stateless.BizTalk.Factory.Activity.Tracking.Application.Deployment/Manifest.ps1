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

#Requires -Modules @{ ModuleName = 'BizTalk.Deployment'; ModuleVersion = '1.0.21350.31793'; GUID = '533b5f59-49ce-4f51-a293-cb78f5cf81b5' }

[CmdletBinding()]
[OutputType([HashTable])]
param(
   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [int]
   $BamArchiveWindowTimeLength = 30,

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [int]
   $BamOnlineWindowTimeLength = 15,

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string[]]
   $BizTalkHostUserGroups = @(Get-BizTalkHost | ForEach-Object NTGroupName | Select-Object -Unique | ConvertTo-SqlLogin),

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $BizTalkServerOperatorEmail = 'biztalk.factory@stateless.be',

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $ClaimStoreCheckOutDirectory = 'C:\Files\Drops\BizTalk.Factory\CheckOut',

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $EnvironmentSettingOverridesTypeName,

   [Parameter(Mandatory = $false)]
   [ValidateScript( { ($_ | Test-None) -or ($_ | Test-Path -PathType Container) } )]
   [string[]]
   $AssemblyProbingFolderPaths = @(),

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $ManagementServer = (Get-BizTalkGroupSettings).MgmtDbServerName,

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $MonitoringServer = (Get-BizTalkGroupSettings).BamDBServerName,

   [Parameter(Mandatory = $false)]
   [ValidateNotNullOrEmpty()]
   [string]
   $ProcessingServer = (Get-BizTalkGroupSettings).SubscriptionDBServerName
)

Set-StrictMode -Version Latest

ApplicationManifest -Name BizTalk.Factory.Activity.Tracking -Description 'BizTalk.Factory''s activity tracking application add-on for general purpose BizTalk Server development.' -Reference BizTalk.Factory -Build {
   Assembly -Path (Get-ResourceItem -Name Be.Stateless.BizTalk.Activity.Tracking)
   BamActivityModel -Path (Get-ResourceItem -Name ActivityModel -Extension .xml)
   BamIndex -Activity Process -Name BeginTime, InterchangeID, ProcessName, Value1, Value2, Value3
   BamIndex -Activity ProcessMessagingStep -Name MessagingStepActivityID, ProcessActivityID
   BamIndex -Activity MessagingStep -Name InterchangeID, Time, Value1, Value2, Value3
   Binding -Path (Get-ResourceItem -Name Be.Stateless.BizTalk.Factory.Activity.Tracking.Binding) `
      -EnvironmentSettingOverridesTypeName $EnvironmentSettingOverridesTypeName `
      -AssemblyProbingFolderPaths $AssemblyProbingFolderPaths
   Map -Path (Get-ResourceItem -Name Be.Stateless.BizTalk.Claim.Check.Maps)
   ProcessDescriptor -Path (Get-ResourceItem -Name Be.Stateless.BizTalk.Activity.Tracking)
   Schema -Path (Get-ResourceItem -Name Be.Stateless.BizTalk.Claim.Check.Schemas)
   SqlDeploymentScript -Path (Get-ResourceItem -Extension .sql -Name TurnOffGlobalTracking, Create.Monitoring.Objects) -Server $ManagementServer
   SqlUndeploymentScript -Path (Get-ResourceItem -Extension .sql -Name Drop.Monitoring.Objects) -Server $ManagementServer
   SqlDeploymentScript -Path (Get-ResourceItem -Extension .sql -Name Create.ClaimCheck.Objects) -Server $ProcessingServer
   SqlUndeploymentScript -Path (Get-ResourceItem -Extension .sql -Name Drop.ClaimCheck.Objects) -Server $ProcessingServer
   SqlDeploymentScript -Path (Get-ResourceItem -Extension .sql -Name Create.BAMPrimaryImport.Objects) -Server $MonitoringServer -Variables @{
      BamOnlineWindowTimeLength = $BamOnlineWindowTimeLength
      BizTalkHostUserGroups     = $BizTalkHostUserGroups -join ';'
   }
   SqlUndeploymentScript -Path (Get-ResourceItem -Extension .sql -Name Drop.BAMPrimaryImport.Objects) -Server $MonitoringServer -Variables @{
      BizTalkHostUserGroups = $BizTalkHostUserGroups -join ';'
   }
   SqlDeploymentScript -Path (Get-ResourceItem -Extension .sql -Name Create.BizTalkServerOperator) -Server $ManagementServer -Variables @{
      BizTalkServerOperatorEmail = $BizTalkServerOperatorEmail
   }
   SqlUndeploymentScript -Path (Get-ResourceItem -Extension .sql -Name Drop.BizTalkServerOperator) -Server $ManagementServer
   SqlDeploymentScript -Path (Get-ResourceItem -Extension .sql -Name Create.BamTrackingActivitiesMaintenanceJob) -Server $MonitoringServer -Variables @{
      BamArchiveWindowTimeLength  = $BamArchiveWindowTimeLength
      ClaimStoreCheckOutDirectory = $ClaimStoreCheckOutDirectory
      MonitoringDatabaseServer    = $MonitoringServer
   }
   SqlUndeploymentScript -Path (Get-ResourceItem -Extension .sql -Name Drop.BamTrackingActivitiesMaintenanceJob) -Server $MonitoringServer
   SsoConfigStore -Path (Get-ResourceItem -Name Be.Stateless.BizTalk.Factory.Activity.Tracking.Binding) `
      -EnvironmentSettingOverridesTypeName $EnvironmentSettingOverridesTypeName `
      -AssemblyProbingFolderPaths $AssemblyProbingFolderPaths
}
