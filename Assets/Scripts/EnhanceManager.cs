using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnhanceManager : MonoBehaviour
{
	// データクラス
	private Data _data;

	// UIボタン
	[SerializeField,Tooltip("ステータス上昇ボタン")]
    private List<Button> _enhanceButtons;
	[SerializeField,Tooltip("もう一度プレイボタン")]
	private Button _goGameButton; 

	void Start()
	{
        // データマネージャからデータ管理クラスを取得
        _data = GameObject.Find("DataManager").GetComponent<Data>();

        // いずれかを強化するまでは「もう一度プレイ」ボタンを押せないようにする
        _goGameButton.interactable = false;
	}

	/// <summary>
	/// (ステータス上昇ボタン)
	/// 最大HPを上昇する
	/// </summary>
	public void Enhance_AddHP()
	{
		// 強化処理
		_data._addHP += 2;
		//sound
        // 強化完了時処理
        EnhanceComplete(); 
	}
	/// <summary>
	/// (ステータス上昇ボタン)
	/// 攻撃力を上昇する
	/// </summary>
	public void Enhance_AddAtk()
	{
		// 強化処理
		_data._addAtk += 1;
		//sound
        // 強化完了時処理
        EnhanceComplete(); 
	}
	/// <summary>
	/// (ステータス上昇ボタン)
	/// 防御力を上昇する
	/// </summary>
	public void Enhance_AddDef()
	{
		// 強化処理
		_data._addDef += 1;
        //sound
        // 強化完了時処理
        EnhanceComplete();
	}
	/// <summary>
	/// プレイヤー強化完了時の共通処理
	/// </summary>
	private void EnhanceComplete()
	{
		// 強化ボタンを押下不可にする
		foreach (Button button in _enhanceButtons)
		{
			button.interactable = false;
		}
		// 「もう一度プレイ」ボタンを押下可能にする
		_goGameButton.interactable = true;

		// 変更をデータに保存
		_data.WriteSaveData();
	}

	/// <summary>
	/// ゲームシーンに切り替える
	/// </summary>
	public void GoGameScene()
	{
        //sound
        SceneManager.LoadScene("Game");
	}
}