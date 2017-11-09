using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    // speedを制御する
    public float speed = 10;

    public Camera targetCamera;

    Vector3 forceV = new Vector3(0,0,0);

    // Use this for initialization
    void Start()
    {
        Input.gyro.enabled = true;//大事

    }
    // Update is called once per frame
    void FixedUpdate()
    {

        Debug.Log(targetCamera.transform.localEulerAngles.y);

        Vector3 gravityV = Input.gyro.gravity;
        // 外力のベクトルを計算.
        float scale = 10.0f;
        if (targetCamera.transform.localEulerAngles.y <= 110.0f) 
        {
            forceV = new Vector3(gravityV.x, 0.0f, gravityV.y) * scale;
        }
        if (targetCamera.transform.localEulerAngles.y >= 130.0f) 
        {
            forceV = new Vector3(gravityV.x, 0.0f, gravityV.y) * -scale;
            Debug.Log("aaa");
        }
        
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(forceV);
    }
}

