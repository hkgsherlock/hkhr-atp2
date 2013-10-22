using System;
using OpenBveApi.Runtime;

namespace HKHR_ATP2
{
	/// <summary>
	/// Description of ATO.
	/// </summary>
	internal partial class ATO
	{
		private int[] Panel;
		
		#region ATO statuses
		private bool ATOFail = false;
		private bool ATOStarted = false;
		enum ATOStates { POWER, COASTING, KEEPSPEED, BRAKE_SPEED, BRAKE_STOP }
		ATOStates ATOCurrentState = ATOStates.BRAKE_STOP;
		#endregion
		
		#region stops mem
		private StationsMemory.StationsCollection Stations = new StationsMemory.StationsCollection();
		private int lastCompletedDockingIndex = -1;
		
		internal void WriteRefreshedStationsCollection(StationsMemory.StationsCollection stations)
		{
			this.Stations = stations;
		}
		
		internal void WriteRefreshedLastDockedIndex(int index)
		{
			this.lastCompletedDockingIndex = index;
		}
		#endregion
		
		#region train brake rates, delays consts, length per car
		double MaxSvcBrkRate = 0.0;
		double[] BrakeRate;
		double BrakeUpDelay = 0.0;
		#endregion
		
		#region train spec and states (incl. handles & dr_state)
		VehicleSpecs vSpec;
		ElapseData vState;
		
		private int handlePower;
		private int handleBrake;
		private bool doorCls = true;
		#endregion
		
		#region last frame mem
		private double lastFrame_PermittedSpeed = 0.0;
		private double lastFrame_EmergencyBrakeSpeed = 0.0;
		private double lastFrame_TargetSpeed = 0.0;
		
		private double lastFrame_Speed = 0.0;
		
		private int lasfFrame_PowerNotch = 0;
		private int lasfFrame_BrakeNotch = 0;
		#endregion
		
		public void Load(LoadProperties properties)
		{
			Panel = properties.Panel;
			
			string[] trainDat = System.IO.File.ReadAllLines(System.IO.Path.Combine(properties.TrainFolder, "train.dat"));
			
			// TODO: re-write trainDatParser
			
			bool inPerformanceTag = false;
			bool inDelayTag = false;
			for (int i = 0; i < trainDat.Length; i++) {
				if (trainDat[i] == "#PERFORMANCE") {
					inPerformanceTag = true;
				} else if (trainDat[i] == "#DELAY") {
					inDelayTag = true;
				}
				
				if (inPerformanceTag) {
					int cmt = trainDat[i+1].IndexOf(";");
					MaxSvcBrkRate = Convert.ToDouble(trainDat[i+1].Substring(0, cmt > -1 ? cmt : trainDat[i+1].Length).Replace(" ", ""));
					inPerformanceTag = false;
				} else if (inDelayTag) {
					int cmt = trainDat[i+1].IndexOf(";");
					BrakeUpDelay = Convert.ToDouble(trainDat[i+3].Substring(0, cmt > -1 ? cmt : trainDat[i+1].Length).Replace(" ", ""));
					inDelayTag = false;
				}
			}
		}
		
		internal void SetVehicleSpecs(VehicleSpecs specs) {
			vSpec = specs;
			
			double[] newBrakeRate = new Double[vSpec.BrakeNotches + 1];
			for (int i = 0; i < vSpec.BrakeNotches + 1; i++) {
				newBrakeRate[i] = -1 * MaxSvcBrkRate / vSpec.BrakeNotches * i;
			}
			BrakeRate = newBrakeRate;
		}
		
