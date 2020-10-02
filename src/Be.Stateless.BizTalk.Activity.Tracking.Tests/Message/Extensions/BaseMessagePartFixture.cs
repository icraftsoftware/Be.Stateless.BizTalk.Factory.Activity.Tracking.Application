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

using System.Xml.Schema;
using Be.Stateless.IO;
using Be.Stateless.Xml.Extensions;
using FluentAssertions;
using Microsoft.BizTalk.Message.Interop;
using Moq;
using Xunit;
using static Be.Stateless.DelegateFactory;

namespace Be.Stateless.BizTalk.Message.Extensions
{
	public class BaseMessagePartFixture
	{
		[Fact]
		public void AsMessageBodyCaptureDescriptor()
		{
			const string expected = "claim";
			var originalStream = ClaimFactory.CreateCheckOut(expected).AsStream();
			var part = new Mock<IBaseMessagePart>();
			part.Setup(p => p.GetOriginalDataStream())
				.Returns(originalStream);

			var descriptor = part.Object.AsMessageBodyCaptureDescriptor();

			descriptor.Data.Should().Be(expected);
		}

		[Fact]
		public void AsMessageBodyCaptureDescriptorThrowsWhenInvalidClaimTokenMessage()
		{
			var originalStream = new StringStream("<ns0:CheckOut xmlns:ns0='urn:schemas.stateless.be:biztalk:claim:2017:04'></ns0:CheckOut>");
			var part = new Mock<IBaseMessagePart>();
			part.Setup(p => p.GetOriginalDataStream())
				.Returns(originalStream);

			Action(() => part.Object.AsMessageBodyCaptureDescriptor()).Should().Throw<XmlSchemaValidationException>();
		}
	}
}
