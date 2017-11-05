using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    // speedを制御する
    public float speed = 10;

    public Camera targetCamera;

    // Use this for initialization
    void Start()
    {
        Input.gyro.enabled = true;//大事

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 gravityV = Input.gyro.gravity;
        // 外力のベクトルを計算.
        float scale = 10.0f;
        Vector3 forceV = new Vector3(gravityV.x, 0.0f, gravityV.y) * scale;
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(forceV);
        //  入力をxとzに代入
        //float x = Input.GetAxis("Horizontal");
        //float z = Input.GetAxis("Vertical");
        //// 同一のGameObjectが持つRigidbodyコンポーネントを取得
        //Rigidbody rigidbody = GetComponent<Rigidbody>();
        //// rigidbodyのx軸（横）とz軸（奥）に力を加える
        //// xとzに10をかけて押す力をアップ
        //rigidbody.AddForce(x * speed, 0, z * speed);

        //カメラの方向を向くようにする。
        //this.transform.LookAt(this.targetCamera.transform.position);  
    }
}

