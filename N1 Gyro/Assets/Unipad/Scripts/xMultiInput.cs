using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;


public enum xInputType{
	OFF,GAMEPAD,TOUCHPAD,GYROPAD,GYROATTITUDE,FULLPAD
}
public enum xInputSensor{
	//GYRO_ATTITUDE,GYRO_UNBAISED,ATTITUDE,ACCEL,AUDIO_VOLUME,TOUCHPOS
	GYRO_ATTITUDE,GYRO_UNBAISED,ACCEL,AUDIO_VOLUME,TOUCHPOS
}
public enum xPadID{
	PAD1,PAD2,PAD3,PAD4
}


public class xPadSensor{

	public string name;
	public bool enable;

	public xPadSensor(string _name ,bool _active){
		name = _name;
		enable = _active;
	}
}

public static class xInputActiveSensors{

	public static Dictionary<xInputSensor,xPadSensor> sensor;

	static xInputActiveSensors(){

		sensor = new Dictionary<xInputSensor,xPadSensor> ();

		sensor.Add(xInputSensor.GYRO_ATTITUDE	,new   xPadSensor ("Atitude", false));
		sensor.Add(xInputSensor.GYRO_UNBAISED	,new   xPadSensor ("RotationRateUnbiased", false));
		//	sensor.Add(xInputSensor.ATTITUDE		,new   xPadSensor ("attitude", false)	);
		sensor.Add(xInputSensor.ACCEL			,new   xPadSensor ("Accel", false)		);
		sensor.Add(xInputSensor.AUDIO_VOLUME	,new   xPadSensor ("Microphone Volume", false));
		sensor.Add(xInputSensor.TOUCHPOS		,new   xPadSensor ("TouchPos", false)	);

		//sensor.Add


	}

	public static void clear(){
		Debug.Log ("CLEARR SENSOR=----------");


		foreach (xInputSensor s in sensor.Keys) {
			xInputActiveSensors.SetActive (s, false);


		}
	}

	public static void SetActive(xInputSensor s,bool t){
		//sensor [s].enable = t;
		sensor [s].enable = t;

	}

}


public struct xPadChannel{
	public string ID;
	public string targetIP;
	public int	 outGoingPort;
	public int	 incommingPort;
	public xMultiInputs _input;

	public xPadChannel(string  id,string targetip,int _out,int _in,xInputType s){

		ID = id;
		targetIP = targetip;
		outGoingPort = _out;
		incommingPort = _in;
		_input = new GameObject (id).AddComponent<xMultiInputs> ();//
		_input.Init (id, s);
	}


}

public struct gyro{
	public  Vector3 rotationRateUnbiased;
	public  Vector4 gyroAttitude;//Vector4
	public  Quaternion attitude;//Quaternion
}



public  enum xKeyState { NONE,DOWN,UP,REPEAT};


public class xMultiInputs  : MonoBehaviour{

	//public static Dictionary<string,float> keycode;
	private long _lastOscTimeStamp = -1;
	private int lastPacketIndex;
	private string xmsg;


	public string ID;
	public xInputType sensorType;
	public bool enable = false;


	public  Vector3 acceleration;
	public  IPAddress myAddr;

	public gyro gyro;


	public class xKey{
		public string name;	//ボタン名称の文字列
		public int downcnt;	//downカウンタ
		public int upcnt;	//upカウンタ
		public xKeyState keystate;	//state

		public xKey(string _name){
			name = _name;
			downcnt = 0;
			upcnt = 0;
			keystate = xKeyState.NONE;
		}

	}

