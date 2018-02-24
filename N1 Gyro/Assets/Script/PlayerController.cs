using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    // speedを制御する
    public float speed = 10;

    public Camera targetCamera;

    Vector3 forceV = new Vector3(0,0,0);

    private Rigidbody rb;

    //ジャンプスピード
    public float jumpspeed;

    private bool isJunmping = false;

    private Vector3 Acceleration;
    private Vector3 preAcceleration;
    float DotProduct;
    public static int ShakeCount;

    int count = 0;

    AudioClip clip;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Input.gyro.enabled = true;//大事

        clip = gameObject.GetComponent<AudioSource>().clip;
    }
    // Update is called once per frame
    void FixedUpdate()
    {

        ShakeCheck();

        //Debug.Log(targetCamera.transform.localEulerAngles.y);

        Vector3 val = Input.acceleration;

        Vector3 gravityV = Input.gyro.gravity;

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

        if (count < ShakeCount && !isJunmping)
        {
            if (forceV.z > 3.0f && forceV.x < 5.0f)
            {
                rb.velocity = new Vector3(0, jumpspeed, 5);
            }
            else if (forceV.z < -5.0f && forceV.x < 5.0f)
            {
                rb.velocity = new Vector3(0, jumpspeed, -3);
            }
            else if(forceV.z <=3.0f || forceV.z >= -5.0f)
            {
                rb.velocity = new Vector3(0, jumpspeed,0);
            }
            else if (forceV.z > 3.0f && forceV.z < 5.0f)
            {
                rb.velocity = new Vector3(5, jumpspeed, 0);
            }
            else if (forceV.x < -5.0f && forceV.z < 5.0f)
            {
                rb.velocity = new Vector3(-3, jumpspeed, 0);
            }
            isJunmping = true;
            count = ShakeCount;
        }

        Debug.Log(jumpspeed);
        
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(forceV);
    }

    void ShakeCheck()
    {
        preAcceleration = Acceleration;
        Acceleration = Input.acceleration;
        DotProduct = Vector3.Dot(Acceleration, preAcceleration);
        if (DotProduct > 6)
        {
            jumpspeed = 5;
            Debug.Log(DotProduct);
        }
        if (DotProduct < 0)
        {
            ShakeCount++;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        jumpspeed = 3;
        isJunmping = false;
        gameObject.GetComponent<AudioSource>().PlayOneShot(clip);
    }
}

