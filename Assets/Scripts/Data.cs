using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data : MonoBehaviour
{
    // シングルトン管理用変数
    [HideInInspector]
	public static bool _instance = false;

    // プレイヤー強化データ
    // 最大HP上昇量
    public int _addHP;
    // 攻撃力上昇量
    public int _addAtk;
    // 防御力上昇量
    public int _addDef;

	// データのキー定義
	public const string Key_AddHP = "Key_AddHP";
	public const string Key_AddAtk = "Key_AddAtk";
	public const string Key_AddDef = "Key_AddDef";

	private void Awake()
	{
		if (_instance)
		{
			Destroy(gameObject);
			return;
		}
		_instance = true; 

		DontDestroyOnLoad(gameObject);

		// セーブデータをPlayerPrefsから読み込み
		_addHP = PlayerPrefs.GetInt(Key_AddHP, 0); 
		_addAtk = PlayerPrefs.GetInt(Key_AddAtk, 0);
		_addDef = PlayerPrefs.GetInt(Key_AddDef, 0);
	}

	/// <summary>
	/// 現在のプレイヤー強化データをPlayerPrefsに保存する
	/// </summary>
	public void WriteSaveData()
	{
		PlayerPrefs.SetInt(Key_AddHP, _addHP);
		PlayerPrefs.SetInt(Key_AddAtk, _addAtk);
		PlayerPrefs.SetInt(Key_AddDef, _addDef);
        // 変更を保存
        PlayerPrefs.Save(); 
	}
}