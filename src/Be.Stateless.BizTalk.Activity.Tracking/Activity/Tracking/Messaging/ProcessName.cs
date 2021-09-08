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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Be.Stateless.Linq.Extensions;
using Be.Stateless.Reflection;

namespace Be.Stateless.BizTalk.Activity.Tracking.Messaging
{
	public abstract class ProcessName<T> where T : class, new()
	{
		static ProcessName()
		{
			const BindingFlags bindingFlags = BindingFlags.DeclaredOnly
				| BindingFlags.NonPublic | BindingFlags.Public
				| BindingFlags.Instance | BindingFlags.Static;
			var members = typeof(T)
				.GetMembers(bindingFlags)
				// filter out default ctor
				.Where(m => !(m.MemberType == MemberTypes.Constructor && ((ConstructorInfo) m).GetParameters().Length == 0))
				// filter out compiler generated code
				.Where(m => !Attribute.IsDefined(m, typeof(CompilerGeneratedAttribute)))
				.ToArray();

			var message = $"{typeof(T).FullName} must only declare non-static public string properties.";
			if (members.Any(m => m.MemberType != MemberTypes.Property)) throw new ArgumentException(message);

			var properties = members.Cast<PropertyInfo>().ToArray();
			// unsupported properties are either non-string or non-public or static
			var unsupportedProperties = properties.Where(
				p => p.PropertyType != typeof(string)
					|| p.GetGetMethod() == null
					|| p.GetGetMethod().IsStatic);
			if (unsupportedProperties.Any()) throw new ArgumentException(message);

			_isCreatingSingletonInstance = true;
			Processes = new();
			var prefix = typeof(T).FullName + ".";
			properties.ForEach(p => Reflector.SetProperty(Processes, p.Name, prefix + p.Name));
		}

		public static T Processes { get; }

		protected ProcessName()
		{
			if (!_isCreatingSingletonInstance)
				throw new InvalidOperationException(
					$"Type '{typeof(T).FullName}' is a singleton meant to be accessed via its Processes member property and is not intended to be instantiated directly.");
			_isCreatingSingletonInstance = false;
		}

		[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
		private static bool _isCreatingSingletonInstance;
	}

	internal static class ProcessNamesExtensions
	{
		public static IEnumerable<string> GetProcessNames(this Type type)
		{
			const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public;
			var instance = type.GetProperty("Processes", bindingFlags)!.GetValue(null);
			return type.GetProperties().Select(pi => (string) pi.GetValue(instance));
		}

		public static bool IsProcessNameClass(this Type type)
		{
			return type.BaseType is { IsGenericType: true } && type.BaseType.GetGenericTypeDefinition() == typeof(ProcessName<>);
		}
	}
}
