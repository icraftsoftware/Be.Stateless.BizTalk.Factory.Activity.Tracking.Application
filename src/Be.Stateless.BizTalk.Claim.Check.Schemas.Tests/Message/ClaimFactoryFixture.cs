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

using FluentAssertions;
using Xunit;
using static FluentAssertions.FluentActions;

namespace Be.Stateless.BizTalk.Message
{
	public class ClaimFactoryFixture
	{
		[Fact]
		public void CreateCheck()
		{
			Invoking(() => ClaimFactory.CreateCheck("file://server/folder/file.bin")).Should().NotThrow();
		}

		[Fact]
		public void CreateCheckIn()
		{
			Invoking(() => ClaimFactory.CreateCheckIn("file://server/folder/file.bin")).Should().NotThrow();
		}

		[Fact]
		public void CreateCheckInWithMessageType()
		{
			Invoking(() => ClaimFactory.CreateCheckIn("message-type", "file://server/folder/file.bin")).Should().NotThrow();
		}

		[Fact]
		public void CreateCheckOut()
		{
			Invoking(() => ClaimFactory.CreateCheckOut("file://server/folder/file.bin")).Should().NotThrow();
		}

		[Fact]
		public void CreateCheckWithMessageType()
		{
			Invoking(() => ClaimFactory.CreateCheck("message-type", "file://server/folder/file.bin")).Should().NotThrow();
		}
	}
}
