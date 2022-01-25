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

using Be.Stateless.BizTalk.Activity.Tracking.Factory;
using Be.Stateless.BizTalk.Dsl.Environment.Settings;
using Be.Stateless.BizTalk.Dsl.Environment.Settings.Convention;

namespace Be.Stateless.BizTalk.Factory.Activity.Tracking
{
	public class Application : CompositeEnvironmentSettings<Application, IClaimStoreSsoSettings>, IClaimStoreSsoSettings, IEnvironmentSettings
	{
		#region IClaimStoreSsoSettings Members

		[SsoSetting]
		public string ClaimStoreCheckInDirectory => GetOverriddenOrDefaultValue(@"C:\Files\Drops\BizTalk.Factory\CheckIn");

		[SsoSetting]
		public string ClaimStoreCheckOutDirectory => GetOverriddenOrDefaultValue(@"C:\Files\Drops\BizTalk.Factory\CheckOut");

		#endregion

		#region IEnvironmentSettings Members

		public string ApplicationName => ClaimStoreSsoSettings.AFFILIATE_APPLICATION_NAME;

		#endregion
	}
}
