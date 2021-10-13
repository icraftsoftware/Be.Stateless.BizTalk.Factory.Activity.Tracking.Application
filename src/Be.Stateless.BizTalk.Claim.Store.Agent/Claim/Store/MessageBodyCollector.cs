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
using System.Linq;
using System.Threading;
using Be.Stateless.BizTalk.Claim.Store.Configuration;
using Be.Stateless.Extensions;
using log4net;

namespace Be.Stateless.BizTalk.Claim.Store
{
	/// <summary>
	/// Bring claimed and tracked message bodies into the central claim store.
	/// </summary>
	public class MessageBodyCollector
	{
		public void Start()
		{
			try
			{
				lock (_lock)
				{
					var agentConfiguration = ClaimStoreConfigurationSection.Current.Agent;
					_logger.InfoFormat(
						"Claim Store Agent Service is initializing with the following check-in directories:{0}\r\n" +
						"and check-out directory:\r\n  {1}.",
						agentConfiguration.CheckInDirectories.Aggregate(string.Empty, (acc, d) => acc + "\r\n  " + d + ";"),
						agentConfiguration.CheckOutDirectory);
					_pollingTimer = new(OnTimerElapsed);
				}
				// do an initial collection and schedule next one
				ThreadPool.QueueUserWorkItem(OnTimerElapsed);
			}
			catch (Exception exception)
			{
				if (_logger.IsErrorEnabled) _logger.Error("An error occurred during Claim Store Agent Service start up.", exception);
				throw;
			}
		}

		public void Stop()
		{
			try
			{
				lock (_lock)
				{
					_pollingTimer.Change(TimeSpan.FromMilliseconds(-1.0), TimeSpan.FromMilliseconds(-1.0));
					_pollingTimer.Dispose();
					_pollingTimer = null;
				}
			}
			catch (Exception exception)
			{
				if (_logger.IsErrorEnabled) _logger.Error("An error occurred during Claim Store Agent Service shutdown.", exception);
				throw;
			}
		}

		private void OnTimerElapsed(object state)
		{
			Collect();
			ScheduleNextCollection();
		}

		private void Collect()
		{
			try
			{
				var agentConfiguration = ClaimStoreConfigurationSection.Current.Agent;
				// collect claimed and tracked message bodies
				agentConfiguration.CheckInDirectories
					.EnumerateMessageBodies(agentConfiguration.FileLockTimeout)
					.Collect(agentConfiguration.CheckOutDirectory);
			}
			catch (Exception exception)
			{
				if (exception.IsFatal())
				{
					_logger.Fatal("A fatal error occurred during Claim Store Agent Service's message body collection.", exception);
					throw;
				}
				if (_logger.IsErrorEnabled) _logger.Error("An error occurred during Claim Store Agent Service's message body collection.", exception);
			}
		}

		private void ScheduleNextCollection()
		{
			if (_pollingTimer == null) return;
			try
			{
				lock (_lock)
				{
					_pollingTimer?.Change(ClaimStoreConfigurationSection.Current.Agent.PollingInterval, TimeSpan.FromMilliseconds(-1.0));
				}
			}
			catch (Exception exception)
			{
				if (_logger.IsErrorEnabled) _logger.Error("An error occurred during Claim Store Agent Service's scheduling of next message body collection.", exception);
				throw;
			}
		}

		private static readonly ILog _logger = LogManager.GetLogger(typeof(MessageBodyCollector));
		private readonly object _lock = new();
		private volatile Timer _pollingTimer;
	}
}
