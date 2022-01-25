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
using System.Data.SqlClient;
using System.Linq;
using Be.Stateless.Extensions;

namespace Be.Stateless.BizTalk.Install.Command
{
	public abstract class ProcessNameBasedCommand
	{
		public string DataSource { get; set; }

		public string[] ProcessNames { get; set; }

		protected SqlConnection BizTalkFactoryManagementDbConnection
		{
			get
			{
				var builder = new SqlConnectionStringBuilder {
					DataSource = DataSource,
					InitialCatalog = MANAGEMENT_DATABASE_NAME,
					IntegratedSecurity = true
				};
				return new(builder.ConnectionString);
			}
		}

		public void Execute(Action<string> logAppender)
		{
			if (!ProcessNames.Any())
			{
				logAppender?.Invoke($"Skipping process names un/registration: {nameof(ProcessNames)} collection is empty.");
			}
			else if (!BizTalkFactoryManagementDbExists())
			{
				logAppender?.Invoke($"Skipping process names un/registration: '{MANAGEMENT_DATABASE_NAME}' database does not exist.");
			}
			else if (!ProcessDescriptorsTableExists())
			{
				logAppender?.Invoke($"Skipping process names un/registration: '{PROCESS_DESCRIPTORS_TABLE_NAME}' table does not exist.");
			}
			else
			{
				ExecuteCore(logAppender);
			}
		}

		protected abstract void ExecuteCore(Action<string> logAppender);

		private bool BizTalkFactoryManagementDbExists()
		{
			if (DataSource.IsNullOrEmpty()) return false;

			var builder = new SqlConnectionStringBuilder {
				DataSource = DataSource,
				InitialCatalog = "master",
				IntegratedSecurity = true
			};
			using (var cnx = new SqlConnection(builder.ConnectionString))
			using (var cmd = new SqlCommand($"SELECT DB_ID('{MANAGEMENT_DATABASE_NAME}')", cnx))
			{
				cnx.Open();
				return cmd.ExecuteScalar() != DBNull.Value;
			}
		}

		private bool ProcessDescriptorsTableExists()
		{
			using (var cnx = BizTalkFactoryManagementDbConnection)
			using (var cmd = new SqlCommand($"SELECT OBJECT_ID('{PROCESS_DESCRIPTORS_TABLE_NAME}')", cnx))
			{
				cnx.Open();
				return cmd.ExecuteScalar() != DBNull.Value;
			}
		}

		protected const string PROCESS_DESCRIPTORS_TABLE_NAME = "monitoring_ProcessDescriptors";
		private const string MANAGEMENT_DATABASE_NAME = "BizTalkFactoryMgmtDb";
	}
}
