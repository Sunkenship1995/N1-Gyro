using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
#if !UNITY_WEBGL
using UnityOSC;
#else
using MiniJSON;
#endif



public class OSCHostConroller : MonoBehaviour {

	public xInputType defaultInputType = xInputType.GAMEPAD;
	public Dictionary<string, xPadChannel> xPadChannels;
	public int portFromUnipad = 6666;//to Unipad
	public int  portToUnipad =  6666;//from Unipad
	public bool showGUI = true;

	private const string  DEFAULT_TARGETIP = "192.168.255.1";
	//public GameObject tartget;
	public GameObject[] targets;

	private Dictionary<string, ServerLog> servers;
	private int lastPacketIndex;

	//private long _lastOscTimeStamp = -1;
	private float waitsec = 1.0f;
	private float _waittime;

	private string debugtext;


	#if UNITY_WEBGL
	public Dictionary<string, string> webch;//socketid,xpadID.string
	#endif


	// Script initialization
	void Start() {  

		xPadChannels = new Dictionary<string, xPadChannel>();


		uint[] colors = new uint[]{ 0xEAB3E4, 0x87BB6A, 0xFFFFFF };

		int i = 0;
		foreach (xPadID pad in Enum.GetValues(typeof(xPadID))) {

			xPadChannels[pad.ToString()] =  new xPadChannel(pad.ToString(),DEFAULT_TARGETIP,portFromUnipad,portToUnipad - i,defaultInputType);

			GameObject g = GameObject.Instantiate (targets[i]);

			g.GetComponent<PlayerController> ().Init (pad );
			g.name = "Player";

			i++;
			if (targets.Length <= i) {
				//Debug.Log ("BREAK");
				break;
			}


		}
		#if !UNITY_WEBGL
		foreach (KeyValuePair<string, xPadChannel> kvp in xPadChannels) {

			OSCHandler.Instance.Init(kvp.Key,
			                         kvp.Value.targetIP, 
			                         kvp.Value.outGoingPort, 
			                         kvp.Value.incommingPort);

			Debug.Log ("Create Server Instance:" + xPadChannels[kvp.Key].targetIP + ":"+ kvp.Value.incommingPort);

		}
		debugtext = UnityEngine.Network.player.ipAddress;
		#else
		webch = new Dictionary<string, string>();
		#endif



		servers = new Dictionary<string, ServerLog>();

		_waittime = waitsec;

	//	ReciveHello ("hogehoge");
	}
	
	// NOTE: The received messages at each server are updated here
	// Hence, this update depends on your application architecture
	// How many frames per second or Update() calls per frame?
	void Update() {
		// must be called before you try to read value from osc server

		#if !UNITY_WEBGL
		OSCHandler.Instance.UpdateLogs();
		//ping to active clients
		#endif

		_waittime -= Time.deltaTime;
		if (_waittime < 0.0f) {

			SendMessageToAllClient ("/ping/a", "a");
			_waittime = waitsec;
		}

		if (Input.GetKeyDown (KeyCode.Space)) {

			showGUI = (showGUI ? false : true);
		}
	}

#if UNITY_WEBGL
	//for  webGL
	public void getPadState(string messagejson){

		JsonNode	json	= JsonNode.Parse (messagejson);
		string 		id		= json ["id"].Get<string> ();

		if (!webch.ContainsKey(id)) {
			debugtext = ("channnel is not setup" + id);
			return;
		}

		List<object> msg = new List<object> ();



		if (json ["value"].Count >= 1) {
			for (int i = 0; i < json ["value"].Count; i++) {
				msg.Add (json ["value"] [i].Get<string> ());

			}

			//repeatの空うちおうけとる
		} else {
			try{

				msg.Add (json ["value"].Get<string>());
			}
			catch(Exception e){

			}

		}


		xPadChannels [webch [id]]._input.updateControll (json ["index"].Get<string> (), msg);
		debugtext = messagejson;//+ "\n\nindex:" +  json ["index"].Get<string>() + "msg:" + (string) msg[0];

	}

#endif


	
	void OnGUI(){
		if (showGUI) {
			if ( GUI.Button(new Rect(10, 10, 100, 40), "GamePad") ) {
				
				ChangeSensorType( xInputType.GAMEPAD);
			}
			if ( GUI.Button(new Rect(10, 50, 100, 40), "GyroPAD") ) {
				ChangeSensorType( xInputType.GYROPAD);
			}
			if ( GUI.Button(new Rect(10, 90, 100, 40), "GyroAttitude") ) {
				ChangeSensorType( xInputType.GYROATTITUDE);
			}

			if ( GUI.Button(new Rect(10, 170, 100, 40), "TouchPad") ) {
				ChangeSensorType( xInputType.TOUCHPAD);
				
			}
			#if UNITY_WEBGL
			if ( GUI.Button(new Rect(10, 210, 100, 40), "Dialog Test") ) {

				SendMessageToAllClient (
					"/dialog/open", 
					"{id:servicectrl,title :node-red,msg : node msg., btns: ['show message','reboot','poweroff']}"
				);
			}
			#endif

			GUI.Label(new Rect(10,0,800,400),debugtext);
		}
	}

