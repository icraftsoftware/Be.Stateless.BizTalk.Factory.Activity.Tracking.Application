﻿#region Copyright & License

// Copyright © 2012 - 2021 François Chabot
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Transactions;
using Be.Stateless.BizTalk.Component;
using Be.Stateless.BizTalk.Dsl.Binding;
using Be.Stateless.BizTalk.Dsl.Binding.Adapter;
using Be.Stateless.BizTalk.Dsl.Binding.Convention;
using Be.Stateless.BizTalk.Dsl.Binding.Convention.Simple;
using Be.Stateless.BizTalk.Factory;
using Be.Stateless.BizTalk.Install;
using Be.Stateless.BizTalk.MicroComponent;
using Be.Stateless.BizTalk.MicroPipelines;
using Microsoft.Adapters.Sql;
using Microsoft.BizTalk.Adapter.Wcf.Config;

namespace Be.Stateless.BizTalk
{
	public class ClaimReceiveLocation : ReceiveLocation<NamingConvention>
	{
		private static TimeSpan ClaimCheckOutPollingInterval => DeploymentContext.TargetEnvironment.IsDevelopmentOrBuild() ? TimeSpan.FromSeconds(5) : TimeSpan.FromMinutes(5);

		public ClaimReceiveLocation()
		{
			Name = ReceiveLocationName.About("CheckOut").FormattedAs.Xml;
			Enabled = true;
			ReceivePipeline = new ReceivePipeline<XmlReceive>(
				pipeline => {
					pipeline.Validator<MicroPipelineComponent>(
						pc => {
							pc.Components = new IMicroComponent[] {
								new ContextPropertyExtractor(),
								new ActivityTracker()
							};
						});
				});
			Transport.Adapter = new WcfSqlAdapter.Inbound(
				a => {
					a.Address = new() {
						InboundId = "AvailableTokens",
						InitialCatalog = "BizTalkFactoryTransientStateDb",
						Server = Platform.Settings.ProcessingDatabaseServer,
						InstanceName = Platform.Settings.ProcessingDatabaseInstance
					};
					a.PolledDataAvailableStatement = "SELECT COUNT(1) FROM vw_claim_AvailableTokens";
					a.PollingStatement = "EXEC usp_claim_CheckOut";
					a.PollingInterval = ClaimCheckOutPollingInterval;
					a.InboundOperationType = InboundOperation.XmlPolling;
					a.XmlStoredProcedureRootNodeName = "BodyWrapper";
					a.InboundBodyLocation = InboundMessageBodySelection.UseBodyPath;
					a.InboundBodyPathExpression = "/BodyWrapper/*";
					a.InboundNodeEncoding = MessageBodyFormat.Xml;
					a.ServiceBehaviors = new[] {
						new SqlAdapterInboundTransactionBehavior {
							TransactionIsolationLevel = IsolationLevel.ReadCommitted,
							TransactionTimeout = TimeSpan.FromMinutes(2)
						}
					};
				});
			Transport.Host = Platform.Settings.HostResolutionPolicy;
		}
	}
}
