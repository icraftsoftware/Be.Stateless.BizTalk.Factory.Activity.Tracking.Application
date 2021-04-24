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
using System.Runtime.Caching;
using Be.Stateless.BizTalk.Activity.Tracking;
using Be.Stateless.Extensions;
using Microsoft.BizTalk.Message.Interop;

namespace Be.Stateless.BizTalk.Runtime.Caching
{
	/// <summary>
	/// Runtime memory cache for the <see cref="TrackingContext"/> associated to <see cref="IBaseMessage"/>-derived types.
	/// </summary>
	[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "Required for unit testing purposes.")]
	public class TrackingContextCache
	{
		///// <summary>
		///// Singleton <see cref="TrackingContextCache"/> instance.
		///// </summary>
		public static TrackingContextCache Instance { get; internal set; } = new();

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Required for unit testsing purposes.")]
		protected TrackingContextCache()
		{
			_cache = new MemoryCache(nameof(TrackingContextCache));
		}

		/// <summary>
		/// Adds a new or update an existing <see cref="TrackingContext"/> in cache with an absolute expiration of <paramref
		/// name="duration"/>.
		/// </summary>
		/// <param name="key">
		/// The cache entry identifier, or key, at which to add the new <paramref name="trackingContext"/>.
		/// </param>
		/// <param name="trackingContext">
		/// The <see cref="TrackingContext"/> to cache.
		/// </param>
		/// <param name="duration">
		/// The duration, in seconds, after which the <paramref name="trackingContext"/> entry will be removed from the cache.
		/// </param>
		public virtual void Set(string key, TrackingContext trackingContext, int duration)
		{
			if (key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(key));
			if (trackingContext.IsEmpty()) throw new ArgumentNullException(nameof(trackingContext));
			if (duration < 1) throw new ArgumentException("Expiration duration must be strictly positive.", nameof(duration));

			var cacheItem = new CacheItem(key, trackingContext);
			var policy = new CacheItemPolicy { AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddSeconds(duration)) };
			_cache.Set(cacheItem, policy);
		}

		/// <summary>
		/// Removes and returns a previously cached <see cref="TrackingContext"/>.
		/// </summary>
		/// <param name="key">
		/// The cache entry identifier, or key, of the <see cref="TrackingContext"/> to retrieve and erase.
		/// </param>
		/// <returns>
		/// The <see cref="TrackingContext"/> that had been previously added to the cache.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// The key is null or empty.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// No entry has been found in cache for the given <paramref name="key"/>.
		/// </exception>
		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
		public virtual TrackingContext Remove(string key)
		{
			if (key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(key));

			var cachedData = _cache.Remove(key);
			if (cachedData == null) throw new InvalidOperationException("TrackingContext could not be found in cache.");
			var trackingContext = (TrackingContext) cachedData;
			if (trackingContext.IsEmpty()) throw new InvalidOperationException("Invalid TrackingContext: None of its individual activity Ids is set.");
			return trackingContext;
		}

		/// <summary>
		/// Returns a previously cached <see cref="TrackingContext"/>.
		/// </summary>
		/// <param name="key">
		/// The cache entry identifier, or key, of the <see cref="TrackingContext"/> to retrieve.
		/// </param>
		/// <returns>
		/// The <see cref="TrackingContext"/> that had been previously added to the cache.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// The key is null or empty.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// No entry has been found in cache for the given <paramref name="key"/>, or the <see cref="TrackingContext"/> is
		/// invalid.
		/// </exception>
		public virtual TrackingContext Get(string key)
		{
			if (key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(key));

			var cachedData = _cache.Get(key);
			if (cachedData == null) throw new InvalidOperationException("TrackingContext could not be found in cache.");
			var trackingContext = (TrackingContext) cachedData;
			if (trackingContext.IsEmpty()) throw new InvalidOperationException("Invalid TrackingContext: None of its individual activity Ids is set.");
			return trackingContext;
		}

		private readonly MemoryCache _cache;
	}
}
