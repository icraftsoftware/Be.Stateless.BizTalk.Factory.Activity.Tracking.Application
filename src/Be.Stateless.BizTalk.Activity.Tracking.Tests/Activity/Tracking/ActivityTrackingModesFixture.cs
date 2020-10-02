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
using FluentAssertions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Deployment.Assembly;
using Xunit;

namespace Be.Stateless.BizTalk.Activity.Tracking
{
	/// <summary>
	/// Unit testing <see cref="FlagsAttribute"/> because it was originally a regular enum, i.e. not qualified as a <see
	/// cref="IPropertyBag"/>. This is only to avoid any regression when being string de-/serialized through a <see
	/// cref="ActivityTrackingModes"/>'s <see cref="PipelineComponent"/>.
	/// </summary>
	public class ActivityTrackingModesFixture
	{
		[Fact]
		public void BodyRequiresContext()
		{
			const ActivityTrackingModes sut = ActivityTrackingModes.Body;

			sut.Should().Be(ActivityTrackingModes.Body | ActivityTrackingModes.Context);
			sut.RequiresBodyTracking().Should().BeTrue();
			sut.RequiresContextTracking().Should().BeTrue();
			sut.RequiresBodyClaimChecking().Should().BeFalse();

			Convert.ToString(sut).Should().Be("Body");

			Enum.Parse(typeof(ActivityTrackingModes), "Body").Should().Be(sut);
			Enum.Parse(typeof(ActivityTrackingModes), "Body, Context").Should().Be(sut);
			Enum.Parse(typeof(ActivityTrackingModes), "Context").Should().NotBe(sut);
		}

		[Fact]
		public void ClaimRequiresBody()
		{
			const ActivityTrackingModes sut = ActivityTrackingModes.Claim;

			sut.Should().Be(ActivityTrackingModes.Claim | ActivityTrackingModes.Body);
			sut.RequiresBodyClaimChecking().Should().BeTrue();
			sut.RequiresBodyTracking().Should().BeTrue();

			Convert.ToString(sut).Should().Be("Claim");

			Enum.Parse(typeof(ActivityTrackingModes), "Claim").Should().Be(sut);
			Enum.Parse(typeof(ActivityTrackingModes), "Claim, Body").Should().Be(sut);
			Enum.Parse(typeof(ActivityTrackingModes), "Body").Should().NotBe(sut);
		}

		[Fact]
		public void ContextRequiresStep()
		{
			const ActivityTrackingModes sut = ActivityTrackingModes.Context;

			sut.Should().Be(ActivityTrackingModes.Context | ActivityTrackingModes.Step);
			sut.RequiresContextTracking().Should().BeTrue();
			sut.RequiresStepTracking().Should().BeTrue();

			Convert.ToString(sut).Should().Be("Context");

			Enum.Parse(typeof(ActivityTrackingModes), "Context").Should().Be(sut);
			Enum.Parse(typeof(ActivityTrackingModes), "Context, Step").Should().Be(sut);
			Enum.Parse(typeof(ActivityTrackingModes), "Step").Should().NotBe(sut);
		}

		[Fact]
		public void StepOnly()
		{
			const ActivityTrackingModes sut = ActivityTrackingModes.Step;

			sut.RequiresStepTracking().Should().BeTrue();
			sut.RequiresContextTracking().Should().BeFalse();
			sut.RequiresBodyTracking().Should().BeFalse();
			sut.RequiresBodyClaimChecking().Should().BeFalse();

			Convert.ToString(sut).Should().Be("Step");
		}
	}
}
