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
using System.Configuration;
using System.Globalization;
using System.Linq;
using Be.Stateless.BizTalk.ClaimStore.Configuration.Validators;

namespace Be.Stateless.BizTalk.ClaimStore.Configuration
{
	public sealed class AgentConfigurationElement : ConfigurationElement
	{
		static AgentConfigurationElement()
		{
			_properties.Add(_checkInDirectoryCollectionProperty);
			_properties.Add(_checkOutDirectoryProperty);
			_properties.Add(_fileLockTimeoutProperty);
			_properties.Add(_pollingIntervalProperty);
		}

		#region Base Class Member Overrides

		/// <summary>
		/// Gets the collection of properties.
		/// </summary>
		/// <returns>
		/// The <see cref="ConfigurationPropertyCollection"/> collection of properties for the element.
		/// </returns>
		protected override ConfigurationPropertyCollection Properties => _properties;

		#endregion

		/// <summary>
		/// The collection of folders to collect claimed and tracked message bodies from, that is the local directories where
		/// they have been checked in.
		/// </summary>
		public IEnumerable<string> CheckInDirectories => DirectoryCollection.Cast<DirectoryConfigurationElement>().Select(dce => dce.Path);

		/// <summary>
		/// The directory where to drop collected claimed and tracked message bodies, that is the central directory from where
		/// they can be checked out.
		/// </summary>
		[ConfigurationProperty(CHECK_OUT_DIRECTORY_PROPERTY_NAME)]
		[DirectoryValidator]
		public string CheckOutDirectory => (string) base[_checkOutDirectoryProperty];

		/// <summary>
		/// The <see cref="TimeSpan"/> interval to wait before considering a lock that has been taken on a claimed or tracked
		/// message body file was not correctly released.
		/// </summary>
		[ConfigurationProperty(FILE_LOCK_TIMEOUT_PROPERTY_NAME, DefaultValue = FILE_LOCK_TIMEOUT_DEFAULT_VALUE)]
		[PositiveTimeSpanValidator]
		public TimeSpan FileLockTimeout => (TimeSpan) base[_fileLockTimeoutProperty];

		/// <summary>
		/// The <see cref="TimeSpan"/> interval to wait before scheduling the next collection.
		/// </summary>
		[ConfigurationProperty(POLLING_INTERVAL_PROPERTY_NAME, DefaultValue = POLLING_INTERVAL_DEFAULT_VALUE)]
		[PositiveTimeSpanValidator]
		public TimeSpan PollingInterval => (TimeSpan) base[_pollingIntervalProperty];

		/// <summary>
		/// The collection of directories to collect claimed and tracked message bodies from.
		/// </summary>
		[ConfigurationProperty(CHECK_IN_DIRECTORY_COLLECTION_PROPERTY_NAME, IsDefaultCollection = true, IsRequired = true)]
		[CollectionValidator]
		private DirectoryConfigurationElementCollection DirectoryCollection => (DirectoryConfigurationElementCollection) base[_checkInDirectoryCollectionProperty];

		internal const string FILE_LOCK_TIMEOUT_DEFAULT_VALUE = "00:30:00";
		internal const string POLLING_INTERVAL_DEFAULT_VALUE = "00:01:00";

		private const string CHECK_IN_DIRECTORY_COLLECTION_PROPERTY_NAME = "checkInDirectories";
		private const string CHECK_OUT_DIRECTORY_PROPERTY_NAME = "checkOutDirectory";
		private const string FILE_LOCK_TIMEOUT_PROPERTY_NAME = "fileLockTimeout";
		private const string POLLING_INTERVAL_PROPERTY_NAME = "pollingInterval";

		private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

		private static readonly ConfigurationProperty _checkInDirectoryCollectionProperty = new ConfigurationProperty(
			CHECK_IN_DIRECTORY_COLLECTION_PROPERTY_NAME,
			typeof(DirectoryConfigurationElementCollection),
			null,
			null,
			new CollectionValidator(),
			ConfigurationPropertyOptions.IsDefaultCollection | ConfigurationPropertyOptions.IsRequired);

		private static readonly ConfigurationProperty _checkOutDirectoryProperty = new ConfigurationProperty(
			CHECK_OUT_DIRECTORY_PROPERTY_NAME,
			typeof(string),
			null,
			null,
			new DirectoryValidator(),
			ConfigurationPropertyOptions.IsRequired);

		private static readonly ConfigurationProperty _fileLockTimeoutProperty = new ConfigurationProperty(
			FILE_LOCK_TIMEOUT_PROPERTY_NAME,
			typeof(TimeSpan),
			TimeSpan.Parse(FILE_LOCK_TIMEOUT_DEFAULT_VALUE, CultureInfo.InvariantCulture),
			null,
			new PositiveTimeSpanValidator(),
			ConfigurationPropertyOptions.None);

		private static readonly ConfigurationProperty _pollingIntervalProperty = new ConfigurationProperty(
			POLLING_INTERVAL_PROPERTY_NAME,
			typeof(TimeSpan),
			TimeSpan.Parse(POLLING_INTERVAL_DEFAULT_VALUE, CultureInfo.InvariantCulture),
			null,
			new PositiveTimeSpanValidator(),
			ConfigurationPropertyOptions.None);
	}
}
