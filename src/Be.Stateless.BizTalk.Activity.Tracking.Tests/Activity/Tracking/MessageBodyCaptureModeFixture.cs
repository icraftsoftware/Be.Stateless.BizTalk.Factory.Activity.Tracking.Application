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

extern alias Monitoring;
using FluentAssertions;
using Xunit;
using MonitoringMessageBodyCaptureMode = Monitoring::Be.Stateless.BizTalk.Activity.Tracking.MessageBodyCaptureMode;

namespace Be.Stateless.BizTalk.Activity.Tracking
{
	public class MessageBodyCaptureModeFixture
	{
		[Fact]
		public void Assembly()
		{
			typeof(MessageBodyCaptureMode).Assembly.GetName().Name.Should().Be("Be.Stateless.BizTalk.Activity.Tracking");
			typeof(MonitoringMessageBodyCaptureMode).Assembly.GetName().Name.Should().Be("Be.Stateless.BizTalk.Activity.Monitoring");
		}

		[Fact]
		public void Claimed()
		{
			((int) MessageBodyCaptureMode.Claimed).Should().Be((int) MonitoringMessageBodyCaptureMode.Claimed);
		}

		[Fact]
		public void Unclaimed()
		{
			((int) MessageBodyCaptureMode.Unclaimed).Should().Be((int) MonitoringMessageBodyCaptureMode.Unclaimed);
		}
	}
}
