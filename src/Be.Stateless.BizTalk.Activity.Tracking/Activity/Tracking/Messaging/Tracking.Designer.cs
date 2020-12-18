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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.BizTalk.Bam.EventObservation;

namespace Be.Stateless.BizTalk.Activity.Tracking.Messaging
{
	[GeneratedCode("BamActivityModel", "2.0.0.1")]
	[Serializable]
	public partial class Process
	{
		public Process(string activityId, EventStream eventStream)
		{
			if (string.IsNullOrEmpty(activityId)) throw new ArgumentNullException(nameof(activityId));
			_activityId = activityId;
			_eventStream = eventStream ?? throw new ArgumentNullException(nameof(eventStream));
		}

		public string ActivityId
		{
			get { return _activityId; }
		}

		public DateTime? BeginTime
		{
			get { return (DateTime?) _activityItems[nameof(BeginTime)]; }
			set { if (value.HasValue) _activityItems[nameof(BeginTime)] = value.Value; }
		}

		public DateTime? EndTime
		{
			get { return (DateTime?) _activityItems[nameof(EndTime)]; }
			set { if (value.HasValue) _activityItems[nameof(EndTime)] = value.Value; }
		}

		public string InterchangeID
		{
			get { return (string) _activityItems[nameof(InterchangeID)]; }
			set { if (value != null) _activityItems[nameof(InterchangeID)] = value; }
		}

		public string ProcessName
		{
			get { return (string) _activityItems[nameof(ProcessName)]; }
			set { if (value != null) _activityItems[nameof(ProcessName)] = value; }
		}

		public string Status
		{
			get { return (string) _activityItems[nameof(Status)]; }
			set { if (value != null) _activityItems[nameof(Status)] = value; }
		}

		public string Value1
		{
			get { return (string) _activityItems[nameof(Value1)]; }
			set { if (value != null) _activityItems[nameof(Value1)] = value; }
		}

		public string Value2
		{
			get { return (string) _activityItems[nameof(Value2)]; }
			set { if (value != null) _activityItems[nameof(Value2)] = value; }
		}

		public string Value3
		{
			get { return (string) _activityItems[nameof(Value3)]; }
			set { if (value != null) _activityItems[nameof(Value3)] = value; }
		}

		/// <summary>
		/// Begins the BAM activity.
		/// </summary>
		public void BeginProcessActivity()
		{
			// Begin the Activity using the passed identifier
			_eventStream.BeginActivity(nameof(Process), _activityId);
		}

		/// <summary>
		/// Write any data changes to the BAM activity. This must be called after any property changes.
		/// </summary>
		public void CommitProcessActivity()
		{
			// We need to provide the key/value pairs to the BAM API
			var al = new List<object>();
			foreach (var kvp in _activityItems)
			{
				al.Add(kvp.Key);
				al.Add(kvp.Value);
			}
			// Update the BAM Activity with all of the data
			_eventStream.UpdateActivity(nameof(Process), _activityId, al.ToArray());
			_eventStream.Flush();
		}

		/// <summary>
		/// End the BAM activity. No more changes will be permitted to this activity except by continuation.
		/// </summary>
		public void EndProcessActivity()
		{
			// End this activity, no more data can be added.
			_eventStream.EndActivity(nameof(Process), _activityId);
		}

		/// <summary>
		/// Add a reference from this activity to another activity.
		/// </summary>
		/// <param name="otherActivityName">The related activity name. Reference names are limited to 128 characters.</param>
		/// <param name="otherActivityId">The related activity Id. Limited to 1024 characters of data.</param>
		public void AddReferenceToAnotherActivity(string otherActivityName, string otherActivityId)
		{
			AddCustomReference("Activity", otherActivityName, otherActivityId);
		}

