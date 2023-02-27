using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI�R���|�[�l���g�������̂ɕK�v
using DG.Tweening;

public class GUIManager : MonoBehaviour
{
	// �X�e�[�^�X�E�B���h�EUI
	public GameObject _statusWindow; 
	public Text _nameText; 
	public Image _attributeIcon; 
	public Image _hpGageImage;
	public Text _hpText;
	public Text _atkText; 
	public Text _defText; 

	public Sprite attr_Water; // �������A�C�R���摜
	public Sprite attr_Fire;  // �Α����A�C�R���摜
	public Sprite attr_Wind;  // �������A�C�R���摜
	public Sprite attr_Soil;  // �y�����A�C�R���摜

	// �L�����N�^�[�̃R�}���h�{�^��
	public GameObject _commandButtons; 
	public Button _skillCommandButton; 
	public Text _skillText; // �I���L�����N�^�[�̓��Z�̐���Text


	// �o�g�����ʕ\��UI�����N���X
	public BattleWindowUI _battleWindowUI;

	// �e�탍�S�摜
	public Image _playerTurnImage; 
	public Image _enemyTurnImage; 
	public Image _gameClearImage;
	public Image _gameOverImage; 

	// �t�F�[�h�C���p�摜
	public Image _fadeImage;

	// �ړ��L�����Z���{�^��UI
	public GameObject _moveCancelButton;

	// �s������E�L�����Z���{�^��UI
	public GameObject _decideButtons;

	void Start()
	{
		// UI������
		HideStatusWindow();
		HideCommandButtons(); 
		HideMoveCancelButton(); 
		HideDecideButtons();
	}

	/// <summary>
	/// �X�e�[�^�X�E�B���h�E��\������
	/// </summary>
	/// <param name="charaData">�\���L�����N�^�[�f�[�^</param>
	public void ShowStatusWindow(Character charaData)
	{
		_statusWindow.SetActive(true);

		_nameText.text = charaData._charaName;

		// ����Image�\��
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

		// HP�Q�[�W�\��
		// �ő�l�ɑ΂��錻��HP�̊������Q�[�WImage��fillAmount�ɃZ�b�g����
		float ratio = (float)charaData._nowHP / charaData._maxHP;
		_hpGageImage.fillAmount = ratio;

		_hpText.text = charaData._nowHP + "/" + charaData._maxHP;
		_atkText.text = charaData._atk.ToString();
		// �h���Text�\��(int����string�ɕϊ�)
		if (!charaData._isDefBreak)
			_defText.text = charaData._def.ToString();
		else // (�h���0�����Ă���ꍇ)
			_defText.text = "<color=red>0</color>";
	}
	/// <summary>
	/// �X�e�[�^�X�E�B���h�E���B��
	/// </summary>
	public void HideStatusWindow()
	{
		// �I�u�W�F�N�g��A�N�e�B�u��
		_statusWindow.SetActive(false);
	}

	/// <summary>
	/// �R�}���h�{�^����\������
	/// </summary>
	/// <param name="selectChara">�s�����̃L�����N�^�[�f�[�^</param>
	public void ShowCommandButtons(Character selectChara)
	{
		_commandButtons.SetActive(true);

		// �I���L�����N�^�[�̓��Z��Text�ɕ\������
		SkillDefine.Skill skill = selectChara._skill; 
		string skillName = SkillDefine.dic_SkillName[skill]; 
		string skillInfo = SkillDefine.dic_SkillInfo[skill]; 
		// ���b�`�e�L�X�g�ŃT�C�Y��ύX���Ȃ��當����\��
		_skillText.text = "<size=40>" + skillName + "</size>\n" + skillInfo;

		// ���Z�g�p�s��ԂȂ���Z�{�^���������Ȃ�����
		if (selectChara._isSkillLock)
			_skillCommandButton.interactable = false;
		else
			_skillCommandButton.interactable = true;
	}

	/// <summary>
	/// �R�}���h�{�^�����B��
	/// </summary>
	public void HideCommandButtons()
	{
		_commandButtons.SetActive(false);
	}

	/// <summary>
	/// �v���C���[�̃^�[���ɐ؂�ւ�������̃��S�摜��\������
	/// </summary>
	public void ShowLogo_PlayerTurn()
	{
		// ���X�ɕ\������\�����s���A�j���[�V����(Tween)
		_playerTurnImage.DOFade(1.0f,1.0f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo); 
	}
	/// <summary>
	/// �G�̃^�[���ɐ؂�ւ�������̃��S�摜��\������
	/// </summary>
	public void ShowLogo_EnemyTurn()
	{
		// ���X�ɕ\������\�����s���A�j���[�V����(Tween)
		_enemyTurnImage.DOFade(1.0f,1.0f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo);
	}

	/// <summary>
	/// �ړ��L�����Z���{�^����\������
	/// </summary>
	public void ShowMoveCancelButton()
	{
		_moveCancelButton.SetActive(true);
	}
	/// <summary>
	/// �ړ��L�����Z���{�^�����\���ɂ���
	/// </summary>
	public void HideMoveCancelButton()
	{
		_moveCancelButton.SetActive(false);
	}

	/// <summary>
	/// �Q�[���N���A���̃��S�摜��\������
	/// </summary>
	public void ShowLogo_GameClear()
	{
		// ���X�ɕ\������A�j���[�V����
		_gameClearImage.DOFade(1.0f,1.0f).SetEase(Ease.OutCubic);

		// �g�偨�k�����s���A�j���[�V����
		_gameClearImage.transform.DOScale(1.5f,1.0f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo);
	}
	/// <summary>
	/// �Q�[���I�[�o�[�̃��S�摜��\������
	/// </summary>
	public void ShowLogo_GameOver()
	{
		// ���X�ɕ\������A�j���[�V����
		_gameOverImage.DOFade(1.0f,1.0f).SetEase(Ease.OutCubic);}

	/// <summary>
	/// �t�F�[�h�C�����J�n����
	/// </summary>
	public void StartFadeIn()
	{
		_fadeImage
			.DOFade(1.0f,5.5f).SetEase(Ease.Linear);
	}

	/// <summary>
	/// �s������E�L�����Z���{�^����\������
	/// </summary>
	public void ShowDecideButtons()
	{
		_decideButtons.SetActive(true);
	}
	/// <summary>
	/// �s������E�L�����Z���{�^�����\���ɂ���
	/// </summary>
	public void HideDecideButtons()
	{
		_decideButtons.SetActive(false);
	}

}