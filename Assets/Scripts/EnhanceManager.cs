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
	public List<Button> _enhanceButtons; 
	public Button _goGameButton; 

	void Start()
	{
		_data = GameObject.Find("DataManager").GetComponent<Data>();

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
		SceneManager.LoadScene("Game");
	}
}