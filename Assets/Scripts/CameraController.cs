using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // カメラ移動用変数
    // カメラ回転中フラグ
    private bool _isCameraRotate;
    // 回転方向反転フラグ
    private bool _isMirror;

    // 回転速度
    const float SPEED = 30.0f;

	void Update()
	{
        // カメラ回転処理
        if (_isCameraRotate)
		{
            // 回転速度を計算する
            float speed = SPEED * Time.deltaTime;

            // 回転方向反転フラグが立っているなら速度反転
            if (_isMirror)
				speed *= -1.0f;

            // 基点の位置を中心にカメラを回転移動させる
            transform.RotateAround( Vector3.zero,Vector3.up,speed);
		}
	}

	/// <summary>
	/// カメラ移動ボタンが押し始められた時に呼び出される処理
	/// </summary>
	/// <param name="rightMode">右向きフラグ(右移動ボタンから呼ばれた時trueになっている)</param>
	public void CameraRotate_Start(bool rightMode)
	{
        // カメラ回転中フラグをON
        _isCameraRotate = true;
        // 回転方向反転フラグを適用する
        _isMirror = rightMode;
	}
	/// <summary>
	/// カメラ移動ボタンが押されなくなった時に呼び出される処理
	/// </summary>
	public void CameraRotate_End()
	{
        // カメラ回転中フラグをOFF
        _isCameraRotate = false;
	}
}