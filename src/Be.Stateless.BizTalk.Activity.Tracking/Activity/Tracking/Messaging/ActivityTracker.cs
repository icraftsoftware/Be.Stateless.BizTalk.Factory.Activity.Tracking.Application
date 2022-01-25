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

using System;
using System.Diagnostics.CodeAnalysis;
using Be.Stateless.BizTalk.Activity.Tracking.Extensions;
using Be.Stateless.BizTalk.Component.Extensions;
using Be.Stateless.BizTalk.Message.Extensions;
using Be.Stateless.BizTalk.Stream;
using Be.Stateless.Extensions;
using log4net;
using Microsoft.BizTalk.Message.Interop;

namespace Be.Stateless.BizTalk.Activity.Tracking.Messaging
{
	[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "Required for unit testing purposes.")]
	internal class ActivityTracker
	{
		public static ActivityTracker Create(MicroComponent.ActivityTracker.Context context)
		{
			return Factory(context);
		}

		#region Mock's Factory Hook Point

		internal static Func<MicroComponent.ActivityTracker.Context, ActivityTracker> Factory { get; set; } = context => new(context);

		#endregion

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Protected for mocking purposes.")]
		protected ActivityTracker(MicroComponent.ActivityTracker.Context context)
		{
			_context = context;
		}

		internal virtual void TrackActivity()
		{
			var trackingStream = (TrackingStream) _context.Message.BodyPart.GetOriginalDataStream();
			// generate ActivityIds and push them in context ASAP so they get a better chance of being propagated by
			// pipeline components, and noticeably dis/assembler components
			Initiate(_context.PipelineContext.GetActivityFactory(), _context.Message, _context.ProcessNameResolver.ResolveProcessName());
			// capture tracking information as late as possible to get as much out of the context as possible. passing
			// trackingStream to Complete() method is essential, as GetOriginalDataStream() might not provide the right
			// TrackingStream instance should it have been replaced or wrapped by some other stream(s) along the pipeline
			trackingStream.AfterLastReadEvent += (_, _) => Complete(_context.TrackingModes, trackingStream);
		}

		private void Initiate(IActivityFactory activityFactory, IBaseMessage message, string processName)
		{
			var previousTrackingContext = message.GetTrackingContext();

			// an outbound message without an ongoing process --i.e. that has no process affiliation-- denotes a
			// publish/subscribe, or messaging-only process and the whole process needs to be tracked; that means the
			// initiating messaging step, whose MessagingStepActivityId is still conveyed by the ambient
			// previousTrackingContext, as well as the currently ongoing messaging step, which completes the process.
			var isMessagingProcess = message.Direction().IsOutbound()
				&& !previousTrackingContext.HasProcessAffiliation()
				&& !previousTrackingContext.MessagingStepActivityId.IsNullOrEmpty();

			if (_logger.IsDebugEnabled) _logger.Debug($"Initiating tracking of a messaging {(isMessagingProcess ? "process" : "step")}.");

			_process = isMessagingProcess
				? activityFactory.CreateProcess(message, processName)
				: previousTrackingContext.HasProcessAffiliation()
					? activityFactory.FindProcess(previousTrackingContext)
					: null;

			_previousMessagingStep = isMessagingProcess
				? activityFactory.FindMessagingStep(previousTrackingContext)
				: null;

			_messagingStep = activityFactory.CreateMessagingStep(message);
		}

		private void Complete(ActivityTrackingModes trackingModes, TrackingStream trackingStream)
		{
			var isMessagingProcess = _previousMessagingStep != null;
			if (_logger.IsDebugEnabled) _logger.Debug($"Completing tracking of a messaging {(isMessagingProcess ? "process" : "step")}.");

			// track the whole wealth of messaging activity, it might be a complete process or a single messaging step
			if (isMessagingProcess)
			{
				_process.TrackActivity();
				_process.AddStep(_previousMessagingStep);
			}
			_messagingStep.TrackActivity(trackingModes, trackingStream);
			_process.IfNotNull(p => p.AddStep(_messagingStep));
		}

		private static readonly ILog _logger = LogManager.GetLogger(typeof(ActivityTracker));
		private readonly MicroComponent.ActivityTracker.Context _context;
		private MessagingStep _messagingStep;
		private MessagingStep _previousMessagingStep;
		private Process _process;
	}
}
