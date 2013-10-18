using System;
using OpenBveApi.Runtime;

namespace HKHR_ATP2
{
	/// <summary>
	/// Description of ModeSelector.
	/// </summary>
	internal class ModeSelector
	{
		bool trainFail = true;
		
		enum Modes {Off, RM_B, RM_F, PM, AM}
		Modes currentMode;
		VehicleSpecs vSpec;
		ElapseData vState;
		
//		int handleReverser = -1;
		int handlePower = -1;
		int handleBrake = -1;
		
		int[] Panel;
		
		// Mode Classes
		StationsMemory stationsMemory;
		Driverless driverless;
		RM rm;
		ATP2 atp2;
		ATO ato;
		
		internal void Load(LoadProperties properties) {
			Panel = properties.Panel;
			
			stationsMemory = new StationsMemory();
			stationsMemory.Load(properties);
			
			driverless = new Driverless();
			driverless.Load(properties);
			stationsMemory.OnStationDataRefreshed += new StationsMemory.NewStationDataReceiver(driverless.WriteRefreshedStationsCollection);
			stationsMemory.OnLastDockIndexRefreshed += new StationsMemory.LastDockIndexReceiver(driverless.WriteRefreshedLastDockedIndex);
			
			rm = new RM();
			rm.Load(properties);
			
			atp2 = new ATP2();
			atp2.Load(properties);
			stationsMemory.OnStationDataRefreshed += new StationsMemory.NewStationDataReceiver(atp2.WriteRefreshedStationsCollection);
			stationsMemory.OnLastDockIndexRefreshed += new StationsMemory.LastDockIndexReceiver(atp2.WriteRefreshedLastDockedIndex);
			
			ato = new ATO();
			ato.Load(properties);
//			driverless.OnDriverlessAvailableElapse += new CallDriverlessAvailableEventHandlers();
		}
		
		internal void SetVehicleSpecs(VehicleSpecs specs) {
			vSpec = specs;
			
			rm.SetVehicleSpecs(specs);
			atp2.SetVehicleSpecs(specs);
			ato.SetVehicleSpecs(specs);
		}
		
		internal void Initialize(InitializationModes mode) {
			trainFail = false;
			switch (mode) {
				case InitializationModes.OnService:
//					handleReverser = 1;
					handlePower = 0;
					handleBrake = vSpec.BrakeNotches;
					break;
				case InitializationModes.OnEmergency:
//					handleReverser = 1;
					handlePower = 0;
					handleBrake = vSpec.BrakeNotches + 1;
					break;
				case InitializationModes.OffEmergency:
//					handleReverser = 0;
					handlePower = 0;
					handleBrake = vSpec.BrakeNotches + 1;
					break;
			}
			
//			handlePower = driverSetPower;
//			handleBrake = driverSetBrake;
		}
		
		internal void Elapse(ElapseData data) {
			vState = data;
			
			stationsMemory.Elapse(data);
			
			#region Reset status LEDs
			Panel[PanelID.StatusLEDs.ATO] = 0;
			Panel[PanelID.StatusLEDs.ATP] = 0;
			Panel[PanelID.StatusLEDs.RM] = 0;
			Panel[PanelID.StatusLEDs.RM_Reverse] = 0;
			Panel[PanelID.StatusLEDs.HKHR_ATP] = 0;
			Panel[PanelID.StatusLEDs.NoSignal] = 0;
			#endregion
			
			#region reset ATP lamps to off if not using
			if ((int)currentMode < (int)Modes.PM)
				atp2.NotElapsing();
			#endregion
			// give privilege of controlling the train to different classes(modules) by state of mode currently using
			switch (currentMode) {
				case ModeSelector.Modes.Off:
					vState.Handles.Reverser = 0;
					vState.Handles.PowerNotch = 0;
					vState.Handles.BrakeNotch = vSpec.BrakeNotches + 1;
					driverless.Elapse(vState);
					break;
				case ModeSelector.Modes.RM_B:
					vState.Handles.Reverser = -1;
					vState.Handles.PowerNotch = handlePower;
					vState.Handles.BrakeNotch = handleBrake;
					rm.Elapse(vState);
					break;
				case ModeSelector.Modes.RM_F:
					vState.Handles.Reverser = 1;
					vState.Handles.PowerNotch = handlePower;
					vState.Handles.BrakeNotch = handleBrake;
					rm.Elapse(vState);
					break;
				case ModeSelector.Modes.PM:
					vState.Handles.Reverser = 1;
					vState.Handles.PowerNotch = handlePower;
					vState.Handles.BrakeNotch = handleBrake;
					Panel[PanelID.StatusLEDs.ATP] = 1;
					atp2.Elapse(vState);
					break;
				case ModeSelector.Modes.AM:
					vState.Handles.Reverser = 1;
					Panel[PanelID.StatusLEDs.ATO] = 0;
					ato.Elapse(atp2.Elapse(vState));
					break;
			}
		}
		
		internal void SetPower(int powerNotch) {
			handlePower = powerNotch;
			rm.SetPower(powerNotch);
			atp2.SetPower(powerNotch);
			ato.SetPower(powerNotch);
		}
		
		internal void SetBrake(int brakeNotch) {
			handleBrake = brakeNotch;
			rm.SetBrake(brakeNotch);
			atp2.SetBrake(brakeNotch);
			ato.SetBrake(brakeNotch);
		}
		
		internal void DoorChange(DoorStates oldState, DoorStates newState) {
			stationsMemory.DoorChange(oldState, newState);
			driverless.DoorChange(oldState, newState);
			atp2.DoorChange(oldState, newState);
			ato.DoorChange(oldState, newState);
		}
		
		internal void KeyDown(VirtualKeys key) {
			if (trainFail) {
				currentMode = Modes.Off;
			}
			else if (vState.Vehicle.Speed.KilometersPerHour == 0
			   && handlePower == 0
			   && handleBrake == vSpec.BrakeNotches + 1) { // validation
				if (key == VirtualKeys.A1 && currentMode != Modes.PM) {
					currentMode++;
					// sound
				} else if (key == VirtualKeys.A2 && currentMode != Modes.Off) {
					currentMode--;
					// sound
				}
			}
			
			ato.KeyDown(key);
		}
		
		public void SetSignal(SignalData[] signal) {
		}
		
		public void SetBeacon(BeaconData beacon) {
			atp2.SetBeacon(beacon);
			stationsMemory.SetBeacon(beacon);
		}
	}
}