		/// <summary>
		/// Add a custom reference to this activity, this enables 'data' to be attached to an activity, such as a message body.
		/// </summary>
		/// <param name="referenceType">The related item type. Reference type identifiers are limited to 128 characters.</param>
		/// <param name="referenceName">The related item name. Reference names are limited to 128 characters.</param>
		/// <param name="referenceData">The related item data. Limited to 1024 characters of data.</param>
		public void AddCustomReference(string referenceType, string referenceName, string referenceData)
		{
			// Add a reference to another activity
			_eventStream.AddReference(nameof(Process), _activityId, referenceType, referenceName, referenceData);
		}

		/// <summary>
		/// Add a custom reference to this activity, this enables 'data' to be attached to an activity, such as a message body.
		/// </summary>
		/// <param name="referenceType">The related item type. Reference type identifiers are limited to 128 characters.</param>
		/// <param name="referenceName">The related item name. Reference names are limited to 128 characters.</param>
		/// <param name="referenceData">The related item data. Limited to 1024 characters of data.</param>
		/// <param name="longReferenceData">The related item data containing up to 512 KB of Unicode characters of data.</param>
		public void AddCustomReference(string referenceType, string referenceName, string referenceData, string longReferenceData)
		{
			// Add a reference to another activity
			_eventStream.AddReference(nameof(Process), _activityId, referenceType, referenceName, referenceData, longReferenceData);
		}

		/// <summary>
		/// Activate continuation for this activity. While in the context that is enabling continuation, this activity can
		/// still be updated and MUST be ended with a call to EndProcessActivity().
		/// </summary>
		public string EnableContinuation()
		{
			string continuationId = ContinuationPrefix + _activityId;
			_eventStream.EnableContinuation(nameof(Process), _activityId, continuationId);
			return continuationId;
		}

		/// <summary>
		/// Flush any buffered events.
		/// </summary>
		public void Flush()
		{
			_eventStream.Flush();
		}

		internal const string ContinuationPrefix = "CONT_";

		private readonly string _activityId;
		private readonly Dictionary<string, object> _activityItems = new Dictionary<string, object>();
		private readonly EventStream _eventStream;
	}

	[GeneratedCode("BamActivityModel", "2.0.0.1")]
	[Serializable]
	public partial class ProcessingStep
	{
		public ProcessingStep(string activityId, EventStream eventStream)
		{
			if (string.IsNullOrEmpty(activityId)) throw new ArgumentNullException(nameof(activityId));
			_activityId = activityId;
			_eventStream = eventStream ?? throw new ArgumentNullException(nameof(eventStream));
		}

		public string ActivityId
		{
			get { return _activityId; }
		}

		public DateTime? BeginTime
		{
			get { return (DateTime?) _activityItems[nameof(BeginTime)]; }
			set { if (value.HasValue) _activityItems[nameof(BeginTime)] = value.Value; }
		}

		public DateTime? EndTime
		{
			get { return (DateTime?) _activityItems[nameof(EndTime)]; }
			set { if (value.HasValue) _activityItems[nameof(EndTime)] = value.Value; }
		}

		public string ErrorDescription
		{
			get { return (string) _activityItems[nameof(ErrorDescription)]; }
			set { if (value != null) _activityItems[nameof(ErrorDescription)] = value; }
		}

		public string MachineName
		{
			get { return (string) _activityItems[nameof(MachineName)]; }
			set { if (value != null) _activityItems[nameof(MachineName)] = value; }
		}

		public string ProcessActivityID
		{
			get { return (string) _activityItems[nameof(ProcessActivityID)]; }
			set { if (value != null) _activityItems[nameof(ProcessActivityID)] = value; }
		}

		public string Status
		{
			get { return (string) _activityItems[nameof(Status)]; }
			set { if (value != null) _activityItems[nameof(Status)] = value; }
		}

		public string StepName
		{
			get { return (string) _activityItems[nameof(StepName)]; }
			set { if (value != null) _activityItems[nameof(StepName)] = value; }
		}

		/// <summary>
		/// Begins the BAM activity.
		/// </summary>
		public void BeginProcessingStepActivity()
		{
			// Begin the Activity using the passed identifier
			_eventStream.BeginActivity(nameof(ProcessingStep), _activityId);
		}

