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
using Be.Stateless.Extensions;
using log4net;

namespace Be.Stateless.BizTalk.Claim.Store.States
{
	internal class UnlockedDataFile : DataFile
	{
		internal UnlockedDataFile(string filePath) : base(filePath)
		{
			var state = Path.Tokenize().State;
			if (!state.IsNullOrEmpty()) throw new InvalidOperationException($"{GetType().Name} cannot handle a data file in the '{state}' state.");
		}

		#region Base Class Member Overrides

		internal override void Gather(MessageBody messageBody, string gatheringDirectory)
		{
			throw new InvalidOperationException();
		}

		internal override void Lock(MessageBody messageBody)
		{
			if (_logger.IsDebugEnabled) _logger.Debug($"Locking {this}.");

			// try to add a .timestamp.locked extension to file name to get exclusive ownership should there be another
			// Claim Store Agent working concurrently from another computer
			var lockedDataFile = new LockedDataFile(this);
			var result = DataFileServant.Instance.TryMoveFile(Path, lockedDataFile.Path);
			messageBody.DataFile = result
				? lockedDataFile
				: new AwaitingRetryDataFile(this);
		}

		internal override void Release(MessageBody messageBody)
		{
			throw new InvalidOperationException();
		}

		internal override void Unlock(MessageBody messageBody)
		{
			throw new InvalidOperationException();
		}

		#endregion

		private static readonly ILog _logger = LogManager.GetLogger(typeof(UnlockedDataFile));
	}
}
