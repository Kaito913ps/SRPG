using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class BattleWindowUI : MonoBehaviour
{
	// バトル結果表示ウィンドウUI
	[SerializeField,Tooltip("名前Text")]
	private Text _nameText;
	[SerializeField, Tooltip("HPゲージImage")]
    private Image _hpGageImage;
    [SerializeField, Tooltip("HPText")]
    private Text _hpText;
    [SerializeField, Tooltip("ダメージ量Text")]
    private Text _damageText;

	void Start()
	{
		//初期化時にウィンドウを隠す
		HideWindow();
	}

	/// <summary>
	/// バトル結果ウィンドウを表示する
	/// </summary>
	/// <param name="charaData">攻撃されたキャラクターのデータ</param>
	/// <param name="damageValue">ダメージ量</param>
	public void ShowWindow(Character charaData, int damageValue)
	{
		//オブジェクトアクティブ化
		gameObject.SetActive(true);

		//名前Text表示
		_nameText.text = charaData._charaName;

		//ダメージ計算後の残りHPを取得する
		int nowHP = charaData._nowHP - damageValue;
        // HPが0～最大値の範囲に収まるよう補正
        nowHP = Mathf.Clamp(nowHP, 0, charaData._maxHP);

        // HPゲージ表示
        // 表示中のFillAmount
        float amount = (float)charaData._nowHP / charaData._maxHP;
        // アニメーション後のFillAmount
        float endAmount = (float)nowHP / charaData._maxHP;
        // HPゲージを徐々に減少させるアニメーション
        DOTween.To( () => amount, (n) => amount = n, endAmount,1.0f).OnUpdate(() =>
			{
				_hpGageImage.fillAmount = amount;
			});

		// HPText表示(現在値と最大値両方を表示)
		_hpText.text = nowHP + "/" + charaData._maxHP;
        // ダメージ量Text表示
        if (damageValue >= 0)
            // ダメージ発生時
            _damageText.text = damageValue + "ダメージ！";
		else
            // HP回復時
            _damageText.text = -damageValue + "回復！";
	}
	/// <summary>
	/// バトル結果ウィンドウを隠す
	/// </summary>
	public void HideWindow()
	{
        // オブジェクト非アクティブ化
        gameObject.SetActive(false);
	}
}