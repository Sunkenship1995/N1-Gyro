using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class fishControll : MonoBehaviour {

	private xInputType sensorType =  xInputType.GAMEPAD;
	public xPadID padid;

	public GUIStyle style;


	public OSCHostConroller controller;
	public  xMultiInputs _input;

	public AudioClip jumpaudio;
	public AudioClip hitaudhio;

	private bool isJumpRequest;
	private bool active = true;

	private IEnumerator coroutine;

	private int hiscore = 0;
	private int score = 0;
	AudioSource audioSource;


	private Color mycolor;



	public void Init(xPadID id,Color c){
		controller = GameObject.Find ("OSCHostController").GetComponent<OSCHostConroller> ();

		audioSource = gameObject.GetComponent<AudioSource>();

		padid = id;
		mycolor = c;

		style.normal.textColor = c;
	}
	void Update () {

		_input = controller.xPadChannels [padid.ToString()]._input;

		sensorType = _input.sensorType;


		float h = _input.GetAxis ("Horizontal")/20;
		float v = _input.GetAxis ("Vertical")/20;
		this.transform.Translate(h, v, 0,Camera.main.transform);



		if (sensorType == xInputType.GYROPAD) {
			transform.localScale = _input.gyro.rotationRateUnbiased + new Vector3(1,1,1);
		} 
		else if(sensorType == xInputType.GYROATTITUDE){
			transform.localScale = _input.gyro.gyroAttitude + new Vector4(2,2,2,1);// * new Vector3(2,2,2);// new Vector3(1,1,1);
		}


		if (_input.GetKeyDown(KeyCode.A)) {

			if( active == false){
				gameObject.transform.position = new Vector3(0,0,0);
				active = true;
			}
			else{
				isJumpRequest = true;
			}


			//transform.Rotate(1, 0, 0);
		}


		if (gameObject.transform.position.y < -20) {
			gameObject.transform.position = new Vector3(0,-100,0);
			active = false;
		}



	}


	void FixedUpdate(){
		if (score > hiscore) {
			hiscore = score;
		}

		float power = 8;

		if (isJumpRequest) {

			audioSource.PlayOneShot( jumpaudio );
			isJumpRequest = false;

			//rigidbody2D.
			GetComponent<Rigidbody2D>().velocity = Vector3.up * power;

		}


	}
	void OnGUI () {

		int boxW = 300;
		int boxH = 60;

		GUI.Box(new Rect( 40+ (boxW *  (int)padid),Screen.height - boxH,boxW,boxH), 
			padid.ToString() + ":" +
			(controller.xPadChannels [padid.ToString()]._input.enable ? 
				score + "/" + hiscore : "none")
			,style
		) ;

	}

	/// <summary>
	/// 入力されたオブジェクト及びその子、全ての色を変える
	/// </summary>
	/// <param name="targetObject">色を変更したいオブジェクト</param>
	/// <param name="color">設定したい色</param>
	/// 
	private IEnumerator ChangeColorOfGameObject(GameObject targetObject, Color color,bool one = true){

		transform.GetComponent<SpriteRenderer> ().color = color;

		if(one){
			yield return new WaitForSeconds (0.5f);
			transform.GetComponent<SpriteRenderer> ().color = mycolor;		
		}


	}

	void OnTriggerExit2D(Collider2D c){

	}
	void OnCollisionEnter2D (Collision2D c){
		//Debug.Log ("Hit");
		audioSource.PlayOneShot( hitaudhio );

		score = 0;

		coroutine = ChangeColorOfGameObject (this.gameObject, Color.red,true);
		StartCoroutine(coroutine);

		//Destroy(gameObject);
	}


}
