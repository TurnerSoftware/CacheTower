using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CacheTower.Internal
{
	public static class DateTimeProvider
	{
		public static DateTime Now { get; internal set; }

#pragma warning disable IDE0052 // Remove unread private members
		private static readonly Timer DateTimeTimer = new Timer(state => Now = DateTime.UtcNow, null, 1000, 1000);
#pragma warning restore IDE0052 // Remove unread private members
	}
}