		/// <summary>
		/// Write any data changes to the BAM activity. This must be called after any property changes.
		/// </summary>
		public void CommitProcessingStepActivity()
		{
			// We need to provide the key/value pairs to the BAM API
			var al = new List<object>();
			foreach (var kvp in _activityItems)
			{
				al.Add(kvp.Key);
				al.Add(kvp.Value);
			}
			// Update the BAM Activity with all of the data
			_eventStream.UpdateActivity(nameof(ProcessingStep), _activityId, al.ToArray());
			_eventStream.Flush();
		}

		/// <summary>
		/// End the BAM activity. No more changes will be permitted to this activity except by continuation.
		/// </summary>
		public void EndProcessingStepActivity()
		{
			// End this activity, no more data can be added.
			_eventStream.EndActivity(nameof(ProcessingStep), _activityId);
		}

		/// <summary>
		/// Add a reference from this activity to another activity.
		/// </summary>
		/// <param name="otherActivityName">The related activity name. Reference names are limited to 128 characters.</param>
		/// <param name="otherActivityId">The related activity Id. Limited to 1024 characters of data.</param>
		public void AddReferenceToAnotherActivity(string otherActivityName, string otherActivityId)
		{
			AddCustomReference("Activity", otherActivityName, otherActivityId);
		}

		/// <summary>
		/// Add a custom reference to this activity, this enables 'data' to be attached to an activity, such as a message body.
		/// </summary>
		/// <param name="referenceType">The related item type. Reference type identifiers are limited to 128 characters.</param>
		/// <param name="referenceName">The related item name. Reference names are limited to 128 characters.</param>
		/// <param name="referenceData">The related item data. Limited to 1024 characters of data.</param>
		public void AddCustomReference(string referenceType, string referenceName, string referenceData)
		{
			// Add a reference to another activity
			_eventStream.AddReference(nameof(ProcessingStep), _activityId, referenceType, referenceName, referenceData);
		}

		/// <summary>
		/// Add a custom reference to this activity, this enables 'data' to be attached to an activity, such as a message body.
		/// </summary>
		/// <param name="referenceType">The related item type. Reference type identifiers are limited to 128 characters.</param>
		/// <param name="referenceName">The related item name. Reference names are limited to 128 characters.</param>
		/// <param name="referenceData">The related item data. Limited to 1024 characters of data.</param>
		/// <param name="longReferenceData">The related item data containing up to 512 KB of Unicode characters of data.</param>
		public void AddCustomReference(string referenceType, string referenceName, string referenceData, string longReferenceData)
		{
			// Add a reference to another activity
			_eventStream.AddReference(nameof(ProcessingStep), _activityId, referenceType, referenceName, referenceData, longReferenceData);
		}

		/// <summary>
		/// Activate continuation for this activity. While in the context that is enabling continuation, this activity can
		/// still be updated and MUST be ended with a call to EndProcessingStepActivity().
		/// </summary>
		public string EnableContinuation()
		{
			string continuationId = ContinuationPrefix + _activityId;
			_eventStream.EnableContinuation(nameof(ProcessingStep), _activityId, continuationId);
			return continuationId;
		}

		/// <summary>
		/// Flush any buffered events.
		/// </summary>
		public void Flush()
		{
			_eventStream.Flush();
		}

		internal const string ContinuationPrefix = "CONT_";

		private readonly string _activityId;
		private readonly Dictionary<string, object> _activityItems = new Dictionary<string, object>();
		private readonly EventStream _eventStream;
	}

	[GeneratedCode("BamActivityModel", "2.0.0.1")]
	[Serializable]
	public partial class ProcessMessagingStep
	{
		public ProcessMessagingStep(string activityId, EventStream eventStream)
		{
			if (string.IsNullOrEmpty(activityId)) throw new ArgumentNullException(nameof(activityId));
			_activityId = activityId;
			_eventStream = eventStream ?? throw new ArgumentNullException(nameof(eventStream));
		}

