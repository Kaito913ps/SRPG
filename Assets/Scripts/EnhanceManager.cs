using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnhanceManager : MonoBehaviour
{
	// �f�[�^�N���X
	private Data _data;

	// UI�{�^��
	public List<Button> _enhanceButtons; 
	public Button _goGameButton; 

	void Start()
	{
		_data = GameObject.Find("DataManager").GetComponent<Data>();

		_goGameButton.interactable = false;
	}

	/// <summary>
	/// (�X�e�[�^�X�㏸�{�^��)
	/// �ő�HP���㏸����
	/// </summary>
	public void Enhance_AddHP()
	{
		// ��������
		_data._addHP += 2;

		EnhanceComplete(); 
	}
	/// <summary>
	/// (�X�e�[�^�X�㏸�{�^��)
	/// �U���͂��㏸����
	/// </summary>
	public void Enhance_AddAtk()
	{
		// ��������
		_data._addAtk += 1;

		EnhanceComplete(); 
	}
	/// <summary>
	/// (�X�e�[�^�X�㏸�{�^��)
	/// �h��͂��㏸����
	/// </summary>
	public void Enhance_AddDef()
	{
		// ��������
		_data._addDef += 1;

		EnhanceComplete();
	}
	/// <summary>
	/// �v���C���[�����������̋��ʏ���
	/// </summary>
	private void EnhanceComplete()
	{
		// �����{�^���������s�ɂ���
		foreach (Button button in _enhanceButtons)
		{
			button.interactable = false;
		}
		// �u������x�v���C�v�{�^���������\�ɂ���
		_goGameButton.interactable = true;

		// �ύX���f�[�^�ɕۑ�
		_data.WriteSaveData();
	}

	/// <summary>
	/// �Q�[���V�[���ɐ؂�ւ���
	/// </summary>
	public void GoGameScene()
	{
		SceneManager.LoadScene("Game");
	}
}