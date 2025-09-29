using System;
using UnityEngine;
// 목적: 챔피언 고유 기본·성장 스탯 정의.
// 주의: AttackSpeed.perLevel은 "비율/레벨" 입력(예: 0.025 = 2.5%/레벨)
namespace GIGISTUDIO
{
    [Serializable]
    public class ChampionDefinition
    {
        public string championName;

        // 핵심 스탯
        public StatGrowth HP = new StatGrowth(600, 100);
        public StatGrowth HPRegen = new StatGrowth(6, 0.5f);
        public StatGrowth Mana = new StatGrowth(0, 0);         // 기력/분노 챔피언은 0
        public StatGrowth ManaRegen = new StatGrowth(0, 0);

        public StatGrowth AD = new StatGrowth(60, 4.5f);
        public StatGrowth AP = new StatGrowth(0, 0);
        public StatGrowth Armor = new StatGrowth(30, 4.0f);
        public StatGrowth MR = new StatGrowth(32, 2.05f);

        // 공격속도: LoL은 고유 AS와 성장률 구조. 간략화해서 perLevel을 %로 사용.
        public StatGrowth AttackSpeed = new StatGrowth(0.658f, 0.025f); // baseAS, perLevelGrowth%
        public float AttackSpeedMin = 0.1f;
        public float AttackSpeedMax = 2.5f;

        public StatGrowth MoveSpeed = new StatGrowth(340, 0);
        public StatGrowth Range = new StatGrowth(175, 0);

        public StatGrowth CritChance = new StatGrowth(0, 0);
        public StatGrowth CritDamageMult = new StatGrowth(2.0f, 0); // 기본 200%
    }
}