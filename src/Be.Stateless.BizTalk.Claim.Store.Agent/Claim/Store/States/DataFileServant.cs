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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Transactions;
using Be.Stateless.Extensions;
using Be.Stateless.IO;
using log4net;
using Path = System.IO.Path;

namespace Be.Stateless.BizTalk.Claim.Store.States
{
	[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "Required for unit testing purposes.")]
	internal class DataFileServant
	{
		public static DataFileServant Instance { get; internal set; } = new();

		private static readonly ILog _logger = LogManager.GetLogger(typeof(MessageBody));

		#region File System Operations

		internal virtual bool TryCreateDirectory(string filePath)
		{
			if (filePath.IsNullOrEmpty()) throw new ArgumentNullException(nameof(filePath));
			var targetDirectory = Path.GetDirectoryName(filePath);
			return TryIoOperation(
				() => Directory.CreateDirectory(targetDirectory!),
				$"Failed to create directory '{targetDirectory}'.");
		}

		internal virtual bool TryCopyFile(string sourceFilePath, string targetFilePath)
		{
			if (sourceFilePath.IsNullOrEmpty()) throw new ArgumentNullException(nameof(sourceFilePath));
			if (targetFilePath.IsNullOrEmpty()) throw new ArgumentNullException(nameof(targetFilePath));
			return TryIoOperation(
				() => File.Copy(sourceFilePath, targetFilePath, true),
				$"Failed to copy file from '{sourceFilePath}' to '{targetFilePath}'.");
		}

		internal virtual bool TryDeleteFile(string filePath)
		{
			if (filePath.IsNullOrEmpty()) throw new ArgumentNullException(nameof(filePath));
			return TryIoOperation(() => File.Delete(filePath), $"Failed to delete file '{filePath}'.");
		}

		internal virtual bool TryMoveFile(string sourceFilePath, string targetFilePath)
		{
			if (sourceFilePath.IsNullOrEmpty()) throw new ArgumentNullException(nameof(sourceFilePath));
			if (targetFilePath.IsNullOrEmpty()) throw new ArgumentNullException(nameof(targetFilePath));

			var transaction = Transaction.Current;
			if (transaction == null)
			{
				return TryIoOperation(() => File.Move(sourceFilePath, targetFilePath), $"Failed to move file from '{sourceFilePath}' to '{targetFilePath}'.");
			}

			// ReSharper disable once SuspiciousTypeConversion.Global
			var kernelTransaction = (IKernelTransaction) TransactionInterop.GetDtcTransaction(transaction);
			return TryIoOperation(
				() => TransactionalFile.Move(sourceFilePath, targetFilePath, kernelTransaction),
				$"Failed to transactionally move file from '{sourceFilePath}' to '{targetFilePath}'.");
		}

		private bool TryIoOperation(Action operation, string message)
		{
			// resilient to IOException and UnauthorizedAccessException but not to any other exception
			try
			{
				operation();
				return true;
			}
			catch (IOException exception)
			{
				if (_logger.IsWarnEnabled) _logger.Warn(message, exception);
				return false;
			}
			catch (UnauthorizedAccessException exception)
			{
				if (_logger.IsWarnEnabled) _logger.Warn(message, exception);
				return false;
			}
		}

		#endregion

		#region Database Operations

		internal virtual bool TryReleaseToken(string token)
		{
			if (token.IsNullOrEmpty()) throw new ArgumentNullException(nameof(token));
			return TrySqlOperation(
				() => {
					using (var cnx = new SqlConnection(ConfigurationManager.ConnectionStrings["TransientStateDb"].ConnectionString))
					using (var cmd = new SqlCommand("usp_claim_Release", cnx))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.AddWithValue("@url", token);
						cnx.Open();
						var rowsAffected = cmd.ExecuteNonQuery();
						return rowsAffected == 1;
					}
				},
				$"Failed to release claim check token '{token}'.");
		}

		private bool TrySqlOperation(Func<bool> operation, string message)
		{
			// tolerant to SqlException but let through any other exception
			try
			{
				return operation();
			}
			catch (SqlException exception)
			{
				if (_logger.IsWarnEnabled) _logger.Warn(message, exception);
				return false;
			}
		}

		#endregion
	}
}
