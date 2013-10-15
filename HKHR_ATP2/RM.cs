using System;
using OpenBveApi.Runtime;

namespace HKHR_ATP2
{
	/// <summary>
	/// Description of RM.
	/// </summary>
	internal class RM
	{
		int[] Panel;
		VehicleSpecs vSpec;
		
		private int handlePower = -1;
		private int handleBrake = -1;
		
		enum SignalPosts { Red, Amber, Green }
		int[] SignalSpeed = { 0, 25, 45 };
		int[] SignalCoastingSpeed = { 0, 20, 40 };
		bool BrakeInForce = false;
		SignalPosts currentSignalPost;
		
		internal RM()
		{
			currentSignalPost = SignalPosts.Amber;
		}
		
		public void Load(LoadProperties properties)
		{
			Panel = properties.Panel;
		}
		
		internal void SetVehicleSpecs(VehicleSpecs specs) {
			vSpec = specs;
		}
		
		internal void Elapse(ElapseData data)
		{
			Panel[PanelID.StatusLEDs.NoSignal] = 1; // assume as no signal
			Panel[PanelID.StatusLEDs.RM] = 1; // in RM mode
			Panel[PanelID.StatusLEDs.RM_Reverse] = data.Handles.Reverser == -1 ? 1 : 0; // in RM-B mode?
			
			if (data.Vehicle.Speed.KilometersPerHour > 45 || data.Vehicle.Speed.KilometersPerHour > SignalSpeed[(int)currentSignalPost]) {
				data.Handles.PowerNotch = 0;
				data.Handles.BrakeNotch = vSpec.BrakeNotches;
				BrakeInForce = true;
			}
			else if (BrakeInForce && data.Vehicle.Speed.KilometersPerHour <= SignalCoastingSpeed[(int)currentSignalPost]) {
				data.Handles.PowerNotch = 0;
				data.Handles.BrakeNotch = 0;
			}
			else if (!BrakeInForce && data.Vehicle.Speed.KilometersPerHour > SignalCoastingSpeed[(int)currentSignalPost]) {
				data.Handles.PowerNotch = 0;
				data.Handles.BrakeNotch = 0;
			}
		}
		
		internal void SetPower(int powerNotch) {
			handlePower = powerNotch;
		}
		
		internal void SetBrake(int brakeNotch) {
			handleBrake = brakeNotch;
		}
		
		internal void SetSignal(SignalData[] signal)
		{
//			switch (signal[0].Aspect) {
//				case :
//					
//					break;
//			}
		}
	}
}
