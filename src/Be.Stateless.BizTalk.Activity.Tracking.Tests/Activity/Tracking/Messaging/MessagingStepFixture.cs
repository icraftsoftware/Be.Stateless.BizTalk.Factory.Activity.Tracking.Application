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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.BizTalk.Message.Extensions;
using Be.Stateless.BizTalk.Stream;
using Be.Stateless.IO.Extensions;
using Be.Stateless.Linq;
using FluentAssertions;
using Microsoft.BizTalk.Bam.EventObservation;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Moq;
using Xunit;
using MessageMock = Be.Stateless.BizTalk.Unit.Message.Mock<Microsoft.BizTalk.Message.Interop.IBaseMessage>;

namespace Be.Stateless.BizTalk.Activity.Tracking.Messaging
{
	public class MessagingStepFixture
	{
		[Fact]
		public void ActivityIsBegunAndActivityIdWrittenInMessageContext()
		{
			var eventStream = new Mock<EventStream>();
			var pipelineContext = new Mock<IPipelineContext>();
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var message = new MessageMock();
			message.Setup(m => m.GetProperty(BtsProperties.InboundTransportLocation)).Returns("inbound-transport-location");

			var sut = new MessagingStep(pipelineContext.Object, message.Object);
			var activityId = sut.ActivityId;

			message.Verify(m => m.SetProperty(TrackingProperties.MessagingStepActivityId, activityId), Times.Once());
			eventStream.Verify(s => s.BeginActivity(nameof(MessagingStep), activityId), Times.Once());
		}

