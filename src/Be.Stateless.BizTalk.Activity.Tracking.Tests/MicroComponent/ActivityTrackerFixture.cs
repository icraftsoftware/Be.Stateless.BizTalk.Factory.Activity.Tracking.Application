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
using System.Text;
using System.Xml;
using Be.Stateless.BizTalk.Activity.Tracking;
using Be.Stateless.BizTalk.Activity.Tracking.Messaging;
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.BizTalk.Message.Extensions;
using Be.Stateless.BizTalk.MicroComponent.Extensions;
using Be.Stateless.BizTalk.Runtime.Caching;
using Be.Stateless.BizTalk.Unit.MicroComponent;
using Be.Stateless.IO;
using FluentAssertions;
using Microsoft.BizTalk.Message.Interop;
using Moq;
using Xunit;

namespace Be.Stateless.BizTalk.MicroComponent
{
	public class ActivityTrackerFixture : MicroComponentFixture<ActivityTracker>, IDisposable
	{
		#region Setup/Teardown

		public ActivityTrackerFixture()
		{
			MessageMock.Setup(m => m.GetProperty(BtsProperties.InboundTransportLocation)).Returns("inbound-transport-location");

			var activityTrackerContext = new ActivityTracker.Context(PipelineContextMock.Object, MessageMock.Object, ActivityTrackingModes.Body);

			_activityTrackerFactory = Activity.Tracking.Messaging.ActivityTracker.Factory;
			ActivityTrackerMock = new Mock<Activity.Tracking.Messaging.ActivityTracker>(activityTrackerContext);
			Activity.Tracking.Messaging.ActivityTracker.Factory = _ => ActivityTrackerMock.Object;

			_messageBodyTrackerFactory = MessageBodyTracker.Factory;
			MessageBodyTrackerMock = new Mock<MessageBodyTracker>(activityTrackerContext);
			MessageBodyTracker.Factory = _ => MessageBodyTrackerMock.Object;

			_trackingContextCacheInstance = TrackingContextCache.Instance;
			CacheMock = new Mock<TrackingContextCache>(MockBehavior.Strict);
			TrackingContextCache.Instance = CacheMock.Object;

			_processNameResolverFactory = ProcessNameResolver.Factory;
			ProcessNameResolverMock = new Mock<ProcessNameResolver>(MessageMock.Object);
			ProcessNameResolver.Factory = _ => ProcessNameResolverMock.Object;
		}

		public void Dispose()
		{
			Activity.Tracking.Messaging.ActivityTracker.Factory = _activityTrackerFactory;
			MessageBodyTracker.Factory = _messageBodyTrackerFactory;
			TrackingContextCache.Instance = _trackingContextCacheInstance;
			ProcessNameResolver.Factory = _processNameResolverFactory;
		}

		#endregion

		[Fact]
		public void Deserialize()
		{
			var microPipelineComponentType = typeof(ActivityTracker);
			var xml = $"<mComponent name=\"{microPipelineComponentType.AssemblyQualifiedName}\">"
				+ "<TrackingContextCacheDuration>00:02:00</TrackingContextCacheDuration>"
				+ "</mComponent>";
			using (var reader = XmlReader.Create(new StringStream(xml)))
			{
				var microPipelineComponent = reader.DeserializeMicroComponent();
				((ActivityTracker) microPipelineComponent).TrackingContextCacheDuration.Should().Be(TimeSpan.FromMinutes(2));
				reader.EOF.Should().BeTrue();
			}
		}

