using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class CharactersManager : MonoBehaviour
{
	
	public Transform _charactersParent;

	// 全キャラクターデータ
	[HideInInspector]
	public List<Character> _characters;

	void Start()
	{
		// マップ上の全キャラクターデータを取得
		// (charactersParent以下の全Characterコンポーネントを検索しリストに格納)
		_characters = new List<Character>();
		_charactersParent.GetComponentsInChildren(_characters);

		// データマネージャからデータ管理クラスを取得
		Data data = GameObject.Find("DataManager").GetComponent<Data>();

		// ステータス上昇量データを適用する
		foreach (Character charaData in _characters)
		{
			// 敵キャラクターの場合は強化しない
			if (charaData._isEnemy)
				continue;

			// キャラクターの能力を上昇させる
			charaData._nowHP += data._addHP; // 最大HP
			charaData._maxHP += data._addHP; // 現在HP
			charaData._atk += data._addAtk; // 攻撃力
			charaData._def += data._addDef; // 防御力
		}
	}

	/// <summary>
	/// 指定した位置に存在するキャラクターデータを検索して返す
	/// </summary>
	/// <param name="xPos">X位置</param>
	/// <param name="zPos">Z位置</param>
	/// <returns>対象のキャラクターデータ</returns>
	public Character GetCharacterDataByPos(int xPos, int zPos)
	{
		// 検索処理
		// (foreachでマップ内の全キャラクターデータ１体１体ずつに同じ処理を行う)
		foreach (Character charaData in _characters)
		{
			// キャラクターの位置が指定された位置と一致しているかチェック
			if ((charaData._xPos == xPos) &&(charaData._zPos == zPos)) 	
			{
				return charaData; 
			}
		}

		// データが見つからなければnullを返す
		return null;
	}

	/// <summary>
	/// 指定したキャラクターを削除する
	/// </summary>
	/// <param name="charaData">対象キャラデータ</param>
	public void DeleteCharaData(Character charaData)
	{
		_characters.Remove(charaData);
		DOVirtual.DelayedCall( 0.5f,() =>
			{
				Destroy(charaData.gameObject);
			}
		);
		GetComponent<GameManager>().CheckGameSet();
	}
}