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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.BizTalk.Bam.EventObservation;
using Microsoft.BizTalk.Component.Interop;
using Moq;
using Xunit;
using MessageMock = Be.Stateless.BizTalk.Unit.Message.Mock<Microsoft.BizTalk.Message.Interop.IBaseMessage>;

namespace Be.Stateless.BizTalk.Activity.Tracking.Messaging
{
	public class ProcessReferenceFixture
	{
		[Fact]
		public void MessagingStepIsAffiliatedToProcess()
		{
			var processMessagingStepActivityId = string.Empty;
			Dictionary<string, object> data = null;
			var eventStream = new Mock<EventStream>();
			eventStream
				.Setup(e => e.BeginActivity(nameof(ProcessMessagingStep), It.IsAny<string>()))
				.Callback<string, string>((_, i) => processMessagingStepActivityId = i);
			eventStream
				.Setup(es => es.UpdateActivity(nameof(ProcessMessagingStep), It.Is<string>(id => id == processMessagingStepActivityId), It.IsAny<object[]>()))
				.Callback<string, string, object[]>((_, _, d) => data = Enumerable.Range(0, d.Length / 2).ToDictionary(i => (string) d[i * 2], i => d[i * 2 + 1]));

			var pipelineContext = new Mock<IPipelineContext>();
			pipelineContext.Setup(pc => pc.GetEventStream()).Returns(eventStream.Object);
			var message = new MessageMock();
			var messagingStep = new MessagingStep(pipelineContext.Object, message.Object);

			var processActivityId = ActivityId.NewActivityId();
			var sut = new ProcessReference(processActivityId, eventStream.Object);
			sut.AddStep(messagingStep);

			eventStream.Verify(s => s.BeginActivity(nameof(ProcessMessagingStep), processMessagingStepActivityId), Times.Once());
			eventStream.Verify(s => s.UpdateActivity(nameof(ProcessMessagingStep), processMessagingStepActivityId, It.IsAny<object[]>()), Times.Once());
			eventStream.Verify(s => s.Flush(), Times.Once());
			eventStream.Verify(s => s.EndActivity(nameof(ProcessMessagingStep), processMessagingStepActivityId), Times.Once());

			var expectedData = new Dictionary<string, object> {
				{ nameof(ProcessMessagingStep.MessagingStepActivityID), messagingStep.ActivityId },
				{ nameof(ProcessMessagingStep.ProcessActivityID), processActivityId }
			};
			data.Should().BeEquivalentTo(expectedData);
		}
	}
}
