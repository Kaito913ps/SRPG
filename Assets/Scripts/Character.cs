using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Character : MonoBehaviour
{
	// メインカメラ
	private Camera _mainCamera;

	// キャラクター初期設定(インスペクタから入力)
	[SerializeField,Header("初期X位置(-4～4)")]
	private int _initPos_X; 
	[SerializeField,Header("初期Z位置(-4～4)")]
	private int _initPos_Z; 
	[Header("敵フラグ(ONで敵キャラとして扱う)")]
	public bool _isEnemy; 
	[Header("キャラクター名")]
	public string _charaName; 
	[Header("最大HP(初期HP)")]
	public int _maxHP; 
	[Header("攻撃力")]
	public int _atk; 
	[Header("防御力")]
	public int _def; 
	[Header("属性")]
	public Attribute _attribute; 
	[Header("移動方法")]
	public MoveType _moveType;
	[Header("特技")]
	public SkillDefine.Skill _skill; 

	// ゲーム中に変化するキャラクターデータ
	[HideInInspector]
	public int _xPos; 
	[HideInInspector]
	public int _zPos; 
	[HideInInspector]
	public int _nowHP; 
	// 各種状態異常
	public bool _isSkillLock;
	public bool _isDefBreak; 

	// キャラクター属性定義(列挙型)
	public enum Attribute
	{
		Water, // 水属性
		Fire,  // 火属性
		Wind,  // 風属性
		Soil,  // 土属性
	}

	// キャラクター移動方法定義(列挙型)
	public enum MoveType
	{
		Rook, // 縦・横
		Bishop, // 斜め
		Queen, // 縦・横・斜め
	}

	void Start()
	{
		// メインカメラの参照を取得
		_mainCamera = Camera.main;

		// 初期位置に対応する座標へオブジェクトを移動させる
		Vector3 pos = new Vector3();
        // x座標：1ブロックのサイズが1(1.0f)なのでそのまま代入
        pos.x = _initPos_X;
        // y座標（固定）
        pos.y = 1.0f;
        // z座標
        pos.z = _initPos_Z;
        // オブジェクトの座標を変更
        transform.position = pos; 

		// オブジェクトを左右反転(ビルボードの処理にて一度反転してしまう為)
		Vector2 scale = transform.localScale;
        // X方向の大きさを正負入れ替える
        scale.x *= -1.0f;
		transform.localScale = scale;

		// その他変数初期化
		_xPos = _initPos_X;
		_zPos = _initPos_Z;
		_nowHP = _maxHP;
	}

	void Update()
	{
        // ビルボード処理(スプライトオブジェクトをメインカメラの方向に向ける)
        Vector3 cameraPos = _mainCamera.transform.position;
        // 現在のカメラ座標を取得
        cameraPos.y = transform.position.y;
        // キャラが地面と垂直に立つようにする
        transform.LookAt(cameraPos);
	}

	/// <summary>
	/// 対象の座標へとキャラクターを移動させる
	/// </summary>
	/// <param name="targetXPos">x座標</param>
	/// <param name="targetZPos">z座標</param>
	public void MovePosition(int targetXPos, int targetZPos)
	{
		// オブジェクトを移動させる
		// 移動先座標への相対座標を取得
		Vector3 movePos = Vector3.zero;
        // x方向の相対距離
        movePos.x = targetXPos - _xPos;
        // z方向の相対距離
        movePos.z = targetZPos - _zPos;

		// DoTweenのTweenを使用して徐々に位置が変化するアニメーションを行う
		transform.DOMove(movePos, // 指定座標まで移動する
				0.5f) // アニメーション時間(秒)
			.SetEase(Ease.Linear) // イージング(変化の度合)を設定
			.SetRelative(); // パラメータを相対指定にする

		// キャラクターデータに位置を保存
		_xPos = targetXPos;
		_zPos = targetZPos;
	}

	/// <summary>
	/// キャラクターの近接攻撃アニメーション
	/// </summary>
	/// <param name="targetChara">相手キャラクター</param>
	public void AttackAnimation(Character targetChara)
	{
		// 攻撃アニメーション(DoTween)
		// 相手キャラクターの位置へジャンプで近づき、同じ動きで元の場所に戻る
		transform.DOJump(targetChara.transform.position, // 指定座標までジャンプしながら移動する
				1.0f, // ジャンプの高さ
				1, // ジャンプ回数
				0.5f) // アニメーション時間(秒)
			.SetEase(Ease.Linear) // イージング(変化の度合)を設定
			.SetLoops(2, LoopType.Yoyo); // ループ回数・方式を指定

		//effect
		//sound
	}
}