	//<T>(string clientId, string address, T value)

	public void SendMessageToAllClient<T>(string address ,T value){

		foreach (KeyValuePair<string, xPadChannel> kvp in xPadChannels) {
			try{
				if(kvp.Value._input.enable){
					#if !UNITY_WEBGL
					OSCHandler.Instance.SendMessageToClient(kvp.Key, address, value);
					#else
					//	Application.ExternalCall("testExtern");
					Application.ExternalCall("SendMessageToAllClient", address,Json.Serialize(value));
					#endif
				}



			}
			catch{
				xPadChannels[kvp.Key]._input.enable = false;
				Debug.Log ("error:Stop ping to :"+kvp.Key);
			}

		}

	}
	public void ChangeSensorType(xInputType s){


		defaultInputType = s;
		SendMessageToAllClient( "/config/type", (int)s);

		foreach (KeyValuePair<string, xPadChannel> kvp in xPadChannels) {			
			xPadChannels[kvp.Key]._input.sensorType =s;
			if(kvp.Value._input.enable){
				#if UNITY_WEBGL
				SendMessageToClient(kvp.Key, "/info",  "ID:" + kvp.Key);
				#endif
			}
		}
		
		
	}


	#if !UNITY_WEBGL

	public  void  ReciveHello(string padid, string ip,int outport){
		
		Debug.Log ("ReciveHello to[" + padid + "](current:" + xPadChannels [padid].targetIP + ")from:" + ip + ":toUnipadPort->:" + outport);

		if (xPadChannels [padid].targetIP == ip ) {

			Debug.Log ("IP is same.");
			Debug.Log ("toUnipadPort(out):" + outport +"/in:"+ xPadChannels [padid].incommingPort);
			
			if(outport == xPadChannels [padid].incommingPort){
				Debug.Log ("Port is same.Setup again[" + padid+ "] send /config/type");

				OSCHandler.Instance.SendMessageToClient (padid, "/config/type", (int)defaultInputType);//(int)defaultInputType

				xPadChannels [padid]._input.enable=true;
			}
			else{
				
				Debug.Log ("Port is different.Setup Channel again:["+padid+"]");
				setupChannel( padid, ip);
			}
			
			
		} else {
			Debug.Log ("IP is different.");
			string target = null;
			
			
			foreach (KeyValuePair<string, xPadChannel> kvp in xPadChannels) {			
				
				if(kvp.Value.targetIP == ip){
					
					target = kvp.Key;
					Debug.Log ("Find same IP in ["+ target +"]");
					break;
				}
				if( kvp.Value._input.enable == false){
					
					target = kvp.Key;
					Debug.Log ("Find non active channel in ["+ target +"]");
					break;
				}
				
			}
			
			if(target == null){
				Debug.Log ("Setup Failed." );
			}
			else{
				setupChannel( target, ip);
			}
		}
	}



