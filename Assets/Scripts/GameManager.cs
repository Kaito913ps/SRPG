using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
	private MapManager _mapManager; 
	private CharactersManager _charactersManager;
	private GUIManager _guiManager;

	// 進行管理変数
	private Character _selectingChara; 
	private SkillDefine.Skill _selectingSkill; 
	private List<MapBlock> _reachableBlocks;
	private List<MapBlock> _attackableBlocks; 
	private bool _isGameSet; 

	// 行動キャンセル処理用変数
	private int _charaStartPos_X;
	private int _charaStartPos_Z; 
	private MapBlock _attackBlock; 

	// ターン進行モード一覧
	private enum Phase
	{
		MyTurn_Start,       // 自分のターン：開始時
		MyTurn_Moving,      // 自分のターン：移動先選択中
		MyTurn_Command,     // 自分のターン：移動後のコマンド選択中
		MyTurn_Targeting,   // 自分のターン：攻撃の対象を選択中
		MyTurn_Result,      // 自分のターン：行動結果表示中
		EnemyTurn_Start,    // 敵のターン：開始時
		EnemyTurn_Result    // 敵のターン：行動結果表示中
	}
	private Phase nowPhase; // 現在の進行モード

	void Start()
	{
		// 参照取得
		_mapManager = GetComponent<MapManager>();
		_charactersManager = GetComponent<CharactersManager>();
		_guiManager = GetComponent<GUIManager>();

		// リストを初期化
		_reachableBlocks = new List<MapBlock>();
		_attackableBlocks = new List<MapBlock>();

		nowPhase = Phase.MyTurn_Start; // 開始時の進行モード
	}

	void Update()
	{
		// ゲーム終了後なら処理せず終了
		if (_isGameSet)
			return;

		// タップ検出処理
		if (Input.GetMouseButtonDown(0) &&
			!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) 
		{
			GetMapBlockByTapPos();
		}
	}

	/// <summary>
	/// タップした場所にあるオブジェクトを見つけ、選択処理などを開始する
	/// </summary>
	private void GetMapBlockByTapPos()
	{
		GameObject targetObject = null; // タップ対象のオブジェクト

		// タップした方向にカメラからRayを飛ばす
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast(ray, out hit))
		{
			// Rayに当たる位置に存在するオブジェクトを取得(対象にColliderが付いている必要がある)
			targetObject = hit.collider.gameObject;
		}

		// 対象オブジェクト(マップブロック)が存在する場合の処理
		if (targetObject != null)
		{
			SelectBlock(targetObject.GetComponent<MapBlock>());
		}
	}

	/// <summary>
	/// 指定したブロックを選択状態にする処理
	/// </summary>
	/// <param name="targetMapBlock">対象のブロックデータ</param>
	private void SelectBlock(MapBlock targetBlock)
	{
		// 現在の進行モードごとに異なる処理を開始する
		switch (nowPhase)
		{
			// 自分のターン：開始時
			case Phase.MyTurn_Start:
				_mapManager.AllSelectionModeClear();
				targetBlock.SetSelectionMode(MapBlock.Highlight.Select);

				// 選択した位置に居るキャラクターのデータを取得
				var charaData =	_charactersManager.GetCharacterDataByPos(targetBlock._xPos, targetBlock._zPos);
				if (charaData != null)
				{
					_selectingChara = charaData;
					// 選択キャラクターの現在位置を記憶
					_charaStartPos_X = _selectingChara._xPos;
					_charaStartPos_Z = _selectingChara._zPos;
					_guiManager.ShowStatusWindow(_selectingChara);

					// 移動可能な場所リストを取得する
					_reachableBlocks = _mapManager.SearchReachableBlocks(charaData._xPos, charaData._zPos);

					// 移動可能な場所リストを表示する
					foreach (MapBlock mapBlock in _reachableBlocks)
						mapBlock.SetSelectionMode(MapBlock.Highlight.Reachable);

					_guiManager.ShowMoveCancelButton();
					ChangePhase(Phase.MyTurn_Moving);
				}
				else
				{
					ClearSelectingChara();
				}
				break;

			// 自分のターン：移動先選択中
			case Phase.MyTurn_Moving:
				// 敵キャラクターを選択中なら移動をキャンセルして終了
				if (_selectingChara._isEnemy)
				{
					CancelMoving();
					break;
				}

				// 選択ブロックが移動可能な場所リスト内にある場合、移動処理を開始
				if (_reachableBlocks.Contains(targetBlock))
				{
					_selectingChara.MovePosition(targetBlock._xPos, targetBlock._zPos);

					_reachableBlocks.Clear();

					_mapManager.AllSelectionModeClear();

					_guiManager.HideMoveCancelButton();

					// 指定秒数経過後に処理を実行する(DoTween)
					DOVirtual.DelayedCall( 0.5f,() =>
						{
							_guiManager.ShowCommandButtons(_selectingChara);
							ChangePhase(Phase.MyTurn_Command);
						}
					);
				}
				break;

			// 自分のターン：移動後のコマンド選択中
			case Phase.MyTurn_Command:
				// 攻撃範囲のブロックを選択した時、行動するかの確認ボタンを表示する
				if (_attackableBlocks.Contains(targetBlock))
				{
					_attackBlock = targetBlock;
					_guiManager.ShowDecideButtons();

					_attackableBlocks.Clear();
					_mapManager.AllSelectionModeClear();

					_attackBlock.SetSelectionMode(MapBlock.Highlight.Attackable);

					ChangePhase(Phase.MyTurn_Targeting);
				}
				break;
		}
	}

	/// <summary>
	/// 選択中のキャラクター情報を初期化する
	/// </summary>
	private void ClearSelectingChara()
	{
		_selectingChara = null;
		_guiManager.HideStatusWindow();
	}

	/// <summary>
	/// 攻撃コマンドボタン処理
	/// </summary>
	public void AttackCommand()
	{
		_selectingSkill = SkillDefine.Skill._None;
		GetAttackableBlocks();
	}
	/// <summary>
	/// 特技コマンドボタン処理
	/// </summary>
	public void SkillCommand()
	{
		_selectingSkill = _selectingChara._skill;
		GetAttackableBlocks();
	}
	/// <summary>
	/// 攻撃・特技コマンド選択後に対象ブロックを表示する処理
	/// </summary>
	private void GetAttackableBlocks()
	{
		_guiManager.HideCommandButtons();

		// 攻撃可能な場所リストを取得する
		// （特技：ファイアボールの場合はマップ全域に対応）
		if (_selectingSkill == SkillDefine.Skill.FireBall)
			_attackableBlocks = _mapManager.MapBlocksToList();
		else
			_attackableBlocks = _mapManager.SearchAttackableBlocks(_selectingChara._xPos, _selectingChara._zPos);

		// 攻撃可能な場所リストを表示する
		foreach (MapBlock mapBlock in _attackableBlocks)
			mapBlock.SetSelectionMode(MapBlock.Highlight.Attackable);
	}

	/// <summary>
	/// 待機コマンドボタン処理
	/// </summary>
	public void StandbyCommand()
	{
		_guiManager.HideCommandButtons();
		ChangePhase(Phase.EnemyTurn_Start);
	}

	/// <summary>
	/// 行動内容決定ボタン処理
	/// </summary>
	public void ActionDecideButton()
	{
		_guiManager.HideDecideButtons();
		_attackBlock.SetSelectionMode(MapBlock.Highlight.Off);

		var targetChara = _charactersManager.GetCharacterDataByPos(_attackBlock._xPos, _attackBlock._zPos);
		if (targetChara != null)
		{
			CharaAttack(_selectingChara, targetChara);

			ChangePhase(Phase.MyTurn_Result);
			return;
		}
		else
		{
			ChangePhase(Phase.EnemyTurn_Start);
		}
	}
	/// <summary>
	/// 行動内容リセットボタン処理
	/// </summary>
	public void ActionCancelButton()
	{
		_guiManager.HideDecideButtons();
		_attackBlock.SetSelectionMode(MapBlock.Highlight.Off);

		_selectingChara.MovePosition(_charaStartPos_X, _charaStartPos_Z);
		ClearSelectingChara();

		ChangePhase(Phase.MyTurn_Start, true);
	}

	/// <summary>
	/// キャラクターが他のキャラクターに攻撃する処理
	/// </summary>
	/// <param name="attackChara">攻撃側キャラデータ</param>
	/// <param name="defenseChara">防御側キャラデータ</param>
	private void CharaAttack(Character attackChara, Character defenseChara)
	{
		// ダメージ計算処理
		int damageValue;
		int attackPoint = attackChara._atk; 
		int defencePoint = defenseChara._def; 
		// 防御力0化デバフがかかっていた時の処理
		if (defenseChara._isDefBreak)
			defencePoint = 0;

		// ダメージ＝攻撃力－防御力で計算
		damageValue = attackPoint - defencePoint;
		// 相性によるダメージ倍率を計算
		float ratio = GetDamageRatioByAttribute(attackChara, defenseChara); 
		damageValue = (int)(damageValue * ratio);
		
	
		if (damageValue < 0)
			damageValue = 0;

		// 選択した特技によるダメージ値補正および効果処理
		switch (_selectingSkill)
		{
			case SkillDefine.Skill.Critical: // 会心の一撃
				
				damageValue *= 2;
				
				attackChara._isSkillLock = true;
				break;

			case SkillDefine.Skill.DefBreak: // シールド破壊
				
				damageValue = 0;
				
				defenseChara._isDefBreak = true;
				break;

			case SkillDefine.Skill.Heal: // ヒール
				damageValue = (int)(attackPoint * -0.5f);
				break;

			case SkillDefine.Skill.FireBall: // ファイアボール
				
				damageValue /= 2;
				break;

			default: // 特技無しor通常攻撃時
				break;
		}

		// キャラクター攻撃アニメーション
		// (ヒール・ファイアボールはアニメなし)
		if (_selectingSkill != SkillDefine.Skill.Heal &&
			_selectingSkill != SkillDefine.Skill.FireBall)
			attackChara.AttackAnimation(defenseChara);
		// アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
		DOVirtual.DelayedCall(0.45f, () =>
			{
				GetComponent<AudioSource>().Play();
			}
		);

		_guiManager._battleWindowUI.ShowWindow(defenseChara, damageValue);

		defenseChara._nowHP -= damageValue;
		defenseChara._nowHP = Mathf.Clamp(defenseChara._nowHP, 0, defenseChara._maxHP);

		// HP0になったキャラクターを削除する
		if (defenseChara._nowHP == 0)
			_charactersManager.DeleteCharaData(defenseChara);

		_selectingSkill = SkillDefine.Skill._None;

		// ターン切り替え処理(遅延実行)
		DOVirtual.DelayedCall( 2.0f,() =>
			{
				_guiManager._battleWindowUI.HideWindow();
				
				if (nowPhase == Phase.MyTurn_Result) 
					ChangePhase(Phase.EnemyTurn_Start);
				else if (nowPhase == Phase.EnemyTurn_Result)
					ChangePhase(Phase.MyTurn_Start);
			}
		);
	}


	/// <summary>
	/// ターン進行モードを変更する
	/// </summary>
	/// <param name="newPhase">変更先モード</param>
	/// <param name="noLogos">ロゴ非表示フラグ(省略可能・省略するとfalse)</param>
	private void ChangePhase(Phase newPhase, bool noLogos = false)
	{
		// ゲーム終了後なら処理せず終了
		if (_isGameSet)
			return;

		
		nowPhase = newPhase;

		// 特定のモードに切り替わったタイミングで行う処理
		switch (nowPhase)
		{

			// 自分のターン：開始時
			case Phase.MyTurn_Start:
				
				if (!noLogos)
					_guiManager.ShowLogo_PlayerTurn();
				break;

			// 敵のターン：開始時
			case Phase.EnemyTurn_Start:
				// 敵のターン開始時のロゴを表示
				if (!noLogos)
					_guiManager.ShowLogo_EnemyTurn();

				
				DOVirtual.DelayedCall( 1.0f, () =>
					{
						EnemyCommand();
					});
				break;
		}
	}

	/// <summary>
	/// (敵のターン開始時に呼出)
	/// 敵キャラクターのうちいずれか一体を行動させてターンを終了する
	/// </summary>
	private void EnemyCommand()
	{
		
		_selectingSkill = SkillDefine.Skill._None;

		// 生存中の敵キャラクターのリストを作成する
		var enemyCharas = new List<Character>(); 
		foreach (Character charaData in _charactersManager._characters)
		{
			// 全生存キャラクターから敵フラグの立っているキャラクターをリストに追加
			if (charaData._isEnemy)
				enemyCharas.Add(charaData);
		}

		
		var actionPlan = TargetFinder.GetRandomActionPlan(_mapManager, _charactersManager, enemyCharas);
		// 組み合わせのデータが存在すれば攻撃開始
		if (actionPlan != null)
		{
			actionPlan._charaData.MovePosition(actionPlan._toMoveBlock._xPos, actionPlan._toMoveBlock._zPos);
			
			DOVirtual.DelayedCall( 1.0f,() =>
				{
					CharaAttack(actionPlan._charaData, actionPlan._toAttackChara);
				}
			);

			ChangePhase(Phase.EnemyTurn_Result);
			return;
		}

		int randId = Random.Range(0, enemyCharas.Count);
		Character targetEnemy = enemyCharas[randId];

		_reachableBlocks = _mapManager.SearchReachableBlocks(targetEnemy._xPos, targetEnemy._zPos);
		randId = Random.Range(0, _reachableBlocks.Count);
		MapBlock targetBlock = _reachableBlocks[randId]; 
		targetEnemy.MovePosition(targetBlock._xPos, targetBlock._zPos);

		_reachableBlocks.Clear();
		_attackableBlocks.Clear();
	
		ChangePhase(Phase.MyTurn_Start);
	}

	/// <summary>
	/// 攻撃側・防御側の属性の相性によるダメージ倍率を返す
	/// </summary>
	/// <returns>ダメージ倍率</returns>
	private float GetDamageRatioByAttribute(Character attackChara, Character defenseChara)
	{
		// 各ダメージ倍率を定義
		const float RATIO_NORMAL = 1.0f; 
		const float RATIO_GOOD = 1.2f; // 相性が良い(攻撃側が有利)
		const float RATIO_BAD = 0.8f; // 相性が悪い(攻撃側が不利)

		Character.Attribute atkAttr = attackChara._attribute; // 攻撃側の属性
		Character.Attribute defAttr = defenseChara._attribute; // 防御側の属性

		// 相性決定処理
		// 属性ごとに良相性→悪相性の順でチェックし、どちらにも当てはまらないなら通常倍率を返す
		switch (atkAttr)
		{
			case Character.Attribute.Water: // 攻撃側：水属性
				if (defAttr == Character.Attribute.Fire)
					return RATIO_GOOD;
				else if (defAttr == Character.Attribute.Soil)
					return RATIO_BAD;
				else
					return RATIO_NORMAL;

			case Character.Attribute.Fire: // 攻撃側：火属性
				if (defAttr == Character.Attribute.Wind)
					return RATIO_GOOD;
				else if (defAttr == Character.Attribute.Water)
					return RATIO_BAD;
				else
					return RATIO_NORMAL;

			case Character.Attribute.Wind: // 攻撃側：風属性
				if (defAttr == Character.Attribute.Soil)
					return RATIO_GOOD;
				else if (defAttr == Character.Attribute.Fire)
					return RATIO_BAD;
				else
					return RATIO_NORMAL;

			case Character.Attribute.Soil: // 攻撃側：土属性
				if (defAttr == Character.Attribute.Water)
					return RATIO_GOOD;
				else if (defAttr == Character.Attribute.Wind)
					return RATIO_BAD;
				else
					return RATIO_NORMAL;

			default:
				return RATIO_NORMAL;
		}
	}

	/// <summary>
	/// 選択中のキャラクターの移動入力待ち状態を解除する
	/// </summary>
	public void CancelMoving()
	{
		_mapManager.AllSelectionModeClear();

		_reachableBlocks.Clear();

		ClearSelectingChara();
		
		_guiManager.HideMoveCancelButton();
		
		ChangePhase(Phase.MyTurn_Start, true);
	}

	/// <summary>
	/// ゲームの終了条件を満たすか確認し、満たすならゲームを終了する
	/// </summary>
	public void CheckGameSet()
	{
		bool isWin = true;
		
		bool isLose = true;

		// それぞれ生きている敵・味方が存在するかをチェック
		foreach (var charaData in _charactersManager._characters)
		{
			if (charaData._isEnemy) // 敵が居るので勝利フラグOff
				isWin = false;
			else // 味方が居るので敗北フラグOff
				isLose = false;
		}

		// 勝利または敗北のフラグが立ったままならゲームを終了する
		// (どちらのフラグも立っていないなら何もせずターンが進行する)
		if (isWin || isLose)
		{
			// ゲーム終了フラグを立てる
			_isGameSet = true;

			// ロゴUIとフェードインを表示する(遅延実行)
			DOVirtual.DelayedCall(1.5f, () =>
				{
					if (isWin) // ゲームクリア演出
						_guiManager.ShowLogo_GameClear();
					else // ゲームオーバー演出
						_guiManager.ShowLogo_GameOver();

					_reachableBlocks.Clear();
					_mapManager.AllSelectionModeClear();
					_guiManager.StartFadeIn();
				}
			);

			DOVirtual.DelayedCall(7.0f, () =>
				{
					SceneManager.LoadScene("Enhance");
				}
			);
		}
	}

}