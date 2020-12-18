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
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.ServiceProcess;

namespace Be.Stateless.BizTalk.Claim.Store.Agent
{
	[RunInstaller(true)]
	[SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
	[SuppressMessage("ReSharper", "LocalizableElement")]
	public partial class Installer : System.Configuration.Install.Installer
	{
		public Installer()
		{
			Installers.Add(
				new EventLogInstaller {
					Log = "Application",
					Source = "Claim Store Agent"
				});

			InitializeComponent();

			_isServiceInstalled = System.ServiceProcess.ServiceController
				.GetServices()
				.Any(sc => sc.ServiceName == _serviceInstaller.ServiceName);

			BeforeInstall += SetupInstaller;
			AfterInstall += StartService;
			BeforeUninstall += StopService;
		}

		#region Base Class Member Overrides

		public override void Install(System.Collections.IDictionary stateSaver)
		{
			if (_isServiceInstalled)
			{
				// clearing Installers collection similarly to what is being done in Uninstall() does not work as base class
				// will commit the state anyway, which will be corrupted if the installers are skipped/have been cleared...
				Console.WriteLine($"\r\nPerforming anew installation of '{_serviceInstaller.ServiceName}' service.");
				base.Uninstall(null);
			}
			base.Install(stateSaver);
		}

		public override void Uninstall(System.Collections.IDictionary savedState)
		{
			if (!_isServiceInstalled)
			{
				Console.WriteLine("\r\nSkipping uninstall: '{0}' service is not installed.", _serviceInstaller.ServiceName);
				Installers.Clear();
			}
			base.Uninstall(savedState);
		}

		#endregion

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
		private void SetupInstaller(object sender, InstallEventArgs installEventArgs)
		{
			_serviceProcessInstaller.Account = ServiceAccount.User;
			_serviceProcessInstaller.Username = Context.Parameters["ServiceAccountName"];
			_serviceProcessInstaller.Password = Context.Parameters["ServiceAccountPassword"];
			_serviceInstaller.StartType = Context.Parameters["ServiceStartMode"].ToLowerInvariant() switch {
				"a" => ServiceStartMode.Automatic,
				"auto" => ServiceStartMode.Automatic,
				"automatic" => ServiceStartMode.Automatic,
				"disabled" => ServiceStartMode.Disabled,
				_ => ServiceStartMode.Manual
			};
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		[SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
		private void StartService(object sender, InstallEventArgs installEventArgs)
		{
			try
			{
				if (_serviceInstaller.StartType != ServiceStartMode.Automatic) return;
				using (var serviceController = new System.ServiceProcess.ServiceController(_serviceInstaller.ServiceName))
				{
					if (serviceController.Status == ServiceControllerStatus.Running) return;
					if (serviceController.Status == ServiceControllerStatus.StartPending) return;
					serviceController.Start();
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("Warning! Could not start service with automatic starting mode after install:");
				Console.WriteLine(exception);
			}
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		private void StopService(object sender, InstallEventArgs installEventArgs)
		{
			try
			{
				if (!_isServiceInstalled) return;
				using (var serviceController = new System.ServiceProcess.ServiceController(_serviceInstaller.ServiceName))
				{
					if (serviceController.CanStop) serviceController.Stop();
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("Warning! Could not stop running service before uninstalling it:");
				Console.WriteLine(exception);
			}
		}

		private readonly bool _isServiceInstalled;
	}
}
