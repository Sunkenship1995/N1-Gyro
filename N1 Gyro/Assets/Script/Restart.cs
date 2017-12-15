using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //接触した時に呼ばれる関数
    void OnCollisionEnter(Collision hit)
    {
        Debug.Log("sss");
        //もしPlayerと接触したら
        if (hit.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene("title");
        }
    }

}
