using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower
{
	internal static class InternalExtensions
	{
		public static DateTime TrimToSeconds(this DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Utc);
		}
	}
}
