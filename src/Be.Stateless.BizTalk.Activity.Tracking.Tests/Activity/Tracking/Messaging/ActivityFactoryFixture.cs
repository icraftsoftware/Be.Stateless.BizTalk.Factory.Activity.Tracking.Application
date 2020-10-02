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

using FluentAssertions;
using Microsoft.BizTalk.Bam.EventObservation;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Moq;
using Xunit;

namespace Be.Stateless.BizTalk.Activity.Tracking.Messaging
{
	public class ActivityFactoryFixture
	{
		[Fact]
		public void CreateMessagingStepReturnsRegularMessagingStep()
		{
			var messageMock = new Unit.Message.Mock<IBaseMessage>();
			var factory = new ActivityFactory(PipelineContextMock.Object);
			factory.CreateMessagingStep(messageMock.Object).Should().BeOfType<MessagingStep>();
		}

		[Fact]
		public void CreateProcessReturnsRegularProcess()
		{
			var messageMock = new Unit.Message.Mock<IBaseMessage>();
			var factory = new ActivityFactory(PipelineContextMock.Object);
			factory.CreateProcess(messageMock.Object, "name").Should().BeOfType<Process>();
		}

		[Fact]
		public void FindMessagingStepReturnsMessagingStepReference()
		{
			var factory = new ActivityFactory(PipelineContextMock.Object);
			factory.FindMessagingStep(new TrackingContext { MessagingStepActivityId = "pseudo-activity-id" }).Should().BeOfType<MessagingStepReference>();
		}

		[Fact]
		public void FindProcessReturnsProcessReference()
		{
			var factory = new ActivityFactory(PipelineContextMock.Object);
			factory.FindProcess(new TrackingContext { ProcessActivityId = "pseudo-activity-id" }).Should().BeOfType<ProcessReference>();
		}

		public ActivityFactoryFixture()
		{
			PipelineContextMock = new Mock<IPipelineContext>();
			PipelineContextMock.Setup(pc => pc.GetEventStream()).Returns(new Mock<EventStream>().Object);
		}

		private Mock<IPipelineContext> PipelineContextMock { get; }
	}
}
