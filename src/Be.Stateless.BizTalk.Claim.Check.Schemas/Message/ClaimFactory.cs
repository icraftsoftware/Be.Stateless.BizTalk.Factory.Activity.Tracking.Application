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
using System.Xml;
using Be.Stateless.BizTalk.Schema;
using Be.Stateless.BizTalk.Schemas.Xml;
using Be.Stateless.Extensions;

namespace Be.Stateless.BizTalk.Message
{
	[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
	public static class ClaimFactory
	{
		/// <summary>
		/// Creates a <see cref="Claim.Check"/> message with a given <paramref name="url"/> claim.
		/// </summary>
		/// <param name="url">
		/// The URL to some payload that will later on be checked out.
		/// </param>
		/// <returns>
		/// The <see cref="Claim.Check"/> message as an <see cref="XmlDocument"/>.
		/// </returns>
		public static XmlDocument CreateCheck(string url)
		{
			if (url.IsNullOrEmpty()) throw new ArgumentNullException(nameof(url));
			var message = MessageBodyFactory.Create<Claim.Check>(
				$"<clm:Check xmlns:clm='{ClaimSchemaTargetNamespace}'>"
				+ $"<clm:Url>{url}</clm:Url>"
				+ "</clm:Check>");
			return message;
		}

		/// <summary>
		/// Creates a <see cref="Claim.Check"/> message with a given <paramref name="messageType"/> and <paramref name="url"/>
		/// claim.
		/// </summary>
		/// <param name="messageType">
		/// The actual message type of the payload being claimed.
		/// </param>
		/// <param name="url">
		/// The URL to some payload that will later on be checked out.
		/// </param>
		/// <returns>
		/// The <see cref="Claim.Check"/> message as an <see cref="XmlDocument"/>.
		/// </returns>
		public static XmlDocument CreateCheck(string messageType, string url)
		{
			if (url.IsNullOrEmpty()) throw new ArgumentNullException(nameof(url));
			if (messageType.IsNullOrEmpty()) return CreateCheck(url);
			var message = MessageBodyFactory.Create<Claim.Check>(
				$"<clm:Check xmlns:clm='{ClaimSchemaTargetNamespace}'>"
				+ $"<clm:MessageType>{messageType}</clm:MessageType>"
				+ $"<clm:Url>{url}</clm:Url>"
				+ "</clm:Check>");
			return message;
		}

		/// <summary>
		/// Creates a <see cref="Claim.CheckIn"/> message with a given <paramref name="url"/> claim.
		/// </summary>
		/// <param name="url">
		/// The URL to some payload that will later on be checked out.
		/// </param>
		/// <returns>
		/// The <see cref="Claim.CheckIn"/> message as an <see cref="XmlDocument"/>.
		/// </returns>
		public static XmlDocument CreateCheckIn(string url)
		{
			if (url.IsNullOrEmpty()) throw new ArgumentNullException(nameof(url));
			var message = MessageBodyFactory.Create<Claim.CheckIn>(
				$"<clm:CheckIn xmlns:clm='{ClaimSchemaTargetNamespace}'>"
				+ $"<clm:Url>{url}</clm:Url>"
				+ "</clm:CheckIn>");
			return message;
		}

		/// <summary>
		/// Creates a <see cref="Claim.CheckIn"/> message with a given <paramref name="messageType"/> and <paramref name="url"/>
		/// claim.
		/// </summary>
		/// <param name="messageType">
		/// The actual message type of the payload being claimed.
		/// </param>
		/// <param name="url">
		/// The URL to some payload that will later on be checked out.
		/// </param>
		/// <returns>
		/// The <see cref="Claim.CheckIn"/> message as an <see cref="XmlDocument"/>.
		/// </returns>
		public static XmlDocument CreateCheckIn(string messageType, string url)
		{
			if (url.IsNullOrEmpty()) throw new ArgumentNullException(nameof(url));
			if (messageType.IsNullOrEmpty()) return CreateCheckIn(url);
			var message = MessageBodyFactory.Create<Claim.CheckIn>(
				$"<clm:CheckIn xmlns:clm='{ClaimSchemaTargetNamespace}'>"
				+ $"<clm:MessageType>{messageType}</clm:MessageType>"
				+ $"<clm:Url>{url}</clm:Url>"
				+ "</clm:CheckIn>");
			return message;
		}

		/// <summary>
		/// Creates a <see cref="Claim.CheckOut"/> message with a given <paramref name="url"/> claim.
		/// </summary>
		/// <param name="url">
		/// The URL to some payload that is to be checked out.
		/// </param>
		/// <returns>
		/// The <see cref="Claim.CheckOut"/> message as an <see cref="XmlDocument"/>.
		/// </returns>
		public static XmlDocument CreateCheckOut(string url)
		{
			if (url.IsNullOrEmpty()) throw new ArgumentNullException(nameof(url));
			var message = MessageBodyFactory.Create<Claim.CheckOut>(
				$"<clm:CheckOut xmlns:clm='{ClaimSchemaTargetNamespace}'>"
				+ $"<clm:Url>{url}</clm:Url>"
				+ "</clm:CheckOut>");
			return message;
		}

		private static string ClaimSchemaTargetNamespace { get; } = SchemaMetadata.For<Claim.Check>().TargetNamespace;
	}
}
