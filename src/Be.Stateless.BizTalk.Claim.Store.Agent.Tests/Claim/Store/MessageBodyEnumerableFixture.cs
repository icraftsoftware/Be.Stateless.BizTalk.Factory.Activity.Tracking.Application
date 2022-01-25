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
using System.IO;
using System.Linq;
using Be.Stateless.BizTalk.Claim.Store.States;
using Be.Stateless.Linq.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Be.Stateless.BizTalk.Claim.Store
{
	public class MessageBodyEnumerableFixture
	{
		[Fact]
		public void CollectingEnumeratorCollectsMessageBodies()
		{
			var mocks = new[] {
				new Mock<MessageBody>(DataFile.Create("20130615DAB80EE5BAB34B0CA2358098CD6A0AFD.trk")),
				new Mock<MessageBody>(DataFile.Create("201306159B13FB4ED82E4A0D9D7CF3F4F43CE388.trk")),
				new Mock<MessageBody>(DataFile.Create("201306158F341A2D6FD7416B87073A0132DD51AE.trk"))
			};

			mocks.Select(m => m.Object).Collect(Path.GetTempPath());

			mocks.ForEach(m => m.Verify(c => c.Collect(Path.GetTempPath()), Times.Once()));
		}

		[Fact]
		public void EnumerateMessageBodiesFiltersOutInvalidDataFileNames()
		{
			DataFile._fileCreationTimeGetter = _ => DateTime.UtcNow;
			var lockTimestamp = DateTime.UtcNow.AddHours(-1).ToString(DataFileNameTokenizer.LOCK_TIMESTAMP_FORMAT);
			var files = new[] {
				"20130615DAB80EE5BAB34B0CA2358098CD6A0AFD.trk",
				"20130615DAB80EE5BAB34B0CA2358798CD6A0AFD.trk." + lockTimestamp + "." + LockedDataFile.STATE_TOKEN,
				"201306159B13FB4ED82E4A0D9D7CF3F4F43CE388.trk." + lockTimestamp + "." + GatheredDataFile.STATE_TOKEN,
				"201306158F341A2D6FD7416B87073A0132DD51AE.chk." + lockTimestamp + "." + ReleasedDataFile.STATE_TOKEN,
				"invalid.file",
				"some.other.invalid.file"
			};
			MessageBodyEnumerable._fileEnumerator = (_, _) => files;

			var entries = new[] { Path.GetTempPath() }.EnumerateMessageBodies(TimeSpan.FromMinutes(30));

			entries.Select(e => e.DataFile.ToString()).Should().BeEquivalentTo(files.Take(4));
		}

		[Fact]
		public void EnumerateMessageBodiesFiltersOutLockedMessageBodies()
		{
			DataFile._fileCreationTimeGetter = _ => DateTime.UtcNow;
			var expiredLockTimestamp = DateTime.UtcNow.AddHours(-1).ToString(DataFileNameTokenizer.LOCK_TIMESTAMP_FORMAT);
			var pendingLockTimestamp = DateTime.UtcNow.AddMinutes(-10).ToString(DataFileNameTokenizer.LOCK_TIMESTAMP_FORMAT);
			var files = new[] {
				"20130615DAB80EE5BAB34B0CA2358098CD6A0AFD.trk",
				"20130615DAB80EE5BAB34B0CA2358798CD6A0AFD.trk." + expiredLockTimestamp + "." + LockedDataFile.STATE_TOKEN,
				"201306159B13FB4ED82E4A0D9D7CF3F4F43CE388.trk." + expiredLockTimestamp + "." + GatheredDataFile.STATE_TOKEN,
				"201306158F341A2D6FD7416B87073A0132DD51AE.chk." + pendingLockTimestamp + "." + ReleasedDataFile.STATE_TOKEN
			};
			MessageBodyEnumerable._fileEnumerator = (_, _) => files;

			var entries = new[] { Path.GetTempPath() }.EnumerateMessageBodies(TimeSpan.FromMinutes(30));

			entries.Select(e => e.DataFile.ToString()).Should().BeEquivalentTo(files.Take(3));
		}
	}
}
