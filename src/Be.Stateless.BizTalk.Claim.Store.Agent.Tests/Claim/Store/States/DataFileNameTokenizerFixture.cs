#region Copyright & License

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
using FluentAssertions;
using Xunit;
using static FluentAssertions.FluentActions;

namespace Be.Stateless.BizTalk.Claim.Store.States
{
	public class DataFileNameTokenizerFixture
	{
		[Fact]
		public void CaptureDate()
		{
			"201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.gathered".Tokenize().CaptureDate.Should().Be("20130615");
		}

		[Fact]
		public void Id()
		{
			"201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.gathered".Tokenize().Id.Should().Be("8F341A2D6FD7416B87073A0132DD51AE");
		}

		[Theory]
		[InlineData(@"201306158F341A2D6FD7416B87073A0132DD51AE.TRK")]
		[InlineData(@"201306158F341A2D6FD7416B87073A0132DD51AE.txt")]
		[InlineData(@"201306158F341A2D6FD7416B87073A0132DD51AE.CHK.20150626160941.GATHERED")]
		[InlineData(@"201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.unlocked")]
		[InlineData(@"c:\temp\201306158F341A2D6FD7416B87073A0132DD51AE.chk.gathered")]
		[InlineData(@"201306158F341A2D6FD7416B87073A0132DD51AE.txt.20150626160941.released")]
		public void IsNotValidDataFilePath(string filePath)
		{
			filePath.IsValidDataFilePath().Should().BeFalse();
		}

		[Theory]
		[InlineData(@"201306158F341A2D6FD7416B87073A0132DD51AE.trk")]
		[InlineData(@"c:\temp\201306158F341A2D6FD7416B87073A0132DD51AE.trk")]
		[InlineData(@"201306158F341A2D6FD7416B87073A0132DD51AE.chk")]
		[InlineData(@"c:\temp\201306158F341A2D6FD7416B87073A0132DD51AE.chk")]
		[InlineData(@"201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.gathered")]
		[InlineData(@"c:\temp\201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.gathered")]
		[InlineData(@"201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.released")]
		[InlineData(@"c:\temp\201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.released")]
		[InlineData(@"201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.locked")]
		[InlineData(@"c:\temp\201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.locked")]
		public void IsValidDataFilePath(string filePath)
		{
			filePath.IsValidDataFilePath().Should().BeTrue();
		}

		[Fact]
		public void LockTime()
		{
			"201306158F341A2D6FD7416B87073A0132DD51AE.chk".Tokenize().LockTime.Should().Be(null);

			"201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.gathered".Tokenize().LockTime
				.Should().Be(new DateTime(2015, 6, 26, 16, 09, 41, DateTimeKind.Utc).ToUniversalTime());
		}

		[Fact]
		public void State()
		{
			"201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.gathered".Tokenize().State.Should().Be(GatheredDataFile.STATE_TOKEN);
		}

		[Fact]
		public void TokenizeThrowsWhenDataFileNameIsInvalid()
		{
			Invoking(() => "201306158F341A2D6FD7416B87073A0132DD51AE.txt".Tokenize().CaptureDate)
				.Should().Throw<ArgumentException>()
				.WithMessage("Claim Store Agent does not recognize the message body's data file path: '201306158F341A2D6FD7416B87073A0132DD51AE.txt'.*");
		}

		[Fact]
		public void TrackingMode()
		{
			"201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.gathered".Tokenize().TrackingMode.Should().Be("chk");
		}

		[Fact]
		public void UnlockedFilePath()
		{
			"201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.gathered".Tokenize().UnlockedFilePath.Should().Be("201306158F341A2D6FD7416B87073A0132DD51AE.chk");
		}
	}
}
