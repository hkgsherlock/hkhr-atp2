/*
 * Created by SharpDevelop.
 * User: Charles Poon
 * Date: 13/01/14
 * Time: 10:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.IO.Pipes;

using OpenBveApi.Runtime;

namespace HKHR_ATP2
{
	
	public class TIMS
	{
		VehicleSpecs vSpec;
		ElapseData vState;
		
		AsyncPipeServer timsPipeServer;
		double lastTotalTimeSendElapseData = 0.0;
		
		#region openBVE
		internal bool Load(LoadProperties properties) {
			timsPipeServer = new AsyncPipeServer();
			timsPipeServer.OnReceiveMessageComplete += new AsyncPipeServer.ReceivedMessageOutput(timsPipeServer_OnReceiveMessageComplete);
			timsPipeServer.Start();
			
			return true;
		}
		
		internal void Unload() {
			timsPipeServer.Stop();
		}
		
		internal void SetVechicleSpecs(VehicleSpecs vs) {
			vSpec = vs;
		}
		
		internal void Initialise(InitializationModes mode) {
			
		}
		
		internal void Elapse(ElapseData data) {
			vState = data;
			
			if (data.TotalTime.Milliseconds - lastTotalTimeSendElapseData >= 1000) {
				timsPipeServer.SendObject(data);
				lastTotalTimeSendElapseData = data.TotalTime.Milliseconds;
			}
		}
		
		internal void KeyDown(VirtualKeys key) {
			
		}
		
		internal void KeyUp(VirtualKeys key) {
			
		}
		
		internal void DoorChange(DoorStates oldStatue, DoorStates newState) {
			
		}
		#endregion
		
		#region AsyncPipeServer
		private void timsPipeServer_OnReceiveMessageComplete(string message)
		{
//			Debug.WriteLine(message);
		}
		#endregion
	}
}
