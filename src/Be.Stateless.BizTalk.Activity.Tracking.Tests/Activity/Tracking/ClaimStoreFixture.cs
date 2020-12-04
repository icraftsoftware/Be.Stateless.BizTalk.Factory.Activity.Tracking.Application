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
using System.Net;
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.BizTalk.Message;
using Be.Stateless.BizTalk.Message.Extensions;
using Be.Stateless.BizTalk.Schema;
using Be.Stateless.BizTalk.Schemas.Xml;
using Be.Stateless.BizTalk.Settings;
using Be.Stateless.BizTalk.Stream;
using Be.Stateless.Dummies.IO;
using Be.Stateless.IO;
using Be.Stateless.IO.Extensions;
using Be.Stateless.Linq.Extensions;
using Be.Stateless.Xml.Extensions;
using FluentAssertions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Moq;
using Xunit;
using static Be.Stateless.Unit.DelegateFactory;
using Path = System.IO.Path;

namespace Be.Stateless.BizTalk.Activity.Tracking
{
	public class ClaimStoreFixture : IDisposable
	{
		[Fact]
		public void CaptureMessageBody()
		{
			var trackingStream = new TrackingStream(TextStreamDummy.Create(1024 * 1024));

			var messageMock = new Unit.Message.Mock<IBaseMessage> { DefaultValue = DefaultValue.Mock };
			messageMock.Object.BodyPart.Data = trackingStream;

			using (var configurationReaderMockInjectionScope = new SsoConfigurationReaderMockInjectionScope())
			{
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_IN_DIRECTORY_PROPERTY_NAME))
					.Returns(Path.GetTempPath());
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_OUT_DIRECTORY_PROPERTY_NAME))
					.Returns(@"\\server\share");

				ClaimStore.Instance.SetupMessageBodyCapture(trackingStream, ActivityTrackingModes.Body, null);

				messageMock.Object.BodyPart.Data.Drain();

				// payload is claimed to disk and file extension is .trk
				var captureDescriptor = trackingStream.CaptureDescriptor;
				captureDescriptor.CaptureMode.Should().Be(MessageBodyCaptureMode.Claimed);
				captureDescriptor.Data.Should().StartWith(DateTime.Today.ToString(@"yyyyMMdd\\"));
				File.Exists(Path.Combine(Path.GetTempPath(), captureDescriptor.Data.Replace("\\", "") + ".trk")).Should().BeTrue();
			}
		}

		[Fact]
		public void CaptureMessageBodyWillHaveMessageClaimed()
		{
			using (var configurationReaderMockInjectionScope = new SsoConfigurationReaderMockInjectionScope())
			{
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_IN_DIRECTORY_PROPERTY_NAME))
					.Returns(Path.GetTempPath());
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_OUT_DIRECTORY_PROPERTY_NAME))
					.Returns(@"\\server\share");

				var trackingStreamMock = new Mock<TrackingStream>(TextStreamDummy.Create(1024 * 1024)) { CallBase = true };

				ClaimStore.Instance.SetupMessageBodyCapture(trackingStreamMock.Object, ActivityTrackingModes.Body, null);

				trackingStreamMock.Verify(
					ts => ts.SetupCapture(It.Is<MessageBodyCaptureDescriptor>(cd => cd.CaptureMode == MessageBodyCaptureMode.Claimed)),
					Times.Never());
				trackingStreamMock.Verify(
					ts => ts.SetupCapture(It.Is<MessageBodyCaptureDescriptor>(cd => cd.CaptureMode == MessageBodyCaptureMode.Unclaimed)),
					Times.Never());
				trackingStreamMock.Verify(
					ts => ts.SetupCapture(It.Is<MessageBodyCaptureDescriptor>(cd => cd.CaptureMode == MessageBodyCaptureMode.Claimed), It.IsAny<System.IO.Stream>()),
					Times.Once());
			}
		}

		[Fact]
		public void CaptureMessageBodyWillHaveMessageClaimedButSsoApplicationDoesNotExist()
		{
			var actualSsoConfigurationReader = SsoConfigurationReader.Instance;
			// setup a mock callback to ensure that, even if the BizTalk.Factory SSO store is deployed, the call will look for an SSO store that does not exist
			using (var configurationReaderMockInjectionScope = new SsoConfigurationReaderMockInjectionScope())
			{
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_IN_DIRECTORY_PROPERTY_NAME))
					.Callback(() => actualSsoConfigurationReader.Read("NONEXISTENT_APPLICATION", BizTalkFactorySettings.CLAIM_STORE_CHECK_IN_DIRECTORY_PROPERTY_NAME))
					.Returns(Path.GetTempPath());

				var trackingStreamMock = new Mock<TrackingStream>(TextStreamDummy.Create(1024 * 1024)) { CallBase = true };

				Action(() => ClaimStore.Instance.SetupMessageBodyCapture(trackingStreamMock.Object, ActivityTrackingModes.Claim, null))
					.Should().Throw<InvalidOperationException>()
					.WithMessage("AffiliateApplication 'NONEXISTENT_APPLICATION' does not exist.");
			}
		}

		[Fact]
		public void CaptureMessageBodyWillHaveMessageUnclaimed()
		{
			var trackingStreamMock = new Mock<TrackingStream>(TextStreamDummy.Create(1024)) { CallBase = true };

			ClaimStore.Instance.SetupMessageBodyCapture(trackingStreamMock.Object, ActivityTrackingModes.Claim, null);

			trackingStreamMock.Verify(
				ts => ts.SetupCapture(It.Is<MessageBodyCaptureDescriptor>(cd => cd.CaptureMode == MessageBodyCaptureMode.Unclaimed)),
				Times.Once());
			trackingStreamMock.Verify(
				ts => ts.SetupCapture(It.Is<MessageBodyCaptureDescriptor>(cd => cd.CaptureMode == MessageBodyCaptureMode.Claimed), It.IsAny<System.IO.Stream>()),
				Times.Never());
			trackingStreamMock.Verify(
				ts => ts.SetupCapture(It.Is<MessageBodyCaptureDescriptor>(cd => cd.CaptureMode == MessageBodyCaptureMode.Unclaimed), It.IsAny<System.IO.Stream>()),
				Times.Never());
		}

		[Fact]
		public void CaptureMessageBodyWithEmptyStreamWillHaveMessageUnclaimed()
		{
			using (var configurationReaderMockInjectionScope = new SsoConfigurationReaderMockInjectionScope())
			{
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_IN_DIRECTORY_PROPERTY_NAME))
					.Returns(Path.GetTempPath());
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_OUT_DIRECTORY_PROPERTY_NAME))
					.Returns(@"\\server\share");

				var trackingStreamMock = new Mock<TrackingStream>(new MemoryStream()) { CallBase = true };

				ClaimStore.Instance.SetupMessageBodyCapture(trackingStreamMock.Object, ActivityTrackingModes.Body, null);

				trackingStreamMock.Verify(
					ts => ts.SetupCapture(It.Is<MessageBodyCaptureDescriptor>(cd => cd.CaptureMode == MessageBodyCaptureMode.Claimed)),
					Times.Never());
				trackingStreamMock.Verify(
					ts => ts.SetupCapture(It.Is<MessageBodyCaptureDescriptor>(cd => cd.CaptureMode == MessageBodyCaptureMode.Unclaimed)),
					Times.Once());
				trackingStreamMock.Verify(
					ts => ts.SetupCapture(It.Is<MessageBodyCaptureDescriptor>(cd => cd.CaptureMode == MessageBodyCaptureMode.Claimed), It.IsAny<System.IO.Stream>()),
					Times.Never());
			}
		}

		[Fact]
		public void ClaimLeavesMessageUnalteredWhenNoTrackingStreamHasBeenSetup()
		{
			using (var stream = new StringStream("content"))
			{
				MessageMock.Object.BodyPart.Data = stream;

				ClaimStore.Instance.Claim(MessageMock.Object, ResourceTrackerMock.Object);
			}

			MessageMock.Object.BodyPart.Data.Should().BeOfType<StringStream>();
			MessageMock.Object.BodyPart.Data.Position.Should().Be(0);
		}

		[Fact]
		public void ClaimMessageBody()
		{
			using (var configurationReaderMockInjectionScope = new SsoConfigurationReaderMockInjectionScope())
			{
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_IN_DIRECTORY_PROPERTY_NAME))
					.Returns(Path.GetTempPath());
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_OUT_DIRECTORY_PROPERTY_NAME))
					.Returns(@"\\server\share");

				using (var contentStream = TextStreamDummy.Create(1024 * 1024))
				using (var trackingStream = new TrackingStream(contentStream))
				{
					MessageMock.Object.BodyPart.Data = trackingStream;

					ClaimStore.Instance.SetupMessageBodyCapture(trackingStream, ActivityTrackingModes.Claim, null);
					ClaimStore.Instance.Claim(MessageMock.Object, ResourceTrackerMock.Object);

					// message's actual body stream has been exhausted (i.e. saved to disk)
					contentStream.Position.Should().Be(contentStream.Length);

					// message's body stream is replaced by a token message
					using (var reader = new StreamReader(MessageMock.Object.BodyPart.Data))
					{
						reader.ReadToEnd().Should().Be(ClaimFactory.CreateCheckIn(trackingStream.CaptureDescriptor.Data).OuterXml);
					}

					// MessageType of token message is promoted in message context
					var schemaMetadata = SchemaMetadata.For<Claim.CheckIn>();
					MessageMock.Verify(m => m.Promote(BtsProperties.MessageType, schemaMetadata.MessageType), Times.Once());
					MessageMock.Verify(m => m.Promote(BtsProperties.SchemaStrongName, schemaMetadata.DocumentSpec.DocSpecStrongName), Times.Once());

					// payload is claimed to disk and file extension is .chk
					var captureDescriptor = trackingStream.CaptureDescriptor;
					captureDescriptor.CaptureMode.Should().Be(MessageBodyCaptureMode.Claimed);
					captureDescriptor.Data.Should().StartWith(DateTime.Today.ToString(@"yyyyMMdd\\"));
					File.Exists(Path.Combine(Path.GetTempPath(), captureDescriptor.Data.Replace("\\", "") + ".chk")).Should().BeTrue();
				}
			}
		}

		[Fact]
		public void RedeemClaimToken()
		{
			const string content = "dummy";
			const string url = "cca95baa39ab4e25a3c54971ea170911";
			using (var file = File.CreateText(Path.Combine(Path.GetTempPath(), url)))
			{
				file.Write(content);
			}

			using (var configurationReaderMockInjectionScope = new SsoConfigurationReaderMockInjectionScope())
			{
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_IN_DIRECTORY_PROPERTY_NAME))
					.Returns(Path.GetTempPath());
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_OUT_DIRECTORY_PROPERTY_NAME))
					.Returns(Path.GetTempPath());

				using (var tokenStream = ClaimFactory.CreateCheck(url).AsStream())
				{
					MessageMock.Object.BodyPart.Data = tokenStream;

					ClaimStore.Instance.Redeem(MessageMock.Object, ResourceTrackerMock.Object);

					MessageMock.Object.BodyPart.Data.Should().BeOfType<TrackingStream>();
					var captureDescriptor = ((TrackingStream) MessageMock.Object.BodyPart.Data).CaptureDescriptor;
					captureDescriptor.CaptureMode.Should().Be(MessageBodyCaptureMode.Claimed);
					// previously captured payload is reused and not captured/claimed anew
					captureDescriptor.Data.Should().Be(url);
				}

				using (var reader = new StreamReader(MessageMock.Object.BodyPart.Data))
				{
					reader.ReadToEnd().Should().Be(content);
				}
			}
		}

		[Fact]
		public void RedeemClaimTokenThrowsWhenUnexpectedToken()
		{
			using (var tokenStream = ClaimFactory.CreateCheckIn("d59cd2ea045744f4a085b18be678e4f0").AsStream())
			{
				MessageMock.Object.BodyPart.Data = tokenStream;
				Action(() => ClaimStore.Instance.Redeem(MessageMock.Object, ResourceTrackerMock.Object))
					.Should().Throw<InvalidOperationException>()
					.WithMessage("Invalid token message, CheckIn token is not expected to be redeemed.");
			}
		}

		[Fact]
		public void RedeemHttpClaimToken()
		{
			using (var tokenStream = ClaimFactory.CreateCheck("http://nothing/that/exists.xml").AsStream())
			{
				MessageMock.Object.BodyPart.Data = tokenStream;
				// fails, and still, shows that resources can be redeemed from somewhere else than the claim store
				Action(() => ClaimStore.Instance.Redeem(MessageMock.Object, ResourceTrackerMock.Object))
					.Should().Throw<WebException>();
			}
		}

		[Fact]
		public void RequiresCheckInAndOutIsCaseInsensitive()
		{
			using (var configurationReaderMockInjectionScope = new SsoConfigurationReaderMockInjectionScope())
			{
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_IN_DIRECTORY_PROPERTY_NAME))
					.Returns(Path.GetTempPath().ToUpper());
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_OUT_DIRECTORY_PROPERTY_NAME))
					.Returns(Path.GetTempPath().ToLower());

				ClaimStore.RequiresCheckInAndOut.Should().BeFalse();
			}
		}

		[Fact]
		public void RequiresCheckInAndOutIsTrailingBackslashInsensitive()
		{
			using (var configurationReaderMockInjectionScope = new SsoConfigurationReaderMockInjectionScope())
			{
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_IN_DIRECTORY_PROPERTY_NAME))
					// makes sure there is one trailing '\'
					.Returns(Path.GetTempPath().ToUpper().TrimEnd('\\') + '\\');
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_OUT_DIRECTORY_PROPERTY_NAME))
					// makes sure there is no trailing '\'
					.Returns(Path.GetTempPath().ToLower().TrimEnd('\\'));

				ClaimStore.RequiresCheckInAndOut.Should().BeFalse();
			}
		}

		[Fact]
		public void RequiresCheckInAndOutIsTrue()
		{
			using (var configurationReaderMockInjectionScope = new SsoConfigurationReaderMockInjectionScope())
			{
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_IN_DIRECTORY_PROPERTY_NAME))
					.Returns(Path.GetTempPath());
				configurationReaderMockInjectionScope.Mock
					.Setup(ssr => ssr.Read(BizTalkFactorySettings.APPLICATION_NAME, BizTalkFactorySettings.CLAIM_STORE_CHECK_OUT_DIRECTORY_PROPERTY_NAME))
					.Returns(@"\\server\share");

				ClaimStore.RequiresCheckInAndOut.Should().BeTrue();
			}
		}

		public ClaimStoreFixture()
		{
			// reset ClaimStore's cached value
			ClaimStore.CheckInDirectory = null;
			ClaimStore.CheckOutDirectory = null;

			MessageMock = new Unit.Message.Mock<IBaseMessage> { DefaultValue = DefaultValue.Mock };
			ResourceTrackerMock = new Mock<IResourceTracker>();
		}

		public void Dispose()
		{
			File.Delete(Path.Combine(Path.GetTempPath(), "cca95baa39ab4e25a3c54971ea170911"));
			Directory.GetFiles(Path.GetTempPath(), "*.chk").ForEach(File.Delete);
			Directory.GetFiles(Path.GetTempPath(), "*.trk").ForEach(File.Delete);
		}

		private Unit.Message.Mock<IBaseMessage> MessageMock { get; }

		private Mock<IResourceTracker> ResourceTrackerMock { get; }
	}
}