	public static Dictionary<string,KeyCode> xKeymap = new Dictionary<string, KeyCode>(){

		{"Left"	,KeyCode.LeftArrow	},
		{"Right",KeyCode.RightArrow	},
		{"Up"	,KeyCode.UpArrow	},
		{"Down"	,KeyCode.DownArrow	},

		{"TouchArea",KeyCode.O	},
		{"TouchArea2",KeyCode.P	},
		{"ButtonA"	,KeyCode.Z},	//優先順位の高いものを上に書く
		//{"ButtonA"	,KeyCode.Space},

		{"ButtonB"	,KeyCode.X	},

		{"ButtonX"	,KeyCode.A	},
		{"ButtonY"	,KeyCode.S	},

		{"ButtonL1"	,KeyCode.Q	},
		{"ButtonL2"	,KeyCode.W	},
		{"ButtonR2"	,KeyCode.E	},
		{"ButtonR1"	,KeyCode.R	},
		{"ButtonS1"	,KeyCode.Alpha1	},
		{"ButtonS2"	,KeyCode.Alpha2	},

		{"Menu"	,KeyCode.Tab	},
		{"menu"	,KeyCode.Tab	},



	};


	private Dictionary<KeyCode,xKey> xKeyCtrls;//xKeymapの逆配列


	//すべてのキーが押されていない時true.配列ループが非効率なため利用する。
	//ただ、そのためにxKeyState
	private bool allKeyisReleased = true;

	public  Dictionary<string,float> keycode = new Dictionary<string,float>(){
		{"Horizontal", 0.0f},
		{"Vertical", 0.0f}
	};

	OSCHostConroller hostcontroller;


	public xMultiInputs Init (string id,xInputType s){
		ID = id;
		sensorType = s;


		xKeyCtrls = new Dictionary<KeyCode,xKey> ();
		foreach (var k in xKeymap) {

			if(!xKeyCtrls.ContainsKey(k.Value))
				xKeyCtrls.Add (k.Value, new xKey(k.Key));
		}


		Debug.Log ("create xMultiInputs.Init:" + xKeyCtrls.Count);
		hostcontroller = GameObject.Find ("OSCHostController").GetComponent<OSCHostConroller> ();

		#if NETFX_CORE    // use this for initialization
		myAddr=	IPAddress.Parse (NetworkUtils.GetMyIPAddress ());
		#elif  UNITY_WEBGL
		myAddr=	IPAddress.Parse ("127.0.0.1");
		#else
		myAddr = IPAddress.Parse (UnityEngine.Network.player.ipAddress);
		#endif



		return this;
	}




	public  float GetAxis (string param){


		switch (param) {
		case "Horizontal":
			return altF(Input.GetAxis(param),keycode["Horizontal"]);
			//			break; 

		case "Vertical":
			return altF(Input.GetAxis(param),keycode["Vertical"]);
			//			break; 
		default:
			return 0.0f;

		}


	}

	public  bool GetKey(KeyCode param){


		if (allKeyisReleased)
			return false;

		//if (xKeymap[param] ==  null) {
		if (!xKeyCtrls.ContainsKey(param)) {
			Debug.Log ("notKeyBind at " + param);
			return false;
		}

		return altB(Input.GetKey(param), (xKeyCtrls[param].keystate == xKeyState.REPEAT ||xKeyCtrls[param].keystate == xKeyState.DOWN ));	
	}
	public  bool GetKeyUp(KeyCode param){

		//	if (allKeyisReleased)
		//		return false;


		if (!xKeyCtrls.ContainsKey(param)) {
			Debug.Log ("notKeyBind at " + param);
			return false;
		}

		bool _r = (xKeyCtrls [param].upcnt > 0  ? true : false);

		//xKeymap[param].upcnt = 0;	//カウンタのリセット
		if (_r) {
			xKeyCtrls [param].upcnt = -1;

		}
		//xKeymap [param].downcnt = 0;

		return altB(	Input.GetKeyUp(param), _r);	
	}

