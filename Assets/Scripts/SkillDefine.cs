using System.Collections;
using System.Collections.Generic;

public static class SkillDefine
{
	// 特技の種類を定義
	public enum Skill
	{
		_None, // スキル無し
		Critical,// 会心の一撃
		DefBreak,// シールド破壊
		Heal, // ヒール
		FireBall, // ファイアボール
	}

	// Dictionaryで特技定義と各データを紐づける
	// 特技名
	public static Dictionary<Skill, string> dic_SkillName = new Dictionary<Skill, string>()
	{
		{Skill._None, "スキル無し"},
		{Skill.Critical, "会心の一撃"},
		{Skill.DefBreak, "シールド破壊"},
		{Skill.Heal, "ヒール"},
		{Skill.FireBall, "ファイアボール"},
	};
	// 表示する説明文
	public static Dictionary<Skill, string> dic_SkillInfo = new Dictionary<Skill, string>()
	{
		{Skill._None, "----"},
		{Skill.Critical, "ダメージ２倍の攻撃\n(１回限り)"},
		{Skill.DefBreak, "敵の防御力を０にします\n(ダメージは０)"},
		{Skill.Heal, "味方のHPを回復します"},
		{Skill.FireBall, "どの位置に居る敵も攻撃できます\n(ダメージは半分)"},
	};

}