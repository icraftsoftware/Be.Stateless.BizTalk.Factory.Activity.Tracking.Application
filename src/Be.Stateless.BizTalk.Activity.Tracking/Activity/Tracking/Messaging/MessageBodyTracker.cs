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
using Be.Stateless.BizTalk.Component.Extensions;
using Be.Stateless.BizTalk.Message.Extensions;
using Be.Stateless.BizTalk.Stream;
using Be.Stateless.IO;
using log4net;

namespace Be.Stateless.BizTalk.Activity.Tracking.Messaging
{
	[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "Required for unit testing purposes.")]
	internal class MessageBodyTracker
	{
		internal static MessageBodyTracker Create(MicroComponent.ActivityTracker.Context context)
		{
			return Factory(context);
		}

		#region Mock's Factory Hook Point

		internal static Func<MicroComponent.ActivityTracker.Context, MessageBodyTracker> Factory { get; set; } = context => new(context);

		#endregion

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Protected for mocking purposes.")]
		protected MessageBodyTracker(MicroComponent.ActivityTracker.Context context)
		{
			_context = context;
		}

		private Func<IKernelTransaction> KernelTransactionFactory
		{
			get
			{
				// kernelTransactionFactory exists only for inbound messages: only receive pipelines support transaction piggybacking
				var kernelTransactionFactory = _context.Message.Direction().IsInbound()
					? () => _context.PipelineContext.GetKernelTransaction()
					: (Func<IKernelTransaction>) null;
				return kernelTransactionFactory;
			}
		}

		/// <summary>
		/// Setup a <see cref="TrackingStream"/> that will enable all archiving, capture and tracking requirements.
		/// </summary>
		/// <remarks>
		/// Setup a TrackingStream, if not already done, to ensure that:
		/// <list type="bullet">
		/// <item><description>Stream can be probed;</description></item>
		/// <item><description>Payload can be captured and possibly archived;</description></item>
		/// <item><description>Activity tracking occurs at end of stream.</description></item>
		/// </list>
		/// </remarks>
		internal virtual void SetupTracking()
		{
			// TODO track other parts than BodyPart as well

			// reuse the TrackingStream setup by an earlier CheckOut/Redeem operation if there is one or install a new one
			_trackingStream = _context.Message.BodyPart.GetOriginalDataStream() as TrackingStream
				?? _context.Message.BodyPart.WrapOriginalDataStream(stream => new TrackingStream(stream), _context.PipelineContext.ResourceTracker);

			if (_context.TrackingModes.RequiresBodyTracking() && _trackingStream.CaptureDescriptor == null)
			{
				ClaimStore.Instance.SetupMessageBodyCapture(_trackingStream, _context.TrackingModes, KernelTransactionFactory);
			}
		}

		/// <summary>
		/// Replace the business message body's payload stream with either a <see cref="Schemas.Xml.Claim.Check"/> or a <see
		/// cref="Schemas.Xml.Claim.CheckIn"/> token message if its content has been assessed to be captured to disk while being tracked.
		/// Leave the message body's payload stream unaltered otherwise.
		/// </summary>
		[SuppressMessage("ReSharper", "InvertIf")]
		internal virtual void TryCheckInMessageBody()
		{
			if (_context.Message.Direction().IsInbound() && _context.TrackingModes.RequiresBodyClaimChecking())
			{
				if (_logger.IsDebugEnabled) _logger.Debug("Claiming token and checking in message payload");
				ClaimStore.Instance.Claim(_context.Message, _context.PipelineContext.ResourceTracker);
			}
		}

		/// <summary>
		/// Restore the original business message body's payload stream if it has been saved to disk while being tracked, that is
		/// if current payload is either of the <see cref="Schemas.Xml.Claim.Check"/> or <see cref="Schemas.Xml.Claim.CheckOut"/> token message. Leave
		/// the message body's payload stream unaltered otherwise.
		/// </summary>
		[SuppressMessage("ReSharper", "InvertIf")]
		internal virtual void TryCheckOutMessageBody()
		{
			if (_context.Message.Direction().IsOutbound() && _context.TrackingModes.RequiresBodyClaimChecking())
			{
				if (_logger.IsDebugEnabled) _logger.Debug("Redeeming token and checking out message payload");
				ClaimStore.Instance.Redeem(_context.Message, _context.PipelineContext.ResourceTracker);
			}
		}

		private static readonly ILog _logger = LogManager.GetLogger(typeof(MessageBodyTracker));
		private readonly MicroComponent.ActivityTracker.Context _context;
		private TrackingStream _trackingStream;
	}
}