	public  bool GetKeyDown(KeyCode param){


		//if (allKeyisReleased)
		//	return false;

		if (!xKeyCtrls.ContainsKey(param)) {
			Debug.Log ("notKeyBind at " + param);
			return false;
		}

		bool _r = (xKeyCtrls [param].downcnt > 0 ? true : false);

		//		Debug.Log (param+"GetKeyDown:cnt=" + xKeymap [param].downcnt);

		if(_r){
			xKeyCtrls [param].downcnt = -1;	//カウンタのリセット
		}
		//xKeymap [param].upcnt = 0;	//カウンタのリセット


		return altB(Input.GetKeyDown(param),_r);	
	}


	private  void changeAxis(KeyCode k,bool on){

		float d = 0.2f;

		if (on) {
			switch (k) {
			case KeyCode.LeftArrow:
				keycode ["Horizontal"] = (keycode ["Horizontal"] > -1.0f ? keycode ["Horizontal"] - d : keycode ["Horizontal"]);
				//keycode ["Horizontal"] = (on && keycode ["Horizontal"] > -1.0f ? d-- : keycode ["Horizontal"]);
				break;
			case KeyCode.RightArrow:
				keycode ["Horizontal"] = (keycode ["Horizontal"] < 1.0f ? keycode ["Horizontal"] + d : keycode ["Horizontal"]);
				//keycode ["Horizontal"] = (on && keycode ["Horizontal"] < 1.0f ? d++ : keycode ["Horizontal"]);
				break;

			case KeyCode.UpArrow:
				keycode ["Vertical"] = (keycode ["Vertical"] < 1.0f ? keycode ["Vertical"] + d : keycode ["Vertical"]);
				break;

			case KeyCode.DownArrow:
				keycode ["Vertical"] = (keycode ["Vertical"] > -1.0f ? keycode ["Vertical"] - d : keycode ["Vertical"]);
				break;


			}
		} else {
			switch (k) {
			case KeyCode.LeftArrow:
			case KeyCode.RightArrow:
				keycode ["Horizontal"] = 0.0f;
				break;

			case KeyCode.UpArrow:
			case KeyCode.DownArrow:
				keycode ["Vertical"] = 0.0f;

				break;
			case KeyCode.O:
				keycode ["Vertical"]	= 0.0f;
				keycode ["Horizontal"]	= 0.0f;
				break;
			}
		}

	}
	private static float altF(float in1,float in2){

		if (in1 != 0.0f) 
			return in1;
		else 
			return in2;
	}

	private static bool altB(bool in1,bool in2){

		if (in1  ) 
			return in1;
		else 
			return  in2;
	}

	#if !UNITY_WEBGL
	public void Update() {

		var buffer = new Dictionary<string,ServerLog>();
		buffer = OSCHandler.Instance.Servers;

		foreach( KeyValuePair<string, ServerLog> item in buffer ){

			if(item.Value.log.Count > 0 && item.Key == ID){
				for( int i=0; i < item.Value.packets.Count; i++ ) { 
					if( _lastOscTimeStamp < item.Value.packets[i].TimeStamp ) {
						reciveLogs(item);
						_lastOscTimeStamp = item.Value.packets[i].TimeStamp;
					}
				}

			}

		}
	}


	private void reciveLogs(KeyValuePair<string, ServerLog> item){


		lastPacketIndex = item.Value.packets.Count - 1;
		string path = item.Value.packets [lastPacketIndex].Address;
		List<object> msg = item.Value.packets [lastPacketIndex].Data;

		updateControll (path, msg);

	}
	#endif

