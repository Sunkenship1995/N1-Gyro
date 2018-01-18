using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public  xMultiInputs _input;
	public OSCHostConroller controller;

	public GameObject wall;



	public float repeatSec = 2;
	private float innerSec = 0;
	public int scene = 0;
	private float power = -0.05f;

	// Use this for initialization
	void Start () {
		innerSec = repeatSec;
		controller = GameObject.Find ("OSCHostController").GetComponent<OSCHostConroller> ();

	}

	public void LevelUp(){

		Debug.Log ("LEVELUP!!!");

		float deg = 1.1f;
		power = power * deg;
		repeatSec = repeatSec / deg;




	}
	// Update is called once per frame
	void Update () {
		_input = controller.xPadChannels [xPadID.PAD1.ToString()]._input;


		if(scene == 0){
			if (_input.GetKeyDown(KeyCode.A)) {
				scene = 1;	
				//Transform

				var children = gameObject.GetComponentInChildren<Transform> ();

				foreach ( Transform  tr  in children) {
					tr.GetComponent<Renderer>().enabled = false;

				}



			}
		}
		else if (scene == 1) {

			innerSec -= Time.deltaTime;
			
			
			if (innerSec < 0) {
				moveWall clone;
				GameObject obj= GameObject.Instantiate(wall);
				clone = obj.GetComponent<moveWall> ();

				clone.power = power;


				bool _gameIsEnd = true;

				foreach (KeyValuePair<string, xPadChannel> kvp in controller.xPadChannels) {

					if (kvp.Value._input.enable) {
						_gameIsEnd = false;
					}

				}

				if (_gameIsEnd) {
					Debug.Log ("GAMEOVER");

					Application.LoadLevel (0);

				}


				innerSec = repeatSec;
			}

			//終了処理を
			//xPadChannels [webch[id]]._input.enable = false;

		}
	
	}
}
