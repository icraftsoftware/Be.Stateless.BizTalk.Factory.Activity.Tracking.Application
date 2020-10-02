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
using System.Xml;
using System.Xml.Schema;
using Be.Stateless.BizTalk.Activity.Tracking;
using Be.Stateless.BizTalk.Schema;
using Be.Stateless.BizTalk.Schemas.Xml;
using Be.Stateless.BizTalk.Xml;
using Be.Stateless.Extensions;
using Be.Stateless.Xml.Extensions;
using Microsoft.BizTalk.Message.Interop;

namespace Be.Stateless.BizTalk.Message.Extensions
{
	public static class BaseMessagePartTrackingExtensions
	{
		/// <summary>
		/// Return the content of a claim token message, either <see cref="Claim.Check"/>, <see cref="Claim.CheckIn"/>, or <see
		/// cref="Claim.CheckOut"/>, as a <see cref="MessageBodyCaptureDescriptor"/> filled in according to the content of the
		/// claim token.
		/// </summary>
		/// <param name="messagePart">
		/// The part whose stream contains a claim token.
		/// </param>
		/// <returns>
		/// The <see cref="MessageBodyCaptureDescriptor"/> corresponding to the content of the claim token.
		/// </returns>
		internal static MessageBodyCaptureDescriptor AsMessageBodyCaptureDescriptor(this IBaseMessagePart messagePart)
		{
			// Claim.Check, Claim.CheckIn, and Claim.CheckOut are all in the same XML Schema: any one can be used to
			// reference the XML Schema to use to validate a token message, whatever its specific type
			using (var reader = ValidatingXmlReader.Create<Claim.CheckOut>(messagePart.GetOriginalDataStream(), XmlSchemaContentProcessing.Lax))
			{
				return reader.AsMessageBodyCaptureDescriptor();
			}
		}

		private static MessageBodyCaptureDescriptor AsMessageBodyCaptureDescriptor(this XmlReader reader)
		{
			var document = new XmlDocument();
			document.Load(reader);
			// Claim.Check, Claim.CheckIn, and Claim.CheckOut are all in the same XML Schema: any one can be used to
			// reference the XML Schema TargetNamespace, whatever its specific type
			var nsm = document.GetNamespaceManager();
			nsm.AddNamespace("s0", SchemaMetadata.For<Claim.CheckOut>().TargetNamespace);
			// extract url from claim token
			var urlNode = document.SelectSingleNode("/*/s0:Url", nsm);
			if (urlNode == null) throw new ArgumentException($"{document.DocumentElement.IfNotNull(de => de.Name)} token message has no Url element.");
			return new(urlNode.InnerText, MessageBodyCaptureMode.Claimed);
		}
	}
}
