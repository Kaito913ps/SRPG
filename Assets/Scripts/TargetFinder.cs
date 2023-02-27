using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Staticでクラスを定義する
public static class TargetFinder
{
	// 行動プランクラス
	// (行動する敵キャラクター、移動先の位置、攻撃相手のキャラクターの３データを１まとめに扱う)
	public class ActionPlan
	{
		public Character _charaData; 
		public MapBlock _toMoveBlock; 
		public Character _toAttackChara; 
	}

	/// <summary>
	/// 攻撃可能な行動プランを全て検索し、その内の１つをランダムに返す処理
	/// </summary>
	/// <param name="mapManager">シーン内のMapManagerの参照</param>
	/// <param name="charactersManager">シーン内のCharactersManagerの参照</param>
	/// <param name="enemyCharas">敵キャラクターのリスト</param>
	/// <returns></returns>
	public static ActionPlan GetRandomActionPlan(MapManager mapManager, CharactersManager charactersManager, List<Character> enemyCharas)
	{
		// 全行動プラン(攻撃可能な相手が見つかる度に追加される)
		var actionPlans = new List<ActionPlan>();
		// 移動範囲リスト
		var reachableBlocks = new List<MapBlock>();
		// 攻撃範囲リスト
		var attackableBlocks = new List<MapBlock>();

		// 全行動プラン検索処理
		foreach (Character enemyData in enemyCharas)
		{
			// 移動可能な場所リストを取得する
			reachableBlocks = mapManager.SearchReachableBlocks(enemyData._xPos, enemyData._zPos);
			// それぞれの移動可能な場所ごとの処理
			foreach (MapBlock block in reachableBlocks)
			{
				// 攻撃可能な場所リストを取得する
				attackableBlocks = mapManager.SearchAttackableBlocks(block._xPos, block._zPos);
				// それぞれの攻撃可能な場所ごとの処理
				foreach (MapBlock attackBlock in attackableBlocks)
				{
					// 攻撃できる相手キャラクター(プレイヤー側のキャラクター)を探す
					Character targetChara =
						charactersManager.GetCharacterDataByPos(attackBlock._xPos, attackBlock._zPos);
					if (targetChara != null &&
						!targetChara._isEnemy)
					{
						var newPlan = new ActionPlan();
						newPlan._charaData = enemyData;
						newPlan._toMoveBlock = block;
						newPlan._toAttackChara = targetChara;

						// 全行動プランリストに追加
						actionPlans.Add(newPlan);
					}
				}
			}
		}

		
		if (actionPlans.Count > 0)
			return actionPlans[Random.Range(0, actionPlans.Count)];
		else
			return null;
	}
}