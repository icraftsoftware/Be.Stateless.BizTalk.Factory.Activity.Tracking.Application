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

using System.Transactions;
using Be.Stateless.BizTalk.Activity.Tracking.Environment.Settings;
using Be.Stateless.BizTalk.Component;
using Be.Stateless.BizTalk.ContextBuilders.Send.Claim;
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.BizTalk.Dsl;
using Be.Stateless.BizTalk.Dsl.Binding;
using Be.Stateless.BizTalk.Dsl.Binding.Adapter;
using Be.Stateless.BizTalk.Dsl.Binding.Convention;
using Be.Stateless.BizTalk.Dsl.Binding.Convention.Simple;
using Be.Stateless.BizTalk.Dsl.Binding.Subscription;
using Be.Stateless.BizTalk.Maps.ToSql.Procedures.Claim;
using Be.Stateless.BizTalk.MicroComponent;
using Be.Stateless.BizTalk.MicroPipelines;
using Be.Stateless.BizTalk.Schemas.Xml;
using Microsoft.Adapters.Sql;
using RetryPolicy = Be.Stateless.BizTalk.Dsl.Binding.Convention.RetryPolicy;

namespace Be.Stateless.BizTalk.Activity.Tracking
{
	public class ClaimCheckInSendPort : SendPort<NamingConvention>
	{
		public ClaimCheckInSendPort()
		{
			Name = SendPortName.Towards("Claim").About("CheckIn").FormattedAs.Xml;
			State = ServiceState.Started;
			SendPipeline = new SendPipeline<XmlTransmit>(
				pipeline => {
					pipeline.Encoder<MicroPipelineComponent>(
						pc => {
							pc.Components = new IMicroComponent[] {
								new ContextBuilder { BuilderType = typeof(ProcessResolver) },
								new ActivityTracker(),
								new XsltRunner { MapType = typeof(ClaimToCheckIn) }
							};
						});
				});
			Transport.Adapter = new WcfSqlAdapter.Outbound(
				a => {
					a.Address = new SqlAdapterConnectionUri {
						InitialCatalog = "BizTalkFactoryTransientStateDb",
						Server = Platform.Settings.ProcessingDatabaseServer,
						InstanceName = Platform.Settings.ProcessingDatabaseInstance
					};
					a.IsolationLevel = IsolationLevel.ReadCommitted;
					a.StaticAction = "TypedProcedure/dbo/usp_claim_CheckIn";
				});
			Transport.Host = Platform.Settings.TransmittingHost;
			Transport.RetryPolicy = RetryPolicy.ShortRunning;
			Filter = new Filter(() => BtsProperties.MessageType == Schema<Claim.CheckIn>.MessageType);
		}
	}
}
