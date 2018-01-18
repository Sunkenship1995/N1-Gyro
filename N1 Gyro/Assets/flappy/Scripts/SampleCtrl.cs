using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class SampleCtrl : MonoBehaviour {
	
	private xInputType sensorType =  xInputType.GAMEPAD;
	public xPadID padid;

	SpriteRenderer MainSpriteRenderer;

	public Sprite Standaed;
	public Sprite PressA;
	public Sprite Damege;

	public GUIStyle style;


	public OSCHostConroller controller;

	public GameController gameController;

	public  xMultiInputs Input;

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


		transform.position = new Vector2 (-9.0f, 0f);

		Debug.Log ("color" + c);

		controller = GameObject.Find ("OSCHostController").GetComponent<OSCHostConroller> ();

		audioSource = gameObject.GetComponent<AudioSource>();

		padid = id;
		mycolor = c;
		transform.GetComponent<SpriteRenderer> ().color = mycolor;		

		style.normal.textColor = c;

		gameController = GameObject.Find ("GameController").GetComponent<GameController> ();

		MainSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();

		controller.SendMessageToClient(padid.ToString(),"/info","["+ padid.ToString()+"]\n" +"score:<size=80>"+ score + "</size>");

	}
	void Update () {
		
		Input = controller.xPadChannels [padid.ToString()]._input;
		
		sensorType = Input.sensorType;
		
		
		float h = Input.GetAxis ("Horizontal")/20;
		float v = Input.GetAxis ("Vertical")/20;
		this.transform.Translate(h, v, 0,Camera.main.transform);

		
		if (sensorType == xInputType.GYROPAD) {
			transform.localScale = Input.gyro.rotationRateUnbiased + new Vector3(1,1,1);
		} 
		else if(sensorType == xInputType.GYROATTITUDE){
			transform.localScale = Input.gyro.gyroAttitude + new Vector4(1,1,1,1);// * new Vector3(2,2,2);// new Vector3(1,1,1);
		}
		/*
		else if(sensorType == xInputType.ACCEL){
			transform.localScale = Input.acceleration + new Vector3(1,1,1);
		}
		*/

		if (Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.Tab)) {

			if (active == false) {
				gameObject.transform.position = new Vector3 (-9, 0, 0);
				active = true;
			} else {
				
				if (isJumpRequest == false) {
					
					isJumpRequest = true;

				} else {
					
				}
			}


			//transform.Rotate(1, 0, 0);
		} 

		if (Mathf.Abs(gameObject.transform.position.x) > 15 || Mathf.Abs(gameObject.transform.position.y) > 20) {
			//gameObject.transform.position = new Vector3(0,-100,0);
			active = false;
		}

		
		
		
	}


	void FixedUpdate(){
		if (score > hiscore) {
			hiscore = score;
		}

		 float power = 8f;

		if (isJumpRequest) {
			MainSpriteRenderer.sprite = PressA;
			coroutine = ChangeColorOfGameObject (this.gameObject, mycolor, true);
			StartCoroutine (coroutine);
			audioSource.PlayOneShot( jumpaudio );
			isJumpRequest = false;
			
			//rigidbody2D.
			GetComponent<Rigidbody2D>().velocity = Vector3.up * power;

		}


	}
	void OnGUI () {

		int boxW = 300;
		int boxH = 60;

//		float h = Input.GetAxis ("Horizontal")/20;
//		float v = Input.GetAxis ("Vertical")/20;

		GUI.Box(new Rect( 40+ (boxW *  (int)padid),Screen.height - boxH,boxW,boxH), 
		        padid.ToString() + ":" +
		        (controller.xPadChannels [padid.ToString()]._input.enable ? 
		 		score + "/" + hiscore : "none")
		        ,style
		        ) ;





	//	GUI.Label(new Rect(boxW *  (int)padid,20,800,400),h +":" + v);

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
			yield return new WaitForSeconds (0.2f);
			transform.GetComponent<SpriteRenderer> ().color = mycolor;
			MainSpriteRenderer.sprite = Standaed;
		}

		
	}

	void OnTriggerExit2D(Collider2D c){
		if(c.gameObject.tag == "pointWall"){
			score +=1;

			GameObject.Destroy (c);
			//Debug.Log (c.gameObject.transform.parent.gameObject.name);
			//coroutine = ChangeColorOfGameObject(c.gameObject.transform.parent.gameObject, Color.magenta,true);
			//StartCoroutine(coroutine);
			controller.SendMessageToClient(padid.ToString(),"/info","["+ padid.ToString()+"]\n" +" score:<size=80>"+ score + "\n</size>");

			if (score > 0 && score % 10 == 0 && Input.enable == true) {
				gameController.LevelUp ();
			}
		}
	}
	void OnCollisionEnter2D (Collision2D c){

		if (c.gameObject.tag == "enemyWall") {

			MainSpriteRenderer.sprite = Damege;
			//Debug.Log ("Hit");
			audioSource.PlayOneShot (hitaudhio);

			score = 0;
			controller.SendMessageToClient(padid.ToString(),"/info","["+ padid.ToString()+"]\n" +" score:<size=80>"+ score + "\n</size>");
			coroutine = ChangeColorOfGameObject (this.gameObject, Color.red, true);
			StartCoroutine (coroutine);
			controller.SendMessageToClient (padid.ToString (), "/vib", "");
		}
		//Destroy(gameObject);
	}


}
