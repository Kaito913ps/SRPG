using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{

    // オブジェクト・プレハブ(インスペクタから指定)
    public Transform _blockParent;
    public GameObject _blockPrefab_Grass; // 草ブロック
    public GameObject _blockPrefab_Water; // 水場ブロック

	// マップデータ
	public MapBlock[,] _mapBlocks;


	// 定数定義
	public const int MAP_WIDTH = 9;
    public const int MAP_HEIGHT = 9; 
    private const int GENERATE_RATIO_GRASS = 90;

    void Start()
    {
		// マップデータを初期化
		_mapBlocks = new MapBlock[MAP_WIDTH, MAP_HEIGHT];

		// ブロック生成位置の基点となる座標を設定
		Vector3 defaultPos = new Vector3(0.0f, 0.0f, 0.0f); 
        defaultPos.x = -(MAP_WIDTH / 2); 
        defaultPos.z = -(MAP_HEIGHT / 2);

		// ブロック生成処理
		for (int i = 0; i < MAP_WIDTH; i++)
		{// マップの横幅分繰り返し処理
			for (int j = 0; j < MAP_HEIGHT; j++)
			{
				Vector3 pos = defaultPos; 
				pos.x += i; // 1個目のfor分の繰り返し回数分x座標をずらす
				pos.z += j; // 2個目のfor分の繰り返し回数分z座標をずらす

				// ブロックの種類を決定
				int rand = Random.Range(0, 100); 
				bool isGrass = false; 
				if (rand < GENERATE_RATIO_GRASS)
					isGrass = true;

				// オブジェクトを生成
				GameObject obj; 
				if (isGrass)
				{// 草ブロック生成フラグ：ON
					obj = Instantiate(_blockPrefab_Grass, _blockParent);
				}
				else
				{// 草ブロック生成フラグ：OFF
					obj = Instantiate(_blockPrefab_Water, _blockParent);
				}
				// オブジェクトの座標を適用
				obj.transform.position = pos;

				// 配列mapBlocksにブロックデータを格納
				var mapBlock = obj.GetComponent<MapBlock>();
				_mapBlocks[i, j] = mapBlock;
				// ブロックデータ設定
				mapBlock._xPos = (int)pos.x; 
				mapBlock._zPos = (int)pos.z; 			}
		}
	}

	/// <summary>
	/// 全てのブロックの選択状態を解除する
	/// </summary>
	public void AllSelectionModeClear()
	{
		for (int i = 0; i < MAP_WIDTH; i++)
			for (int j = 0; j < MAP_HEIGHT; j++)
				_mapBlocks[i, j].SetSelectionMode(MapBlock.Highlight.Off);
	}

	/// <summary>
	/// 渡された位置からキャラクターが到達できる場所のブロックをリストにして返す
	/// </summary>
	/// <param name="xPos">基点x位置</param>
	/// <param name="zPos">基点z位置</param>
	/// <returns>条件を満たすブロックのリスト</returns>
	public List<MapBlock> SearchReachableBlocks(int xPos, int zPos)
	{
		// 条件を満たすブロックのリスト
		var results = new List<MapBlock>();

		// 基点となるブロックの配列内番号(index)を検索
		int baseX = -1, baseZ = -1; 
		for (int i = 0; i < MAP_WIDTH; i++)
		{
			for (int j = 0; j < MAP_HEIGHT; j++)
			{
				if ((_mapBlocks[i, j]._xPos == xPos) &&
					(_mapBlocks[i, j]._zPos == zPos))
				{
					baseX = i;
					baseZ = j;
					break;
				}
			}
			if (baseX != -1)
				break;
		}

		// 移動するキャラクターの移動方法を取得
		var moveType = Character.MoveType.Rook; 
		var moveChara = GetComponent<CharactersManager>().GetCharacterDataByPos(xPos, zPos); 
		if (moveChara != null)
			moveType = moveChara._moveType; 

		// キャラクターの移動方法に合わせて異なる方向のブロックデータを取得していく
		// 縦・横
		if (moveType == Character.MoveType.Rook ||
			moveType == Character.MoveType.Queen)
		{
			// X+方向
			for (int i = baseX + 1; i < MAP_WIDTH; i++)
				if (AddReachableList(results, _mapBlocks[i, baseZ]))
					break;
			// X-方向
			for (int i = baseX - 1; i >= 0; i--)
				if (AddReachableList(results, _mapBlocks[i, baseZ]))
					break;
			// Z+方向
			for (int j = baseZ + 1; j < MAP_HEIGHT; j++)
				if (AddReachableList(results, _mapBlocks[baseX, j]))
					break;
			// Z-方向
			for (int j = baseZ - 1; j >= 0; j--)
				if (AddReachableList(results, _mapBlocks[baseX, j]))
					break;
		}
		// 斜めへの移動
		if (moveType == Character.MoveType.Bishop ||
			moveType == Character.MoveType.Queen)
		{
			// X+Z+方向
			for (int i = baseX + 1, j = baseZ + 1;
				i < MAP_WIDTH && j < MAP_HEIGHT;
				i++, j++)
				if (AddReachableList(results, _mapBlocks[i, j]))
					break;
			// X-Z+方向
			for (int i = baseX - 1, j = baseZ + 1;
				i >= 0 && j < MAP_HEIGHT;
				i--, j++)
				if (AddReachableList(results, _mapBlocks[i, j]))
					break;
			// X+Z-方向
			for (int i = baseX + 1, j = baseZ - 1;
				i < MAP_WIDTH && j >= 0;
				i++, j--)
				if (AddReachableList(results, _mapBlocks[i, j]))
					break;
			// X-Z-方向
			for (int i = baseX - 1, j = baseZ - 1;
				i >= 0 && j >= 0;
				i--, j--)
				if (AddReachableList(results, _mapBlocks[i, j]))
					break;
		}
		// 足元のブロック
		results.Add(_mapBlocks[baseX, baseZ]);

		return results;
	}

	/// <summary>
	/// (キャラクター到達ブロック検索処理用)
	/// 指定したブロックを到達可能ブロックリストに追加する
	/// </summary>
	/// <param name="reachableList">到達可能ブロックリスト</param>
	/// <param name="targetBlock">対象ブロック</param>
	/// <returns>行き止まりフラグ(行き止まりならtrueが返る)</returns>
	private bool AddReachableList(List<MapBlock> reachableList, MapBlock targetBlock)
	{
		// 対象のブロックが通行不可ならそこを行き止まりとして終了
		if (!targetBlock._passable)
			return true;

		// 対象の位置に他のキャラが既にいるなら到達不可にして終了(行き止まりにはしない)
		var charaData =	GetComponent<CharactersManager>().GetCharacterDataByPos(targetBlock._xPos, targetBlock._zPos);
		if (charaData != null)
			return false;

		reachableList.Add(targetBlock);
		return false;
	}

	/// <summary>
	/// 渡された位置からキャラクターが攻撃できる場所のマップブロックをリストにして返す
	/// </summary>
	/// <param name="xPos">基点x位置</param>
	/// <param name="zPos">基点z位置</param>
	/// <returns>条件を満たすマップブロックのリスト</returns>
	public List<MapBlock> SearchAttackableBlocks(int xPos, int zPos)
	{
		// 条件を満たすマップブロックのリスト
		var results = new List<MapBlock>();

		// 基点となるブロックの配列内番号(index)を検索
		int baseX = -1, baseZ = -1;
		for (int i = 0; i < MAP_WIDTH; i++)
		{
			for (int j = 0; j < MAP_HEIGHT; j++)
			{
				if ((_mapBlocks[i, j]._xPos == xPos) &&
					(_mapBlocks[i, j]._zPos == zPos))
				{
					baseX = i;
					baseZ = j;
					break; 
				}
			}
			// 既に発見済みなら1個目のループを抜ける
			if (baseX != -1)
				break;
		}

		// 4方向に1マス進んだ位置のブロックをそれぞれセット
		// (縦・横4マス)
		// X+方向
		AddAttackableList(results, baseX + 1, baseZ);
		// X-方向
		AddAttackableList(results, baseX - 1, baseZ);
		// Z+方向
		AddAttackableList(results, baseX, baseZ + 1);
		// Z-方向
		AddAttackableList(results, baseX, baseZ - 1);
		// (斜め4マス)
		// X+Z+方向
		AddAttackableList(results, baseX + 1, baseZ + 1);
		// X-Z+方向
		AddAttackableList(results, baseX - 1, baseZ + 1);
		// X+Z-方向
		AddAttackableList(results, baseX + 1, baseZ - 1);
		// X-Z-方向
		AddAttackableList(results, baseX - 1, baseZ - 1);

		return results;
	}

	/// <summary>
	/// (キャラクター攻撃可能ブロック検索処理用)
	/// マップデータの指定された配列内番号に対応するブロックを攻撃可能ブロックリストに追加する
	/// </summary>
	/// <param name="attackableList">攻撃可能ブロックリスト</param>
	/// <param name="indexX">X方向の配列内番号</param>
	/// <param name="indexZ">Z方向の配列内番号</param>
	private void AddAttackableList(List<MapBlock> attackableList, int indexX, int indexZ)
	{
		// 指定された番号が配列の外に出ていたら追加せず終了
		if (indexX < 0 || indexX >= MAP_WIDTH ||
			indexZ < 0 || indexZ >= MAP_HEIGHT)
			return;

		attackableList.Add(_mapBlocks[indexX, indexZ]);
	}

	/// <summary>
	/// マップデータ配列をリストにして返す
	/// </summary>
	/// <returns>マップデータのリスト</returns>
	public List<MapBlock> MapBlocksToList()
	{
		// 結果用リスト
		var results = new List<MapBlock>();

		for (int i = 0; i < MAP_WIDTH; i++)
		{
			for (int j = 0; j < MAP_HEIGHT; j++)
			{
				results.Add(_mapBlocks[i, j]);
			}
		}

		return results;
	}

}
