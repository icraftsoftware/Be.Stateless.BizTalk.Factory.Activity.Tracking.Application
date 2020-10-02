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
using System.Collections.Generic;
using System.Linq;
using Be.Stateless.BizTalk.Dummies;
using FluentAssertions;
using Xunit;

namespace Be.Stateless.BizTalk.Install
{
	public class OrchestrationProcessNameInstallerFixture : OrchestrationProcessNameInstaller
	{
		[Fact]
		public void DiscoverProcessNamesForAllOrchestrations()
		{
			var expected = new[] {
				typeof(Orchestration1).Namespace,
				typeof(Orchestration2).Namespace
			};

			_excludedTypes = () => base.ExcludedTypes;

			GetProcessNames().Should().BeEquivalentTo(expected);
		}

		[Fact]
		public void DiscoverProcessNamesForSomeOrchestrations()
		{
			var expected = new[] {
				typeof(Orchestration2).Namespace
			};

			_excludedTypes = () => base.ExcludedTypes.Concat(new List<Type> { typeof(Orchestration1) });

			GetProcessNames().Should().BeEquivalentTo(expected);
		}

		[Fact]
		public void ExcludedTypesByDefault()
		{
			object[] expected = {
				typeof(Step),
				typeof(SubProcess)
			};

			_excludedTypes = () => base.ExcludedTypes;

			ExcludedTypes.Should().BeEquivalentTo(expected);
		}

		protected override IEnumerable<Type> ExcludedTypes => _excludedTypes();

		private Func<IEnumerable<Type>> _excludedTypes;
	}
}
