﻿#region Copyright & License

// Copyright © 2012 - 2020 François Chabot
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

using Be.Stateless.BizTalk.Claim.Store.States;
using log4net;

namespace Be.Stateless.BizTalk.Claim.Store
{
	internal class ClaimedMessageBody : MessageBody
	{
		internal ClaimedMessageBody(DataFile dataFile) : base(dataFile) { }

		#region Base Class Member Overrides

		internal override void Collect(string gatheringDirectory)
		{
			if (_logger.IsDebugEnabled) _logger.Debug($"Collecting claimed message body file {DataFile}.");
			DataFile.Lock(this);
			DataFile.Gather(this, gatheringDirectory);
			DataFile.Release(this);
			DataFile.Unlock(this);
		}

		#endregion

		private static readonly ILog _logger = LogManager.GetLogger(typeof(ClaimedMessageBody));
	}
}
