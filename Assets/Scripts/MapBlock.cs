using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBlock : MonoBehaviour
{
	// 強調表示オブジェクト
	private GameObject _selectionBlockObj;

	// 強調表示マテリアル
	[Header("強調表示マテリアル：選択時")]
	public Material _selMat_Select;
	[Header("強調表示マテリアル：到達可能")]
	public Material _selMat_Reachable;
	[Header("強調表示マテリアル：攻撃可能")]
	public Material _selMat_Attackable; 
	public enum Highlight
	{
		Off, // オフ
		Select, // 選択時
		Reachable, // キャラクターが到達可能
		Attackable, // キャラクターが攻撃可能
	}

	// ブロックデータ
	[HideInInspector,Tooltip("X方向の位置")] 
	public int _xPos; 
	[HideInInspector,Tooltip("Z方向の位置")]
	public int _zPos; 
	[Header("通行可能フラグ(trueなら通行可能である)")]
	public bool _passable;

	void Start()
	{
        // 強調表示オブジェクトを取得
        _selectionBlockObj = transform.GetChild(0).gameObject;

        // 初期状態では強調表示をしない
        SetSelectionMode(Highlight.Off);
	}

	/// <summary>
	/// 選択状態表示オブジェクトの表示・非表示を設定する
	/// </summary>
	/// <param name="mode">強調表示モード</param>
	public void SetSelectionMode(Highlight mode)
	{
		switch (mode)
		{
			// 強調表示なし
			case Highlight.Off:
				_selectionBlockObj.SetActive(false);
				break;
			// 選択時
			case Highlight.Select:
				_selectionBlockObj.GetComponent<Renderer>().material = _selMat_Select;
				_selectionBlockObj.SetActive(true);
				break;
			// キャラクターが到達可能
			case Highlight.Reachable:
				_selectionBlockObj.GetComponent<Renderer>().material = _selMat_Reachable;
				_selectionBlockObj.SetActive(true);
				break;
			// キャラクターが攻撃可能
			case Highlight.Attackable:
				_selectionBlockObj.GetComponent<Renderer>().material = _selMat_Attackable;
				_selectionBlockObj.SetActive(true);
				break;
		}
	}

}