	#else
	//for webgl ver. called by Bworser
	public  void  ReciveHello(string id){
		Debug.Log ("ReciveHello."  + id);
		string target = null;

		foreach (KeyValuePair<string, xPadChannel> kvp in xPadChannels) {
			//sameuser
			if(kvp.Value.targetIP == id){
		//		Application.ExternalCall("SendMessageToClient", kvp.Key, "/config/type", (int)defaultInputType);
				SendMessageToClient(kvp.Key, "/config/type",  (int)defaultInputType);
				SendMessageToClient(kvp.Key, "/info",  "ID:" + kvp.Key);
				xPadChannels [kvp.Key]._input.enable=true;
				target = kvp.Key;
				break;
			}
			else if(kvp.Value._input.enable == false ){

				target = kvp.Key;
				break;
			}

		}
		
		if(target == null){
			Debug.Log ("Setup Failed." );
		}
		else{
			Debug.Log ("Setup Start.:" +id +":"+ target);
			setupChannel( target, id);
		}

	}


	#endif

	public void setupChannel(string target,string ip){
		
		
		
		Debug.Log ("setupChannel:Remove same Channel["+ target+"]" );
		Destroy (GameObject.Find (target));//GameObject.Find (kvp.Key)



		xPadChannels[target] =  new xPadChannel(target,ip,xPadChannels[target].outGoingPort,xPadChannels[target].incommingPort,defaultInputType);
		//Debug.Log ("Create Channel["+target+"]("+ip + ") out:" +  xPadChannels[target].outGoingPort + " in:" +  xPadChannels[target].incommingPort);
		
		#if !UNITY_WEBGL
		OSCHandler.Instance.Servers[target].server.Close();
		OSCHandler.Instance.Clients[target].client.Close();
		
		OSCHandler.Instance.Servers.Remove(target);
		OSCHandler.Instance.Clients.Remove(target);
		
		OSCHandler.Instance.Init(target,
		                         ip, 
			xPadChannels[target].outGoingPort, 
		                         xPadChannels[target].incommingPort);
		
		
		
		
		
		Debug.Log ("Create Channel[" + target + "] Active" + xPadChannels[target].targetIP + " out:" +  xPadChannels[target].outGoingPort + " in:" + xPadChannels[target].incommingPort);
		List<string> network = new List<string> ();
		network.Add (target);
		network.Add (UnityEngine.Network.player.ipAddress);
		network.Add (xPadChannels[target].incommingPort.ToString());
		network.Add (xPadChannels[target].outGoingPort.ToString());
		
		OSCHandler.Instance.SendMessageToClient (target, "/config/network", network);//(int)defaultInputType
		
		#else
		webch.Remove (ip);
		webch.Add(ip,target);
		Debug.Log ("webch.Add" + ip + ":" + target );
		SendMessageToClient(target, "/config/type",  (int)defaultInputType);
		SendMessageToClient(target, "/info",  "ID:" + target);
		#endif

		xPadChannels[target]._input.enable = true;
		Debug.Log ("x");
		
	}
#if !UNITY_WEBGL
	public  void ReciveBye(string id,string ip){

		Debug.Log ("ReviceBye:" + id + ":" + ip );

		if (xPadChannels [id].targetIP == ip) {
			xPadChannels [id]._input.enable = false;
		}
		
	}
#else
	public  void ReciveBye(string id){

		if (webch.ContainsKey (id) && xPadChannels [webch[id]]._input.enable) {
			xPadChannels [webch[id]]._input.enable = false;

		}

	}
#endif


	public void SendMessageToClient <T>(string padid,string address,T value ){

		if (xPadChannels [padid]._input.enable) {
			
			#if UNITY_WEBGL
			string socketid = xPadChannels [padid].targetIP;

			Application.ExternalCall ("SendMessageToClient", socketid, address, value);
			#else
			OSCHandler.Instance.SendMessageToClient (padid, address, value);//(int)defaultInputType
			#endif
		}
	}
	void SendMessageToClient<T>(string padid, string address, List<T> values){
		if (xPadChannels [padid]._input.enable) {

			#if UNITY_WEBGL
			string socketid = xPadChannels [padid].targetIP;
			Application.ExternalCall("SendMessageToClient",socketid, address,new JSONObject(Json.Serialize(values)));

			#else
			OSCHandler.Instance.SendMessageToClient (padid, address, values);//(int)defaultInputType
			#endif
		}

	}



	void OnBecameVisible() {
		#if UNITY_WEBGL
		Application.ExternalCall("UnityApplicationDidActive");
		#endif
	}

}
