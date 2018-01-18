using UnityEngine;
using System.Collections;

public class moveWall : MonoBehaviour {

	// Use this for initialization

	public GameObject gameController;


	public float power = 0f;
	void Start () {

		gameController = GameObject.Find ("GameController");

		if (gameController.GetComponent<GameController> ().scene == 1) {
			gameObject.transform.Translate (0, Random.Range (-5, 5), 0);
		}



	}
	
	// Update is called once per frame
	void FixedUpdate () {


		if (gameController.GetComponent<GameController> ().scene == 1) {
			gameObject.transform.Translate(power, 0, 0);
			
			
			if (gameObject.transform.position.x < -20) {
				Destroy(gameObject);
			}

		}
	}


}
