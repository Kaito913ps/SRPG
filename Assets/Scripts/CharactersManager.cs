using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class CharactersManager : MonoBehaviour
{
	
	public Transform _charactersParent;

	// �S�L�����N�^�[�f�[�^
	[HideInInspector]
	public List<Character> _characters;

	void Start()
	{
		// �}�b�v��̑S�L�����N�^�[�f�[�^���擾
		// (charactersParent�ȉ��̑SCharacter�R���|�[�l���g�����������X�g�Ɋi�[)
		_characters = new List<Character>();
		_charactersParent.GetComponentsInChildren(_characters);

		// �f�[�^�}�l�[�W������f�[�^�Ǘ��N���X���擾
		Data data = GameObject.Find("DataManager").GetComponent<Data>();

		// �X�e�[�^�X�㏸�ʃf�[�^��K�p����
		foreach (Character charaData in _characters)
		{
			// �G�L�����N�^�[�̏ꍇ�͋������Ȃ�
			if (charaData._isEnemy)
				continue;

			// �L�����N�^�[�̔\�͂��㏸������
			charaData._nowHP += data._addHP; // �ő�HP
			charaData._maxHP += data._addHP; // ����HP
			charaData._atk += data._addAtk; // �U����
			charaData._def += data._addDef; // �h���
		}
	}

	/// <summary>
	/// �w�肵���ʒu�ɑ��݂���L�����N�^�[�f�[�^���������ĕԂ�
	/// </summary>
	/// <param name="xPos">X�ʒu</param>
	/// <param name="zPos">Z�ʒu</param>
	/// <returns>�Ώۂ̃L�����N�^�[�f�[�^</returns>
	public Character GetCharacterDataByPos(int xPos, int zPos)
	{
		// ��������
		// (foreach�Ń}�b�v���̑S�L�����N�^�[�f�[�^�P�̂P�̂��ɓ����������s��)
		foreach (Character charaData in _characters)
		{
			// �L�����N�^�[�̈ʒu���w�肳�ꂽ�ʒu�ƈ�v���Ă��邩�`�F�b�N
			if ((charaData._xPos == xPos) &&(charaData._zPos == zPos)) 	
			{
				return charaData; 
			}
		}

		// �f�[�^��������Ȃ����null��Ԃ�
		return null;
	}

	/// <summary>
	/// �w�肵���L�����N�^�[���폜����
	/// </summary>
	/// <param name="charaData">�ΏۃL�����f�[�^</param>
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