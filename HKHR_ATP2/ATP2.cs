using System;
using System.Collections.Generic;
using OpenBveApi.Runtime;

namespace HKHR_ATP2
{
	/// <summary>
	/// Description of ATP2.
	/// </summary>
	internal partial class ATP2
	{
		private bool OnCabSignallingSection;
		
		private int handlePower;
		private int handleBrake;
		private bool doorCls = true; // tested. on default, doors are closed. If first stop should open the door, DoorChange will be called to change status
		
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
		double LengthPerCar = 0.0;
		#endregion
		
		#region SpeedFlag, SpeedFlagCollection classes definitions
		internal class SpeedFlag
		{
			internal int Speed = 0;
			internal int StartingLocation = 0;
			
			public SpeedFlag(int speed, int startingLocation)
			{
				this.Speed = speed;
				this.StartingLocation = startingLocation;
			}
		}
		
		internal class SpeedFlagsCollection
		{
			private System.Collections.Generic.List<SpeedFlag> _speedFlags = new System.Collections.Generic.List<SpeedFlag>();
			
			public SpeedFlag this[int index]
			{
				get
				{
					if (index == -1) {
						return new SpeedFlag(int.MaxValue, int.MinValue);
					} else if (index > -1 && index < _speedFlags.Count) {
						return _speedFlags[index];
					} else {
						throw new ArgumentOutOfRangeException();
					}
				}
				
				set
				{
					value = _speedFlags[index];
				}
			}
			
			public int Count
			{
				get
				{
					return _speedFlags.Count;
				}
			}
			
			public void Add(SpeedFlag item)
			{
				_speedFlags.Add(item);
			}
			
			public void AddRange(SpeedFlag[] item)
			{
				for (int i = 0; i < item.Length; i++) {
					this.Add(item[i]);
				}
			}
			
			public int VlookupCurrentIndex(double location)
			{
				for (int i = 0; i < this._speedFlags.Count; i++) {
					if (i+1 == _speedFlags.Count) {
						return _speedFlags.Count - 1;
					} else if (this._speedFlags[i+1].StartingLocation >= location) {
						return i;
					}
				}
				
				return -1;
			}
			
			private static int SortSpeedFlags(SpeedFlag x, SpeedFlag y)
			{
				if (x.StartingLocation < y.StartingLocation) {
					return -1;
				} else if (x.StartingLocation == y.StartingLocation) {
					return 1;
				} else {
					if (x.Speed < y.Speed) {
						return -1;
					} else if (x.Speed > y.Speed) {
						return 0;
					} else {
						return 1;
					}
				}
			}
			
			internal void Sort()
			{
				this._speedFlags.Sort(SortSpeedFlags);
			}
		}
		#endregion
		
		#region ATPElapseData class definition
		/// <summary>
		/// Represents data given to any other post-processing classes like ATO in the ATP2.Elapse call. <br />
		/// It also retrieves the data give from the OpenBveApi. 
		/// </summary>
		internal class ATPElapseData
		{
			/// <summary>
			/// States that the ATP cab signalling is working on current frame and location. 
			/// </summary>
			internal bool OnCabSignallingSection;
			/// <summary>
			/// Represents the current permitted speed determined by the ATP system.
			/// </summary>
			internal double? CurrentPermittedSpeed = null;
			/// <summary>
			/// Gets the speed flag in which train is using currently.
			/// </summary>
			internal SpeedFlag CurrentSpeedFlag;
			/// <summary>
			/// Gets the next speed flag in which train will use.
			/// </summary>
			internal SpeedFlag NextSpeedFlag;
			/// <summary>
			/// Represents the retrieved data given to the plugin in the Elapse call.
			/// </summary>
			internal ElapseData ElapseData;
		}
		#endregion
		
		VehicleSpecs vSpec;
		int[] Panel;
		SpeedFlagsCollection SpeedFlags;
		bool ATPFail;
		int cSpeedFlagIndexFront = -1;
		int cSpeedFlagIndexRear = -1;
		int cSpeedFlagIndexWholeTrain = -1;
		
