namespace HKHR_ATP2
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public static class PanelID
	{
		/// <summary>
		/// Defines the pilot lamp numbers of actual power, brake and reverser state.
		/// </summary>
		public static class ActualHandle
		{
			public const int Reverser = 0;
			public const int Power = 1;
			public const int Brake = 2;
			public const int ConstSpd = 3;
		}
		
		/// <summary>
		/// Defines the pilot lamp numbers of each item on status panel. 
		/// </summary>
		public static class StatusLEDs
		{
			public const int NoSignal = 128;
			public const int HKHR_ATP = 129;
			public const int RM_Reverse = 130;
			public const int RM = 131;
			public const int ATP = 132;
			public const int ATO = 133;
		}
		
		/// <summary>
		/// Distance to Next Station (if docking, refresh when complete docking)<br/>
		/// (0-9 = 0-9 | 10 = "." | 11 = " " | 12 = "-")
		/// </summary>
		public static class DistanceToNextStation
		{
			public const int FirstDigit = 140;
			public const int SecondDigit = 141;
			public const int ThirdDigit = 142;
			public const int ForthDigit = 143;
			public const int Unit = 144;
		}
		
		public static class PermittedSpeed
		{
			public const int Speedometer = 150;
			public const int HundredsDigit = 151;
			public const int TensDigit = 152;
			public const int SingleDigit = 153;
			public const int SpeedometerTimes100 = 154;
			
			public static class EmgSpeed
			{
				public const int Speedometer = 155;
				public const int HundredsDigit = 156;
				public const int TensDigit = 157;
				public const int SingleDigit = 158;
				public const int SpeedometerTimes100 = 159;
			}
			
			public static class NextFlagSpeed
			{
				public const int IsShowing = 160;
				public const int Speedometer = 161;
				public const int HundredsDigit = 162;
				public const int TensDigit = 163;
				public const int SingleDigit = 164;
			}
		}
	}
}