		public string ActivityId
		{
			get { return _activityId; }
		}

		public string MessagingStepActivityID
		{
			get { return (string) _activityItems[nameof(MessagingStepActivityID)]; }
			set { if (value != null) _activityItems[nameof(MessagingStepActivityID)] = value; }
		}

		public string MessagingStepStatus
		{
			get { return (string) _activityItems[nameof(MessagingStepStatus)]; }
			set { if (value != null) _activityItems[nameof(MessagingStepStatus)] = value; }
		}

		public string ProcessActivityID
		{
			get { return (string) _activityItems[nameof(ProcessActivityID)]; }
			set { if (value != null) _activityItems[nameof(ProcessActivityID)] = value; }
		}

		/// <summary>
		/// Begins the BAM activity.
		/// </summary>
		public void BeginProcessMessagingStepActivity()
		{
			// Begin the Activity using the passed identifier
			_eventStream.BeginActivity(nameof(ProcessMessagingStep), _activityId);
		}

		/// <summary>
		/// Write any data changes to the BAM activity. This must be called after any property changes.
		/// </summary>
		public void CommitProcessMessagingStepActivity()
		{
			// We need to provide the key/value pairs to the BAM API
			var al = new List<object>();
			foreach (var kvp in _activityItems)
			{
				al.Add(kvp.Key);
				al.Add(kvp.Value);
			}
			// Update the BAM Activity with all of the data
			_eventStream.UpdateActivity(nameof(ProcessMessagingStep), _activityId, al.ToArray());
			_eventStream.Flush();
		}

		/// <summary>
		/// End the BAM activity. No more changes will be permitted to this activity except by continuation.
		/// </summary>
		public void EndProcessMessagingStepActivity()
		{
			// End this activity, no more data can be added.
			_eventStream.EndActivity(nameof(ProcessMessagingStep), _activityId);
		}

		/// <summary>
		/// Add a reference from this activity to another activity.
		/// </summary>
		/// <param name="otherActivityName">The related activity name. Reference names are limited to 128 characters.</param>
		/// <param name="otherActivityId">The related activity Id. Limited to 1024 characters of data.</param>
		public void AddReferenceToAnotherActivity(string otherActivityName, string otherActivityId)
		{
			AddCustomReference("Activity", otherActivityName, otherActivityId);
		}

		/// <summary>
		/// Add a custom reference to this activity, this enables 'data' to be attached to an activity, such as a message body.
		/// </summary>
		/// <param name="referenceType">The related item type. Reference type identifiers are limited to 128 characters.</param>
		/// <param name="referenceName">The related item name. Reference names are limited to 128 characters.</param>
		/// <param name="referenceData">The related item data. Limited to 1024 characters of data.</param>
		public void AddCustomReference(string referenceType, string referenceName, string referenceData)
		{
			// Add a reference to another activity
			_eventStream.AddReference(nameof(ProcessMessagingStep), _activityId, referenceType, referenceName, referenceData);
		}

		/// <summary>
		/// Add a custom reference to this activity, this enables 'data' to be attached to an activity, such as a message body.
		/// </summary>
		/// <param name="referenceType">The related item type. Reference type identifiers are limited to 128 characters.</param>
		/// <param name="referenceName">The related item name. Reference names are limited to 128 characters.</param>
		/// <param name="referenceData">The related item data. Limited to 1024 characters of data.</param>
		/// <param name="longReferenceData">The related item data containing up to 512 KB of Unicode characters of data.</param>
		public void AddCustomReference(string referenceType, string referenceName, string referenceData, string longReferenceData)
		{
			// Add a reference to another activity
			_eventStream.AddReference(nameof(ProcessMessagingStep), _activityId, referenceType, referenceName, referenceData, longReferenceData);
		}

