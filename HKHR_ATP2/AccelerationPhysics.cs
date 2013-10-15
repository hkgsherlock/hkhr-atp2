using System;

namespace HKHR_ATP2
{
	public static class AccelerationPhysics
	{
		public static double GetInitialSpeed(double finalSpeed, double accelerationRate, double displacement)
		{
			finalSpeed /= 3.6;
			accelerationRate /= 3.6;
			
			double initSpd1 = Math.Sqrt(finalSpeed * finalSpeed - 2 * accelerationRate * displacement) * 3.6;
			double initSpd2 = -1 * Math.Sqrt(finalSpeed * finalSpeed - 2 * accelerationRate * displacement) * 3.6;
			
			if (Double.IsNaN(initSpd1) && Double.IsNaN(initSpd2)) {
				return 0;
			} else if (Math.Abs(initSpd1) == Math.Abs(initSpd2) || initSpd1 > initSpd2) {
				return initSpd1;
			} else {
				return initSpd2;
			}
		}
		
		/// <summary>
		/// Calculate the displacement to accelerate or decelerate using specified acceleration rate, from initial speed to final speed.
		/// </summary>
		/// <param name="initialSpeed">The initial speed in km/h.</param>
		/// <param name="finalSpeed">The final speed in km/h.</param>
		/// <param name="accelerationRate">The acceleration rate in km/h/s.</param>
		/// <returns>The displacement in m.</returns>
		public static double GetDisplacement(double initialSpeed, double finalSpeed, double accelerationRate)
		{
			initialSpeed /= 3.6;
			finalSpeed /= 3.6;
			accelerationRate /= 3.6;
			return (finalSpeed * finalSpeed - initialSpeed * initialSpeed) / 2 / accelerationRate;
		}
	}
}
