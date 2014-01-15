using Newtonsoft.Json;
using System;
using System.IO.Pipes;
using System.Diagnostics;

namespace HKHR_ATP2
{
	/// <summary>
	/// Description of AsyncPipeServer.
	/// </summary>
	internal class AsyncPipeServer
	{
		NamedPipeServerStream pipeServer = null;
		
		// properties
		internal bool Connected = false;
		
		// buffers
		string receiveMessage;
        byte[] receiveBuffer = new byte[4096];	// Client -> Server
		string sendMessage;
        byte[] sendBuffer;								// Server -> Client
        bool sendRequested = false;
        
        // delegates AND EventHandlers
        internal delegate void ReceivedMessageOutput(string message);
        internal event ReceivedMessageOutput OnReceiveMessageComplete;
        
        // constructor
        public AsyncPipeServer()
        {
        	
        }
		
		internal void Start() {
			// setting up pipeServer
			PipeSecurity pipeSec = new PipeSecurity();
			pipeSec.SetAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));
			pipeServer = new NamedPipeServerStream(
				"HKHRATP2\\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName + System.Diagnostics.Process.GetCurrentProcess().Id,
				PipeDirection.InOut,
				1,
				PipeTransmissionMode.Message, 
				PipeOptions.Asynchronous,
				4096,
				4096,
				pipeSec,
				System.IO.HandleInheritability.None
			);
			
			StartListening();
		}
        
        private void StartListening() {
        	pipeServer.BeginWaitForConnection(BeginWaitForConnectionCallback, pipeServer);
        }
        
        internal void Stop() {
        	if (pipeServer.IsConnected) {
        		pipeServer.WaitForPipeDrain();
        		pipeServer.Disconnect();
        	}
        	pipeServer.Close();
        	pipeServer.Dispose();
        }
        
        internal void SendMessage(string message) {
        	if (pipeServer.IsConnected && !sendRequested) {
        		sendRequested = true;
        		sendMessage = message + "\0"; // prevent from error
	        	sendBuffer = System.Text.Encoding.UTF8.GetBytes(sendMessage);
	        	if (sendBuffer.Length > 4096) {
	        		Debug.WriteLine("[WARN] sending message larger than 4096B, message = \"" + message + "\"");
	        	}
	        	try {
	        		pipeServer.BeginWrite(sendBuffer, 0, sendBuffer.Length, BeginWriteCallback, pipeServer);
	        	} catch (System.IO.IOException) {
	        		OnClientDisconnected();
	        	}
        	}
        }
        
        internal void SendObject(object content) {
        	string result = content.GetType().ToString() + "|" + JsonConvert.SerializeObjectAsync(content, Formatting.None, null).Result;
//        	Debug.WriteLine(result + System.Environment.NewLine);
        	SendMessage(result);
        }
        
        private void OnClientDisconnected() {
        	pipeServer.Disconnect();
			StartListening();
        }
		
		private void BeginWaitForConnectionCallback(IAsyncResult iar) {
			pipeServer = (NamedPipeServerStream)iar.AsyncState;
			pipeServer.EndWaitForConnection(iar);
			
			if (pipeServer.IsConnected)
				pipeServer.BeginRead(receiveBuffer, 0, receiveBuffer.Length, BeginReadCallback, pipeServer);
		}
        
        private void BeginReadCallback(IAsyncResult iar) {
        	pipeServer = (NamedPipeServerStream)iar.AsyncState;
        	int bytesRead = pipeServer.EndRead(iar);
        	
        	if (bytesRead > 0) {
        		receiveMessage += System.Text.Encoding.UTF8.GetString(receiveBuffer, 0, receiveBuffer.Length);
        		
        		if (pipeServer.IsMessageComplete) {
        			OnReceiveMessageComplete(receiveMessage);
        			receiveMessage = "";
        		}
        	}
        	
        	pipeServer.BeginRead(receiveBuffer, 0, receiveBuffer.Length, BeginReadCallback, pipeServer);
        }
        
        private void BeginWriteCallback(IAsyncResult iar) {
        	pipeServer = (NamedPipeServerStream)iar.AsyncState;
        	pipeServer.EndWrite(iar);
        	sendRequested = false;
        	pipeServer.Flush();
        }
	}
}
