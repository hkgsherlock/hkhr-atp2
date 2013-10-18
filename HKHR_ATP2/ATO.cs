using System;
using OpenBveApi.Runtime;

namespace HKHR_ATP2
{
	/// <summary>
	/// Description of ATO.
	/// </summary>
	internal partial class ATO
	{
		private bool ATOFail = false;
		private bool ATOStarted = false;
		enum ATOStates { POWER, COASTING, HOLDSPEED, BRAKE }
		ATOStates ATOCurrentState = ATOStates.BRAKE;
		
		private int[] Panel;
		
		private int handlePower;
		private int handleBrake;
		private bool doorCls = true;
		
		VehicleSpecs vSpec;
		ATP2.ATPElapseData vState;
		
		public void Load(LoadProperties properties)
		{
			Panel = properties.Panel;
		}
		
		internal void SetVehicleSpecs(VehicleSpecs specs) {
			vSpec = specs;
		}
		
		internal void Elapse(ATP2.ATPElapseData data)
		{
			System.Windows.Forms.MessageBox.Show("ATO is not yet implemeted. Please switch back to any other modes to drive the train.");
			return;
			
			vState = data;
			
			// fail the ATO if handles fail whilst ATO running
			if (ATOStarted && 
			   	vState.ElapseData.Handles.PowerNotch != 0 && 
			   	vState.ElapseData.Handles.BrakeNotch != vSpec.BrakeNotches + 1)
			{
				ATOFail = true;
			}
			
			// reset fail only if (1) ATO is not start and (2) train is steady and (3) handles are OK and (4) ATO is on fail state
			if (ATOFail && !ATOStarted && 
			    vState.ElapseData.Vehicle.Speed.KilometersPerHour == 0 && 
			   	vState.ElapseData.Handles.PowerNotch == 0 && 
			   	vState.ElapseData.Handles.BrakeNotch == vSpec.BrakeNotches + 1)
			{
				ATOFail = false;
			}
			
			// how is ATO working?
			if (ATOStarted) {
				#region Analysis and Switch States
				#endregion
				
				#region Behaviour
				switch (ATOCurrentState) {
					case ATOStates.POWER:
						
						break;
					case ATOStates.COASTING:
						if (HoldSpeedNotCoasting)
							goto case ATOStates.HOLDSPEED;
						
						// TODO:
						break;
					case ATOStates.HOLDSPEED:
						
						break;
					case ATOStates.BRAKE:
						
						break;
				}
				#endregion
			}
		}
		
		internal void SetPower(int powerNotch) {
			handlePower = powerNotch;
		}
		
		internal void SetBrake(int brakeNotch) {
			handleBrake = brakeNotch;
		}
		
		internal void DoorChange(DoorStates oldState, DoorStates newState) {
			if (newState == DoorStates.None) {
				doorCls = true;
			} else {
				doorCls = false;
			}
		}
		
		internal void KeyDown(VirtualKeys key) {
			switch (key) {
				case VirtualKeys.S:
					if (vState.ElapseData.Vehicle.Speed.KilometersPerHour == 0 && 
					    doorCls && 
					   	vState.ElapseData.Handles.PowerNotch == 0 && 
					   	vState.ElapseData.Handles.BrakeNotch == vSpec.BrakeNotches + 1)
					{
						ATOCurrentState = ATOStates.POWER;
						ATOStarted = true;
					}
					break;
				case VirtualKeys.L:
						ATOFail = true;
					break;
			}
		}
	}
}
