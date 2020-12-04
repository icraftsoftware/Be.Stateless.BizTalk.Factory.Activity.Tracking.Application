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

namespace Be.Stateless.BizTalk.ClaimStore.States
{
	public class ReleasedDataFileFixture
	{
		[Fact]
		public void GatherDoesNotTransitionToNewState()
		{
			var sut = new ReleasedDataFile("201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.released");
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			sut.Gather(messageBodyMock.Object, Path.GetTempPath());

			messageBodyMock.Object.DataFile.Should().BeSameAs(sut);
		}

		[Fact]
		public void LockTransitionsToAwaitingRetryDataFileWhenOperationFails()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.released";
			var sut = new ReleasedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			servantMock.Setup(s => s.TryMoveFile(filePath, It.IsAny<string>())).Returns(false);

			sut.Lock(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeOfType<AwaitingRetryDataFile>();
		}

		[Fact]
		public void LockTransitionsToNewReleasedDataFileWhenOperationSucceeds()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.released";
			var sut = new ReleasedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			servantMock.Setup(s => s.TryMoveFile(filePath, It.IsAny<string>())).Returns(true);

			sut.Lock(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeOfType<ReleasedDataFile>();
			messageBodyMock.Object.DataFile.Should().NotBeSameAs(sut);
			(sut.Path.Tokenize().LockTime < messageBodyMock.Object.DataFile.Path.Tokenize().LockTime).Should().BeTrue();
		}

		[Fact]
		public void ReleaseDoesNotTransitionToNewState()
		{
			var sut = new ReleasedDataFile("201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.released");
			var messageBodyMock = new Mock<MessageBody>(sut);
			messageBodyMock.SetupAllProperties();

			sut.Release(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeSameAs(sut);
		}

		[Fact]
		public void UnlockDeletesLocalDataFile()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.released";
			var sut = new ReleasedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);

			servantMock.Setup(s => s.TryDeleteFile(filePath)).Returns(true).Verifiable();

			sut.Unlock(messageBodyMock.Object);

			servantMock.VerifyAll();
		}

		[Fact]
		public void UnlockDoesNotTransitionToNewStateWhenOperationSucceeds()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.released";
			var sut = new ReleasedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);

			servantMock.Setup(s => s.TryDeleteFile(filePath)).Returns(true);

			sut.Unlock(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeSameAs(sut);
		}

		[Fact]
		public void UnlockTransitionsToAwaitingRetryDataFileWhenOperationFails()
		{
			var servantMock = new Mock<DataFileServant>();
			DataFileServant.Instance = servantMock.Object;

			const string filePath = "201306158F341A2D6FD7416B87073A0132DD51AE.chk.20150627111406.released";
			var sut = new ReleasedDataFile(filePath);
			var messageBodyMock = new Mock<MessageBody>(sut);

			servantMock.Setup(s => s.TryDeleteFile(filePath)).Returns(false);

			sut.Unlock(messageBodyMock.Object);

			messageBodyMock.Object.DataFile.Should().BeOfType<AwaitingRetryDataFile>();
		}
	}
}
