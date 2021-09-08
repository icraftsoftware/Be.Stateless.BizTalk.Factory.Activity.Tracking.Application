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

using System.ServiceProcess;

namespace Be.Stateless.BizTalk.Claim.Store.Agent
{
	internal partial class ServiceController : ServiceBase
	{
		public ServiceController()
		{
			InitializeComponent();
			_collector = new();
		}

		#region Base Class Member Overrides

		protected override void OnContinue()
		{
			_collector.Start();
		}

		protected override void OnPause()
		{
			_collector.Stop();
		}

		protected override void OnStart(string[] args)
		{
			_collector.Start();
		}

		protected override void OnStop()
		{
			_collector.Stop();
		}

		#endregion

		private readonly MessageBodyCollector _collector;
	}
}
