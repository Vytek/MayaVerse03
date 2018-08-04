using System;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Convenient timer to measure execution time.
	/// Can be used in "using" statement (IDisposable).
	/// </summary>
	public class MeasureTime : IDisposable
	{
		public static bool disableProfileTraces = false;

		private string msg;
		private DateTime start;
		private bool stopped = false;

		public MeasureTime(string _msg)
		{
			msg = _msg;
			start = DateTime.Now;
		}

		public double MillisecondsPassed()
		{
			return (DateTime.Now - start).TotalMilliseconds;
		}

		public void Measure()
		{
			if (!disableProfileTraces)
				Debug.LogFormat ("{0} took {1} ms", msg, MillisecondsPassed ());
		}

		public void Stop()
		{
			if (!stopped)
				Measure ();
			stopped = true;
		}

		public void Dispose ()
		{
			Stop ();
		}
	}
}
