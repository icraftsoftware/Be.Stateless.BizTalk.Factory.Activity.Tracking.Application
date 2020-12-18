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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.BizTalk.Message.Extensions;
using Be.Stateless.BizTalk.Schemas.Sql.Procedures.Claim;
using Be.Stateless.BizTalk.Unit;
using Be.Stateless.BizTalk.Unit.Transform;
using Be.Stateless.Resources;
using FluentAssertions;
using Xunit;

namespace Be.Stateless.BizTalk.Maps.ToSql.Procedures.Claim
{
	public class ClaimToCheckInFixture : TransformFixture<ClaimToCheckIn>
	{
		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public void ValidateTransformClaimTokenWithContext()
		{
			var contextMock = new MessageContextMock();
			contextMock
				.Setup(c => c.GetProperty(BizTalkFactoryProperties.MessageType))
				.Returns("context-claimed-message-type");
			contextMock
				.Setup(c => c.GetProperty(BizTalkFactoryProperties.CorrelationId))
				.Returns("context-correlation-token");
			contextMock
				.Setup(c => c.GetProperty(BizTalkFactoryProperties.EnvironmentTag))
				.Returns("context-environment-tag");

			using (var stream = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.BizTalk.Resources.Message.ClaimToken.1.xml"))
			{
				var setup = Given(input => input.Message(stream).Context(contextMock.Object))
					.Transform
					.OutputsXml(output => output.ConformingTo<CheckIn>().WithStrictConformanceLevel());
				var result = setup.Validate();
				result.Select("//usp:url/text()").Should().HaveCount(1);
				result.SelectSingleNode("//usp:messageType/text()").Value.Should().Be("context-claimed-message-type");
				result.SelectSingleNode("//usp:correlationId/text()").Value.Should().Be("context-correlation-token");
				result.SelectSingleNode("//usp:environmentTag/text()").Value.Should().Be("context-environment-tag");
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public void ValidateTransformClaimTokenWithEmbeddedDataAndContext()
		{
			var contextMock = new MessageContextMock();
			contextMock
				.Setup(c => c.GetProperty(BizTalkFactoryProperties.MessageType))
				.Returns("context-claimed-message-type");
			contextMock
				.Setup(c => c.GetProperty(BizTalkFactoryProperties.CorrelationId))
				.Returns("context-correlation-token");
			contextMock
				.Setup(c => c.GetProperty(BizTalkFactoryProperties.EnvironmentTag))
				.Returns("context-environment-tag");
			contextMock
				.Setup(c => c.GetProperty(BizTalkFactoryProperties.ReceiverName))
				.Returns("context-receiver-name");

			using (var stream = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.BizTalk.Resources.Message.ClaimToken.3.xml"))
			{
				var setup = Given(input => input.Message(stream).Context(contextMock.Object))
					.Transform
					.OutputsXml(output => output.ConformingTo<CheckIn>().WithStrictConformanceLevel());
				var result = setup.Validate();
				result.Select("//usp:url/text()").Should().HaveCount(1);
				result.SelectSingleNode("//usp:correlationId/text()").Value.Should().Be("embedded-correlation-token");
				result.SelectSingleNode("//usp:environmentTag/text()").Value.Should().Be("embedded-environment-tag");
				result.SelectSingleNode("//usp:messageType/text()").Value.Should().Be("embedded-claimed-message-type");
				result.SelectSingleNode("//usp:receiverName/text()").Value.Should().Be("context-receiver-name");
				result.SelectSingleNode("//usp:senderName/text()").Value.Should().Be("embedded-sender-name");
				result.SelectSingleNode("//usp:any/text()").Value
					.Should().Be("<parent><child>one</child><child>two</child></parent><parent><child>six</child><child>ten</child></parent>");
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public void ValidateTransformComplexClaimToken()
		{
			using (var stream = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.BizTalk.Resources.Message.ClaimToken.2.xml"))
			{
				var setup = Given(input => input.Message(stream).Context(new MessageContextMock().Object))
					.Transform
					.OutputsXml(output => output.ConformingTo<CheckIn>().WithStrictConformanceLevel());
				var result = setup.Validate();
				result.Select("//usp:url/text()").Should().HaveCount(1);
				result.Select("//usp:any").Should().HaveCount(1);
				result.SelectSingleNode("//usp:any/text()").Value
					.Should().Be("<parent><child>one</child><child>two</child></parent><parent><child>six</child><child>ten</child></parent>");
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public void ValidateTransformSimpleClaimToken()
		{
			using (var stream = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.BizTalk.Resources.Message.ClaimToken.1.xml"))
			{
				var setup = Given(input => input.Message(stream).Context(new MessageContextMock().Object))
					.Transform
					.OutputsXml(output => output.ConformingTo<CheckIn>().WithStrictConformanceLevel());
				var result = setup.Validate();
				result.Select("//usp:url/text()").Should().HaveCount(1);
				result.Select("//usp:any").Should().HaveCount(0);
			}
		}
	}
}
