namespace HKHR_ATP2
{
	/// <summary>
	/// Description of BeaconID.
	/// </summary>
	public static class BeaconID
	{
		// Int32.MaxValue == 2147483647
		public static class HKHRATP2
		{
			/// <summary>
			/// 1 = Enable | 0 = Disable
			/// </summary>
			public const int EnDisableCabSignal = 8000;
			/// <summary>
			/// XXXYYYYYY | X=speed | Y=starting location <br/>Speed will apply from starting location to next one
			/// </summary>
			public const int SpeedFlag = 8010;
		}
		
		public static class StationsMemory
		{
			/// <summary>
			/// (+/-)XZYYYYYY | X=doorside | Y=trackpos | Z=TBSStart{1;0}
			/// </summary>
			public const int StationStopProvider = 8020;
		}
		
		public static class TrackGradientMemory
		{
			/// <summary>
			/// (+/-)XXXYYYYYY 	X=pitch Y=trackpos
			/// </summary>
			public const int TrackGradientPoint = 8021;
		}
	}
}
