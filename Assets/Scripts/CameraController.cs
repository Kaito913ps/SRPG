using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	// カメラ移動用変数
	private bool _isCameraRotate; 
	private bool _isMirror; 

	const float SPEED = 30.0f;

	void Update()
	{
		
		if (_isCameraRotate)
		{
			
			float speed = SPEED * Time.deltaTime;

			if (_isMirror)
				speed *= -1.0f;

			transform.RotateAround( Vector3.zero,Vector3.up,speed);
		}
	}

	/// <summary>
	/// カメラ移動ボタンが押し始められた時に呼び出される処理
	/// </summary>
	/// <param name="rightMode">右向きフラグ(右移動ボタンから呼ばれた時trueになっている)</param>
	public void CameraRotate_Start(bool rightMode)
	{
		_isCameraRotate = true;
		_isMirror = rightMode;
	}
	/// <summary>
	/// カメラ移動ボタンが押されなくなった時に呼び出される処理
	/// </summary>
	public void CameraRotate_End()
	{
		_isCameraRotate = false;
	}
}