	public void updateControll(string path,List<object> msg){


		#if !UNITY_WEBGL
		if (path.StartsWith ("/hello")) {

			Debug.Log ("hello From:" + msg [1]);

			hostcontroller.ReciveHello(ID,msg[0].ToString(),int.Parse( (string) msg[1]));

		} else if (path.StartsWith ("/bye") ) {

			hostcontroller.ReciveBye(ID,msg[0].ToString());

		}


		#endif



		if(path.StartsWith("/sensor/gyroUnbiased") ){
			this.gyro.rotationRateUnbiased =  new Vector3( float.Parse((string)msg[0]),
				float.Parse((string)msg[1]),
				float.Parse((string)msg[2]));



		}
		else if(path.StartsWith("/sensor/gyroAttitude") ){

			this.gyro.gyroAttitude =  new Vector4( float.Parse((string)msg[0]),
				float.Parse((string)msg[1]),
				float.Parse((string)msg[2]),
				float.Parse((string)msg[3])
			);

			this.gyro.attitude = new Quaternion (
				float.Parse((string)msg[0]),
				float.Parse((string)msg[1]),
				float.Parse((string)msg[2]),
				float.Parse((string)msg[3])
			);

		}
		else if(path.StartsWith("/sensor/accel") ){
			this.acceleration =  new Vector3( float.Parse((string)msg[0]),
				float.Parse((string)msg[1]),
				float.Parse((string)msg[2]));

		}
		else if(path.EndsWith("/touch/pos") ){	//use EndsWith for ignoring pos2

			//xMultiInput.xInputs[item.Key.ToString()].keycode["Horizontal"]	= float.Parse((string)	msg[0]);
			//xMultiInput.xInputs[item.Key.ToString()].keycode["Vertical"] 	= float.Parse((string)	msg[1]);

			this.keycode["Horizontal"]	= float.Parse((string)	msg[0]) * 2;
			this.keycode["Vertical"] 	= float.Parse((string)	msg[1]) * 2;

			//changeKeyState(KeyCode.Tab,true);
			if (allKeyisReleased == true || xKeyCtrls[KeyCode.O].keystate == xKeyState.UP ) {
				changeKeyState2("TouchArea",xKeyState.DOWN);
			} else {
				changeKeyState2("TouchArea",xKeyState.REPEAT);
			}


			allKeyisReleased = false;


		}
		//startとendはひとつしか飛んでこない
		else if (path.StartsWith ("/touch/start")) {

			changeKeyState2 ( (string)msg[0], xKeyState.DOWN);

			allKeyisReleased = false;


		}else if (path.StartsWith ("/touch/repeat")) {

			if (msg.Count > 0) {
				for (int i = 0; i < msg.Count; i++) {

					changeKeyState2 ( (string)msg[i], xKeyState.REPEAT);

					allKeyisReleased = false;
				}

			} else {
				//	Debug.Log ("すべてのキーをリセットする");
				//すべてのキーをリセットする場合
				allKeyisReleased = true;
				keycode ["Vertical"]	= 0.0f;
				keycode ["Horizontal"]	= 0.0f;
			}

		}else if (path.StartsWith ("/touch/end")) {


			changeKeyState2 ( (string) msg [0], xKeyState.UP);
			allKeyisReleased = false;


		} 

		if (allKeyisReleased) {
			foreach (var k in xKeyCtrls) {
				if(k.Value.downcnt == -1)
					k.Value.downcnt = 0;

				if(k.Value.upcnt == -1)
					k.Value.upcnt = 0;

				k.Value.keystate = xKeyState.NONE;

			}
		}

	}


	private void changeKeyState2(string keyname,xKeyState s){

		xKey _k = xKeyCtrls [xKeymap [keyname]];

		switch( s){
		case xKeyState.DOWN:

			_k.keystate = xKeyState.DOWN;
			_k.downcnt  =1;
			_k.upcnt 	= 0;
			break;
		case xKeyState.REPEAT:
			_k.keystate = xKeyState.REPEAT;
			_k.downcnt = (_k.downcnt >= 0 ? 2 : _k.downcnt);
			_k.upcnt 	= 0;
			break;
		case xKeyState.UP:

			_k.keystate = xKeyState.UP;
			//_k.downcnt = 0;
			_k.upcnt = 1;
			break;

		}

		//	Debug.Log("[" + ID + "]:" +keyname + ":" +s + ":" + _k.downcnt);


		if (s == xKeyState.UP) {
			changeAxis (xKeymap [keyname], false);
		} else {
			changeAxis (xKeymap [keyname], true);
		}




	}

}