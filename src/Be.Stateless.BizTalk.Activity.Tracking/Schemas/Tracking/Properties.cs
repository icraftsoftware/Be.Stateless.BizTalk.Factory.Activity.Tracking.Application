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
using System.Xml;
using Microsoft.XLANGs.BaseTypes;

namespace Be.Stateless.BizTalk.Schemas.Tracking
{
	[Serializable]
	[PropertyType(nameof(ProcessName), TrackingPropertySchema.NAMESPACE, "string", "System.String")]
	[PropertyGuid("1636fd4e-0d55-40ff-b6ef-54602e177df6")]
	public sealed class ProcessName : MessageContextPropertyBase
	{
		#region Base Class Member Overrides

		public override XmlQualifiedName Name => _qualifiedName;

		public override Type Type => typeof(string);

		#endregion

		[NonSerialized]
		private static readonly XmlQualifiedName _qualifiedName = new XmlQualifiedName(nameof(ProcessName), TrackingPropertySchema.NAMESPACE);
	}

	[Serializable]
	[PropertyType(nameof(ProcessActivityId), TrackingPropertySchema.NAMESPACE, "string", "System.String")]
	[PropertyGuid("f784b644-d4b5-4842-8112-ee67974c49e4")]
	public sealed class ProcessActivityId : MessageContextPropertyBase
	{
		#region Base Class Member Overrides

		public override XmlQualifiedName Name => _qualifiedName;

		public override Type Type => typeof(string);

		#endregion

		[NonSerialized]
		private static readonly XmlQualifiedName _qualifiedName = new XmlQualifiedName(nameof(ProcessActivityId), TrackingPropertySchema.NAMESPACE);
	}

	[Serializable]
	[PropertyType(nameof(ProcessingStepActivityId), TrackingPropertySchema.NAMESPACE, "string", "System.String")]
	[PropertyGuid("140f64b4-7a8a-45a0-ac82-21b03fcc12c6")]
	public sealed class ProcessingStepActivityId : MessageContextPropertyBase
	{
		#region Base Class Member Overrides

		public override XmlQualifiedName Name => _qualifiedName;

		public override Type Type => typeof(string);

		#endregion

		[NonSerialized]
		private static readonly XmlQualifiedName _qualifiedName = new XmlQualifiedName(nameof(ProcessingStepActivityId), TrackingPropertySchema.NAMESPACE);
	}

	[Serializable]
	[PropertyType(nameof(MessagingStepActivityId), TrackingPropertySchema.NAMESPACE, "string", "System.String")]
	[PropertyGuid("3730292c-05c1-418d-83ea-7aba201fb7f0")]
	public sealed class MessagingStepActivityId : MessageContextPropertyBase
	{
		#region Base Class Member Overrides

		public override XmlQualifiedName Name => _qualifiedName;

		public override Type Type => typeof(string);

		#endregion

		[NonSerialized]
		private static readonly XmlQualifiedName _qualifiedName = new XmlQualifiedName(nameof(MessagingStepActivityId), TrackingPropertySchema.NAMESPACE);
	}

	/// <summary>
	/// Placeholder for any business key value that needs to be monitored alongside a process.
	/// </summary>
	[Serializable]
	[PropertyType(nameof(Value1), TrackingPropertySchema.NAMESPACE, "string", "System.String")]
	[PropertyGuid("d9ad6c9f-c626-4d45-9080-09394d87a1c9")]
	public sealed class Value1 : MessageContextPropertyBase
	{
		#region Base Class Member Overrides

		public override XmlQualifiedName Name => _qualifiedName;

		public override Type Type => typeof(string);

		#endregion

		[NonSerialized]
		private static readonly XmlQualifiedName _qualifiedName = new XmlQualifiedName(nameof(Value1), TrackingPropertySchema.NAMESPACE);
	}

	/// <summary>
	/// Placeholder for any business key value that needs to be monitored alongside a process.
	/// </summary>
	[Serializable]
	[PropertyType(nameof(Value2), TrackingPropertySchema.NAMESPACE, "string", "System.String")]
	[PropertyGuid("e472767a-2c62-4d91-8939-7e0578877348")]
	public sealed class Value2 : MessageContextPropertyBase
	{
		#region Base Class Member Overrides

		public override XmlQualifiedName Name => _qualifiedName;

		public override Type Type => typeof(string);

		#endregion

		[NonSerialized]
		private static readonly XmlQualifiedName _qualifiedName = new XmlQualifiedName(nameof(Value2), TrackingPropertySchema.NAMESPACE);
	}

	/// <summary>
	/// Placeholder for any business key value that needs to be monitored alongside a process.
	/// </summary>
	[Serializable]
	[PropertyType(nameof(Value3), TrackingPropertySchema.NAMESPACE, "string", "System.String")]
	[PropertyGuid("d077de25-55bf-4eb9-a776-ea851f8e1592")]
	public sealed class Value3 : MessageContextPropertyBase
	{
		#region Base Class Member Overrides

		public override XmlQualifiedName Name => _qualifiedName;

		public override Type Type => typeof(string);

		#endregion

		[NonSerialized]
		private static readonly XmlQualifiedName _qualifiedName = new XmlQualifiedName(nameof(Value3), TrackingPropertySchema.NAMESPACE);
	}

	internal static class TrackingPropertySchema
	{
		public const string NAMESPACE = "urn:schemas.stateless.be:biztalk:properties:tracking:2012:04";
	}
}
