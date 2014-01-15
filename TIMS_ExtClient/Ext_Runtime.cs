using System;

namespace TIMS_ExtClient
{
	public class ElapseData
	{
		public Vehicle Vehicle = new Vehicle();
		public PrecedingVehicleState PrecedingVehicleState = null;
		public Handles Handles = new Handles();
		public Time TotalTime = new Time();
		public Time ElapsedTime = new Time();
		public string DebugMessage = null;
	}
	
	public class Vehicle
	{
		public double Location = 0.0;
		public Speed Speed = new Speed();
		public double BcPressure = 0.0;
		public double MrPressure = 0.0;
		public double ErPressure = 0.0;
		public double BpPressure = 0.0;
		public double SapPressure = 0.0;
	}
	
	public class Speed
	{
		public double MetersPerSecond { get; set; }
		public double KilmetersPerHoue { get; set; }
		public double MilesPerHour { get; set; }
	}
	
	public class Handles
	{
		public int Reverser { get; set; }
		public int PowerNotch { get; set; }
		public int BrakeNotch { get; set; }
		public bool ConstSpeed { get; set; }
	}
	
	public class PrecedingVehicleState
	{
		public double Location { get; set; }
		public double Distance { get; set; }
		public Speed Speed = new Speed();
	}
	
	public class Time
	{
		public double Seconds { get; set; }
		public double Milliseconds { get; set; }
	}
}