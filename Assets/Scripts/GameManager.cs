using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    // マップマネージャ
    private MapManager _mapManager;
    // 全キャラクター管理クラス
    private CharactersManager _charactersManager;
    // GUIマネージャ
    private GUIManager _guiManager;

    // 進行管理変数
    // 選択中のキャラクター(誰も選択していないならfalse)
    private Character _selectingChara;
    // 選択中の特技(通常攻撃時はNONE固定)
    private SkillDefine.Skill _selectingSkill;
    // 選択中のキャラクターの移動可能ブロックリスト
    private List<MapBlock> _reachableBlocks;
    // 選択中のキャラクターの攻撃可能ブロックリスト
    private List<MapBlock> _attackableBlocks;
    // ゲーム終了フラグ(決着がついた後ならtrue)
    private bool _isGameSet;

    // 行動キャンセル処理用変数
    // 選択キャラクターの移動前の位置(X方向)
    private int _charaStartPos_X;
    // 選択キャラクターの移動前の位置(Z方向)
    private int _charaStartPos_Z;

    // 選択キャラクターの攻撃先のブロック
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
    // 現在の進行モード
    private Phase nowPhase;

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
        // タップ対象のオブジェクト
        GameObject targetObject = null; 

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
            // ブロック選択時処理
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
                // 全ブロックの選択状態を解除
                _mapManager.AllSelectionModeClear();
                // ブロックを選択状態の表示にする
                targetBlock.SetSelectionMode(MapBlock.Highlight.Select);

				// 選択した位置に居るキャラクターのデータを取得
				var charaData =	_charactersManager.GetCharacterDataByPos(targetBlock._xPos, targetBlock._zPos);
				if (charaData != null)
				{
                    // キャラクターが存在する選択中のキャラクター情報に記憶
                    _selectingChara = charaData;
					// 選択キャラクターの現在位置を記憶
					_charaStartPos_X = _selectingChara._xPos;
					_charaStartPos_Z = _selectingChara._zPos;
                    // キャラクターのステータスをUIに表示する
                    _guiManager.ShowStatusWindow(_selectingChara);

					// 移動可能な場所リストを取得する
					_reachableBlocks = _mapManager.SearchReachableBlocks(charaData._xPos, charaData._zPos);

					// 移動可能な場所リストを表示する
					foreach (MapBlock mapBlock in _reachableBlocks)
						mapBlock.SetSelectionMode(MapBlock.Highlight.Reachable);

                    // 移動キャンセルボタン表示
                    _guiManager.ShowMoveCancelButton();
                    // 進行モードを進める：移動先選択中
                    ChangePhase(Phase.MyTurn_Moving);
				}
				else
				{
                    // キャラクターが存在しない選択中のキャラクター情報を初期化する
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
                    // 選択中のキャラクターを移動させる
                    _selectingChara.MovePosition(targetBlock._xPos, targetBlock._zPos);

                    // 移動可能な場所リストを初期化する
                    _reachableBlocks.Clear();

                    // 全ブロックの選択状態を解除
                    _mapManager.AllSelectionModeClear();

                    // 移動キャンセルボタン非表示
                    _guiManager.HideMoveCancelButton();

                    // 指定秒数経過後に処理を実行する(DoTween)
                    DOVirtual.DelayedCall(
                        0.5f, // 遅延時間(秒)
                        () =>
                        {
							// 遅延実行する内容
							// コマンドボタンを表示する
                            _guiManager.ShowCommandButtons(_selectingChara);
                            // 進行モードを進める：移動後のコマンド選択中
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
                    // 攻撃先のブロック情報を記憶
                    _attackBlock = targetBlock;
					// 行動決定・キャンセルボタンを表示する
					_guiManager.ShowDecideButtons();

                    // 攻撃可能な場所リストを初期化する
                    _attackableBlocks.Clear();
                    // 全ブロックの選択状態を解除
                    _mapManager.AllSelectionModeClear();

                    // 攻撃先のブロックを強調表示する
                    _attackBlock.SetSelectionMode(MapBlock.Highlight.Attackable);

                    // 進行モードを進める：攻撃の対象を選択中
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
        // 選択中のキャラクターを初期化する
        _selectingChara = null;
        // 攻撃範囲を取得して表示する
        _guiManager.HideStatusWindow();
	}

	/// <summary>
	/// 攻撃コマンドボタン処理
	/// </summary>
	public void AttackCommand()
	{
        // 特技の選択をオフにする
        _selectingSkill = SkillDefine.Skill._None;
        // 攻撃範囲を取得して表示する
        GetAttackableBlocks();
	}
	/// <summary>
	/// 特技コマンドボタン処理
	/// </summary>
	public void SkillCommand()
	{
        // 特技の選択をオフにする
        _selectingSkill = _selectingChara._skill;
        // 攻撃範囲を取得して表示する
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
        // コマンドボタンを非表示にする
        _guiManager.HideCommandButtons();
        // 進行モードを進める(敵のターンへ)
        ChangePhase(Phase.EnemyTurn_Start);
	}

	/// <summary>
	/// 行動内容決定ボタン処理
	/// </summary>
	public void ActionDecideButton()
	{
        // 行動決定・キャンセルボタンを非表示にする
        _guiManager.HideDecideButtons();
        // 攻撃先のブロックの強調表示を解除する
        _attackBlock.SetSelectionMode(MapBlock.Highlight.Off);

        // 攻撃対象の位置に居るキャラクターのデータを取得
        var targetChara = _charactersManager.GetCharacterDataByPos(_attackBlock._xPos, _attackBlock._zPos);
		if (targetChara != null)
		{
            // 攻撃対象のキャラクターが存在する
            // キャラクター攻撃処理
            CharaAttack(_selectingChara, targetChara);

            // 進行モードを進める(行動結果表示へ)
            ChangePhase(Phase.MyTurn_Result);
			return;
		}
		else
		{
            // 攻撃対象が存在しない
            // 進行モードを進める(敵のターンへ)
            ChangePhase(Phase.EnemyTurn_Start);
		}
	}
	/// <summary>
	/// 行動内容リセットボタン処理
	/// </summary>
	public void ActionCancelButton()
	{
        // 行動決定・キャンセルボタンを非表示にする
        _guiManager.HideDecideButtons();
        // 攻撃先のブロックの強調表示を解除する
        _attackBlock.SetSelectionMode(MapBlock.Highlight.Off);

        // キャラクターを移動前の位置に戻す
        _selectingChara.MovePosition(_charaStartPos_X, _charaStartPos_Z);

        // キャラクターの選択を解除する
        ClearSelectingChara();

        // 進行モードを戻す(ターンの最初へ)
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
            // 会心の一撃
            case SkillDefine.Skill.Critical:
                // ダメージ２倍
                damageValue *= 2;
                // 特技使用不可状態にする
                attackChara._isSkillLock = true;
				break;

            // シールド破壊
            case SkillDefine.Skill.DefBreak:
                // ダメージ０固定
                damageValue = 0;
                // 防御力0化デバフをセット
                defenseChara._isDefBreak = true;
				break;

            // ヒール
            case SkillDefine.Skill.Heal:
                // 回復
                // (回復量は攻撃力の半分。負数にする事でダメージ計算時に回復する)
                damageValue = (int)(attackPoint * -0.5f);
				break;

            // ファイアボール
            case SkillDefine.Skill.FireBall:
                // ダメージ半減
                damageValue /= 2;
				break;

            // 特技無しor通常攻撃時
            default:
				break;
		}

		// キャラクター攻撃アニメーション
		// (ヒール・ファイアボールはアニメなし)
		if (_selectingSkill != SkillDefine.Skill.Heal &&
			_selectingSkill != SkillDefine.Skill.FireBall)
			attackChara.AttackAnimation(defenseChara);
        // アニメーション内で攻撃が当たったくらいのタイミングでSEを再生
        DOVirtual.DelayedCall(
            0.45f, // 遅延時間(秒)
            () =>
            {// 遅延実行する内容
             // AudioSourceを再生
                GetComponent<AudioSource>().Play();
            }
        );

        // バトル結果表示ウィンドウの表示設定
        _guiManager._battleWindowUI.ShowWindow(defenseChara, damageValue);

        // ダメージ量分防御側のHPを減少
        defenseChara._nowHP -= damageValue;
        // HPが0～最大値の範囲に収まるよう補正
        defenseChara._nowHP = Mathf.Clamp(defenseChara._nowHP, 0, defenseChara._maxHP);

		// HP0になったキャラクターを削除する
		if (defenseChara._nowHP == 0)
			_charactersManager.DeleteCharaData(defenseChara);

        // 特技の選択状態を解除する
        _selectingSkill = SkillDefine.Skill._None;

        // ターン切り替え処理(遅延実行)
        DOVirtual.DelayedCall(
            2.0f, // 遅延時間(秒)
            () =>
            {// 遅延実行する内容
             // ウィンドウを非表示化
                _guiManager._battleWindowUI.HideWindow();
                // ターンを切り替える
                // 敵のターンへ
                if (nowPhase == Phase.MyTurn_Result) 
                    ChangePhase(Phase.EnemyTurn_Start);
                // 自分のターンへ
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

        // モード変更を保存
        nowPhase = newPhase;

		// 特定のモードに切り替わったタイミングで行う処理
		switch (nowPhase)
		{

			// 自分のターン：開始時
			case Phase.MyTurn_Start:

                // 自分のターン開始時のロゴを表示
                if (!noLogos)
					_guiManager.ShowLogo_PlayerTurn();
				break;

			// 敵のターン：開始時
			case Phase.EnemyTurn_Start:
				// 敵のターン開始時のロゴを表示
				if (!noLogos)
					_guiManager.ShowLogo_EnemyTurn();

                // 敵の行動を開始する処理
                // (ロゴ表示後に開始したいので遅延処理にする)
                DOVirtual.DelayedCall(
                    1.0f, // 遅延時間(秒)
                    () =>
                    {// 遅延実行する内容
                        EnemyCommand();
                    }
                );
                break;
        }
	}

	/// <summary>
	/// (敵のターン開始時に呼出)
	/// 敵キャラクターのうちいずれか一体を行動させてターンを終了する
	/// </summary>
	private void EnemyCommand()
	{
        // 特技の選択をオフにする
        _selectingSkill = SkillDefine.Skill._None;

		// 生存中の敵キャラクターのリストを作成する
		var enemyCharas = new List<Character>(); 
		foreach (Character charaData in _charactersManager._characters)
		{
			// 全生存キャラクターから敵フラグの立っているキャラクターをリストに追加
			if (charaData._isEnemy)
				enemyCharas.Add(charaData);
		}

        // 攻撃可能なキャラクター・位置の組み合わせの内１つをランダムに取得
        var actionPlan = TargetFinder.GetRandomActionPlan(_mapManager, _charactersManager, enemyCharas);
		// 組み合わせのデータが存在すれば攻撃開始
		if (actionPlan != null)
		{
            // 敵キャラクター移動処理
            actionPlan._charaData.MovePosition(actionPlan._toMoveBlock._xPos, actionPlan._toMoveBlock._zPos);

            // 敵キャラクター攻撃処理
            // (移動後のタイミングで攻撃開始するよう遅延実行)
            DOVirtual.DelayedCall(
                1.0f, // 遅延時間(秒)
                () =>
                {// 遅延実行する内容
                    CharaAttack(actionPlan._charaData, actionPlan._toAttackChara);
                }
            );

            // 進行モードを進める(行動結果表示へ)
            ChangePhase(Phase.EnemyTurn_Result);
			return;
		}

        // 攻撃可能な相手が見つからなかった場合移動させる１体をランダムに選ぶ
        int randId = Random.Range(0, enemyCharas.Count);
        // 行動対象の敵データ
        Character targetEnemy = enemyCharas[randId];

		_reachableBlocks = _mapManager.SearchReachableBlocks(targetEnemy._xPos, targetEnemy._zPos);
		randId = Random.Range(0, _reachableBlocks.Count);
		MapBlock targetBlock = _reachableBlocks[randId]; 
		targetEnemy.MovePosition(targetBlock._xPos, targetBlock._zPos);

        // 移動場所・攻撃場所リストをクリアする
        _reachableBlocks.Clear();
		_attackableBlocks.Clear();

        // 進行モードを進める(自分のターンへ)
        ChangePhase(Phase.MyTurn_Start);
	}

	/// <summary>
	/// 攻撃側・防御側の属性の相性によるダメージ倍率を返す
	/// </summary>
	/// <returns>ダメージ倍率</returns>
	private float GetDamageRatioByAttribute(Character attackChara, Character defenseChara)
	{
        // 各ダメージ倍率を定義
        // 通常
        const float RATIO_NORMAL = 1.0f;
        // 相性が良い(攻撃側が有利)
        const float RATIO_GOOD = 1.2f;
        // 相性が悪い(攻撃側が不利)
        const float RATIO_BAD = 0.8f;

        // 攻撃側の属性
        Character.Attribute atkAttr = attackChara._attribute;
        // 防御側の属性
        Character.Attribute defAttr = defenseChara._attribute; 

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
        // 全ブロックの選択状態を解除
        _mapManager.AllSelectionModeClear();
        // 移動可能な場所リストを初期化する
        _reachableBlocks.Clear();
        // 選択中のキャラクター情報を初期化する
        ClearSelectingChara();
        // 移動やめるボタン非表示
        _guiManager.HideMoveCancelButton();
        // フェーズを元に戻す(ロゴを表示しない設定)
        ChangePhase(Phase.MyTurn_Start, true);
	}

	/// <summary>
	/// ゲームの終了条件を満たすか確認し、満たすならゲームを終了する
	/// </summary>
	public void CheckGameSet()
	{
        // プレイヤー勝利フラグ(生きている敵がいるならOffになる)
        bool isWin = true;
        // プレイヤー敗北フラグ(生きている味方がいるならOffになる)
        bool isLose = true;

		// それぞれ生きている敵・味方が存在するかをチェック
		foreach (var charaData in _charactersManager._characters)
		{
            // 敵が居るので勝利フラグOff
            if (charaData._isEnemy)
				isWin = false;
            // 味方が居るので敗北フラグOff
            else
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
                    // ゲームクリア演出
                    if (isWin) 
						_guiManager.ShowLogo_GameClear();
                    // ゲームオーバー演出
                    else
                        _guiManager.ShowLogo_GameOver();

                    // 移動可能な場所リストを初期化する
                    _reachableBlocks.Clear();
                    // 全ブロックの選択状態を解除
                    _mapManager.AllSelectionModeClear();
                    // フェードイン開始
                    _guiManager.StartFadeIn();
				}
			);

            // Gameシーンの再読み込み(遅延実行)
            DOVirtual.DelayedCall(7.0f, () =>
				{
					SceneManager.LoadScene("Enhance");
				}
			);
		}
	}

}