		internal ATP2()
		{
			SpeedFlags = new SpeedFlagsCollection();
			ATPFail = false;
		}
		
		internal void Load(LoadProperties properties) {
			Panel = properties.Panel;
			
			string[] trainDat = System.IO.File.ReadAllLines(System.IO.Path.Combine(properties.TrainFolder, "train.dat"));
			
			// re-write trainDatParser
			
			bool inPerformanceTag = false;
			bool inDelayTag = false;
			bool inCarTag = false;
			for (int i = 0; i < trainDat.Length; i++) {
				if (trainDat[i] == "#PERFORMANCE") {
					inPerformanceTag = true;
				} else if (trainDat[i] == "#DELAY") {
					inDelayTag = true;
				} else if (trainDat[i] == "#CAR") {
					inCarTag = true;
				}
//				else if (!trainDat[i].StartsWith(";")) {
//					
//				}
				
				if (inPerformanceTag) {
					int cmt = trainDat[i+1].IndexOf(";");
					MaxSvcBrkRate = Convert.ToDouble(trainDat[i+1].Substring(0, cmt > -1 ? cmt : trainDat[i+1].Length).Replace(" ", ""));
					inPerformanceTag = false;
				} else if (inDelayTag) {
					int cmt = trainDat[i+1].IndexOf(";");
					BrakeUpDelay = Convert.ToDouble(trainDat[i+3].Substring(0, cmt > -1 ? cmt : trainDat[i+1].Length).Replace(" ", ""));
					inDelayTag = false;
				} else if (inCarTag) {
					int cmt = trainDat[i+1].IndexOf(";");
					LengthPerCar = Convert.ToDouble(trainDat[i+5].Substring(0, cmt > -1 ? cmt : trainDat[i+1].Length).Replace(" ", ""));
					inCarTag = false;
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
		
		internal ATPElapseData Elapse(ElapseData vState)
		{
			ATPElapseData atpElapseData = new ATPElapseData();
			
			double PermittedSpeed = 0.0;
			double EmergencyBrakeSpeed = 0.0;
			
			#region calculation part
			if (OnCabSignallingSection) {
				// yes we have singal now
				Panel[PanelID.StatusLEDs.NoSignal] = 0;
				#region atp speed part
				// === speed flag check on a frame ===
				// check if front of the train is at area with higher permitted speed, 
				// that of back of the train is at area with lower...
				// if so, continue to use the lower one until the whole train is in higher one
				cSpeedFlagIndexFront = SpeedFlags.VlookupCurrentIndex(vState.Vehicle.Location);
				cSpeedFlagIndexRear = SpeedFlags.VlookupCurrentIndex(vState.Vehicle.Location - vSpec.Cars * LengthPerCar);
				cSpeedFlagIndexWholeTrain = SpeedFlags[cSpeedFlagIndexRear].Speed < SpeedFlags[cSpeedFlagIndexFront].Speed ? cSpeedFlagIndexRear : cSpeedFlagIndexFront;
				
				PermittedSpeed = SpeedFlags[cSpeedFlagIndexWholeTrain].Speed;
				EmergencyBrakeSpeed = SpeedFlags[cSpeedFlagIndexWholeTrain].Speed + 5;
				// validate current speed flag
	//			if (cSpeedFlagIndexWholeTrain - 1 >= 0) {
	//				
	//			}
				
				// check next speed flag
				// TODO:
				Panel[PanelID.PermittedSpeed.NextFlagSpeed.IsShowing] = 0;
				Panel[PanelID.PermittedSpeed.NextFlagSpeed.Speedometer] = (int)PermittedSpeed;
				if (cSpeedFlagIndexWholeTrain > -1 && cSpeedFlagIndexWholeTrain + 1 < SpeedFlags.Count) {
				//if next flag exists
					SpeedFlag nextFlag = atpElapseData.NextSpeedFlag = SpeedFlags[cSpeedFlagIndexWholeTrain + 1];
					// if speed of next speed flag is lower than current one, 
					// and about to approrach, start to decelerate
					if (nextFlag.Speed < PermittedSpeed) {
						// TODO: decel curve w/ geo props
						if (nextFlag.StartingLocation - vState.Vehicle.Location <= 
						    AccelerationPhysics.GetDisplacement(PermittedSpeed, 
						                                        nextFlag.Speed, 
						                                        BrakeRate[DefaultNormalBrakeNotch])
						   ) {
							#region show next flag speed to left side LCD display
							Panel[PanelID.PermittedSpeed.NextFlagSpeed.IsShowing] = 1;
							Panel[PanelID.PermittedSpeed.NextFlagSpeed.Speedometer] = nextFlag.Speed;
							// TODO: 100, 10, 1 next flag speed
							#endregion
							PermittedSpeed = AccelerationPhysics.GetInitialSpeed(
								nextFlag.Speed, 
								BrakeRate[DefaultNormalBrakeNotch],
								nextFlag.Speed > 0 ? nextFlag.StartingLocation - vState.Vehicle.Location : nextFlag.StartingLocation - vState.Vehicle.Location - 10
							);
							EmergencyBrakeSpeed = AccelerationPhysics.GetInitialSpeed(
								nextFlag.Speed > 0 ? nextFlag.Speed + 5 : 0,
								BrakeRate[DefaultNormalBrakeNotch],
								nextFlag.Speed > 0 ? nextFlag.StartingLocation - vState.Vehicle.Location + 5 : nextFlag.StartingLocation - vState.Vehicle.Location - 5
							);
						}
					}
				} else {
					atpElapseData.NextSpeedFlag = null;
				}
				
				atpElapseData.CurrentPermittedSpeed = PermittedSpeed;
				atpElapseData.CurrentSpeedFlag = SpeedFlags[cSpeedFlagIndexWholeTrain];
				// === END of speed flag check on a frame ===
				#endregion
				#region limit speed when arriving station
				// if next stop is available
				if (lastCompletedDockingIndex + 1 < Stations.Count) {
					// if next stop should stop
					if (Stations[lastCompletedDockingIndex + 1].DoorOpen > -2) {
						// arriving at platform
						// overrun (+5m)
						if (vState.Vehicle.Location >= Stations[lastCompletedDockingIndex + 1].StopPosition + 5) {
							PermittedSpeed = 0;
							EmergencyBrakeSpeed = 0;
						} else if (vState.Vehicle.Location >= Stations[lastCompletedDockingIndex + 1].StopPosition && vState.Vehicle.Location <= Stations[lastCompletedDockingIndex + 1].StopPosition + 5) {
							PermittedSpeed = 0;
							EmergencyBrakeSpeed = Stations[lastCompletedDockingIndex + 1].StopPosition + 5 - vState.Vehicle.Location;
						} else if (Stations[lastCompletedDockingIndex + 1].StopPosition - vState.Vehicle.Location <=
						    AccelerationPhysics.GetDisplacement(PermittedSpeed, 0, BrakeRate[DefaultNormalBrakeNotch])
						   ) {
							#region show next flag speed to left side LCD display
							Panel[PanelID.PermittedSpeed.NextFlagSpeed.IsShowing] = 1;
							Panel[PanelID.PermittedSpeed.NextFlagSpeed.Speedometer] = 0;
							// TODO: 100, 10, 1 next flag speed
							#endregion
							PermittedSpeed = AccelerationPhysics.GetInitialSpeed(0, BrakeRate[DefaultNormalBrakeNotch], Stations[lastCompletedDockingIndex + 1].StopPosition - vState.Vehicle.Location);
							EmergencyBrakeSpeed = PermittedSpeed + 5;
						}
						
		//				// next stop is nearer than next speed flag
		//				if (cSpeedFlagIndexWholeTrain + 1 < SpeedFlags.Count &&
		//			    	Stations[lastCompletedDockingIndex + 1].StopPosition < SpeedFlags[cSpeedFlagIndexWholeTrain + 1].StartingLocation)
		//				{
		//					
		//				}
					}
				}
				#endregion
			} else {
				// no we don't have signal
				Panel[PanelID.StatusLEDs.NoSignal] = 0;
			}
			#endregion
			
			#region behaviours part
			#region output to left side LCD display
			Panel[PanelID.PermittedSpeed.Speedometer] = (int)PermittedSpeed;
			// TODO: 100, 10, 1 pspeed
			Panel[PanelID.PermittedSpeed.SpeedometerTimes100] = (int)(PermittedSpeed * 100);
			Panel[PanelID.PermittedSpeed.EmgSpeed.Speedometer] = (int)EmergencyBrakeSpeed;
			// TODO: 100, 10, 1 ebspeed
			Panel[PanelID.PermittedSpeed.EmgSpeed.SpeedometerTimes100] = (int)(EmergencyBrakeSpeed * 100);
			#endregion
			
			string permittedSpeedString = ((int)PermittedSpeed).ToString();
			if (doorCls) { // Door is closed
				if (ATPFail) {
					if (vState.Vehicle.Speed.KilometersPerHour == 0) {
						ATPFail = false;
					} else {
						// emg beep beep beep
						vState.Handles.PowerNotch = 0;
						vState.Handles.BrakeNotch = vSpec.BrakeNotches + 1;
					}
				} else {
					if (vState.Vehicle.Speed.KilometersPerHour > EmergencyBrakeSpeed) {
						ATPFail = true;
					} else {
						if (vState.Vehicle.Speed.KilometersPerHour > PermittedSpeed) {
							// beep beep beep
						} else {
							// turn off the beep beep beep
						}
						
					}
				}
			} else { // Door is open
				vState.Handles.PowerNotch = 0;
				vState.Handles.BrakeNotch = handleBrake > vSpec.BrakeNotches ? handleBrake : vSpec.BrakeNotches;
			}
			#endregion
			
			// return ok?(CabSignalling)
			atpElapseData.OnCabSignallingSection = OnCabSignallingSection;
			// No Signal? OR HKHR-ATP2 OK?
			if (OnCabSignallingSection) {
				Panel[PanelID.StatusLEDs.HKHR_ATP] = 1;
				Panel[PanelID.StatusLEDs.NoSignal] = 0;
			} else {
				Panel[PanelID.StatusLEDs.HKHR_ATP] = 0;
				Panel[PanelID.StatusLEDs.NoSignal] = 1;
			}
			// return atpElapseData
			atpElapseData.ElapseData = vState;
			
			return atpElapseData;
		}
		
		public void NotElapsing()
		{
			for (int i = PanelID.PermittedSpeed.Speedometer; i < PanelID.PermittedSpeed.NextFlagSpeed.SingleDigit; i++) {
				Panel[i] = 0;
			}
		}
		
		public void DoorChange(DoorStates oldState, DoorStates newState) {
			if (newState == DoorStates.None) {
				doorCls = true;
			} else {
				doorCls = false;
			}
		}
		
		internal void SetPower(int powerNotch) {
			handlePower = powerNotch;
		}
		
		internal void SetBrake(int brakeNotch) {
			handleBrake = brakeNotch;
		}
		
		public void SetBeacon(BeaconData beacon)
		{
			switch (beacon.Type) {
				case BeaconID.HKHRATP2.EnDisableCabSignal:
					OnCabSignallingSection = Convert.ToBoolean(beacon.Optional);
					break;
				case BeaconID.HKHRATP2.SpeedFlag:
					if (OnCabSignallingSection && beacon.Optional >= 0) {
						int speed = beacon.Optional / 1000000;
						int startloc = beacon.Optional - (speed * 1000000);
						SpeedFlags.Add(new SpeedFlag(speed, startloc));
						SpeedFlags.Sort();
					}
					break;
			}
			
		}
		
		public void SetSignal(SignalData[] signal) {
			// TODO: ATP2 set signal
		}
	}
}
