using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	// �J�����ړ��p�ϐ�
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
	/// �J�����ړ��{�^���������n�߂�ꂽ���ɌĂяo����鏈��
	/// </summary>
	/// <param name="rightMode">�E�����t���O(�E�ړ��{�^������Ă΂ꂽ��true�ɂȂ��Ă���)</param>
	public void CameraRotate_Start(bool rightMode)
	{
		_isCameraRotate = true;
		_isMirror = rightMode;
	}
	/// <summary>
	/// �J�����ړ��{�^����������Ȃ��Ȃ������ɌĂяo����鏈��
	/// </summary>
	public void CameraRotate_End()
	{
		_isCameraRotate = false;
	}
}