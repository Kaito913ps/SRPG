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

	// �i�s�Ǘ��ϐ�
	private Character _selectingChara; 
	private SkillDefine.Skill _selectingSkill; 
	private List<MapBlock> _reachableBlocks;
	private List<MapBlock> _attackableBlocks; 
	private bool _isGameSet; 

	// �s���L�����Z�������p�ϐ�
	private int _charaStartPos_X;
	private int _charaStartPos_Z; 
	private MapBlock _attackBlock; 

	// �^�[���i�s���[�h�ꗗ
	private enum Phase
	{
		MyTurn_Start,       // �����̃^�[���F�J�n��
		MyTurn_Moving,      // �����̃^�[���F�ړ���I��
		MyTurn_Command,     // �����̃^�[���F�ړ���̃R�}���h�I��
		MyTurn_Targeting,   // �����̃^�[���F�U���̑Ώۂ�I��
		MyTurn_Result,      // �����̃^�[���F�s�����ʕ\����
		EnemyTurn_Start,    // �G�̃^�[���F�J�n��
		EnemyTurn_Result    // �G�̃^�[���F�s�����ʕ\����
	}
	private Phase nowPhase; // ���݂̐i�s���[�h

	void Start()
	{
		// �Q�Ǝ擾
		_mapManager = GetComponent<MapManager>();
		_charactersManager = GetComponent<CharactersManager>();
		_guiManager = GetComponent<GUIManager>();

		// ���X�g��������
		_reachableBlocks = new List<MapBlock>();
		_attackableBlocks = new List<MapBlock>();

		nowPhase = Phase.MyTurn_Start; // �J�n���̐i�s���[�h
	}

	void Update()
	{
		// �Q�[���I����Ȃ珈�������I��
		if (_isGameSet)
			return;

		// �^�b�v���o����
		if (Input.GetMouseButtonDown(0) &&
			!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) 
		{
			GetMapBlockByTapPos();
		}
	}

	/// <summary>
	/// �^�b�v�����ꏊ�ɂ���I�u�W�F�N�g�������A�I�������Ȃǂ��J�n����
	/// </summary>
	private void GetMapBlockByTapPos()
	{
		GameObject targetObject = null; // �^�b�v�Ώۂ̃I�u�W�F�N�g

		// �^�b�v���������ɃJ��������Ray���΂�
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast(ray, out hit))
		{
			// Ray�ɓ�����ʒu�ɑ��݂���I�u�W�F�N�g���擾(�Ώۂ�Collider���t���Ă���K�v������)
			targetObject = hit.collider.gameObject;
		}

		// �ΏۃI�u�W�F�N�g(�}�b�v�u���b�N)�����݂���ꍇ�̏���
		if (targetObject != null)
		{
			SelectBlock(targetObject.GetComponent<MapBlock>());
		}
	}

	/// <summary>
	/// �w�肵���u���b�N��I����Ԃɂ��鏈��
	/// </summary>
	/// <param name="targetMapBlock">�Ώۂ̃u���b�N�f�[�^</param>
	private void SelectBlock(MapBlock targetBlock)
	{
		// ���݂̐i�s���[�h���ƂɈقȂ鏈�����J�n����
		switch (nowPhase)
		{
			// �����̃^�[���F�J�n��
			case Phase.MyTurn_Start:
				_mapManager.AllSelectionModeClear();
				targetBlock.SetSelectionMode(MapBlock.Highlight.Select);

				// �I�������ʒu�ɋ���L�����N�^�[�̃f�[�^���擾
				var charaData =	_charactersManager.GetCharacterDataByPos(targetBlock._xPos, targetBlock._zPos);
				if (charaData != null)
				{
					_selectingChara = charaData;
					// �I���L�����N�^�[�̌��݈ʒu���L��
					_charaStartPos_X = _selectingChara._xPos;
					_charaStartPos_Z = _selectingChara._zPos;
					_guiManager.ShowStatusWindow(_selectingChara);

					// �ړ��\�ȏꏊ���X�g���擾����
					_reachableBlocks = _mapManager.SearchReachableBlocks(charaData._xPos, charaData._zPos);

					// �ړ��\�ȏꏊ���X�g��\������
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

			// �����̃^�[���F�ړ���I��
			case Phase.MyTurn_Moving:
				// �G�L�����N�^�[��I�𒆂Ȃ�ړ����L�����Z�����ďI��
				if (_selectingChara._isEnemy)
				{
					CancelMoving();
					break;
				}

				// �I���u���b�N���ړ��\�ȏꏊ���X�g���ɂ���ꍇ�A�ړ��������J�n
				if (_reachableBlocks.Contains(targetBlock))
				{
					_selectingChara.MovePosition(targetBlock._xPos, targetBlock._zPos);

					_reachableBlocks.Clear();

					_mapManager.AllSelectionModeClear();

					_guiManager.HideMoveCancelButton();

					// �w��b���o�ߌ�ɏ��������s����(DoTween)
					DOVirtual.DelayedCall( 0.5f,() =>
						{
							_guiManager.ShowCommandButtons(_selectingChara);
							ChangePhase(Phase.MyTurn_Command);
						}
					);
				}
				break;

			// �����̃^�[���F�ړ���̃R�}���h�I��
			case Phase.MyTurn_Command:
				// �U���͈͂̃u���b�N��I���������A�s�����邩�̊m�F�{�^����\������
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
	/// �I�𒆂̃L�����N�^�[��������������
	/// </summary>
	private void ClearSelectingChara()
	{
		_selectingChara = null;
		_guiManager.HideStatusWindow();
	}

	/// <summary>
	/// �U���R�}���h�{�^������
	/// </summary>
	public void AttackCommand()
	{
		_selectingSkill = SkillDefine.Skill._None;
		GetAttackableBlocks();
	}
	/// <summary>
	/// ���Z�R�}���h�{�^������
	/// </summary>
	public void SkillCommand()
	{
		_selectingSkill = _selectingChara._skill;
		GetAttackableBlocks();
	}
	/// <summary>
	/// �U���E���Z�R�}���h�I����ɑΏۃu���b�N��\�����鏈��
	/// </summary>
	private void GetAttackableBlocks()
	{
		_guiManager.HideCommandButtons();

		// �U���\�ȏꏊ���X�g���擾����
		// �i���Z�F�t�@�C�A�{�[���̏ꍇ�̓}�b�v�S��ɑΉ��j
		if (_selectingSkill == SkillDefine.Skill.FireBall)
			_attackableBlocks = _mapManager.MapBlocksToList();
		else
			_attackableBlocks = _mapManager.SearchAttackableBlocks(_selectingChara._xPos, _selectingChara._zPos);

		// �U���\�ȏꏊ���X�g��\������
		foreach (MapBlock mapBlock in _attackableBlocks)
			mapBlock.SetSelectionMode(MapBlock.Highlight.Attackable);
	}

	/// <summary>
	/// �ҋ@�R�}���h�{�^������
	/// </summary>
	public void StandbyCommand()
	{
		_guiManager.HideCommandButtons();
		ChangePhase(Phase.EnemyTurn_Start);
	}

	/// <summary>
	/// �s�����e����{�^������
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
	/// �s�����e���Z�b�g�{�^������
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
	/// �L�����N�^�[�����̃L�����N�^�[�ɍU�����鏈��
	/// </summary>
	/// <param name="attackChara">�U�����L�����f�[�^</param>
	/// <param name="defenseChara">�h�䑤�L�����f�[�^</param>
	private void CharaAttack(Character attackChara, Character defenseChara)
	{
		// �_���[�W�v�Z����
		int damageValue;
		int attackPoint = attackChara._atk; 
		int defencePoint = defenseChara._def; 
		// �h���0���f�o�t���������Ă������̏���
		if (defenseChara._isDefBreak)
			defencePoint = 0;

		// �_���[�W���U���́|�h��͂Ōv�Z
		damageValue = attackPoint - defencePoint;
		// �����ɂ��_���[�W�{�����v�Z
		float ratio = GetDamageRatioByAttribute(attackChara, defenseChara); 
		damageValue = (int)(damageValue * ratio);
		
	
		if (damageValue < 0)
			damageValue = 0;

		// �I���������Z�ɂ��_���[�W�l�␳����ь��ʏ���
		switch (_selectingSkill)
		{
			case SkillDefine.Skill.Critical: // ��S�̈ꌂ
				
				damageValue *= 2;
				
				attackChara._isSkillLock = true;
				break;

			case SkillDefine.Skill.DefBreak: // �V�[���h�j��
				
				damageValue = 0;
				
				defenseChara._isDefBreak = true;
				break;

			case SkillDefine.Skill.Heal: // �q�[��
				damageValue = (int)(attackPoint * -0.5f);
				break;

			case SkillDefine.Skill.FireBall: // �t�@�C�A�{�[��
				
				damageValue /= 2;
				break;

			default: // ���Z����or�ʏ�U����
				break;
		}

		// �L�����N�^�[�U���A�j���[�V����
		// (�q�[���E�t�@�C�A�{�[���̓A�j���Ȃ�)
		if (_selectingSkill != SkillDefine.Skill.Heal &&
			_selectingSkill != SkillDefine.Skill.FireBall)
			attackChara.AttackAnimation(defenseChara);
		// �A�j���[�V�������ōU���������������炢�̃^�C�~���O��SE���Đ�
		DOVirtual.DelayedCall(0.45f, () =>
			{
				GetComponent<AudioSource>().Play();
			}
		);

		_guiManager._battleWindowUI.ShowWindow(defenseChara, damageValue);

		defenseChara._nowHP -= damageValue;
		defenseChara._nowHP = Mathf.Clamp(defenseChara._nowHP, 0, defenseChara._maxHP);

		// HP0�ɂȂ����L�����N�^�[���폜����
		if (defenseChara._nowHP == 0)
			_charactersManager.DeleteCharaData(defenseChara);

		_selectingSkill = SkillDefine.Skill._None;

		// �^�[���؂�ւ�����(�x�����s)
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
	/// �^�[���i�s���[�h��ύX����
	/// </summary>
	/// <param name="newPhase">�ύX�惂�[�h</param>
	/// <param name="noLogos">���S��\���t���O(�ȗ��\�E�ȗ������false)</param>
	private void ChangePhase(Phase newPhase, bool noLogos = false)
	{
		// �Q�[���I����Ȃ珈�������I��
		if (_isGameSet)
			return;

		
		nowPhase = newPhase;

		// ����̃��[�h�ɐ؂�ւ�����^�C�~���O�ōs������
		switch (nowPhase)
		{

			// �����̃^�[���F�J�n��
			case Phase.MyTurn_Start:
				
				if (!noLogos)
					_guiManager.ShowLogo_PlayerTurn();
				break;

			// �G�̃^�[���F�J�n��
			case Phase.EnemyTurn_Start:
				// �G�̃^�[���J�n���̃��S��\��
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
	/// (�G�̃^�[���J�n���Ɍďo)
	/// �G�L�����N�^�[�̂��������ꂩ��̂��s�������ă^�[�����I������
	/// </summary>
	private void EnemyCommand()
	{
		
		_selectingSkill = SkillDefine.Skill._None;

		// �������̓G�L�����N�^�[�̃��X�g���쐬����
		var enemyCharas = new List<Character>(); 
		foreach (Character charaData in _charactersManager._characters)
		{
			// �S�����L�����N�^�[����G�t���O�̗����Ă���L�����N�^�[�����X�g�ɒǉ�
			if (charaData._isEnemy)
				enemyCharas.Add(charaData);
		}

		
		var actionPlan = TargetFinder.GetRandomActionPlan(_mapManager, _charactersManager, enemyCharas);
		// �g�ݍ��킹�̃f�[�^�����݂���΍U���J�n
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
	/// �U�����E�h�䑤�̑����̑����ɂ��_���[�W�{����Ԃ�
	/// </summary>
	/// <returns>�_���[�W�{��</returns>
	private float GetDamageRatioByAttribute(Character attackChara, Character defenseChara)
	{
		// �e�_���[�W�{�����`
		const float RATIO_NORMAL = 1.0f; 
		const float RATIO_GOOD = 1.2f; // �������ǂ�(�U�������L��)
		const float RATIO_BAD = 0.8f; // ����������(�U�������s��)

		Character.Attribute atkAttr = attackChara._attribute; // �U�����̑���
		Character.Attribute defAttr = defenseChara._attribute; // �h�䑤�̑���

		// �������菈��
		// �������ƂɗǑ������������̏��Ń`�F�b�N���A�ǂ���ɂ����Ă͂܂�Ȃ��Ȃ�ʏ�{����Ԃ�
		switch (atkAttr)
		{
			case Character.Attribute.Water: // �U�����F������
				if (defAttr == Character.Attribute.Fire)
					return RATIO_GOOD;
				else if (defAttr == Character.Attribute.Soil)
					return RATIO_BAD;
				else
					return RATIO_NORMAL;

			case Character.Attribute.Fire: // �U�����F�Α���
				if (defAttr == Character.Attribute.Wind)
					return RATIO_GOOD;
				else if (defAttr == Character.Attribute.Water)
					return RATIO_BAD;
				else
					return RATIO_NORMAL;

			case Character.Attribute.Wind: // �U�����F������
				if (defAttr == Character.Attribute.Soil)
					return RATIO_GOOD;
				else if (defAttr == Character.Attribute.Fire)
					return RATIO_BAD;
				else
					return RATIO_NORMAL;

			case Character.Attribute.Soil: // �U�����F�y����
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
	/// �I�𒆂̃L�����N�^�[�̈ړ����͑҂���Ԃ���������
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
	/// �Q�[���̏I�������𖞂������m�F���A�������Ȃ�Q�[�����I������
	/// </summary>
	public void CheckGameSet()
	{
		bool isWin = true;
		
		bool isLose = true;

		// ���ꂼ�ꐶ���Ă���G�E���������݂��邩���`�F�b�N
		foreach (var charaData in _charactersManager._characters)
		{
			if (charaData._isEnemy) // �G������̂ŏ����t���OOff
				isWin = false;
			else // ����������̂Ŕs�k�t���OOff
				isLose = false;
		}

		// �����܂��͔s�k�̃t���O���������܂܂Ȃ�Q�[�����I������
		// (�ǂ���̃t���O�������Ă��Ȃ��Ȃ牽�������^�[�����i�s����)
		if (isWin || isLose)
		{
			// �Q�[���I���t���O�𗧂Ă�
			_isGameSet = true;

			// ���SUI�ƃt�F�[�h�C����\������(�x�����s)
			DOVirtual.DelayedCall(1.5f, () =>
				{
					if (isWin) // �Q�[���N���A���o
						_guiManager.ShowLogo_GameClear();
					else // �Q�[���I�[�o�[���o
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