		/// <summary>
		/// Activate continuation for this activity. While in the context that is enabling continuation, this activity can
		/// still be updated and MUST be ended with a call to EndProcessMessagingStepActivity().
		/// </summary>
		public string EnableContinuation()
		{
			string continuationId = ContinuationPrefix + _activityId;
			_eventStream.EnableContinuation(nameof(ProcessMessagingStep), _activityId, continuationId);
			return continuationId;
		}

		/// <summary>
		/// Flush any buffered events.
		/// </summary>
		public void Flush()
		{
			_eventStream.Flush();
		}

		internal const string ContinuationPrefix = "CONT_";

		private readonly string _activityId;
		private readonly Dictionary<string, object> _activityItems = new Dictionary<string, object>();
		private readonly EventStream _eventStream;
	}

	[GeneratedCode("BamActivityModel", "2.0.0.1")]
	[Serializable]
	public partial class MessagingStep
	{
		public MessagingStep(string activityId, EventStream eventStream)
		{
			if (string.IsNullOrEmpty(activityId)) throw new ArgumentNullException(nameof(activityId));
			_activityId = activityId;
			_eventStream = eventStream ?? throw new ArgumentNullException(nameof(eventStream));
		}

		public string ActivityId
		{
			get { return _activityId; }
		}

		public string ErrorCode
		{
			get { return (string) _activityItems[nameof(ErrorCode)]; }
			set { if (value != null) _activityItems[nameof(ErrorCode)] = value; }
		}

		public string ErrorDescription
		{
			get { return (string) _activityItems[nameof(ErrorDescription)]; }
			set { if (value != null) _activityItems[nameof(ErrorDescription)] = value; }
		}

		public string InterchangeID
		{
			get { return (string) _activityItems[nameof(InterchangeID)]; }
			set { if (value != null) _activityItems[nameof(InterchangeID)] = value; }
		}

		public string MachineName
		{
			get { return (string) _activityItems[nameof(MachineName)]; }
			set { if (value != null) _activityItems[nameof(MachineName)] = value; }
		}

		public string MessageID
		{
			get { return (string) _activityItems[nameof(MessageID)]; }
			set { if (value != null) _activityItems[nameof(MessageID)] = value; }
		}

		public int? MessageSize
		{
			get { return (int?) _activityItems[nameof(MessageSize)]; }
			set { if (value.HasValue) _activityItems[nameof(MessageSize)] = value.Value; }
		}

		public string MessageType
		{
			get { return (string) _activityItems[nameof(MessageType)]; }
			set { if (value != null) _activityItems[nameof(MessageType)] = value; }
		}

		public string PortName
		{
			get { return (string) _activityItems[nameof(PortName)]; }
			set { if (value != null) _activityItems[nameof(PortName)] = value; }
		}

		public int? RetryCount
		{
			get { return (int?) _activityItems[nameof(RetryCount)]; }
			set { if (value.HasValue) _activityItems[nameof(RetryCount)] = value.Value; }
		}

		public string Status
		{
			get { return (string) _activityItems[nameof(Status)]; }
			set { if (value != null) _activityItems[nameof(Status)] = value; }
		}

		public DateTime? Time
		{
			get { return (DateTime?) _activityItems[nameof(Time)]; }
			set { if (value.HasValue) _activityItems[nameof(Time)] = value.Value; }
		}

		public string TransportLocation
		{
			get { return (string) _activityItems[nameof(TransportLocation)]; }
			set { if (value != null) _activityItems[nameof(TransportLocation)] = value; }
		}

		public string TransportType
		{
			get { return (string) _activityItems[nameof(TransportType)]; }
			set { if (value != null) _activityItems[nameof(TransportType)] = value; }
		}

		public string Value1
		{
			get { return (string) _activityItems[nameof(Value1)]; }
			set { if (value != null) _activityItems[nameof(Value1)] = value; }
		}

		public string Value2
		{
			get { return (string) _activityItems[nameof(Value2)]; }
			set { if (value != null) _activityItems[nameof(Value2)] = value; }
		}

