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

namespace Be.Stateless.BizTalk.ClaimStore.States
{
	public class LockedDataFileFixture
	{
		[Fact]
		public void GatherCopiesDataFileToCentralClaimStoreAndRenamesLocalDataFile()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.locked";
			var sut = new LockedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);

			servantMock.Setup(s => s.TryCreateDirectory(Path.Combine(Path.GetTempPath(), sut.ClaimStoreRelativePath))).Returns(true).Verifiable();
			servantMock.Setup(s => s.TryCopyFile(filePath, It.Is<string>(path => path == Path.Combine(Path.GetTempPath(), sut.ClaimStoreRelativePath)))).Returns(true).Verifiable();
			servantMock.Setup(s => s.TryMoveFile(filePath, It.Is<string>(path => path.Tokenize().State == GatheredDataFile.STATE_TOKEN))).Returns(true).Verifiable();

			sut.Gather(messageBodyMock.Object, Path.GetTempPath());

			servantMock.VerifyAll();
		}

		[Fact]
		public void GatherTransitionsToAwaitingRetryDataFileWhenOperationFails()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.locked";
			var sut = new LockedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			servantMock.Setup(s => s.TryCreateDirectory(Path.Combine(Path.GetTempPath(), sut.ClaimStoreRelativePath))).Returns(true);
			servantMock.Setup(s => s.TryCopyFile(filePath, It.Is<string>(path => path == Path.Combine(Path.GetTempPath(), sut.ClaimStoreRelativePath)))).Returns(true);
			servantMock.Setup(s => s.TryMoveFile(filePath, It.Is<string>(path => path.Tokenize().State == GatheredDataFile.STATE_TOKEN))).Returns(false);

			sut.Gather(messageBodyMock.Object, Path.GetTempPath());

			messageBodyMock.Object.DataFile.Should().BeOfType<AwaitingRetryDataFile>();
		}

		[Fact]
		public void GatherTransitionsToGatheredDataFileWhenOperationSucceeds()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.locked";
			var sut = new LockedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			servantMock.Setup(s => s.TryCreateDirectory(Path.Combine(Path.GetTempPath(), sut.ClaimStoreRelativePath))).Returns(true);
			servantMock.Setup(s => s.TryCopyFile(filePath, It.Is<string>(path => path == Path.Combine(Path.GetTempPath(), sut.ClaimStoreRelativePath)))).Returns(true);
			servantMock.Setup(s => s.TryMoveFile(filePath, It.Is<string>(path => path.Tokenize().State == GatheredDataFile.STATE_TOKEN))).Returns(true);

			sut.Gather(messageBodyMock.Object, Path.GetTempPath());

			messageBodyMock.Object.DataFile.Should().BeOfType<GatheredDataFile>();
		}

		[Fact]
		public void LockTransitionsToAwaitingRetryDataFileWhenOperationFails()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.locked";
			var sut = new LockedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			servantMock.Setup(s => s.TryMoveFile(filePath, It.IsAny<string>())).Returns(false);

			sut.Lock(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeOfType<AwaitingRetryDataFile>();
		}

		[Fact]
		public void LockTransitionsToNewLockedDataFileWhenOperationSucceeds()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.locked";
			var sut = new LockedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			servantMock.Setup(s => s.TryMoveFile(filePath, It.IsAny<string>())).Returns(true);

			sut.Lock(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeOfType<LockedDataFile>();
			messageBodyMock.Object.DataFile.Should().NotBeSameAs(sut);
			(sut.Path.Tokenize().LockTime < messageBodyMock.Object.DataFile.Path.Tokenize().LockTime).Should().BeTrue();
		}

		[Fact]
		public void ReleasingLockedDataFileIsInvalid()
		{
			var sut = new LockedDataFile("201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.locked");
			var messageBodyMock = new Mock<MessageBody>(sut);

			Action(() => sut.Release(messageBodyMock.Object)).Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void UnlockingLockedDataFileIsInvalid()
		{
			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.locked";
			var sut = new LockedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);

			Action(() => sut.Unlock(messageBodyMock.Object)).Should().Throw<InvalidOperationException>();
		}
	}
}
