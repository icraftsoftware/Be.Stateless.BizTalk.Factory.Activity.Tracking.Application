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
using System.Linq;
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.BizTalk.Message.Extensions;
using FluentAssertions;
using Microsoft.BizTalk.Bam.EventObservation;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Moq;
using Xunit;

namespace Be.Stateless.BizTalk.Activity.Tracking.Messaging
{
	public class ProcessFixture
	{
		[Fact]
		public void ActivityIsBegunAndActivityIdWrittenInMessageContext()
		{
			var eventStream = new Mock<EventStream>();
			var pipelineContext = new Mock<IPipelineContext>();
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var message = new Unit.Message.Mock<IBaseMessage>();

			var sut = new Process(pipelineContext.Object, message.Object, "process-name");
			var activityId = sut.ActivityId;

			message.Verify(m => m.SetProperty(TrackingProperties.ProcessActivityId, activityId), Times.Once());
			eventStream.Verify(s => s.BeginActivity(nameof(Process), activityId), Times.Once());
		}

		[Fact]
		public void ActivityIsCommittedAndEnded()
		{
			var eventStream = new Mock<EventStream>();
			var pipelineContext = new Mock<IPipelineContext>();
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var message = new Unit.Message.Mock<IBaseMessage>();

			var sut = new Process(pipelineContext.Object, message.Object, "process-name");
			var activityId = sut.ActivityId;
			sut.TrackActivity();

			eventStream.Verify(s => s.UpdateActivity(nameof(Process), activityId, It.IsAny<object[]>()), Times.Once());
			eventStream.Verify(s => s.Flush(), Times.Once());
			eventStream.Verify(s => s.EndActivity(nameof(Process), activityId), Times.Once());
		}

		[Fact]
		public void MessagingStepIsAffiliatedToProcess()
		{
			Dictionary<string, object> data = null;
			var processMessagingStepActivityId = string.Empty;
			var eventStream = new Mock<EventStream>();
			eventStream
				.Setup(e => e.BeginActivity(nameof(ProcessMessagingStep), It.IsAny<string>()))
				.Callback<string, string>((_, i) => processMessagingStepActivityId = i);
			eventStream
				.Setup(es => es.UpdateActivity(nameof(ProcessMessagingStep), It.Is<string>(id => id == processMessagingStepActivityId), It.IsAny<object[]>()))
				.Callback<string, string, object[]>((_, _, d) => data = Enumerable.Range(0, d.Length / 2).ToDictionary(i => (string) d[i * 2], i => d[i * 2 + 1]))
				.Verifiable();

			var pipelineContext = new Mock<IPipelineContext>();
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var message = new Unit.Message.Mock<IBaseMessage>();
			message.Setup(m => m.GetProperty(ErrorReportProperties.ErrorType)).Returns(TrackingStatus.FailedMessage);

			var processActivityId = ActivityId.NewActivityId();
			var sut = new Process(processActivityId, eventStream.Object);
			var messagingStep = new MessagingStep(pipelineContext.Object, message.Object);
			var messagingStepActivityId = messagingStep.ActivityId;
			sut.AddStep(messagingStep);

			eventStream.Verify();
			eventStream.Verify(s => s.BeginActivity(nameof(ProcessMessagingStep), processMessagingStepActivityId), Times.Once());
			eventStream.Verify(s => s.UpdateActivity(nameof(ProcessMessagingStep), processMessagingStepActivityId, It.IsAny<object[]>()), Times.Once());
			eventStream.Verify(s => s.Flush(), Times.Once());
			eventStream.Verify(s => s.EndActivity(nameof(ProcessMessagingStep), processMessagingStepActivityId), Times.Once());

			var expectedData = new Dictionary<string, object> {
				{ nameof(ProcessMessagingStep.MessagingStepActivityID), messagingStepActivityId },
				// capture of Status is what distinguishes affiliation of a MessagingStep from affiliation of a MessagingStepReference
				{ nameof(ProcessMessagingStep.MessagingStepStatus), TrackingStatus.FailedMessage },
				{ nameof(ProcessMessagingStep.ProcessActivityID), processActivityId }
			};
			data.Should().BeEquivalentTo(expectedData);
		}

