using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    private bool _m_bInit = false;
    private Vector3 _m_vAccel_L = new Vector3();		// 加速度
    private Vector3 _m_vAccel_Start = new Vector3();	// 起動時の加速度

    //[ 初期化時 ]
    void Awake()
    {
        //[ FPS設定 ]
        Application.targetFrameRate = 60;
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //[ 加速度を取得し感覚的な向きへ変更（取得される加速度の向きが違う） ]
        Vector3 awv_accel = new Vector3(-Input.acceleration.y, Input.acceleration.x, -Input.acceleration.z);
        if (awv_accel.x >= 0.5) awv_accel.x = 0.5f;
        if (awv_accel.x <= -0.5) awv_accel.x = 0.5f;
        //[ ローパスフィルタ（手ぶれ補正のなめらかなカーブ） ]
        _m_vAccel_L = 0.9f * _m_vAccel_L + 0.1f * awv_accel;

        if (!_m_bInit)
        {
            //[ 起動時の加速度を保存しておく ]
            _m_vAccel_Start = _m_vAccel_L;
            _m_bInit = true;
        }

        //[ 停止したときに物理計算が解除されるので強制的にＯＮ ]
        GetComponent<Rigidbody>().WakeUp();

        //[ 加速度の差分から回転を取得 ]
        Quaternion awq_rot = Quaternion.FromToRotation(_m_vAccel_Start, _m_vAccel_L);
        if (awq_rot.x >= 1.0f) awq_rot.x = 1.0f;
        if (awq_rot.x <= -1.0f) awq_rot.x = -1.0f;
        //[ 重力を変更 ]
        Physics.gravity = awq_rot * (-9.8f * 10 * Vector3.up);	// 1単位 = 10cmとしているので10倍

       
        Debug.Log(Physics.gravity.x);

#if UNITY_ANDROID
		//[ 戻るキーでアプリを終了させる ]
		if( Input.GetKey(KeyCode.Escape) ){
			Application.Quit();
		}
#endif
    }
}

