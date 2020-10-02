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
using System.Text;
using Be.Stateless.BizTalk.Activity.Tracking;
using Be.Stateless.IO.Extensions;
using Be.Stateless.Reflection;
using FluentAssertions;
using Microsoft.BizTalk.Streaming;
using Xunit;
using static FluentAssertions.FluentActions;

namespace Be.Stateless.BizTalk.Stream
{
	public class TrackingStreamFixture
	{
		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public void CaptureDrainsInnerStream()
		{
			using (var innerStream = new MemoryStream(_content))
			using (var stream = new TrackingStream(innerStream))
			{
				stream.SetupCapture(new("some-data", MessageBodyCaptureMode.Claimed), new MemoryStream());
				innerStream.Position.Should().Be(0);
				Invoking(() => stream.Capture()).Should().NotThrow();
				innerStream.Position.Should().Be(innerStream.Length);
			}
		}

		[Fact]
		public void CaptureSetupResetMarkablePosition()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content)))
			{
				var ms = stream.AsMarkable();
				ms.Drain();
				stream.Position.Should().Be(_content.Length);
				stream.SetupCapture(new("some-data", MessageBodyCaptureMode.Unclaimed));
				stream.Position.Should().Be(0);
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public void CaptureThrowsWhenCaptureModeIsNotClaimed()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content)))
			{
				stream.SetupCapture(new("some-data", MessageBodyCaptureMode.Unclaimed));
				Invoking(() => stream.Capture()).Should().Throw<InvalidOperationException>()
					.WithMessage("TrackingStream cannot be captured because its Descriptor's CaptureMode has not been set to Claimed but to Unclaimed.");
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public void CaptureThrowsWhenNoCaptureDescriptor()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content)))
			{
				Invoking(() => stream.Capture()).Should().Throw<InvalidOperationException>()
					.WithMessage("TrackingStream cannot be captured because its Descriptor is null and has not been initialized for tracking.");
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public void CaptureThrowsWhenNoCapturingStream()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content), new("url", MessageBodyCaptureMode.Claimed)))
			{
				Invoking(() => stream.Capture()).Should().Throw<InvalidOperationException>()
					.WithMessage("TrackingStream cannot be captured unless it has been setup with another capturing stream to replicate its payload to.");
			}
		}

		[Fact]
		public void EventsAreNotFiredThroughMarkableInnerStream()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content)))
			{
				var edgeEventsCount = 0;
				stream.AfterLastReadEvent += (_, _) => ++edgeEventsCount;
				stream.BeforeFirstReadEvent += (_, _) => ++edgeEventsCount;
				var eventsCount = 0;
				stream.ReadEvent += (_, _) => ++eventsCount;

				var markableForwardOnlyEventingReadStream = stream.AsMarkable();
				edgeEventsCount.Should().Be(0);
				eventsCount.Should().Be(0);

				markableForwardOnlyEventingReadStream.Drain();
				edgeEventsCount.Should().Be(0);
				eventsCount.Should().Be(0);
			}
		}

		[Fact]
		public void InnerStreamIsWrappedByMarkableStream()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content)))
			{
				stream.AsMarkable();
				Reflector.GetProperty(stream, "InnerStream").Should().BeOfType<MarkableForwardOnlyEventingReadStream>();
			}
		}

		[Fact]
		public void InnerStreamIsWrappedByReplicatingStreamIfTracked()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content)))
			{
				stream.SetupCapture(new("some-data", MessageBodyCaptureMode.Claimed), new MemoryStream());
				Reflector.GetProperty(stream, "InnerStream").Should().BeOfType<ReplicatingReadStream>();
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
		public void PayloadIsBeingRedeemed()
		{
			Invoking(() => new TrackingStream(new MemoryStream(), new("url", MessageBodyCaptureMode.Unclaimed)))
				.Should().Throw<ArgumentException>()
				.WithMessage(
					"A TrackingStream, whose payload is being redeemed, cannot be instantiated with a CaptureDescriptor having a CaptureMode of Unclaimed; the only compliant CaptureMode is Claimed.*");

			using (var stream = new TrackingStream(new MemoryStream(_content), new("url", MessageBodyCaptureMode.Claimed)))
			{
				stream.IsRedeemed.Should().BeTrue();
			}
		}

		[Fact]
		public void PayloadIsNotBeingRedeemed()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content)))
			{
				stream.IsRedeemed.Should().BeFalse();
				stream.SetupCapture(new("some-data", MessageBodyCaptureMode.Unclaimed));
				stream.IsRedeemed.Should().BeFalse();
			}
			using (var stream = new TrackingStream(new MemoryStream(_content)))
			{
				stream.IsRedeemed.Should().BeFalse();
				stream.SetupCapture(new("url", MessageBodyCaptureMode.Claimed), new MemoryStream());
				stream.IsRedeemed.Should().BeFalse();
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public void SetupCaptureAsClaimedThrowsWhenCaptureModeIsOther()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content)))
			{
				Invoking(() => stream.SetupCapture(new("some-data", MessageBodyCaptureMode.Unclaimed), new MemoryStream()))
					.Should().Throw<InvalidOperationException>()
					.WithMessage("TrackingStream's capture cannot be setup with a CaptureMode of Unclaimed; other CaptureMode than Claimed cannot use a capturing stream.");
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public void SetupCaptureAsClaimedThrowsWithoutCapturingStream()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content)))
			{
				Invoking(() => stream.SetupCapture(new("some-data", MessageBodyCaptureMode.Claimed), null))
					.Should().Throw<ArgumentNullException>()
					.Which.ParamName.Should().Be("capturingStream");
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public void SetupCaptureAsUnclaimedThrowsWhenCaptureModeIsOther()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content)))
			{
				Invoking(() => stream.SetupCapture(new("some-data", MessageBodyCaptureMode.Claimed)))
					.Should().Throw<InvalidOperationException>()
					.WithMessage("TrackingStream's capture cannot be setup with a CaptureMode of Claimed; other CaptureMode than Unclaimed requires a capturing stream.");
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public void SetupCaptureThrowsIfCaptureDescriptorHasAlreadyBeenSetup()
		{
			using (var stream = new TrackingStream(new MemoryStream(_content), new("some-data", MessageBodyCaptureMode.Claimed)))
			{
				Invoking(() => stream.SetupCapture(new("other-data", MessageBodyCaptureMode.Unclaimed)))
					.Should().Throw<InvalidOperationException>()
					.WithMessage("TrackingStream's capture has already been setup and cannot be overwritten.");

				Invoking(() => stream.SetupCapture(new("other-data", MessageBodyCaptureMode.Claimed), new MemoryStream()))
					.Should().Throw<InvalidOperationException>()
					.WithMessage("TrackingStream's capture has already been setup and cannot be overwritten.");
			}
		}

		private readonly byte[] _content = Encoding.Unicode.GetBytes(new string('A', 3999));
	}
}
