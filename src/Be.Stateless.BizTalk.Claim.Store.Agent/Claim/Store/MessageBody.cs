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

using System;
using System.Diagnostics.CodeAnalysis;
using Be.Stateless.BizTalk.Claim.Store.States;

namespace Be.Stateless.BizTalk.Claim.Store
{
	internal abstract class MessageBody
	{
		[SuppressMessage("ReSharper", "LocalizableElement")]
		internal static MessageBody Create(DataFile dataFile)
		{
			if (dataFile == null) throw new ArgumentNullException(nameof(dataFile));

			var trackingMode = dataFile.TrackingMode;
			if (trackingMode.Equals("chk", StringComparison.OrdinalIgnoreCase))
			{
				return new ClaimedMessageBody(dataFile);
			}
			if (trackingMode.Equals("trk", StringComparison.OrdinalIgnoreCase))
			{
				return new TrackedMessageBody(dataFile);
			}
			throw new ArgumentException($"Claim Store Agent does not support the tracking mode '{trackingMode}' of message body data file name '{dataFile}'.", nameof(dataFile));
		}

		protected internal MessageBody(DataFile dataFile)
		{
			DataFile = dataFile;
		}

		internal DataFile DataFile { get; set; }

		internal abstract void Collect(string gatheringDirectory);
	}
}