		[Fact]
		public void MessagingStepReferenceIsAffiliatedToProcess()
		{
			Dictionary<string, object> data = null;
			var processMessagingStepActivityId = string.Empty;
			var eventStream = new Mock<EventStream>();
			eventStream
				.Setup(e => e.BeginActivity(nameof(ProcessMessagingStep), It.IsAny<string>()))
				.Callback<string, string>((_, i) => processMessagingStepActivityId = i);
			eventStream
				.Setup(es => es.UpdateActivity(nameof(ProcessMessagingStep), It.Is<string>(id => id == processMessagingStepActivityId), It.IsAny<object[]>()))
				.Callback<string, string, object[]>((_, _, d) => data = Enumerable.Range(0, d.Length / 2).ToDictionary(i => (string) d[i * 2], i => d[i * 2 + 1]))
				.Verifiable();

			var processActivityId = ActivityId.NewActivityId();
			var messagingStepActivityId = ActivityId.NewActivityId();

			var sut = new Process(processActivityId, eventStream.Object);
			sut.AddStep(new MessagingStepReference(messagingStepActivityId, eventStream.Object));

			eventStream.Verify();
			eventStream.Verify(s => s.BeginActivity(nameof(ProcessMessagingStep), processMessagingStepActivityId), Times.Once());
			eventStream.Verify(s => s.UpdateActivity(nameof(ProcessMessagingStep), processMessagingStepActivityId, It.IsAny<object[]>()), Times.Once());
			eventStream.Verify(s => s.Flush(), Times.Once());
			eventStream.Verify(s => s.EndActivity(nameof(ProcessMessagingStep), processMessagingStepActivityId), Times.Once());

			var expectedData = new Dictionary<string, object> {
				{ nameof(ProcessMessagingStep.MessagingStepActivityID), messagingStepActivityId },
				{ nameof(ProcessMessagingStep.ProcessActivityID), processActivityId }
			};
			data.Should().BeEquivalentTo(expectedData);
		}

		[Fact]
		public void ProcessPropertiesAreTracked()
		{
			var interchangeId = Guid.NewGuid();
			var message = new Unit.Message.Mock<IBaseMessage>();
			message.Setup(m => m.GetProperty(BtsProperties.InterchangeID)).Returns(interchangeId.ToString());
			message.Setup(m => m.GetProperty(TrackingProperties.Value1)).Returns("value-1");
			message.Setup(m => m.GetProperty(TrackingProperties.Value2)).Returns("value-2");
			message.Setup(m => m.GetProperty(TrackingProperties.Value3)).Returns("value-3");
			message.Setup(m => m.GetProperty(ErrorReportProperties.ErrorType)).Returns(TrackingStatus.FailedMessage);

			var eventStream = new Mock<EventStream>();

			var pipelineContext = new Mock<IPipelineContext>();
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);

			var sut = new Process(pipelineContext.Object, message.Object, "process-name");

			Dictionary<string, object> data = null;
			eventStream
				.Setup(es => es.UpdateActivity(nameof(Process), It.Is<string>(id => id == sut.ActivityId), It.IsAny<object[]>()))
				.Callback<string, string, object[]>((_, _, d) => data = Enumerable.Range(0, d.Length / 2).ToDictionary(i => (string) d[i * 2], i => d[i * 2 + 1]))
				.Verifiable();

			sut.TrackActivity();

			eventStream.Verify();

			var expectedData = new Dictionary<string, object> {
				{ nameof(Process.InterchangeID), interchangeId.AsNormalizedActivityId() },
				{ nameof(Process.ProcessName), "process-name" },
				{ nameof(Process.Status), TrackingStatus.Failed },
				{ nameof(Process.Value1), "value-1" },
				{ nameof(Process.Value2), "value-2" },
				{ nameof(Process.Value3), "value-3" },
				{ nameof(Process.BeginTime), sut.BeginTime!.Value },
				{ nameof(Process.EndTime), sut.EndTime!.Value }
			};
			data.Should().BeEquivalentTo(expectedData);
		}
	}
}
