namespace HKHR_ATP2
{
	internal partial class ATO
	{
		private class Profile
		{
			internal bool HoldSpeedNotCoasting;
			internal double StopAcceleratingSpeed;
			internal double StopBrakingSpeed;
			internal double CoastingTooSlowForcePowerSpeed;
			internal double CoastingTooFastForcePowerSpeed;
			internal int CoastingTooFastForceBrakeNotch;
			
			/// <summary>
			/// Create an ATO profile for desired behaviour whilst running in ATO with this profile. 
			/// </summary>
			/// <param name="holdSpeedNotCoasting">Set whether the train should continue to accelerate to keep the speed in sections intentionally for coasting. </param>
			/// <param name="stopAcceleratingSpeed">Set the speed difference with permitted speed the train should stop accelerating. Therefore, the train should stop accelerating when train's speed &gt;= permitted speed + value of this variable.</param>
			/// <param name="stopBrakingSpeed">Set the speed difference with target speed the train should stop braking. Therefore, the train should stop braking when train's speed &lt;= target speed + value of this variable.</param>
			/// <param name="coastingTooSlowForcePowerSpeed">Set the speed difference with permitted speed the train should re-accelerate back to permitted speed. Therefore, the train should re-accelerate when train's speed &lt;= permitted speed + value of this variable.</param>
			/// <param name="coastingTooFastForceBrakeSpeed">(Currently, this variable does not have any effect to behaviours of ATO. )</param>
			/// <param name="coastingTooFastForceBrakeNotch">(Currently, this variable does not have any effect to behaviours of ATO. )</param>
			public Profile(bool holdSpeedNotCoasting, 
			               double stopAcceleratingSpeed, 
			               double stopBrakingSpeed, 
			               double coastingTooSlowForcePowerSpeed, 
			               double coastingTooFastForceBrakeSpeed, 
			               int coastingTooFastForceBrakeNotch)
			{
				if (coastingTooSlowForcePowerSpeed > 0)
					throw new System.ArgumentOutOfRangeException("coastingTooSlowForcePowerSpeed", coastingTooSlowForcePowerSpeed, "Value must be greater than " + 0);
				if (coastingTooFastForceBrakeSpeed < 0)
					throw new System.ArgumentOutOfRangeException("coastingTooFastForcePowerSpeed", coastingTooFastForceBrakeSpeed, "Value must be less than " + 0);
				if (coastingTooFastForceBrakeNotch < 0)
					throw new System.ArgumentOutOfRangeException("coastingTooFastForceBrakeNotch", coastingTooFastForceBrakeNotch, "Value must be less than " + 0);
				this.HoldSpeedNotCoasting = holdSpeedNotCoasting;
				this.StopAcceleratingSpeed = stopAcceleratingSpeed;
				this.StopBrakingSpeed = stopBrakingSpeed;
				this.CoastingTooSlowForcePowerSpeed = coastingTooSlowForcePowerSpeed;
				this.CoastingTooFastForcePowerSpeed = coastingTooFastForceBrakeSpeed;
				this.CoastingTooFastForceBrakeNotch = coastingTooFastForceBrakeNotch;
			}
		}
		
		private static Profile[] atoProfiles = 
		{
			new Profile(false,
			            -2, 
			            +0,
			            -25,
			            +3,
			            3),
			new Profile(true, 
			            0,
			            +0,
			            -10,
			            +3,
			            3)
		};
		
		private int profileID = 0;
	}
}