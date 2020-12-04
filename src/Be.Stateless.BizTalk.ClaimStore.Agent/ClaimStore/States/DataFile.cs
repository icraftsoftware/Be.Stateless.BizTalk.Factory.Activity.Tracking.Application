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
using System.IO;

namespace Be.Stateless.BizTalk.ClaimStore.States
{
	internal abstract class DataFile
	{
		internal static DataFile Create(string filePath)
		{
			return filePath.Tokenize().State switch {
				GatheredDataFile.STATE_TOKEN => new GatheredDataFile(filePath),
				LockedDataFile.STATE_TOKEN => new LockedDataFile(filePath),
				ReleasedDataFile.STATE_TOKEN => new ReleasedDataFile(filePath),
				_ => new UnlockedDataFile(filePath)
			};
		}

		protected DataFile(string filePath)
		{
			Path = filePath;
		}

		#region Base Class Member Overrides

		public override string ToString()
		{
			return System.IO.Path.GetFileName(Path);
		}

		#endregion

		protected internal string ClaimStoreRelativePath
		{
			get
			{
				// data file 'yyyyMMdd<GUID>.(chk|trk)' will have claim store relative path 'yyyyMMdd\<GUID>'
				var tokens = Path.Tokenize();
				return System.IO.Path.Combine(tokens.CaptureDate, tokens.Id);
			}
		}

		internal DateTime CreationTime => _fileCreationTimeGetter(Path);

		internal DateTime? LockTime => Path.Tokenize().LockTime;

		internal string Path { get; }

		internal string TrackingMode => Path.Tokenize().TrackingMode;

		internal abstract void Lock(MessageBody messageBody);

		internal abstract void Gather(MessageBody messageBody, string gatheringDirectory);

		internal abstract void Release(MessageBody messageBody);

		internal abstract void Unlock(MessageBody messageBody);

		internal static Func<string, DateTime> _fileCreationTimeGetter = File.GetCreationTimeUtc;
	}
}
