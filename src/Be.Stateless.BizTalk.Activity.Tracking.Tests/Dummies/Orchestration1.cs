﻿#region Copyright & License

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
using Microsoft.BizTalk.XLANGs.BTXEngine;
using Microsoft.XLANGs.BaseTypes;

namespace Be.Stateless.BizTalk.Dummies
{
	[StaticSubscription(0, "CompensationCommandReceivePort", "Receive", -1, -1, true)]
	public abstract class Orchestration1 : BTXService
	{
		protected Orchestration1() : base(0, Guid.NewGuid(), null, "stuff") { }
	}
}