		public string Value3
		{
			get { return (string) _activityItems[nameof(Value3)]; }
			set { if (value != null) _activityItems[nameof(Value3)] = value; }
		}

		/// <summary>
		/// Begins the BAM activity.
		/// </summary>
		public void BeginMessagingStepActivity()
		{
			// Begin the Activity using the passed identifier
			_eventStream.BeginActivity(nameof(MessagingStep), _activityId);
		}

		/// <summary>
		/// Write any data changes to the BAM activity. This must be called after any property changes.
		/// </summary>
		public void CommitMessagingStepActivity()
		{
			// We need to provide the key/value pairs to the BAM API
			var al = new List<object>();
			foreach (var kvp in _activityItems)
			{
				al.Add(kvp.Key);
				al.Add(kvp.Value);
			}
			// Update the BAM Activity with all of the data
			_eventStream.UpdateActivity(nameof(MessagingStep), _activityId, al.ToArray());
			_eventStream.Flush();
		}

		/// <summary>
		/// End the BAM activity. No more changes will be permitted to this activity except by continuation.
		/// </summary>
		public void EndMessagingStepActivity()
		{
			// End this activity, no more data can be added.
			_eventStream.EndActivity(nameof(MessagingStep), _activityId);
		}

		/// <summary>
		/// Add a reference from this activity to another activity.
		/// </summary>
		/// <param name="otherActivityName">The related activity name. Reference names are limited to 128 characters.</param>
		/// <param name="otherActivityId">The related activity Id. Limited to 1024 characters of data.</param>
		public void AddReferenceToAnotherActivity(string otherActivityName, string otherActivityId)
		{
			AddCustomReference("Activity", otherActivityName, otherActivityId);
		}

		/// <summary>
		/// Add a custom reference to this activity, this enables 'data' to be attached to an activity, such as a message body.
		/// </summary>
		/// <param name="referenceType">The related item type. Reference type identifiers are limited to 128 characters.</param>
		/// <param name="referenceName">The related item name. Reference names are limited to 128 characters.</param>
		/// <param name="referenceData">The related item data. Limited to 1024 characters of data.</param>
		public void AddCustomReference(string referenceType, string referenceName, string referenceData)
		{
			// Add a reference to another activity
			_eventStream.AddReference(nameof(MessagingStep), _activityId, referenceType, referenceName, referenceData);
		}

		/// <summary>
		/// Add a custom reference to this activity, this enables 'data' to be attached to an activity, such as a message body.
		/// </summary>
		/// <param name="referenceType">The related item type. Reference type identifiers are limited to 128 characters.</param>
		/// <param name="referenceName">The related item name. Reference names are limited to 128 characters.</param>
		/// <param name="referenceData">The related item data. Limited to 1024 characters of data.</param>
		/// <param name="longReferenceData">The related item data containing up to 512 KB of Unicode characters of data.</param>
		public void AddCustomReference(string referenceType, string referenceName, string referenceData, string longReferenceData)
		{
			// Add a reference to another activity
			_eventStream.AddReference(nameof(MessagingStep), _activityId, referenceType, referenceName, referenceData, longReferenceData);
		}

		/// <summary>
		/// Activate continuation for this activity. While in the context that is enabling continuation, this activity can
		/// still be updated and MUST be ended with a call to EndMessagingStepActivity().
		/// </summary>
		public string EnableContinuation()
		{
			string continuationId = ContinuationPrefix + _activityId;
			_eventStream.EnableContinuation(nameof(MessagingStep), _activityId, continuationId);
			return continuationId;
		}

		/// <summary>
		/// Flush any buffered events.
		/// </summary>
		public void Flush()
		{
			_eventStream.Flush();
		}

		internal const string ContinuationPrefix = "CONT_";

		private readonly string _activityId;
		private readonly Dictionary<string, object> _activityItems = new Dictionary<string, object>();
		private readonly EventStream _eventStream;
	}

}