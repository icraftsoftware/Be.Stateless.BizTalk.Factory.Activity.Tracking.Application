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
using System.Data;
using System.Data.SqlClient;

namespace Be.Stateless.BizTalk.Install.Command
{
	public class ProcessNameRegistrationCommand : ProcessNameBasedCommand
	{
		#region Base Class Member Overrides

		protected override void ExecuteCore(Action<string> logAppender)
		{
			logAppender?.Invoke("Registering process names.");
			// insert or update should the process name not have been previously deleted
			const string cmdText = @"MERGE INTO monitoring_ProcessDescriptors PD
USING (SELECT @name AS Name) NPD ON PD.Name = NPD.Name
WHEN NOT MATCHED THEN INSERT (Name) VALUES (NPD.Name);";
			using (var cnx = BizTalkFactoryManagementDbConnection)
			using (var cmd = new SqlCommand(cmdText, cnx))
			{
				cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar));
				cnx.Open();
				foreach (var processName in ProcessNames)
				{
					cmd.Parameters["@name"].Value = processName;
					cmd.ExecuteNonQuery();
				}
			}
		}

		#endregion
	}
}