		internal void Elapse(ATP2.ATPElapseData data)
		{
			vState = data.ElapseData;
			
			Handles atpHandlesResult = data.ElapseData.Handles;
			
			// fail the ATO if handles fail whilst ATO running
			if (ATOStarted && 
			   	vState.Handles.PowerNotch != 0 && 
			   	vState.Handles.BrakeNotch != vSpec.BrakeNotches)
			{
				ATOFail = true;
			}
			
			// reset fail only if (1) ATO is not start and (2) train is steady and (3) handles are OK and (4) ATO is on fail state
			if (ATOFail && !ATOStarted && 
			    vState.Vehicle.Speed.KilometersPerHour == 0 && 
			   	vState.Handles.PowerNotch == 0 && 
			   	vState.Handles.BrakeNotch == vSpec.BrakeNotches)
			{
				ATOFail = false;
			}
			
			// what will happen if ATO fails?
			if (ATOFail) {
				vState.DebugMessage = "!ATO FAIL!";
				if (vState.Vehicle.Speed.KilometersPerHour == 0) {
					ATOFail = false;
					ATOStarted = false;
				} else {
					// emg beep beep beep
					vState.Handles.PowerNotch = 0;
					vState.Handles.BrakeNotch = vSpec.BrakeNotches + 1;
				}
			}
			// how is ATO working?
			else if (ATOStarted) {
				// jump to stop working if not powering and stopped
				if (vState.Vehicle.Speed.KilometersPerHour == 0 && ATOCurrentState > ATOStates.POWER) {
					ATOStarted = false;
				}
				
				#region Analysis and Switch States
				// brake dure to arriving at next stop
				// if next stop is available
				if (lastCompletedDockingIndex + 1 < Stations.Count) {
					// if next stop should stop
					if (Stations[lastCompletedDockingIndex + 1].DoorOpen > -2) {
						// arriving at platform
						if (Stations[lastCompletedDockingIndex + 1].StopPosition - vState.Vehicle.Location <=
						    AccelerationPhysics.GetDisplacement(data.CurrentPermittedSpeed, 0, BrakeRate[ATP2.DefaultNormalBrakeNotch])
						   ) {
							ATOCurrentState = ATOStates.BRAKE_STOP;
						}
					}
				}
				// brake due to next speed flag
				else if (data.CurrentTargetSpeed < lastFrame_TargetSpeed) {
					ATOCurrentState = ATOStates.BRAKE_SPEED;
				}
				// accelerate
				else if (data.CurrentPermittedSpeed > lastFrame_PermittedSpeed) {
					ATOCurrentState = ATOStates.POWER;
				}
				
				// remember things for analysis on next frame
				lastFrame_PermittedSpeed = data.CurrentPermittedSpeed;
				lastFrame_EmergencyBrakeSpeed = data.CurrentEmergencyBrakeSpeed;
				lastFrame_TargetSpeed = data.CurrentTargetSpeed;
				#endregion
				
				#region Behaviour
				if (data.ATPFail) {
					ATOFail = true;
				} else if (!data.ATPBrakeApplying) {
					switch (ATOCurrentState) {
						case ATOStates.POWER:
							if (vState.Vehicle.Speed.KilometersPerHour >= data.CurrentPermittedSpeed + StopAcceleratingSpeed)
							{
								ATOCurrentState = ATOStates.COASTING;
								goto case ATOStates.COASTING;
							}
							
							vState.Handles.PowerNotch = vSpec.PowerNotches;
							vState.Handles.BrakeNotch = 0;
							break;
						case ATOStates.COASTING:
							if (HoldSpeedNotCoasting)
							{
								goto case ATOStates.KEEPSPEED;
							}
							
							// TODO: brake to hold speed?
							if (vState.Vehicle.Speed.KilometersPerHour >= data.CurrentPermittedSpeed + CoastingTooFastForcePowerSpeed) {
								vState.Handles.PowerNotch = 0;
								vState.Handles.BrakeNotch = CoastingTooFastForceBrakeNotch;
							}
							// TODO: power if too low speed?
							else if (vState.Vehicle.Speed.KilometersPerHour <= data.CurrentPermittedSpeed + CoastingTooSlowForcePowerSpeed) {
								ATOCurrentState = ATOStates.POWER;
								goto case ATOStates.POWER;
							}
							else {
								vState.Handles.PowerNotch = 0;
								vState.Handles.BrakeNotch = 0;
							}
							break;
						case ATOStates.KEEPSPEED:
							throw new NotImplementedException();
//							break;
						case ATOStates.BRAKE_SPEED:
							if (vState.Vehicle.Speed.KilometersPerHour <= data.CurrentPermittedSpeed + StopBrakingSpeed)
							{
								ATOCurrentState = ATOStates.COASTING;
								goto case ATOStates.COASTING;
							}
							
							vState.Handles.PowerNotch = 0;
							for (int i = 0; i < BrakeRate.Length; i++) {
								// brake rates are negative numbers
								if (Math.Abs(BrakeRate[i]) >= Math.Abs(AccelerationPhysics.GetAccelerationRate(data.CurrentTargetSpeed,
								                                           	vState.Vehicle.Speed.KilometersPerHour, 
								                                           	data.NextSpeedFlag.StartingLocation - vState.Vehicle.Location)))
								{
									vState.Handles.BrakeNotch = i;
									break;
								}
							}
							break;
						case ATOStates.BRAKE_STOP:
							vState.Handles.PowerNotch = 0;
							for (int i = 0; i < BrakeRate.Length; i++) {
								// brake rates are negative numbers
								if (Math.Abs(BrakeRate[i]) >= Math.Abs(AccelerationPhysics.GetAccelerationRate(0,
								                                            vState.Vehicle.Speed.KilometersPerHour, 
								                                            Stations[lastCompletedDockingIndex + 1].StopPosition - vState.Vehicle.Location)))
								{
									vState.Handles.BrakeNotch = i;
									break;
								}
							}
							break;
					}
				}
				#endregion
			} else {
				vState.Handles.PowerNotch = 0; 
			   	vState.Handles.BrakeNotch = vSpec.BrakeNotches;
			}
			
			lastFrame_Speed = vState.Vehicle.Speed.KilometersPerHour;
			lasfFrame_PowerNotch = vState.Handles.PowerNotch;
			lasfFrame_BrakeNotch = vState.Handles.BrakeNotch;
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
					if (vState.Vehicle.Speed.KilometersPerHour == 0 && 
					    doorCls && 
					   	vState.Handles.PowerNotch == 0 && 
					   	vState.Handles.BrakeNotch == vSpec.BrakeNotches)
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