		[Fact]
		public void ActivityIsCommittedAndEnded()
		{
			var eventStream = new Mock<EventStream>();
			var pipelineContext = new Mock<IPipelineContext>();
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var message = new MessageMock();
			message.Setup(m => m.GetProperty(BtsProperties.InboundTransportLocation)).Returns("inbound-transport-location");

			var sut = new MessagingStep(pipelineContext.Object, message.Object);
			using (var trackingStream = new TrackingStream(new MemoryStream(_content)))
			{
				// TrackActivity is supposed to occur at stream's end
				trackingStream.Drain();
				sut.TrackActivity(ActivityTrackingModes.Step, trackingStream);
			}

			eventStream.Verify(s => s.UpdateActivity(nameof(MessagingStep), sut.ActivityId, It.IsAny<object[]>()), Times.Once());
			eventStream.Verify(s => s.Flush(), Times.Once());
			eventStream.Verify(s => s.EndActivity(nameof(MessagingStep), sut.ActivityId), Times.Once());
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public void InboundFailedPropertiesAreTracked()
		{
			var message = new MessageMock();
			SetupCommonProperties(message);
			SetupOutboundSuccessfulProperties(message);
			SetupCommonFailedProperties(message);
			message.Setup(m => m.GetProperty(ErrorReportProperties.ReceivePortName)).Returns("failed-receive-port-name");
			message.Setup(m => m.GetProperty(ErrorReportProperties.InboundTransportLocation)).Returns("failed-inbound-transport-location");

			var activityId = string.Empty;
			Dictionary<string, object> data = null;
			var eventStream = new Mock<EventStream>();
			eventStream
				.Setup(es => es.UpdateActivity(nameof(MessagingStep), It.Is<string>(id => id == activityId), It.IsAny<object[]>()))
				.Callback<string, string, object[]>((_, _, d) => data = ToDictionary(d))
				.Verifiable();

			var pipelineContext = new Mock<IPipelineContext> { DefaultValue = DefaultValue.Mock };
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var sut = new MessagingStep(pipelineContext.Object, message.Object);
			activityId = sut.ActivityId;
			using (var trackingStream = new TrackingStream(new MemoryStream(_content)))
			{
				// TrackActivity is supposed to occur at stream's end
				trackingStream.Drain();
				sut.TrackActivity(ActivityTrackingModes.Step, trackingStream);
			}

			eventStream.Verify();

			var expectedData = new Dictionary<string, object> {
					{ nameof(MessagingStep.TransportType), "outbound-transport-type" },
					{ nameof(MessagingStep.PortName), "failed-receive-port-name" },
					{ nameof(MessagingStep.TransportLocation), "failed-inbound-transport-location" }
				}
				.Union(ExpectedCommonFailedData, new LambdaComparer<KeyValuePair<string, object>>((kvp1, kvp2) => kvp1.Key == kvp2.Key))
				.Union(ExpectedCommonData, new LambdaComparer<KeyValuePair<string, object>>((kvp1, kvp2) => kvp1.Key == kvp2.Key))
				.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			data.Should().BeEquivalentTo(expectedData);
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public void InboundSuccessfulPropertiesAreTracked()
		{
			var message = new MessageMock();
			SetupCommonProperties(message);
			SetupInboundSuccessfulProperties(message);

			var activityId = string.Empty;
			Dictionary<string, object> data = null;
			var eventStream = new Mock<EventStream>();
			eventStream
				.Setup(es => es.UpdateActivity(nameof(MessagingStep), It.Is<string>(id => id == activityId), It.IsAny<object[]>()))
				.Callback<string, string, object[]>((_, _, d) => data = ToDictionary(d))
				.Verifiable();

			var pipelineContext = new Mock<IPipelineContext> { DefaultValue = DefaultValue.Mock };
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var sut = new MessagingStep(pipelineContext.Object, message.Object);
			activityId = sut.ActivityId;
			using (var trackingStream = new TrackingStream(new MemoryStream(_content)))
			{
				// TrackActivity is supposed to occur at stream's end
				trackingStream.Drain();
				sut.TrackActivity(ActivityTrackingModes.Step, trackingStream);
			}

			eventStream.Verify();

			var expectedData = new Dictionary<string, object> {
					{ nameof(MessagingStep.MessageID), _messageId.AsNormalizedActivityId() },
					{ nameof(MessagingStep.MessageType), "message-type" },
					{ nameof(MessagingStep.PortName), "receive-location-name" },
					{ nameof(MessagingStep.TransportLocation), "inbound-transport-location" },
					{ nameof(MessagingStep.TransportType), "inbound-transport-type" },
					{ nameof(MessagingStep.Status), TrackingStatus.Received },
					{ nameof(MessagingStep.MachineName), Environment.MachineName },
					{ nameof(MessagingStep.Time), sut.Time }
				}
				.Union(ExpectedCommonData, new LambdaComparer<KeyValuePair<string, object>>((kvp1, kvp2) => kvp1.Key == kvp2.Key))
				.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			data.Should().BeEquivalentTo(expectedData);
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public void InboundSuccessfulPropertiesForSolicitResponsePortAreTracked()
		{
			var message = new MessageMock();
			SetupCommonProperties(message);
			// no ReceiveLocationName on the inbound of a solicit-response port but a ReceivePortName
			message.Setup(m => m.GetProperty(BtsProperties.ReceivePortName)).Returns("receive-port-name");
			message.Setup(m => m.GetProperty(BtsProperties.InboundTransportLocation)).Returns("inbound-transport-location");
			message.Setup(m => m.GetProperty(BtsProperties.InboundTransportType)).Returns("inbound-transport-type");

			var activityId = string.Empty;
			Dictionary<string, object> data = null;
			var eventStream = new Mock<EventStream>();
			eventStream
				.Setup(es => es.UpdateActivity(nameof(MessagingStep), It.Is<string>(id => id == activityId), It.IsAny<object[]>()))
				.Callback<string, string, object[]>((_, _, d) => data = ToDictionary(d))
				.Verifiable();

			var pipelineContext = new Mock<IPipelineContext> { DefaultValue = DefaultValue.Mock };
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var sut = new MessagingStep(pipelineContext.Object, message.Object);
			activityId = sut.ActivityId;
			using (var trackingStream = new TrackingStream(new MemoryStream(_content)))
			{
				// TrackActivity is supposed to occur at stream's end
				trackingStream.Drain();
				sut.TrackActivity(ActivityTrackingModes.Step, trackingStream);
			}

			eventStream.Verify();

			var expectedData = new Dictionary<string, object> {
					{ nameof(MessagingStep.MessageID), _messageId.AsNormalizedActivityId() },
					{ nameof(MessagingStep.MessageType), "message-type" },
					{ nameof(MessagingStep.PortName), "receive-port-name" },
					{ nameof(MessagingStep.TransportLocation), "inbound-transport-location" },
					{ nameof(MessagingStep.TransportType), "inbound-transport-type" },
					{ nameof(MessagingStep.Status), TrackingStatus.Received },
					{ nameof(MessagingStep.MachineName), Environment.MachineName },
					{ nameof(MessagingStep.Time), sut.Time }
				}
				.Union(ExpectedCommonData, new LambdaComparer<KeyValuePair<string, object>>((kvp1, kvp2) => kvp1.Key == kvp2.Key))
				.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			data.Should().BeEquivalentTo(expectedData);
		}

		[Fact]
		public void MessageBodyIsTracked()
		{
			var message = new MessageMock();
			message.Setup(m => m.GetProperty(BtsProperties.InboundTransportLocation)).Returns("inbound-transport-location");

			var eventStream = new Mock<EventStream>();

			var pipelineContext = new Mock<IPipelineContext> { DefaultValue = DefaultValue.Mock };
			pipelineContext
				.Setup(pc => pc.GetEventStream())
				.Returns(eventStream.Object);

			var sut = new MessagingStep(pipelineContext.Object, message.Object);
			using (var trackingStream = new TrackingStream(new MemoryStream(_content), new MessageBodyCaptureDescriptor("data", MessageBodyCaptureMode.Claimed)))
			{
				// TrackActivity is supposed to occur at stream's end
				trackingStream.Drain();
				sut.TrackActivity(ActivityTrackingModes.Body, trackingStream);
			}

			eventStream.Verify(
				es => es.AddReference(
					nameof(MessagingStep),
					sut.ActivityId,
					MessageBodyCaptureMode.Claimed.ToString(),
					MessagingStep.MESSAGE_BODY_REFERENCE_NAME,
					It.IsAny<string>(),
					It.IsAny<string>()),
				Times.Once());
		}

		[Fact]
		public void MessageContextIsTracked()
		{
			var ns = BtsProperties.MessageType.Namespace;
			var name = BtsProperties.MessageType.Name;

			var message = new MessageMock();
			message.Setup(m => m.GetProperty(BtsProperties.InboundTransportLocation)).Returns("inbound-transport-location");
			message.Setup(m => m.Context.CountProperties).Returns(1);
			message.Setup(m => m.Context.ReadAt(0, out name, out ns)).Returns("message-type");
			message.Setup(m => m.Context.IsPromoted(name, ns)).Returns(false);

			var eventStream = new Mock<EventStream>();

			var pipelineContext = new Mock<IPipelineContext> { DefaultValue = DefaultValue.Mock };
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var sut = new MessagingStep(pipelineContext.Object, message.Object);
			using (var trackingStream = new TrackingStream(new MemoryStream(_content)))
			{
				// TrackActivity is supposed to occur at stream's end
				trackingStream.Drain();
				sut.TrackActivity(ActivityTrackingModes.Context, trackingStream);
			}

			eventStream.Verify(
				es => es.AddReference(
					nameof(MessagingStep),
					sut.ActivityId,
					MessagingStep.MESSAGE_CONTEXT_REFERENCE_TYPE,
					MessagingStep.MESSAGE_CONTEXT_REFERENCE_NAME,
					It.IsAny<string>(),
					message.Object.Context.ToXml()),
				Times.Once());
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public void OutboundFailedPropertiesAreTracked()
		{
			var message = new MessageMock();
			SetupCommonProperties(message);
			SetupInboundSuccessfulProperties(message);
			SetupOutboundSuccessfulProperties(message);
			SetupCommonFailedProperties(message);
			message.Setup(m => m.GetProperty(ErrorReportProperties.SendPortName)).Returns("failed-send-port-name");
			message.Setup(m => m.GetProperty(ErrorReportProperties.OutboundTransportLocation)).Returns("failed-outbound-transport-location");

			var activityId = string.Empty;
			Dictionary<string, object> data = null;
			var eventStream = new Mock<EventStream>();
			eventStream
				.Setup(es => es.UpdateActivity(nameof(MessagingStep), It.Is<string>(id => id == activityId), It.IsAny<object[]>()))
				.Callback<string, string, object[]>((_, _, d) => data = ToDictionary(d))
				.Verifiable();

			var pipelineContext = new Mock<IPipelineContext> { DefaultValue = DefaultValue.Mock };
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var sut = new MessagingStep(pipelineContext.Object, message.Object);
			activityId = sut.ActivityId;
			using (var trackingStream = new TrackingStream(new MemoryStream(_content)))
			{
				// TrackActivity is supposed to occur at stream's end
				trackingStream.Drain();
				sut.TrackActivity(ActivityTrackingModes.Step, trackingStream);
			}

			eventStream.Verify();

			var expectedData = new Dictionary<string, object> {
					{ nameof(MessagingStep.TransportType), "outbound-transport-type" },
					{ nameof(MessagingStep.PortName), "failed-send-port-name" },
					{ nameof(MessagingStep.TransportLocation), "failed-outbound-transport-location" }
				}
				.Union(ExpectedCommonFailedData, new LambdaComparer<KeyValuePair<string, object>>((kvp1, kvp2) => kvp1.Key == kvp2.Key))
				.Union(ExpectedCommonData, new LambdaComparer<KeyValuePair<string, object>>((kvp1, kvp2) => kvp1.Key == kvp2.Key))
				.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			data.Should().BeEquivalentTo(expectedData);
			sut.MessageType.Should().Be("failed-message-type");
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public void OutboundSuccessfulPropertiesAreTrackedAtStreamExhaustion()
		{
			var message = new MessageMock();
			SetupCommonProperties(message);
			SetupInboundSuccessfulProperties(message);
			SetupOutboundSuccessfulProperties(message);

			var activityId = string.Empty;
			Dictionary<string, object> data = null;
			var eventStream = new Mock<EventStream>();
			eventStream
				.Setup(es => es.UpdateActivity(nameof(MessagingStep), It.Is<string>(id => id == activityId), It.IsAny<object[]>()))
				.Callback<string, string, object[]>((_, _, d) => data = ToDictionary(d))
				.Verifiable();

			var pipelineContext = new Mock<IPipelineContext> { DefaultValue = DefaultValue.Mock };
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var sut = new MessagingStep(pipelineContext.Object, message.Object);
			activityId = sut.ActivityId;
			using (var trackingStream = new TrackingStream(new MemoryStream(_content)))
			{
				// TrackActivity is supposed to occur at stream's end
				trackingStream.Drain();
				sut.TrackActivity(ActivityTrackingModes.Step, trackingStream);
			}

			eventStream.Verify();

			var expectedData = new Dictionary<string, object> {
					{ nameof(MessagingStep.MessageID), _messageId.AsNormalizedActivityId() },
					{ nameof(MessagingStep.PortName), "send-port-name" },
					{ nameof(MessagingStep.TransportLocation), "outbound-transport-location" },
					{ nameof(MessagingStep.TransportType), "outbound-transport-type" },
					{ nameof(MessagingStep.Status), TrackingStatus.Sent },
					{ nameof(MessagingStep.MachineName), Environment.MachineName },
					{ nameof(MessagingStep.Time), sut.Time }
				}
				.Union(ExpectedCommonData)
				.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			data.Should().BeEquivalentTo(expectedData);
			sut.MessageType.Should().Be("message-type");
		}

		private Dictionary<string, object> ExpectedCommonData => new() {
			{ nameof(MessagingStep.InterchangeID), _interchangeId.AsNormalizedActivityId() },
			{ nameof(MessagingStep.MessageID), _messageId.AsNormalizedActivityId() },
			{ nameof(MessagingStep.MessageSize), _content.Length },
			{ nameof(MessagingStep.MessageType), "message-type" },
			{ nameof(MessagingStep.RetryCount), 3 },
			{ nameof(MessagingStep.Value1), "value-1" },
			{ nameof(MessagingStep.Value2), "value-2" },
			{ nameof(MessagingStep.Value3), "value-3" }
		};

		private Dictionary<string, object> ExpectedCommonFailedData => new() {
			{ nameof(MessagingStep.MessageID), _failedMessageId.AsNormalizedActivityId() },
			{ nameof(MessagingStep.MessageType), "failed-message-type" },
			{ nameof(MessagingStep.Status), TrackingStatus.FailedMessage },
			{ nameof(MessagingStep.TransportType), "inbound-transport-type" },
			{ nameof(MessagingStep.ErrorCode), "failure-code" },
			{ nameof(MessagingStep.ErrorDescription), "failure-description" },
			{ nameof(MessagingStep.MachineName), "failing-machine" },
			{ nameof(MessagingStep.Time), _failureTime }
		};

		private void SetupCommonProperties(Unit.Message.Mock<IBaseMessage> message)
		{
			_interchangeId = Guid.NewGuid();
			message.Setup(m => m.GetProperty(BtsProperties.InterchangeID)).Returns(_interchangeId.ToString());

			_messageId = Guid.NewGuid();
			message.Setup(m => m.MessageID).Returns(_messageId);
			message.Setup(m => m.GetProperty(BtsProperties.MessageType)).Returns("message-type");
			message.Setup(m => m.GetProperty(BtsProperties.ActualRetryCount)).Returns(3);
			message.Setup(m => m.GetProperty(TrackingProperties.Value1)).Returns("value-1");
			message.Setup(m => m.GetProperty(TrackingProperties.Value2)).Returns("value-2");
			message.Setup(m => m.GetProperty(TrackingProperties.Value3)).Returns("value-3");
		}

		private void SetupCommonFailedProperties(Unit.Message.Mock<IBaseMessage> message)
		{
			_failedMessageId = Guid.NewGuid();
			_failureTime = DateTime.UtcNow;
			message.Setup(m => m.GetProperty(ErrorReportProperties.FailureMessageID)).Returns(_failedMessageId.ToString());
			message.Setup(m => m.GetProperty(ErrorReportProperties.MessageType)).Returns("failed-message-type");
			message.Setup(m => m.GetProperty(ErrorReportProperties.ErrorType)).Returns(TrackingStatus.FailedMessage);
			message.Setup(m => m.GetProperty(ErrorReportProperties.FailureCode)).Returns("failure-code");
			message.Setup(m => m.GetProperty(ErrorReportProperties.Description)).Returns("failure-description");
			message.Setup(m => m.GetProperty(ErrorReportProperties.ProcessingServer)).Returns("failing-machine");
			message.Setup(m => m.GetProperty(ErrorReportProperties.FailureTime)).Returns(_failureTime);
		}

		private void SetupInboundSuccessfulProperties(Unit.Message.Mock<IBaseMessage> message)
		{
			message.Setup(m => m.GetProperty(BtsProperties.ReceiveLocationName)).Returns("receive-location-name");
			message.Setup(m => m.GetProperty(BtsProperties.InboundTransportLocation)).Returns("inbound-transport-location");
			message.Setup(m => m.GetProperty(BtsProperties.InboundTransportType)).Returns("inbound-transport-type");
		}

		private static void SetupOutboundSuccessfulProperties(Unit.Message.Mock<IBaseMessage> message)
		{
			message.Setup(m => m.GetProperty(BtsProperties.SendPortName)).Returns("send-port-name");
			message.Setup(m => m.GetProperty(BtsProperties.OutboundTransportLocation)).Returns("outbound-transport-location");
			message.Setup(m => m.GetProperty(BtsProperties.OutboundTransportType)).Returns("outbound-transport-type");
		}

		[SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
		private static Dictionary<string, object> ToDictionary(object[] data)
		{
			return Enumerable.Range(0, data.Length / 2)
				.ToDictionary(i => (string) data[i * 2], i => data[i * 2 + 1])
				.OrderBy(kvp => kvp.Key)
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		private readonly byte[] _content = Encoding.Unicode.GetBytes(new string('A', 512));
		private Guid _interchangeId;
		private Guid _messageId;
		private Guid _failedMessageId;
		private DateTime _failureTime;
	}
}
