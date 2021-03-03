using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CacheTower.Internal
{
	internal static class DateTimeProvider
	{
		public static DateTime Now { get; internal set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Establishes timer and prevents it being garbage collected")]
		private static readonly Timer DateTimeTimer = new(state => Now = DateTime.UtcNow, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
	}
}
