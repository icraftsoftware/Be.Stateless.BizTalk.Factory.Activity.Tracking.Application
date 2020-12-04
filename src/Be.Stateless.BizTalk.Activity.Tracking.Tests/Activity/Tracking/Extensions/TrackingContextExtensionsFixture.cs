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
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.BizTalk.Message.Extensions;
using FluentAssertions;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.XLANGs.BaseTypes;
using Moq;
using Xunit;
using static Be.Stateless.Unit.DelegateFactory;

namespace Be.Stateless.BizTalk.Activity.Tracking.Extensions
{
	public class TrackingContextExtensionsFixture
	{
		[Fact]
		public void GetTrackingContextForMessagingStep()
		{
			var message = new Unit.Message.Mock<IBaseMessage>(MockBehavior.Strict);
			message
				.Setup(m => m.GetProperty(TrackingProperties.ProcessActivityId))
				.Returns(It.IsAny<string>())
				.Verifiable();
			message
				.Setup(m => m.GetProperty(TrackingProperties.ProcessingStepActivityId))
				.Returns(It.IsAny<string>())
				.Verifiable();
			message
				.Setup(m => m.GetProperty(TrackingProperties.MessagingStepActivityId))
				.Returns(It.IsAny<string>())
				.Verifiable();

			message.Object.GetTrackingContext();

			message.Verify();
		}

		[Fact]
		public void GetTrackingContextForMessagingStepThrowsOnEmpty()
		{
			var message = new Unit.Message.Mock<IBaseMessage>();

			Action(() => message.Object.GetTrackingContext(true))
				.Should().Throw<InvalidOperationException>()
				.WithMessage("Invalid TrackingContext: None of its discrete activity Ids are set.");
		}

		[Fact]
		public void GetTrackingContextForProcessingStep()
		{
			var message = new Mock<XLANGMessage>(MockBehavior.Strict);
			message.Setup(
					m => m.GetPropertyValue(TrackingProperties.ProcessActivityId.Type))
				.Returns(It.IsAny<string>())
				.Verifiable();
			message.Setup(
					m => m.GetPropertyValue(TrackingProperties.ProcessingStepActivityId.Type))
				.Returns(It.IsAny<string>())
				.Verifiable();
			message.Setup(
					m => m.GetPropertyValue(TrackingProperties.MessagingStepActivityId.Type))
				.Returns(It.IsAny<string>())
				.Verifiable();

			message.Object.GetTrackingContext();

			message.Verify();
		}

		[Fact]
		public void SetTrackingContextForMessagingStep()
		{
			var message = new Unit.Message.Mock<IBaseMessage>();

			var tp = message.Object.SetTrackingContext(CreateTrackingContext());

			// processing step activity id is propagated
			message.Verify(
				m => m.SetProperty(TrackingProperties.ProcessingStepActivityId, tp.ProcessingStepActivityId),
				Times.Once());
			// and all other tracking properties also are
			message.Verify(
				m => m.SetProperty(TrackingProperties.ProcessActivityId, tp.ProcessActivityId),
				Times.Once());
			message.Verify(
				m => m.SetProperty(TrackingProperties.MessagingStepActivityId, tp.MessagingStepActivityId),
				Times.Once());
		}

		[Fact]
		public void SetTrackingContextForProcessingStep()
		{
			var message = new Mock<XLANGMessage>();

			var tp = CreateTrackingContext();
			message.Object.SetTrackingContext(tp);

			// messaging step activity id is propagated
			message.Verify(
				m => m.SetPropertyValue(
					TrackingProperties.MessagingStepActivityId.Type,
					tp.MessagingStepActivityId),
				Times.Once());
			// and all other tracking properties also are
			message.Verify(
				m => m.SetPropertyValue(
					TrackingProperties.ProcessActivityId.Type,
					tp.ProcessActivityId),
				Times.Once());
			message.Verify(
				m => m.SetPropertyValue(
					TrackingProperties.ProcessingStepActivityId.Type,
					tp.ProcessingStepActivityId),
				Times.Once());
		}

		[Fact]
		public void ThrowsWhenOrchestrationMessageIsNull()
		{
			Action(() => ((XLANGMessage) null).GetTrackingContext()).Should().Throw<ArgumentNullException>();
			Action(() => ((XLANGMessage) null).SetTrackingContext(new TrackingContext())).Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public void ThrowsWhenPipelineMessageIsNull()
		{
			Action(() => ((IBaseMessage) null).GetTrackingContext()).Should().Throw<ArgumentNullException>();
			Action(() => ((IBaseMessage) null).SetTrackingContext(new TrackingContext())).Should().Throw<ArgumentNullException>();
		}

		private TrackingContext CreateTrackingContext()
		{
			return new TrackingContext {
				ProcessActivityId = Guid.NewGuid().ToString(),
				ProcessingStepActivityId = Guid.NewGuid().ToString(),
				MessagingStepActivityId = Guid.NewGuid().ToString()
			};
		}
	}
}
