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
using Be.Stateless.BizTalk.Factory.Areas;
using FluentAssertions;
using Xunit;
using static FluentAssertions.FluentActions;

namespace Be.Stateless.BizTalk.Activity.Tracking.Messaging
{
	public class ProcessNameFixture
	{
		[Theory]
		//[MemberData(nameof(BatchProcessNames))]
		[MemberData(nameof(ClaimProcessNames))]
		[MemberData(nameof(DefaultProcessNames))]
		public void BizTalkFactoryProcessNames(string actualName, string expectedName)
		{
			actualName.Should().Be(expectedName);
		}

		[Fact]
		[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
		public void CannotDirectlyInstantiateProcessNameDerivedClass()
		{
			Invoking(() => new DiscoverableArea())
				.Should().Throw<InvalidOperationException>()
				.WithMessage(
					$"Type '{typeof(DiscoverableArea).FullName}' is a singleton meant to be accessed via its Processes member property and is not intended to be instantiated directly.");

			Invoking(() => new SampleArea())
				.Should().Throw<InvalidOperationException>()
				.WithMessage(
					$"Type '{typeof(SampleArea).FullName}' is a singleton meant to be accessed via its Processes member property and is not intended to be instantiated directly.");
		}

		[Fact]
		public void InstanceStringPropertyValuesAreComputed()
		{
			DiscoverableArea.Processes.ProcessOne.Should().Be(typeof(DiscoverableArea).FullName + ".ProcessOne");
		}

		[Fact]
		public void NonPublicPropertyThrows()
		{
			Invoking(() => UndiscoverableNonPublic.Processes.ProcessFour)
				.Should().Throw<TypeInitializationException>()
				.WithInnerException<ArgumentException>()
				.WithMessage(string.Format(MESSAGE, typeof(UndiscoverableNonPublic).FullName));
		}

		[Fact]
		public void NonStringPropertyThrows()
		{
			Invoking(() => UndiscoverableNonString.Processes.ProcessThree)
				.Should().Throw<TypeInitializationException>()
				.WithInnerException<ArgumentException>()
				.WithMessage(string.Format(MESSAGE, typeof(UndiscoverableNonString).FullName));
		}

		[Fact]
		public void StaticPropertyThrows()
		{
			Invoking(() => UndiscoverableStatic.Processes)
				.Should().Throw<TypeInitializationException>()
				.WithInnerException<ArgumentException>()
				.WithMessage(string.Format(MESSAGE, typeof(UndiscoverableStatic).FullName));
		}

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class DiscoverableArea : ProcessName<DiscoverableArea>
		{
			public string ProcessOne { get; private set; }
		}

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private class SampleArea : ProcessName<SampleArea>
		{
			public string ProcessOne { get; private set; }
		}

		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private class UndiscoverableStatic : ProcessName<UndiscoverableStatic>
		{
			public static string ProcessTwo { get; private set; }
		}

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class UndiscoverableNonString : ProcessName<UndiscoverableNonString>
		{
			public int ProcessThree { get; private set; }
		}

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class UndiscoverableNonPublic : ProcessName<UndiscoverableNonPublic>
		{
			internal string ProcessFour { get; private set; }
		}

		private const string MESSAGE = "{0} must only declare non-static public string properties.";

		//public static readonly object[] BatchProcessNames = {
		//	new object[] { Batch.Processes.Aggregate, "Be.Stateless.BizTalk.Factory.Areas.Batch.Aggregate" },
		//	new object[] { Batch.Processes.Release, "Be.Stateless.BizTalk.Factory.Areas.Batch.Release" },
		//};

		public static readonly object[] ClaimProcessNames = {
			new object[] { Claim.Processes.Check, "Be.Stateless.BizTalk.Factory.Areas.Claim.Check" }
		};

		public static readonly object[] DefaultProcessNames = {
			new object[] { Default.Processes.Failed, "Be.Stateless.BizTalk.Factory.Areas.Default.Failed" },
			new object[] { Default.Processes.Unidentified, "Be.Stateless.BizTalk.Factory.Areas.Default.Unidentified" }
		};
	}
}
