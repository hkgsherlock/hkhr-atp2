namespace HKHR_ATP2
{
	internal partial class ATO
	{
		private bool HoldSpeedNotCoasting = false;
		private const double StopAcceleratingSpeed = -2;
		private const double StopBrakingSpeed = +0;
		private const double CoastingTooSlowForcePowerSpeed = -15;
		private const double CoastingTooFastForcePowerSpeed = +3;
		private const int CoastingTooFastForceBrakeNotch = 3;
	}
}