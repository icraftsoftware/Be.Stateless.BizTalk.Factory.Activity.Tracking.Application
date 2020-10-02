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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Transactions;
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.BizTalk.Message.Extensions;
using Be.Stateless.BizTalk.Stream;
using Be.Stateless.BizTalk.Unit;
using Be.Stateless.IO;
using FluentAssertions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Moq;
using Xunit;

namespace Be.Stateless.BizTalk.Activity.Tracking.Messaging
{
	public class MessageBodyTrackerFixture : IDisposable
	{
		#region Setup/Teardown

		public MessageBodyTrackerFixture()
		{
			_claimStoreInstance = ClaimStore.Instance;
			ClaimStoreMock = new();
			ClaimStore.Instance = ClaimStoreMock.Object;

			MessageMock = new();
			MessageMock.Setup(m => m.GetProperty(BtsProperties.InboundTransportLocation)).Returns("inbound-transport-location");

			PipelineContextMock = new();
			PipelineContextMock.Setup(pc => pc.ResourceTracker).Returns(new Mock<IResourceTracker>().Object);

			_processNameResolverFactory = ProcessNameResolver.Factory;
			ProcessNameResolverMock = new(MessageMock.Object);
			ProcessNameResolver.Factory = _ => ProcessNameResolverMock.Object;
		}

		public void Dispose()
		{
			ClaimStore.Instance = _claimStoreInstance;
			ProcessNameResolver.Factory = _processNameResolverFactory;
		}

		#endregion

		[Fact]
		public void CaptureIsNotSetupIfTrackingStreamAlreadyHasCaptureDescriptor()
		{
			using (var trackingStream = new TrackingStream(new MemoryStream(), new("url", MessageBodyCaptureMode.Claimed)))
			{
				MessageMock.Object.BodyPart.Data = trackingStream;

				var sut = MessageBodyTracker.Create(new(PipelineContextMock.Object, MessageMock.Object, ActivityTrackingModes.Body));
				sut.SetupTracking();
			}

			ClaimStoreMock.Verify(
				cs => cs.SetupMessageBodyCapture(It.IsAny<TrackingStream>(), It.IsAny<ActivityTrackingModes>(), It.IsAny<Func<IKernelTransaction>>()),
				Times.Never());
		}

		[Fact]
		public void CaptureOfInboundMessageIsSetup()
		{
			using (var stream = new MemoryStream())
			{
				MessageMock.Object.BodyPart.Data = stream;

				var sut = MessageBodyTracker.Create(new(PipelineContextMock.Object, MessageMock.Object, ActivityTrackingModes.Body));
				sut.SetupTracking();
			}

			ClaimStoreMock.Verify(
				cs => cs.SetupMessageBodyCapture(It.IsAny<TrackingStream>(), It.IsAny<ActivityTrackingModes>(), It.Is<Func<IKernelTransaction>>(ktf => ktf != null)),
				Times.Once());

			MessageMock.Object.BodyPart.Data.Should().BeOfType<TrackingStream>();
		}

