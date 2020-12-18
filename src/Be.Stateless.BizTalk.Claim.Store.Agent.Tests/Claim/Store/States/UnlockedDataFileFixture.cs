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

using System;
using System.IO;
using FluentAssertions;
using Moq;
using Xunit;
using static Be.Stateless.Unit.DelegateFactory;

namespace Be.Stateless.BizTalk.Claim.Store.States
{
	public class UnlockedDataFileFixture
	{
		[Fact]
		public void GatheringUnlockedDataFileIsInvalid()
		{
			var sut = new UnlockedDataFile("201306158F341A2D6FD7416B87073A0132DD51AE.chk");
			var messageBodyMock = new Mock<MessageBody>(sut);

			Action(() => sut.Gather(messageBodyMock.Object, Path.GetTempPath())).Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void LockRenamesLocalDataFile()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk";
			var sut = new UnlockedDataFile("201306158F341A2D6FD7416B87073A0132DD51AE.chk");
			var messageBodyMock = new Mock<MessageBody>(sut);

			servantMock.Setup(s => s.TryMoveFile(filePath, It.Is<string>(path => path.Tokenize().State == LockedDataFile.STATE_TOKEN))).Returns(true).Verifiable();

			sut.Lock(messageBodyMock.Object);

			servantMock.VerifyAll();
		}

		[Fact]
		public void LockTransitionsToAwaitingRetryDataFileWhenOperationFails()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk";
			var sut = new UnlockedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			servantMock.Setup(s => s.TryMoveFile(filePath, It.Is<string>(path => path.Tokenize().State == LockedDataFile.STATE_TOKEN))).Returns(false);

			sut.Lock(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeOfType<AwaitingRetryDataFile>();
		}

		[Fact]
		public void LockTransitionsToLockedDataFileWhenOperationSucceeds()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk";
			var sut = new UnlockedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			servantMock.Setup(s => s.TryMoveFile(filePath, It.Is<string>(path => path.Tokenize().State == LockedDataFile.STATE_TOKEN))).Returns(true);

			sut.Lock(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeOfType<LockedDataFile>();
		}

		[Fact]
		public void ReleasingUnlockedDataFileIsInvalid()
		{
			var sut = new UnlockedDataFile("201306158F341A2D6FD7416B87073A0132DD51AE.chk");
			var messageBodyMock = new Mock<MessageBody>(sut);

			Action(() => sut.Release(messageBodyMock.Object)).Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void UnlockingAlreadyUnlockedDataFileIsInvalid()
		{
			var sut = new UnlockedDataFile("201306158F341A2D6FD7416B87073A0132DD51AE.chk");
			var messageBodyMock = new Mock<MessageBody>(sut);

			Action(() => sut.Unlock(messageBodyMock.Object)).Should().Throw<InvalidOperationException>();
		}
	}
}
