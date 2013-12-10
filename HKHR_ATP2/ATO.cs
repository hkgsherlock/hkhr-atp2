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
		/// <summary>
		/// Brake rates of each brake notches calculated by fetching data in Train.dat. The unit of values in this array is km/h/s. 
		/// </summary>
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
		
		#region brake to hold speed
		Time lastCheckSpeedCoasting = new Time(0);
		
		int BrakeToHoldSpeedNotch = 0;
		#endregion
		
		#region last frame mem
		private ATOStates lastFrame_ATOState = ATOStates.BRAKE_STOP;
		
		private double lastFrame_Speed = 0.0;
		
		private int lastFrame_PowerNotch = 0;
		private int lastFrame_BrakeNotch = 0;
		#endregion
		
		#region last 500ms mem
		private double last1s_Speed = 0.0;
		private int last1s_PowerNotch = -1;
		private int last1s_BrakeNotch = -1;
		#endregion
		
		#region prev tspeed, pspeed, ebspeed
		double prevTargetSpeed = 0.0;
		double prevPermittedSpeed = 0.0;
		double prevEmergencyBrakeSpeed = 0.0;
		#endregion
		
		#region slight slope
		bool slightSlopeBrakeEnabled = false;
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
				// brake due to next speed flag
				if (data.CurrentTargetSpeed < prevTargetSpeed) {
					ATOCurrentState = ATOStates.BRAKE_SPEED;
				}
				// accelerate
				else if (data.CurrentPermittedSpeed > prevPermittedSpeed) {
					ATOCurrentState = ATOStates.POWER;
				}
				// brake due to arriving at next stop
				// if next stop is available
				else if (lastCompletedDockingIndex + 1 < Stations.Count) {
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
				
				// remember things for analysis on next frame
				if (data.CurrentTargetSpeed != prevTargetSpeed)
					prevTargetSpeed = data.CurrentTargetSpeed;
				
				if (data.CurrentPermittedSpeed != prevPermittedSpeed)
					prevPermittedSpeed = data.CurrentPermittedSpeed;
				
				if (data.CurrentEmergencyBrakeSpeed != prevEmergencyBrakeSpeed)
					prevEmergencyBrakeSpeed = data.CurrentEmergencyBrakeSpeed;
				#endregion
				
				#region Behaviour
				if (data.ATPFail) {
					ATOFail = true;
				} else if (!data.ATPBrakeApplying) {
					data.ElapseData.DebugMessage += ATOCurrentState.ToString() + ", ";
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
							
							#region brake to hold speed?
							// if new to coasting state, reset brake notch
							if(lastFrame_ATOState != ATOCurrentState)
								vState.Handles.BrakeNotch = BrakeToHoldSpeedNotch = 0;
							
							// if notches are interrupted, then recount the timer
							if (lastFrame_ATOState != ATOCurrentState || 
								last1s_PowerNotch != lastFrame_PowerNotch || last1s_BrakeNotch != lastFrame_BrakeNotch) {
//								System.Windows.Forms.MessageBox.Show(lastCheckSpeedCoasting + "");
								lastCheckSpeedCoasting = vState.TotalTime;
								last1s_Speed = vState.Vehicle.Speed.KilometersPerHour;
								last1s_PowerNotch = lastFrame_BrakeNotch;
								last1s_BrakeNotch = lastFrame_BrakeNotch;
							}
							
							if (vState.TotalTime.Milliseconds >= lastCheckSpeedCoasting.Milliseconds + 1000) {
								if (vState.Vehicle.Speed.KilometersPerHour - last1s_Speed > 0) {
									for (int i = BrakeRate.Length - 1; i >= 0; i--) {
//										System.Windows.Forms.MessageBox.Show((Math.Abs(last500ms_Speed - vState.Vehicle.Speed.KilometersPerHour) / 0.5 >= Math.Abs(BrakeRate[i])) + "");
										if (vState.Vehicle.Speed.KilometersPerHour - last1s_Speed >= BrakeRate[i]) {
											vState.Handles.PowerNotch = 0;
											vState.Handles.BrakeNotch = BrakeToHoldSpeedNotch = i;
											break;
										}
									}
								}
								lastCheckSpeedCoasting = vState.TotalTime;
								last1s_Speed = vState.Vehicle.Speed.KilometersPerHour;
								last1s_PowerNotch = vState.Handles.PowerNotch;
								last1s_BrakeNotch = vState.Handles.BrakeNotch;
							}
							#endregion
							#region slight slope that above command cannot detect
							else if (vState.Vehicle.Speed.KilometersPerHour <= data.CurrentPermittedSpeed && slightSlopeBrakeEnabled) {
								slightSlopeBrakeEnabled = false;
								vState.Handles.PowerNotch = 0;
								vState.Handles.BrakeNotch = BrakeToHoldSpeedNotch = 0;
							}
							else if (vState.Vehicle.Speed.KilometersPerHour >= data.CurrentPermittedSpeed + 4) {
								slightSlopeBrakeEnabled = true;
								vState.Handles.PowerNotch = 0;
								vState.Handles.BrakeNotch = BrakeToHoldSpeedNotch = 3;
							}
							else if (vState.Vehicle.Speed.KilometersPerHour >= data.CurrentPermittedSpeed + 3) {
								slightSlopeBrakeEnabled = true;
								vState.Handles.PowerNotch = 0;
								vState.Handles.BrakeNotch = BrakeToHoldSpeedNotch = 2;
							}
							else if (vState.Vehicle.Speed.KilometersPerHour >= data.CurrentPermittedSpeed + 2) {
								slightSlopeBrakeEnabled = true;
								vState.Handles.PowerNotch = 0;
								vState.Handles.BrakeNotch = BrakeToHoldSpeedNotch = 1;
							}
							#endregion
							
							if (BrakeToHoldSpeedNotch > 0)
								vState.DebugMessage += "BrakeToHoldSpeed(" + BrakeToHoldSpeedNotch + "),";
							if (slightSlopeBrakeEnabled)
								vState.DebugMessage += "slightSlopeBrakeEnabled,";
							
							// TODO: power if too low speed?
							else if (vState.Vehicle.Speed.KilometersPerHour <= data.CurrentPermittedSpeed + CoastingTooSlowForcePowerSpeed) {
								ATOCurrentState = ATOStates.POWER;
								goto case ATOStates.POWER;
							}
							else {
								vState.Handles.PowerNotch = 0;
								vState.Handles.BrakeNotch = BrakeToHoldSpeedNotch;
							}
							break;
						case ATOStates.KEEPSPEED:
							throw new NotImplementedException();
//							break;
						case ATOStates.BRAKE_SPEED:
							if (vState.Vehicle.Speed.KilometersPerHour <= data.CurrentTargetSpeed + StopBrakingSpeed)
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
							
							if (vState.Vehicle.Speed.KilometersPerHour < data.CurrentPermittedSpeed - 3)
							{
								vState.Handles.BrakeNotch = 0;
								break;
							}
								
							for (int i = 0; i < BrakeRate.Length; i++) {
								// TODO: 
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
			
			lastFrame_ATOState = ATOCurrentState;
			lastFrame_Speed = vState.Vehicle.Speed.KilometersPerHour;
			lastFrame_PowerNotch = vState.Handles.PowerNotch;
			lastFrame_BrakeNotch = vState.Handles.BrakeNotch;
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
