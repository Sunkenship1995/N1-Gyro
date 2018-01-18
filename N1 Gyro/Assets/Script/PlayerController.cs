using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    private xInputType sensorType = xInputType.GYROPAD;
    public xPadID padid;
    public OSCHostConroller controller;
    public xMultiInputs Inputpad;

    // speedを制御する
    public float speed = 10;

    public Camera targetCamera;

    Vector3 forceV = new Vector3(0,0,0);

    // Use this for initialization
    void Start()
    {
        Input.gyro.enabled = true;//大事
        targetCamera = Camera.main;
    }
    // Update is called once per frame
    void FixedUpdate()
    {

        Debug.Log(targetCamera.transform.localEulerAngles.y);

        Inputpad = controller.xPadChannels[padid.ToString()]._input;
        sensorType = Inputpad.sensorType;

        Vector3 gravityV = Inputpad.gyro.rotationRateUnbiased;
        // 外力のベクトルを計算.
        float scale = 10.0f;
        //カメラのY軸のアングルでx,yに与える力を変える
        if (targetCamera.transform.localEulerAngles.y >= 300.0f || targetCamera.transform.localEulerAngles.y <= 50.0f)
        {
            forceV = new Vector3(gravityV.y, 0.0f, -gravityV.x) * -scale;
        }
        if (targetCamera.transform.localEulerAngles.y >= 55.0f && targetCamera.transform.localEulerAngles.y <= 123.0f)
        {
            forceV = new Vector3(gravityV.x, 0.0f, gravityV.y) * scale;
        }
        if (targetCamera.transform.localEulerAngles.y >= 124.0f && targetCamera.transform.localEulerAngles.y <= 220.0f)
        {
        forceV = new Vector3(-gravityV.y, 0.0f, gravityV.x) * -scale;
        }
        if (targetCamera.transform.localEulerAngles.y >= 225.0f && targetCamera.transform.localEulerAngles.y <= 290.0f)
        {
            forceV = new Vector3(gravityV.x, 0.0f, gravityV.y) * -scale;
        }
             
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(forceV);
    }

    public void Init(xPadID id)
    {
        controller = GameObject.Find("OSCHostController").GetComponent<OSCHostConroller>();
        padid = id;
    }
}