		[Fact]
		public void Serialize()
		{
			var component = new ActivityTracker();
			var builder = new StringBuilder();
			using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { OmitXmlDeclaration = true }))
			{
				component.Serialize(writer);
			}
			builder.ToString().Should().Be(
				$"<mComponent name=\"{component.GetType().AssemblyQualifiedName}\">"
				+ "<TrackingContextCacheDuration>00:01:00</TrackingContextCacheDuration>"
				+ "<TrackingModes>Body</TrackingModes>"
				+ "</mComponent>");
		}

		[Fact]
		public void TrackingContextIsCachedForSolicitResponseOutboundMessage()
		{
			var transmitWorkId = Guid.NewGuid().ToString();

			var trackingContext = new TrackingContext {
				ProcessActivityId = ActivityId.NewActivityId(),
				ProcessingStepActivityId = ActivityId.NewActivityId(),
				MessagingStepActivityId = ActivityId.NewActivityId()
			};

			MessageMock.Setup(m => m.GetProperty(BtsProperties.IsSolicitResponse)).Returns(true);
			MessageMock.Setup(m => m.GetProperty(BtsProperties.OutboundTransportLocation)).Returns("outbound-transport-location");
			MessageMock.Setup(m => m.GetProperty(BtsProperties.TransmitWorkId)).Returns(transmitWorkId);
			MessageMock.Setup(m => m.GetProperty(TrackingProperties.ProcessActivityId)).Returns(trackingContext.ProcessActivityId);
			MessageMock.Setup(m => m.GetProperty(TrackingProperties.ProcessingStepActivityId)).Returns(trackingContext.ProcessingStepActivityId);
			MessageMock.Setup(m => m.GetProperty(TrackingProperties.MessagingStepActivityId)).Returns(trackingContext.MessagingStepActivityId);

			CacheMock.Setup(c => c.Set(transmitWorkId, trackingContext, 60 + 1));

			var sut = new ActivityTracker();
			sut.Execute(PipelineContextMock.Object, MessageMock.Object);

			CacheMock.VerifyAll();
		}

		[Fact]
		public void TrackingContextIsCachedForSolicitResponseOutboundMessageUnlessNegativeCacheDuration()
		{
			MessageMock.Setup(m => m.GetProperty(BtsProperties.IsSolicitResponse)).Returns(true);
			MessageMock.Setup(m => m.GetProperty(BtsProperties.OutboundTransportLocation)).Returns("outbound-transport-location");
			MessageMock.Setup(m => m.GetProperty(TrackingProperties.ProcessActivityId)).Returns(ActivityId.NewActivityId());

			var sut = new ActivityTracker { TrackingContextCacheDuration = TimeSpan.FromSeconds(-1) };

			sut.Execute(PipelineContextMock.Object, MessageMock.Object);

			CacheMock.VerifyAll();
		}

		[Fact]
		public void TrackingContextIsNotPropagatedForOneWayPort()
		{
			var sut = new ActivityTracker();

			sut.Execute(PipelineContextMock.Object, MessageMock.Object);

			CacheMock.VerifyAll();
		}

		[Fact]
		public void TrackingContextIsNotPropagatedForRequestResponsePort()
		{
			MessageMock.Setup(m => m.GetProperty(BtsProperties.IsRequestResponse)).Returns(true);

			var sut = new ActivityTracker();

			sut.Execute(PipelineContextMock.Object, MessageMock.Object);

			CacheMock.VerifyAll();
		}

		[Fact]
		public void TrackingContextIsRestoredForSolicitResponseInboundMessage()
		{
			var transmitWorkId = Guid.NewGuid().ToString();

			var trackingContext = new TrackingContext {
				ProcessActivityId = ActivityId.NewActivityId(),
				ProcessingStepActivityId = ActivityId.NewActivityId(),
				MessagingStepActivityId = ActivityId.NewActivityId()
			};

			MessageMock.Setup(m => m.GetProperty(BtsProperties.IsSolicitResponse)).Returns(true);
			MessageMock.Setup(m => m.GetProperty(BtsProperties.InboundTransportLocation)).Returns("inbound-transport-location");
			MessageMock.Setup(m => m.GetProperty(BtsProperties.TransmitWorkId)).Returns(transmitWorkId);

			CacheMock.Setup(c => c.Get(transmitWorkId)).Returns(trackingContext);

			var sut = new ActivityTracker();
			sut.Execute(PipelineContextMock.Object, MessageMock.Object);

			CacheMock.VerifyAll();

			// verifies that TrackingContext fields have been restored in message.Context
			MessageMock.Verify(m => m.SetProperty(TrackingProperties.ProcessActivityId, trackingContext.ProcessActivityId));
			MessageMock.Verify(m => m.SetProperty(TrackingProperties.ProcessingStepActivityId, trackingContext.ProcessingStepActivityId));
			MessageMock.Verify(m => m.SetProperty(TrackingProperties.MessagingStepActivityId, trackingContext.MessagingStepActivityId));
		}

		[Fact]
		public void TrackingContextIsRestoredForSolicitResponseInboundMessageUnlessNegativeCacheDuration()
		{
			var transmitWorkId = Guid.NewGuid().ToString();

			var trackingContext = new TrackingContext {
				ProcessActivityId = ActivityId.NewActivityId(),
				ProcessingStepActivityId = ActivityId.NewActivityId(),
				MessagingStepActivityId = ActivityId.NewActivityId()
			};

			MessageMock.Setup(m => m.GetProperty(BtsProperties.IsSolicitResponse)).Returns(true);
			MessageMock.Setup(m => m.GetProperty(BtsProperties.InboundTransportLocation)).Returns("inbound-transport-location");
			MessageMock.Setup(m => m.GetProperty(BtsProperties.TransmitWorkId)).Returns(transmitWorkId);

			var sut = new ActivityTracker { TrackingContextCacheDuration = TimeSpan.FromSeconds(-1) };

			sut.Execute(PipelineContextMock.Object, MessageMock.Object);

			CacheMock.VerifyAll();

			// verifies that TrackingContext fields have been restored in message.Context
			MessageMock.Verify(m => m.SetProperty(TrackingProperties.ProcessActivityId, trackingContext.ProcessActivityId), Times.Never());
			MessageMock.Verify(m => m.SetProperty(TrackingProperties.ProcessingStepActivityId, trackingContext.ProcessingStepActivityId), Times.Never());
			MessageMock.Verify(m => m.SetProperty(TrackingProperties.MessagingStepActivityId, trackingContext.MessagingStepActivityId), Times.Never());
		}

		[Fact]
		public void TrackingModesDefaultsToBody()
		{
			new ActivityTracker().TrackingModes.Should().Be(ActivityTrackingModes.Body);
		}

		[Fact]
		public void TrackingModesIsAnythingButNone()
		{
			// MockBehavior must be Strict for following test
			var activityTrackerComponentContext = new ActivityTracker.Context(PipelineContextMock.Object, MessageMock.Object, ActivityTrackingModes.Body);
			ActivityTrackerMock = new Mock<Activity.Tracking.Messaging.ActivityTracker>(MockBehavior.Strict, activityTrackerComponentContext);
			MessageBodyTrackerMock = new Mock<MessageBodyTracker>(MockBehavior.Strict, activityTrackerComponentContext);

			// method call ordering is important as only one of the first two methods, i.e. either TryCheckOutMessageBody()
			// or SetupCapture(), ensures a TrackingStream is setup
			MessageBodyTrackerMock.Setup(mbt => mbt.TryCheckOutMessageBody())
				.Callback(
					() => MessageBodyTrackerMock.Setup(mbt => mbt.SetupTracking())
						.Callback(
							() => ActivityTrackerMock.Setup(at => at.TrackActivity())
								.Callback(
									() => MessageBodyTrackerMock.Setup(mbt => mbt.TryCheckInMessageBody()).Verifiable())
								.Verifiable())
						.Verifiable())
				.Verifiable();

			var sut = new ActivityTracker { TrackingModes = ActivityTrackingModes.Step };

			sut.Execute(PipelineContextMock.Object, MessageMock.Object);

			ActivityTrackerMock.VerifyAll();
			MessageBodyTrackerMock.VerifyAll();
		}

		[Fact]
		public void TrackingModesIsNone()
		{
			var sut = new ActivityTracker { TrackingModes = ActivityTrackingModes.None };

			sut.Execute(PipelineContextMock.Object, MessageMock.Object);

			MessageBodyTrackerMock.Verify(mbt => mbt.SetupTracking(), Times.Never());
			MessageBodyTrackerMock.Verify(mbt => mbt.TryCheckInMessageBody(), Times.Never());
			MessageBodyTrackerMock.Verify(mbt => mbt.TryCheckOutMessageBody(), Times.Never());
		}

		private Mock<Activity.Tracking.Messaging.ActivityTracker> ActivityTrackerMock { get; set; }

		private Mock<TrackingContextCache> CacheMock { get; }

		private Mock<MessageBodyTracker> MessageBodyTrackerMock { get; set; }

		private Mock<ProcessNameResolver> ProcessNameResolverMock { get; }

		private readonly Func<ActivityTracker.Context, Activity.Tracking.Messaging.ActivityTracker> _activityTrackerFactory;
		private readonly Func<ActivityTracker.Context, MessageBodyTracker> _messageBodyTrackerFactory;
		private readonly TrackingContextCache _trackingContextCacheInstance;
		private readonly Func<IBaseMessage, ProcessNameResolver> _processNameResolverFactory;
	}
}
