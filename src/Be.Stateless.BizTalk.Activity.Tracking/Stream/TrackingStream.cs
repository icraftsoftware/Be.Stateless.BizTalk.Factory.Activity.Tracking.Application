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
using Be.Stateless.BizTalk.Activity.Tracking;
using Be.Stateless.Extensions;
using Be.Stateless.IO.Extensions;
using Microsoft.BizTalk.Streaming;

namespace Be.Stateless.BizTalk.Stream
{
	/// <summary>
	/// Endows a <see cref="System.IO.Stream"/> with both capture and tracking support.
	/// </summary>
	/// <remarks>
	/// The capture capability allows the <see cref="TrackingStream"/> to replicate its payload on-the-fly, that is to say while
	/// being read, for tracking purposes. The tracking capability ensures descriptive information about the payload can be
	/// tracked, see <see cref="CaptureDescriptor"/>.
	/// </remarks>
	/// <seealso cref="ReplicatingReadStream"/>
	[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "Required for unit testing purposes.")]
	internal class TrackingStream : EventingReadStream
	{
		/// <summary>
		/// Creates a new instance of a <see cref="TrackingStream"/>.
		/// </summary>
		/// <param name="stream">
		/// The <see cref="System.IO.Stream"/> to endow with tracking and claim support.
		/// </param>
		internal TrackingStream(System.IO.Stream stream) : base(stream) { }

		/// <summary>
		/// Creates a new instance of a <see cref="TrackingStream"/> and provides information about how and where a payload has
		/// already been tracked, typically when a claim check token is redeemed.
		/// </summary>
		/// <param name="stream">
		/// The <see cref="System.IO.Stream"/> to endow with tracking and claim support.
		/// </param>
		/// <param name="captureDescriptor">
		/// The <see cref="MessageBodyCaptureDescriptor"/> describing the previously captured-to-disk payload stream to reuse.
		/// </param>
		internal TrackingStream(System.IO.Stream stream, MessageBodyCaptureDescriptor captureDescriptor) : this(stream)
		{
			if (captureDescriptor == null) throw new ArgumentNullException(nameof(captureDescriptor));
			if (captureDescriptor.CaptureMode != MessageBodyCaptureMode.Claimed)
				throw new ArgumentException(
					$"A {nameof(TrackingStream)}, whose payload is being redeemed, cannot be instantiated with a CaptureDescriptor having a CaptureMode of {captureDescriptor.CaptureMode}; the only compliant CaptureMode is {MessageBodyCaptureMode.Claimed}.",
					nameof(captureDescriptor));
			CaptureDescriptor = captureDescriptor;
		}

		/// <summary>
		/// Describes how and where the <see cref="TrackingStream"/> payload is captured.
		/// </summary>
		internal MessageBodyCaptureDescriptor CaptureDescriptor { get; private set; }

		/// <summary>
		/// <c>true</c> for an outbound claim-checked message whose payload is being redeemed.
		/// </summary>
		internal bool IsRedeemed => CaptureDescriptor.IfNotNull(cd => cd.CaptureMode == MessageBodyCaptureMode.Claimed) && !(InnerStream is ReplicatingReadStream);

		/// <summary>
		/// Drains the stream thereby forcing its payload to be captured.
		/// </summary>
		internal void Capture()
		{
			if (CaptureDescriptor == null)
				throw new InvalidOperationException($"{nameof(TrackingStream)} cannot be captured because its Descriptor is null and has not been initialized for tracking.");
			if (CaptureDescriptor.CaptureMode != MessageBodyCaptureMode.Claimed)
				throw new InvalidOperationException(
					$"{nameof(TrackingStream)} cannot be captured because its Descriptor's CaptureMode has not been set to {MessageBodyCaptureMode.Claimed} but to {CaptureDescriptor.CaptureMode}.");
			if (!(InnerStream is ReplicatingReadStream))
				throw new InvalidOperationException(
					$"{nameof(TrackingStream)} cannot be captured unless it has been setup with another capturing stream to replicate its payload to.");
			ThrowIfDisposed();
			this.Drain();
		}

		/// <summary>
		/// Provides the <see cref="TrackingStream"/> with probing capabilities without firing any <see
		/// cref="EventingReadStream"/> events.
		/// </summary>
		/// <returns></returns>
		internal MarkableForwardOnlyEventingReadStream AsMarkable()
		{
			// use a markable stream to be able to rewind to the start after capture mode assessment
			var markableForwardOnlyEventingReadStream = MarkableForwardOnlyEventingReadStream.EnsureMarkable(InnerStream);
			// and keep a ref to it to ensure it will be properly disposed
			InnerStream = markableForwardOnlyEventingReadStream;

			// ensure we can rewind TrackingStream
			markableForwardOnlyEventingReadStream.MarkPosition();

			// return markableForwardOnlyEventingReadStream, i.e. InnerStream, and not this TrackingStream to prevent base
			// EventingReadStream class' events from firing while assessing TrackingStream's capture mode
			return markableForwardOnlyEventingReadStream;
		}

		/// <summary>
		/// Provide information about how and where the <see cref="TrackingStream"/> payload has been captured.
		/// </summary>
		/// <param name="captureDescriptor">
		/// Describes how and where the stream is captured.
		/// </param>
		internal virtual void SetupCapture(MessageBodyCaptureDescriptor captureDescriptor)
		{
			if (captureDescriptor == null) throw new ArgumentNullException(nameof(captureDescriptor));
			if (captureDescriptor.CaptureMode != MessageBodyCaptureMode.Unclaimed)
				throw new InvalidOperationException(
					$"{nameof(TrackingStream)}'s capture cannot be setup with a CaptureMode of {captureDescriptor.CaptureMode}; other CaptureMode than {MessageBodyCaptureMode.Unclaimed} requires a capturing stream.");
			if (CaptureDescriptor != null) throw new InvalidOperationException($"{nameof(TrackingStream)}'s capture has already been setup and cannot be overwritten.");
			ThrowIfDisposed();
			ResetPositionAndStopMarking();
			CaptureDescriptor = captureDescriptor;
		}

		/// <summary>
		/// Setup how and where the <see cref="TrackingStream"/> will capture its payload while being read.
		/// </summary>
		/// <param name="captureDescriptor">
		/// Describes how and where the stream is captured.
		/// </param>
		/// <param name="capturingStream">
		/// The <see cref="System.IO.Stream"/> to capture, or replicate, its payload to while being read.
		/// </param>
		internal virtual void SetupCapture(MessageBodyCaptureDescriptor captureDescriptor, System.IO.Stream capturingStream)
		{
			if (captureDescriptor == null) throw new ArgumentNullException(nameof(captureDescriptor));
			if (capturingStream == null) throw new ArgumentNullException(nameof(capturingStream));
			if (captureDescriptor.CaptureMode != MessageBodyCaptureMode.Claimed)
				throw new InvalidOperationException(
					$"{nameof(TrackingStream)}'s capture cannot be setup with a CaptureMode of {captureDescriptor.CaptureMode}; other CaptureMode than {MessageBodyCaptureMode.Claimed} cannot use a capturing stream.");
			if (CaptureDescriptor != null) throw new InvalidOperationException($"{nameof(TrackingStream)}'s capture has already been setup and cannot be overwritten.");
			ThrowIfDisposed();
			ResetPositionAndStopMarking();
			CaptureDescriptor = captureDescriptor;
			InnerStream = new ReplicatingReadStream(InnerStream, capturingStream);
		}

		[SuppressMessage("ReSharper", "InvertIf")]
		private void ResetPositionAndStopMarking()
		{
			if (InnerStream is MarkableForwardOnlyEventingReadStream markableForwardOnlyEventingReadStream)
			{
				markableForwardOnlyEventingReadStream.ResetPosition();
				markableForwardOnlyEventingReadStream.StopMarking();
			}
		}
	}
}
