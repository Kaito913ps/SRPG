using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを扱うのに必要
using DG.Tweening;

public class GUIManager : MonoBehaviour
{
	// ステータスウィンドウUI
	public GameObject _statusWindow; 
	public Text _nameText; 
	public Image _attributeIcon; 
	public Image _hpGageImage;
	public Text _hpText;
	public Text _atkText; 
	public Text _defText; 

	public Sprite attr_Water; // 水属性アイコン画像
	public Sprite attr_Fire;  // 火属性アイコン画像
	public Sprite attr_Wind;  // 風属性アイコン画像
	public Sprite attr_Soil;  // 土属性アイコン画像

	// キャラクターのコマンドボタン
	public GameObject _commandButtons; 
	public Button _skillCommandButton; 
	public Text _skillText; // 選択キャラクターの特技の説明Text


	// バトル結果表示UI処理クラス
	public BattleWindowUI _battleWindowUI;

	// 各種ロゴ画像
	public Image _playerTurnImage; 
	public Image _enemyTurnImage; 
	public Image _gameClearImage;
	public Image _gameOverImage; 

	// フェードイン用画像
	public Image _fadeImage;

	// 移動キャンセルボタンUI
	public GameObject _moveCancelButton;

	// 行動決定・キャンセルボタンUI
	public GameObject _decideButtons;

	void Start()
	{
		// UI初期化
		HideStatusWindow();
		HideCommandButtons(); 
		HideMoveCancelButton(); 
		HideDecideButtons();
	}

	/// <summary>
	/// ステータスウィンドウを表示する
	/// </summary>
	/// <param name="charaData">表示キャラクターデータ</param>
	public void ShowStatusWindow(Character charaData)
	{
		_statusWindow.SetActive(true);

		_nameText.text = charaData._charaName;

		// 属性Image表示
		switch (charaData._attribute)
		{
			case Character.Attribute.Water:
				_attributeIcon.sprite = attr_Water;
				break;
			case Character.Attribute.Fire:
				_attributeIcon.sprite = attr_Fire;
				break;
			case Character.Attribute.Wind:
				_attributeIcon.sprite = attr_Wind;
				break;
			case Character.Attribute.Soil:
				_attributeIcon.sprite = attr_Soil;
				break;
		}

		// HPゲージ表示
		// 最大値に対する現在HPの割合をゲージImageのfillAmountにセットする
		float ratio = (float)charaData._nowHP / charaData._maxHP;
		_hpGageImage.fillAmount = ratio;

		_hpText.text = charaData._nowHP + "/" + charaData._maxHP;
		_atkText.text = charaData._atk.ToString();
		// 防御力Text表示(intからstringに変換)
		if (!charaData._isDefBreak)
			_defText.text = charaData._def.ToString();
		else // (防御力0化している場合)
			_defText.text = "<color=red>0</color>";
	}
	/// <summary>
	/// ステータスウィンドウを隠す
	/// </summary>
	public void HideStatusWindow()
	{
		// オブジェクト非アクティブ化
		_statusWindow.SetActive(false);
	}

	/// <summary>
	/// コマンドボタンを表示する
	/// </summary>
	/// <param name="selectChara">行動中のキャラクターデータ</param>
	public void ShowCommandButtons(Character selectChara)
	{
		_commandButtons.SetActive(true);

		// 選択キャラクターの特技をTextに表示する
		SkillDefine.Skill skill = selectChara._skill; 
		string skillName = SkillDefine.dic_SkillName[skill]; 
		string skillInfo = SkillDefine.dic_SkillInfo[skill]; 
		// リッチテキストでサイズを変更しながら文字を表示
		_skillText.text = "<size=40>" + skillName + "</size>\n" + skillInfo;

		// 特技使用不可状態なら特技ボタンを押せなくする
		if (selectChara._isSkillLock)
			_skillCommandButton.interactable = false;
		else
			_skillCommandButton.interactable = true;
	}

	/// <summary>
	/// コマンドボタンを隠す
	/// </summary>
	public void HideCommandButtons()
	{
		_commandButtons.SetActive(false);
	}

	/// <summary>
	/// プレイヤーのターンに切り替わった時のロゴ画像を表示する
	/// </summary>
	public void ShowLogo_PlayerTurn()
	{
		// 徐々に表示→非表示を行うアニメーション(Tween)
		_playerTurnImage.DOFade(1.0f,1.0f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo); 
	}
	/// <summary>
	/// 敵のターンに切り替わった時のロゴ画像を表示する
	/// </summary>
	public void ShowLogo_EnemyTurn()
	{
		// 徐々に表示→非表示を行うアニメーション(Tween)
		_enemyTurnImage.DOFade(1.0f,1.0f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo);
	}

	/// <summary>
	/// 移動キャンセルボタンを表示する
	/// </summary>
	public void ShowMoveCancelButton()
	{
		_moveCancelButton.SetActive(true);
	}
	/// <summary>
	/// 移動キャンセルボタンを非表示にする
	/// </summary>
	public void HideMoveCancelButton()
	{
		_moveCancelButton.SetActive(false);
	}

	/// <summary>
	/// ゲームクリア時のロゴ画像を表示する
	/// </summary>
	public void ShowLogo_GameClear()
	{
		// 徐々に表示するアニメーション
		_gameClearImage.DOFade(1.0f,1.0f).SetEase(Ease.OutCubic);

		// 拡大→縮小を行うアニメーション
		_gameClearImage.transform.DOScale(1.5f,1.0f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo);
	}
	/// <summary>
	/// ゲームオーバーのロゴ画像を表示する
	/// </summary>
	public void ShowLogo_GameOver()
	{
		// 徐々に表示するアニメーション
		_gameOverImage.DOFade(1.0f,1.0f).SetEase(Ease.OutCubic);}

	/// <summary>
	/// フェードインを開始する
	/// </summary>
	public void StartFadeIn()
	{
		_fadeImage
			.DOFade(1.0f,5.5f).SetEase(Ease.Linear);
	}

	/// <summary>
	/// 行動決定・キャンセルボタンを表示する
	/// </summary>
	public void ShowDecideButtons()
	{
		_decideButtons.SetActive(true);
	}
	/// <summary>
	/// 行動決定・キャンセルボタンを非表示にする
	/// </summary>
	public void HideDecideButtons()
	{
		_decideButtons.SetActive(false);
	}

}