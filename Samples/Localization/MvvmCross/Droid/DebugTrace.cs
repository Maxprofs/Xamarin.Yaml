namespace Yaml.Localization.MvvmCross.Droid
{
	using System;
	using System.Diagnostics;
	using global::MvvmCross.Platform.Platform;

	public class DebugTrace : IMvxTrace
	{
		public void Trace(MvxTraceLevel level, string tag, Func<string> message)
		{
			Debug.WriteLine(tag + ":" + level + ":" + message());
		}

		public void Trace(MvxTraceLevel level, string tag, string message)
		{
			Debug.WriteLine(tag + ":" + level + ":" + message);
		}

		public void Trace(MvxTraceLevel level, string tag, string message, params object[] args)
		{
			try
			{
				Debug.WriteLine(tag + ":" + level + ":" + message, args);
			}
			catch (FormatException)
			{
				this.Trace(MvxTraceLevel.Error, tag, "Exception during trace of {0} {1}", level, message);
			}
		}
	}
}
