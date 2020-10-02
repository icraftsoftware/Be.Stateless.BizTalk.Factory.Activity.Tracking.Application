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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using Be.Stateless.BizTalk.Activity.Tracking.Extensions;
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.BizTalk.Message.Extensions;
using Be.Stateless.Extensions;
using Be.Stateless.IO.Extensions;
using BizTalk.Factory.XLang.Extensions;
using Microsoft.XLANGs.BaseTypes;

namespace Be.Stateless.BizTalk.Activity.Tracking.Processing
{
	public partial class MessagingStep
	{
		[SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "Optional parameters are not supported in XLang.")]
		public static void TrackDirectReceive(TrackingContext trackingContext, XLANGMessage message)
		{
			TrackDirectReceive(trackingContext, message, false, false);
		}

		[SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "Optional parameters are not supported in XLang.")]
		public static void TrackDirectReceive(TrackingContext trackingContext, XLANGMessage message, bool trackMessageBody)
		{
			TrackDirectReceive(trackingContext, message, trackMessageBody, false);
		}

		public static void TrackDirectReceive(TrackingContext trackingContext, XLANGMessage message, bool trackMessageBody, bool skipMessageContextTracking)
		{
			var messagingStepTracking = new MessagingStep(Tracking.ActivityId.NewActivityId(), message);
			messagingStepTracking.BeginMessagingStepActivity();
			if (trackMessageBody) messagingStepTracking.TrackMessageBody();
			if (!skipMessageContextTracking) messagingStepTracking.TrackMessageContext();
			messagingStepTracking.TrackStep(TrackingStatus.Received);
			messagingStepTracking.CommitMessagingStepActivity();
			messagingStepTracking.EndMessagingStepActivity();

			new Process(trackingContext.ProcessActivityId).AddStep(messagingStepTracking);
		}

		[SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "Optional parameters are not supported in XLang.")]
		public static void TrackDirectSend(TrackingContext trackingContext, XLANGMessage message)
		{
			TrackDirectSend(trackingContext, message, false, false);
		}

		[SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "Optional parameters are not supported in XLang.")]
		public static void TrackDirectSend(TrackingContext trackingContext, XLANGMessage message, bool trackMessageBody)
		{
			TrackDirectSend(trackingContext, message, trackMessageBody, false);
		}

		public static void TrackDirectSend(TrackingContext trackingContext, XLANGMessage message, bool trackMessageBody, bool skipMessageContextTracking)
		{
			var messagingStepTracking = new MessagingStep(Tracking.ActivityId.NewActivityId(), message);
			messagingStepTracking.BeginMessagingStepActivity();
			if (trackMessageBody) messagingStepTracking.TrackMessageBody();
			if (!skipMessageContextTracking) messagingStepTracking.TrackMessageContext();
			messagingStepTracking.TrackStep(TrackingStatus.Sent);
			messagingStepTracking.CommitMessagingStepActivity();
			messagingStepTracking.EndMessagingStepActivity();

			trackingContext.MessagingStepActivityId = messagingStepTracking.ActivityId;
			trackingContext.Apply(message);

			new Process(trackingContext.ProcessActivityId).AddStep(messagingStepTracking);
		}

		internal MessagingStep(XLANGMessage message) : this(message.GetTrackingContext().MessagingStepActivityId, message) { }

		internal MessagingStep(string activityId, XLANGMessage message) : this(activityId)
		{
			Message = message ?? throw new ArgumentNullException(nameof(message));
		}

		internal XLANGMessage Message { get; }

		private void TrackMessageBody()
		{
			if (Message[0] != null)
				AddCustomReference(
					MessageBodyCaptureMode.Unclaimed.ToString(),
					Messaging.MessagingStep.MESSAGE_BODY_REFERENCE_NAME,
					// timestamp because it is mandatory to use a non-null reference data
					DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
					Message[0].AsStream().CompressToBase64String());
		}

		private void TrackMessageContext()
		{
			AddCustomReference(
				Messaging.MessagingStep.MESSAGE_CONTEXT_REFERENCE_TYPE,
				Messaging.MessagingStep.MESSAGE_CONTEXT_REFERENCE_NAME,
				// timestamp because it is mandatory to use a non-null reference data
				DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
				Message.GetContext().ToXml());
		}

		private void TrackStep(string trackingStatus)
		{
			Message.GetProperty(BtsProperties.InterchangeID).IfNotNull(id => InterchangeID = id.AsNormalizedActivityId());
			Message.GetProperty(BtsProperties.MessageID).IfNotNull(id => MessageID = id.AsNormalizedActivityId());
			MachineName = Environment.MachineName;
			// http://social.msdn.microsoft.com/Forums/en-US/biztalkgeneral/thread/01f66ad3-7116-4051-a2dd-0cc88a926546/
			try
			{
				MessageType = Message.GetProperty(BtsProperties.MessageType);
			}
			catch (XmlException) { }
			Status = trackingStatus;
			// pointless as will be affiliated to process anyway: PortName = ProcessTracking.GetCurrentProcessName();
			Time = DateTime.UtcNow;
			TransportLocation = "MessageBox";
			TransportType = "Direct";
			Value1 = Message.GetProperty(TrackingProperties.Value1);
			Value2 = Message.GetProperty(TrackingProperties.Value2);
			Value3 = Message.GetProperty(TrackingProperties.Value3);
		}
	}
}
