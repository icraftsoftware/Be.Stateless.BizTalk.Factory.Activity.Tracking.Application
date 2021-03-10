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
	public class DataFileFixture
	{
		[Fact]
		public void CreateGatheredDataFile()
		{
			DataFile.Create("201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.gathered").Should().BeOfType<GatheredDataFile>();
		}

		[Fact]
		public void CreateLockedDataFile()
		{
			DataFile.Create("201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.locked").Should().BeOfType<LockedDataFile>();
		}

		[Fact]
		public void CreateReleasedDataFile()
		{
			DataFile.Create("201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150626160941.released").Should().BeOfType<ReleasedDataFile>();
		}

		[Fact]
		public void CreateThrowsWhenInvalidMessageBodyDataFileName()
		{
			Invoking(() => DataFile.Create("201306158F341A2D6FD7416B87073A0132DD51AE.thk")).Should().Throw<ArgumentException>();
		}

		[Fact]
		public void CreateUnlockedDataFile()
		{
			DataFile.Create("201306158F341A2D6FD7416B87073A0132DD51AE.chk").Should().BeOfType<UnlockedDataFile>();
		}
	}
}
