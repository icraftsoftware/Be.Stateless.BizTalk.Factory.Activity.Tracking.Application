#region Copyright & License

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

using System.IO;
using FluentAssertions;
using Moq;
using Xunit;

namespace Be.Stateless.BizTalk.Claim.Store.States
{
	public class AwaitingRetryDataFileFixture
	{
		[Fact]
		public void GatherDoesNotTransitionToNewState()
		{
			var sut = new AwaitingRetryDataFile(new UnlockedDataFile("201306158F341A2D6FD7416B87073A0132DD51AE.trk"));
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			sut.Gather(messageBodyMock.Object, Path.GetTempPath());

			messageBodyMock.Object.DataFile.Should().BeSameAs(sut);
		}

		[Fact]
		public void LockDoesNotTransitionToNewState()
		{
			var sut = new AwaitingRetryDataFile(new UnlockedDataFile("201306158F341A2D6FD7416B87073A0132DD51AE.trk"));
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			sut.Lock(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeSameAs(sut);
		}

		[Fact]
		public void ReleaseDoesNotTransitionToNewState()
		{
			var sut = new AwaitingRetryDataFile(new UnlockedDataFile("201306158F341A2D6FD7416B87073A0132DD51AE.trk"));
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			sut.Release(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeSameAs(sut);
		}

		[Fact]
		public void UnlockDoesNotTransitionToNewState()
		{
			var sut = new AwaitingRetryDataFile(new UnlockedDataFile("201306158F341A2D6FD7416B87073A0132DD51AE.trk"));
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			sut.Unlock(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeSameAs(sut);
		}
	}
}
