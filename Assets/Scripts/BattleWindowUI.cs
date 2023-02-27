using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class BattleWindowUI : MonoBehaviour
{
	// �o�g�����ʕ\���E�B���h�EUI
	[SerializeField]
	private Text _nameText;
	[SerializeField]
	private Image _hpGageImage;
	[SerializeField]
	private Text _hpText;
	[SerializeField]
	private Text _damageText;

	void Start()
	{
		HideWindow();
	}

	/// <summary>
	/// �o�g�����ʃE�B���h�E��\������
	/// </summary>
	/// <param name="charaData">�U�����ꂽ�L�����N�^�[�̃f�[�^</param>
	/// <param name="damageValue">�_���[�W��</param>
	public void ShowWindow(Character charaData, int damageValue)
	{
		gameObject.SetActive(true);

		_nameText.text = charaData._charaName;

		int nowHP = charaData._nowHP - damageValue;
		nowHP = Mathf.Clamp(nowHP, 0, charaData._maxHP);

		// HP�Q�[�W�\��
		float amount = (float)charaData._nowHP / charaData._maxHP;
		float endAmount = (float)nowHP / charaData._maxHP;
		DOTween.To( () => amount, (n) => amount = n, endAmount,1.0f).OnUpdate(() =>
			{
				_hpGageImage.fillAmount = amount;
			});

		// HPText�\��(���ݒl�ƍő�l������\��)
		_hpText.text = nowHP + "/" + charaData._maxHP;
		if (damageValue >= 0)
			_damageText.text = damageValue + "�_���[�W�I";
		else
			_damageText.text = -damageValue + "�񕜁I";
	}
	/// <summary>
	/// �o�g�����ʃE�B���h�E���B��
	/// </summary>
	public void HideWindow()
	{
		gameObject.SetActive(false);
	}
}