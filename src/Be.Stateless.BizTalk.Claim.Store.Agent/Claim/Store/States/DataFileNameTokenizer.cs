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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Be.Stateless.Extensions;

namespace Be.Stateless.BizTalk.Claim.Store.States
{
	internal static class DataFileNameTokenizer
	{
		#region Nested Type: Tokens

		internal class Tokens
		{
			internal Tokens(GroupCollection groups)
			{
				_groups = groups;
			}

			internal string CaptureDate => _groups[CAPTURE_DATE_TOKEN].Value;

			internal string Id => _groups[ID_TOKEN].Value;

			internal DateTime? LockTime
			{
				get
				{
					var lockTimeToken = _groups[LOCK_TIME_TOKEN];
					var lockTime = lockTimeToken.Success
						? DateTime.ParseExact(lockTimeToken.Value, LOCK_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime()
						: (DateTime?) null;
					return lockTime;
				}
			}

			internal string State => _groups[STATE_TOKEN].Value;

			internal string TrackingMode => _groups[TRACKING_MODE_TOKEN].Value;

			internal string UnlockedFilePath => _groups[UNLOCKED_FILE_PATH_TOKEN].Value;

			private readonly GroupCollection _groups;
		}

		#endregion

		internal static bool IsValidDataFilePath(this string filePath)
		{
			if (filePath.IsNullOrEmpty()) throw new ArgumentNullException(nameof(filePath));
			return Regex.IsMatch(filePath, _pattern);
		}

		internal static string NewNameForState(this string filePath, string stateToken)
		{
			return $"{filePath.Tokenize().UnlockedFilePath}.{DateTime.UtcNow.ToString(LOCK_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture)}.{stateToken}";
		}

		[SuppressMessage("ReSharper", "LocalizableElement")]
		internal static Tokens Tokenize(this string filePath)
		{
			if (filePath.IsNullOrEmpty()) throw new ArgumentNullException(nameof(filePath));
			var match = Regex.Match(filePath, _pattern);
			if (!match.Success) throw new ArgumentException($"Claim Store Agent does not recognize the message body's data file path: '{filePath}'.", nameof(filePath));
			return new Tokens(match.Groups);
		}

		internal const string LOCK_TIMESTAMP_FORMAT = "yyyyMMddHHmmss";
		private const string CAPTURE_DATE_TOKEN = "CaptureDate";
		private const string ID_TOKEN = "Id";
		private const string LOCK_TIME_TOKEN = "LockTime";
		private const string STATE_TOKEN = "State";
		private const string TRACKING_MODE_TOKEN = "TrackingMode";
		private const string UNLOCKED_FILE_PATH_TOKEN = "UnlockedFilePath";

		// ReSharper disable once CommentTypo
		// file name is made of either 2 or 4 tokens: yyyyMMdd<GUID>.(chk|trk) or yyyyMMdd<GUID>.(chk|trk).yyyyMMddHHmmss.(gathered|locked|released)
		private static readonly string _pattern = string.Format(
			CultureInfo.InvariantCulture,
			@"^(?<{0}>(?:.*\\)?(?<{1}>\d{{8}})(?<{2}>[\dA-Fa-f]{{32}})\.(?<{3}>(?:trk|chk)))(?:\.(?<{4}>\d{{14}})\.(?<{5}>locked|gathered|released))?$",
			UNLOCKED_FILE_PATH_TOKEN,
			CAPTURE_DATE_TOKEN,
			ID_TOKEN,
			TRACKING_MODE_TOKEN,
			LOCK_TIME_TOKEN,
			STATE_TOKEN);
	}
}
