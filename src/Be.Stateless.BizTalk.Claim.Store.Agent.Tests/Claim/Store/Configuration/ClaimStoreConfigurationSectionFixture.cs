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
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;
using static FluentAssertions.FluentActions;

namespace Be.Stateless.BizTalk.Claim.Store.Configuration
{
	public class ClaimStoreConfigurationSectionFixture
	{
		[Fact]
		public void DeclaredSection()
		{
			var claimStoreConfigurationSection = (ClaimStoreConfigurationSection) ConfigurationManager.GetSection("be.stateless.test/biztalk/claimStore");
			claimStoreConfigurationSection.Should().NotBeNull();
			claimStoreConfigurationSection.Agent.Should().NotBeNull();
			claimStoreConfigurationSection.Agent.PollingInterval.Should().Be(TimeSpan.Parse("00:03:33"));
			claimStoreConfigurationSection.Agent.CheckInDirectories.Should().NotBeNull();
			claimStoreConfigurationSection.Agent.CheckInDirectories.Should().BeEquivalentTo(@"c:\windows", @"c:\windows\temp");
		}

		[Fact]
		public void DefaultFileLockTimeout()
		{
			var claimStoreConfigurationSection = (ClaimStoreConfigurationSection) ConfigurationManager.GetSection("be.stateless.test/biztalk/claimStoreWithoutPollingInterval");
			claimStoreConfigurationSection.Agent.FileLockTimeout.Should().Be(TimeSpan.Parse(AgentConfigurationElement.FILE_LOCK_TIMEOUT_DEFAULT_VALUE));
		}

		[Fact]
		public void DefaultPollingInterval()
		{
			var claimStoreConfigurationSection = (ClaimStoreConfigurationSection) ConfigurationManager.GetSection("be.stateless.test/biztalk/claimStoreWithoutPollingInterval");
			claimStoreConfigurationSection.Agent.PollingInterval.Should().Be(TimeSpan.Parse(AgentConfigurationElement.POLLING_INTERVAL_DEFAULT_VALUE));
		}

		[Fact]
		public void InvalidCheckInDirectories()
		{
			Invoking(() => ConfigurationManager.GetSection("be.stateless.test/biztalk/claimStoreWithInvalidCheckInDirectories"))
				.Should().Throw<ConfigurationErrorsException>()
				.WithMessage("Required attribute 'path' not found.*");
		}

		[Fact]
		public void NegativeFileLockTimeout()
		{
			Invoking(() => ConfigurationManager.GetSection("be.stateless.test/biztalk/claimStoreWithNegativeFileLockTimeout"))
				.Should().Throw<ConfigurationErrorsException>()
				.WithMessage("The value for the property 'fileLockTimeout' is not valid. The error is: The time span value must be positive.*");
		}

		[Fact]
		public void NegativePollingInterval()
		{
			Invoking(() => ConfigurationManager.GetSection("be.stateless.test/biztalk/claimStoreWithNegativePollingInterval"))
				.Should().Throw<ConfigurationErrorsException>()
				.WithMessage("The value for the property 'pollingInterval' is not valid. The error is: The time span value must be positive.*");
		}

		[Fact]
		public void NoCheckInDirectories()
		{
			Invoking(() => ConfigurationManager.GetSection("be.stateless.test/biztalk/claimStoreWithoutCheckInDirectories"))
				.Should().Throw<ConfigurationErrorsException>()
				.WithMessage("DirectoryConfigurationElementCollection collection contains no items.*");
		}

		[Fact]
		public void NonexistentCheckInDirectories()
		{
			Invoking(() => ConfigurationManager.GetSection("be.stateless.test/biztalk/claimStoreWithNonexistentCheckInDirectories"))
				.Should().Throw<ConfigurationErrorsException>()
				.WithMessage(@"The value for the property 'path' is not valid. The error is: Could not find a part of the path 'c:\some-nonexistent-folder'.*");
		}

		[Fact]
		public void NonexistentCheckOutDirectory()
		{
			Invoking(() => ConfigurationManager.GetSection("be.stateless.test/biztalk/claimStoreWithNonexistentCheckOutDirectory"))
				.Should().Throw<ConfigurationErrorsException>()
				.WithMessage(@"The value for the property 'checkOutDirectory' is not valid. The error is: Could not find a part of the path 'c:\some-nonexistent-folder'.*");
		}

		[Fact]
		[SuppressMessage("ReSharper", "IdentifierTypo")]
		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		public void UnconfiguredSection()
		{
			Invoking(() => ConfigurationManager.GetSection("be.stateless.test/biztalk/unconfiguredClaimStore"))
				.Should().Throw<ConfigurationErrorsException>()
				.WithMessage("Required attribute 'agent' not found.*");
		}

		[Fact]
		public void UndeclaredSection()
		{
			var claimStoreConfigurationSection = ClaimStoreConfigurationSection.Current;
			claimStoreConfigurationSection.Should().BeNull();
		}
	}
}
