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
		
		#region Acceleration Rate calculated by ATO itself
		double ATO_AccelerationRate = 0.0;
		Time lastTimeRefreshATOAccelerationRate = new Time(0);
		double lastTimeRefreshATOAccelerationRate_SpeedKMHS = 0.0; 
		#endregion
		
		#region ATO results on current frame
		int ATOPower = 0;
		int ATOBrake = 0;
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
		
		#region track gradient memory
		private TrackGradientMemory.GradientPointCollection gPtMem = new TrackGradientMemory.GradientPointCollection();
		
		internal void WriteRefreshedGradientPointCollection(TrackGradientMemory.GradientPointCollection pGPts)
		{
			this.gPtMem = pGPts;
		}
		#endregion
		
		#region train brake rates, delays consts, length per car
		double MaxSvcBrkRate = 0.0;
		/// <summary>
		/// Acceleration rates of each brake notches calculated by fetching data in Train.dat. The unit of values in this array is km/h/s. <br/>Values in this array should always be negative numbers.		
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
		
		#region don't decelerate if speed is too low
		// TODO: resist braking notch
		int resistBrakingNotch = 0;
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
			
			if (vState.TotalTime.Milliseconds >= lastTimeRefreshATOAccelerationRate.Milliseconds + 100) {
				ATO_AccelerationRate = (vState.Vehicle.Speed.KilometersPerHour - lastTimeRefreshATOAccelerationRate_SpeedKMHS) / (vState.TotalTime.Milliseconds - lastTimeRefreshATOAccelerationRate.Milliseconds) * 1000;
				
				lastTimeRefreshATOAccelerationRate_SpeedKMHS = vState.Vehicle.Speed.KilometersPerHour;
				lastTimeRefreshATOAccelerationRate = vState.TotalTime;
			}
			vState.DebugMessage = "Accel:" + ATO_AccelerationRate + "km/h/s; " + vState.DebugMessage;
			
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
					ATOPower = 0;
					ATOBrake = vSpec.BrakeNotches + 1;
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
							
							ATOPower = vSpec.PowerNotches;
							ATOBrake = 0;
							break;
						case ATOStates.COASTING:
							if (HoldSpeedNotCoasting)
							{
								goto case ATOStates.KEEPSPEED;
							}
							
							#region brake to hold speed?
							// if new to coasting state, reset brake notch
							if(lastFrame_ATOState != ATOCurrentState)
								ATOBrake = BrakeToHoldSpeedNotch = 0;
							
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
								if (vState.Vehicle.Speed.KilometersPerHour - last1s_Speed > 0) { // accelerating
									for (int i = BrakeRate.Length - 1; i >= 0; i--) {
										if (vState.Vehicle.Speed.KilometersPerHour - last1s_Speed >= BrakeRate[i] * -1) {
											ATOPower = 0;
											ATOBrake = BrakeToHoldSpeedNotch = i;
											break;
										}
									}
								} else {
									ATOBrake = BrakeToHoldSpeedNotch = 0;
								}
								lastCheckSpeedCoasting = vState.TotalTime;
								last1s_Speed = vState.Vehicle.Speed.KilometersPerHour;
								last1s_PowerNotch = ATOPower;
								last1s_BrakeNotch = ATOBrake;
							}
							#endregion
							#region slight slope that above command cannot detect
							else if (vState.Vehicle.Speed.KilometersPerHour <= data.CurrentPermittedSpeed && slightSlopeBrakeEnabled) {
								slightSlopeBrakeEnabled = false;
								ATOPower = 0;
								ATOBrake = BrakeToHoldSpeedNotch = 0;
							}
							else if (vState.Vehicle.Speed.KilometersPerHour >= data.CurrentPermittedSpeed + 4) {
								vState.DebugMessage += "(S)";
								slightSlopeBrakeEnabled = true;
								ATOPower = 0;
								ATOBrake = BrakeToHoldSpeedNotch = 3;
							}
							else if (vState.Vehicle.Speed.KilometersPerHour >= data.CurrentPermittedSpeed + 3) {
								vState.DebugMessage += "(S)";
								slightSlopeBrakeEnabled = true;
								ATOPower = 0;
								ATOBrake = BrakeToHoldSpeedNotch = 2;
							}
							else if (vState.Vehicle.Speed.KilometersPerHour >= data.CurrentPermittedSpeed + 2) {
								vState.DebugMessage += "(S)";
								slightSlopeBrakeEnabled = true;
								ATOPower = 0;
								ATOBrake = BrakeToHoldSpeedNotch = 1;
							}
							#endregion
							
							if (BrakeToHoldSpeedNotch > 0)
								vState.DebugMessage += "(" + (vState.Vehicle.Speed.KilometersPerHour - last1s_Speed) + ")BrakeToHoldSpeed(" + BrakeToHoldSpeedNotch + "),";
							if (slightSlopeBrakeEnabled)
								vState.DebugMessage += "slightSlopeBrakeEnabled,";
							
							// TODO: power if too low speed?
							else if (vState.Vehicle.Speed.KilometersPerHour <= data.CurrentPermittedSpeed + CoastingTooSlowForcePowerSpeed) {
								ATOCurrentState = ATOStates.POWER;
								goto case ATOStates.POWER;
							}
							else {
								ATOPower = 0;
								ATOBrake = BrakeToHoldSpeedNotch;
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
							
							ATOPower = 0;
							
							// TODO: deceleration baused by gradient
							//
							
							for (int i = 0; i < BrakeRate.Length; i++) {
								// brake rates are negative numbers
								if (Math.Abs(BrakeRate[i]) >= Math.Abs(AccelerationPhysics.GetAccelerationRate(data.CurrentTargetSpeed,
								                                           	vState.Vehicle.Speed.KilometersPerHour, 
								                                           	data.NextSpeedFlag.StartingLocation - vState.Vehicle.Location)))
								{
									ATOBrake = i;
									break;
								}
							}
							break;
						case ATOStates.BRAKE_STOP:
							ATOPower = 0;
							
							// TODO: more accurate calculation for gradient
							#region deceleration caused by gradient
							// Rate = 1000 * Y / X
							double pitchInPerMill = gPtMem[gPtMem.CurrentIndex(vState.Vehicle.Location)].Pitch;
							// Acceleration due to gravity is 9.79 ms^-2
							double decelerationCausedByGradient = 9.79 / (1 / (pitchInPerMill / 1000));
							vState.DebugMessage += "pitch: " + pitchInPerMill + "; DCBG: " + decelerationCausedByGradient + ", ";
							#endregion
							
							#region calculation
							for (int i = 0; i < BrakeRate.Length; i++) {
								// TODO: lighter brake when 5m
								// brake rates are negative numbers
								if (Math.Abs(BrakeRate[i]) + decelerationCausedByGradient  >= Math.Abs(AccelerationPhysics.GetAccelerationRate(0,
								                                            vState.Vehicle.Speed.KilometersPerHour, 
								                                            Stations[lastCompletedDockingIndex + 1].StopPosition - vState.Vehicle.Location)))
								{
									ATOBrake = i;
									break;
								}
							}
							#endregion
							
							#region don't decelerate if speed is too low
							if (vState.Vehicle.Speed.KilometersPerHour < data.CurrentPermittedSpeed - 4 && ATOBrake > 0) {
								ATOBrake -= 2;
							} else if (vState.Vehicle.Speed.KilometersPerHour < data.CurrentPermittedSpeed - 2 && ATOBrake > 0) {
								ATOBrake--;
							} else {
								
							}
							#endregion
							
							break;
					}
				}
				#endregion
			} else {
				ATOPower = 0;
			   	ATOBrake = vSpec.BrakeNotches;
			}
			
			vState.Handles.PowerNotch = ATOPower;
			vState.Handles.BrakeNotch = ATOBrake; 
			
			lastFrame_ATOState = ATOCurrentState;
			lastFrame_Speed = vState.Vehicle.Speed.KilometersPerHour;
			lastFrame_PowerNotch = ATOPower;
			lastFrame_BrakeNotch = ATOBrake;
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
