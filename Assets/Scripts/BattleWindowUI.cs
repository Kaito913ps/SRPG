using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class BattleWindowUI : MonoBehaviour
{
	// バトル結果表示ウィンドウUI
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
	/// バトル結果ウィンドウを表示する
	/// </summary>
	/// <param name="charaData">攻撃されたキャラクターのデータ</param>
	/// <param name="damageValue">ダメージ量</param>
	public void ShowWindow(Character charaData, int damageValue)
	{
		gameObject.SetActive(true);

		_nameText.text = charaData._charaName;

		int nowHP = charaData._nowHP - damageValue;
		nowHP = Mathf.Clamp(nowHP, 0, charaData._maxHP);

		// HPゲージ表示
		float amount = (float)charaData._nowHP / charaData._maxHP;
		float endAmount = (float)nowHP / charaData._maxHP;
		DOTween.To( () => amount, (n) => amount = n, endAmount,1.0f).OnUpdate(() =>
			{
				_hpGageImage.fillAmount = amount;
			});

		// HPText表示(現在値と最大値両方を表示)
		_hpText.text = nowHP + "/" + charaData._maxHP;
		if (damageValue >= 0)
			_damageText.text = damageValue + "ダメージ！";
		else
			_damageText.text = -damageValue + "回復！";
	}
	/// <summary>
	/// バトル結果ウィンドウを隠す
	/// </summary>
	public void HideWindow()
	{
		gameObject.SetActive(false);
	}
}