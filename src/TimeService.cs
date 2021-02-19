#nullable enable
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	public static class TimeService
	{
		private readonly static long s_timeInitialized;
		private readonly static double s_dFreq;
		private readonly static int s_iFreq;
		private readonly static double s_dInvFreq;
		private readonly static double s_dInvMilliFreq;
		private readonly static double s_epochOffset;

		private readonly static DateTime s_epoch;

		/// <summary>
		/// Wall time in seconds since TimeService was initialized
		/// </summary>
		public static double Wall { get { return (double)(Stopwatch.GetTimestamp() - s_timeInitialized) * s_dInvFreq; } }

		public static int TicksPerSecond { get { return s_iFreq; } }

		/// <summary>
		/// Seconds since 1970-01-01 00:00
		/// </summary>
		public static double EpochWall { get { return Wall + s_epochOffset; } }

		/// <summary>
		/// Convert from time produced by TimeService.Wall to corresponding Stopwatch.GetTimestamp ticks
		/// </summary>
		public static long WallToTicks(double wall)
		{
			return s_timeInitialized + (long)(wall * s_dFreq);
		}

		/// <summary>
		/// Convert duration in seconds to duration in ticks
		/// </summary>
		public static long SecondsToTicks(double seconds)
		{
			return (long)(seconds * s_dFreq);
		}

		static TimeService()
		{
			s_timeInitialized = Stopwatch.GetTimestamp();
			var freq = Stopwatch.Frequency;
			s_dFreq = (double)freq;
			s_iFreq = (int)freq;
			s_dInvFreq = 1.0 / s_dFreq;
			s_dInvMilliFreq = 1000.0 * s_dInvFreq;
			s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			s_epochOffset = (DateTime.UtcNow - s_epoch).TotalSeconds;
		}

		public static string CompactDuration(double seconds)
		{
			if (seconds <= 0.1)
				return String.Format(CultureInfo.InvariantCulture, "{0:0.#}", seconds * 1000) + "ms";

			if (seconds < 1.0)
				return (int)(seconds * 1000) + "ms";

			if (seconds < 60.0)
				return String.Format(CultureInfo.InvariantCulture, "{0:0.##}", seconds) + "s";

			// 1m to <60m
			if (seconds < 60 * 60)
			{
				int mins = (int)(seconds / 60);
				seconds -= (mins * 60);
				if (seconds == 0)
					return string.Format(CultureInfo.InvariantCulture, "{0}m", mins);
				return string.Format(CultureInfo.InvariantCulture, "{0}m{1:0.##}s", mins, seconds);
			}

			{
				const double secondsInHour = 60 * 60;
				int hrs = (int)(seconds / secondsInHour);
				seconds -= (hrs * secondsInHour);
				if (seconds == 0)
					return string.Format(CultureInfo.InvariantCulture, "{0}h", hrs);
				int mins = (int)(seconds / 60);
				seconds -= (mins * 60);
				if (seconds == 0)
					return string.Format(CultureInfo.InvariantCulture, "{0}h{1}m", hrs, mins);

				return string.Format(CultureInfo.InvariantCulture, "{0}h{1}m{2:0.##}s", hrs, mins, seconds);
			}
		}

		public static string TicksToCompactDuration(long tickCount)
		{
			return CompactDuration((double)(tickCount * s_dInvFreq));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double TicksToSeconds(long tickCount)
		{
			return (double)tickCount * s_dInvFreq;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double TicksToMilliSeconds(long stopwatchTicks)
		{
			return (double)stopwatchTicks * s_dInvMilliFreq;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Measure(long stopwatchTicksStarted)
		{
			var end = Stopwatch.GetTimestamp();
			var dur = end - stopwatchTicksStarted;
			return CompactDuration((double)dur * s_dInvFreq);
		}

		/// <summary>
		/// Parse "1d8h4m10s100ms" to number of seconds
		/// </summary>
		public static bool TryParseDuration(string str, out double seconds)
		{
			seconds = 0.0;

			// cover all-digits case
			bool isAllDigits = true;
			foreach (var c in str)
			{
				if (char.IsDigit(c) == false)
				{
					isAllDigits = false;
					break;
				}
			}
			if (isAllDigits)
				return double.TryParse(str, out seconds);

			int idx = 0;
			while (idx < str.Length)
			{
				// get number
				int n = idx;
				for(; ;)
				{
					if (n >= str.Length)
						break;
					var c = str[n];
					if (char.IsDigit(c) == false && c != '.' && c != '-')
						break;
					n++;
				}
				if (n == idx || n >= str.Length)
					return false;

				double amount;
				if (double.TryParse(str.AsSpan(idx, n - idx), NumberStyles.Number, CultureInfo.InvariantCulture, out amount) == false)
					return false;

				// get unit
				double multiplier;
				switch (str[n])
				{
					case 's':
						multiplier = 1.0;
						idx = n + 1; // skip unit
						break;
					case 'm':
						// minutes or milliseconds?
						if (str.Length > n + 1 && str[n + 1] == 's')
						{
							multiplier = 0.001;
							idx = n + 2; // skip unit
						}
						else
						{
							multiplier = 60.0;
							idx = n + 1; // skip unit
						}
						break;
					case 'h':
						multiplier = (60.0 * 60.0);
						idx = n + 1; // skip unit
						break;
					case 'd':
						multiplier = (60.0 * 60.0 * 24.0);
						idx = n + 1; // skip unit
						break;
					default:
						return false;
				}

				amount *= multiplier;
				seconds += amount;
			}
			return true;
		}

		/// <summary>
		/// Seconds since 1970-01-01
		/// </summary>
		public static double ToEpochSeconds(DateTime? time)
		{
			if (time.HasValue == false)
				return 0;
			var t = time.Value;
			if (t.Kind == DateTimeKind.Utc)
				return (t - s_epoch).TotalSeconds;
			var ut = t.ToUniversalTime();
			return (ut - s_epoch).TotalSeconds;
		}

		/// <summary>
		/// Pass seconds since 1970-01-01
		/// </summary>
		public static DateTime FromEpochSeconds(double seconds)
		{
			if (seconds >= TimeSpan.MaxValue.TotalSeconds)
				return DateTime.MaxValue;
			return s_epoch + TimeSpan.FromSeconds(seconds);
		}
	}
}