		[Fact(Skip = "Broken by Moq.")]
		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		public void CaptureOfInboundMessagePiggiesBackKernelTransaction()
		{
			using (new TransactionScope())
			using (var stream = new MemoryStream())
			{
				MessageMock.Object.BodyPart.Data = stream;

				var transaction = (IKernelTransaction) TransactionInterop.GetDtcTransaction(Transaction.Current);
				PipelineContextMock.As<IPipelineContextEx>()
					.Setup(pc => pc.GetTransaction())
					.Returns(transaction);

				var sut = MessageBodyTracker.Create(new(PipelineContextMock.Object, MessageMock.Object, ActivityTrackingModes.Body));
				sut.SetupTracking();

				// TODO upgrade Moq
				ClaimStoreMock.Verify(
					cs => cs.SetupMessageBodyCapture(
						It.IsAny<TrackingStream>(),
						It.IsAny<ActivityTrackingModes>(),
						It.Is<Func<IKernelTransaction>>(ktf => ReferenceEquals(ktf(), transaction))),
					Times.Once());
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		public void CaptureOfOutboundMessageDoesNotPiggyBackKernelTransaction()
		{
			MessageMock.Setup(m => m.GetProperty(BtsProperties.OutboundTransportLocation)).Returns("outbound-transport-location");

			using (new TransactionScope())
			using (var stream = new MemoryStream())
			{
				MessageMock.Object.BodyPart.Data = stream;

				var transaction = TransactionInterop.GetDtcTransaction(Transaction.Current);
				PipelineContextMock.As<IPipelineContextEx>()
					.Setup(pc => pc.GetTransaction())
					.Returns((IKernelTransaction) transaction);

				var sut = MessageBodyTracker.Create(new(PipelineContextMock.Object, MessageMock.Object, ActivityTrackingModes.Body));
				sut.SetupTracking();

				ClaimStoreMock.Verify(
					cs => cs.SetupMessageBodyCapture(
						It.IsAny<TrackingStream>(),
						It.IsAny<ActivityTrackingModes>(),
						It.Is<Func<IKernelTransaction>>(ktf => ktf == null)),
					Times.Once());
			}
		}

		[Fact]
		public void CaptureOfOutboundMessageIsSetup()
		{
			MessageMock.Setup(m => m.GetProperty(BtsProperties.OutboundTransportLocation)).Returns("outbound-transport-location");

			using (var stream = new MemoryStream())
			{
				MessageMock.Object.BodyPart.Data = stream;

				var sut = MessageBodyTracker.Create(new(PipelineContextMock.Object, MessageMock.Object, ActivityTrackingModes.Body));
				sut.SetupTracking();
			}

			ClaimStoreMock.Verify(
				cs => cs.SetupMessageBodyCapture(It.IsAny<TrackingStream>(), It.IsAny<ActivityTrackingModes>(), It.Is<Func<IKernelTransaction>>(ktf => ktf == null)),
				Times.Once());

			MessageMock.Object.BodyPart.Data.Should().BeOfType<TrackingStream>();
		}

		[Fact]
		public void InboundMessageBodyIsCheckedIn()
		{
			var sut = MessageBodyTracker.Create(new(PipelineContextMock.Object, MessageMock.Object, ActivityTrackingModes.Claim));
			sut.TryCheckInMessageBody();

			ClaimStoreMock.Verify(cs => cs.Claim(MessageMock.Object, It.IsAny<IResourceTracker>()), Times.Once());
		}

		[Fact]
		public void InboundMessageBodyIsNotCheckedIn()
		{
			var sut = MessageBodyTracker.Create(new(PipelineContextMock.Object, MessageMock.Object, ActivityTrackingModes.Body));
			sut.TryCheckInMessageBody();

			ClaimStoreMock.Verify(cs => cs.Claim(MessageMock.Object, It.IsAny<IResourceTracker>()), Times.Never());
		}

		[Fact]
		public void OutboundMessageBodyIsCheckedOut()
		{
			MessageMock.Setup(m => m.GetProperty(BtsProperties.OutboundTransportLocation)).Returns("outbound-transport-location");

			var sut = MessageBodyTracker.Create(new(PipelineContextMock.Object, MessageMock.Object, ActivityTrackingModes.Claim));
			sut.TryCheckOutMessageBody();

			ClaimStoreMock.Verify(cs => cs.Redeem(MessageMock.Object, It.IsAny<IResourceTracker>()), Times.Once());
		}

		[Fact]
		public void OutboundMessageBodyIsNotCheckedOut()
		{
			MessageMock.Setup(m => m.GetProperty(BtsProperties.OutboundTransportLocation)).Returns("outbound-transport-location");

			var sut = MessageBodyTracker.Create(new(PipelineContextMock.Object, MessageMock.Object, ActivityTrackingModes.Body));
			sut.TryCheckOutMessageBody();

			ClaimStoreMock.Verify(cs => cs.Redeem(MessageMock.Object, It.IsAny<IResourceTracker>()), Times.Never());
		}

		private Mock<ClaimStore> ClaimStoreMock { get; }

		private MessageMock MessageMock { get; }

		private Mock<IPipelineContext> PipelineContextMock { get; }

		private Mock<ProcessNameResolver> ProcessNameResolverMock { get; }

		private readonly ClaimStore _claimStoreInstance;
		private readonly Func<IBaseMessage, ProcessNameResolver> _processNameResolverFactory;
	}
}
