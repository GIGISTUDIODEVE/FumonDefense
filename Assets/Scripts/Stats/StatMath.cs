// 목적: 스탯 합성/저항/치명타 등 핵심 수학 연산 모듈.
// 핵심 규칙 요약:
// 1) 스탯 합성 순서: Flat → PercentAdd(합산) → PercentMult(곱연산)
// 2) 저항 파이프라인: 대상%감소 → 대상고정감소 → 공격자%관통 → 공격자고정관통
// 3) 피해 경감: 양의 저항 100/(100+R), 음의 저항 2 - 100/(100-R)
// 4) 기대 치명 배율: 1 + CritChance*(CritMult-1)
using System.Collections.Generic;
using UnityEngine;

namespace GIGISTUDIO
{
    public static class StatMath
    {
        /// <summary>
        /// 스탯 합성. 같은 StatType에 한해 각 ModOp를 규칙에 따라 결합.
        /// PercentAdd는 모두 더해 1회 적용. PercentMult는 요소별 곱.
        /// </summary>
        public static float Combine(float baseValue, List<StatModifier> mods, StatType type)
        {
            float flat = 0f;
            float pctAdd = 0f;
            float pctMult = 1f;

            for (int i = 0; i < mods.Count; i++)
            {
                var m = mods[i];
                if (m.stat != type) continue;
                switch (m.op)
                {
                    case ModOp.Flat: flat += m.value; break;
                    case ModOp.PercentAdd: pctAdd += m.value; break;
                    case ModOp.PercentMult: pctMult *= (1f + m.value); break;
                }
            }

            return (baseValue + flat) * (1f + pctAdd) * pctMult;
        }

        /// <summary>
        /// 레벨 기반 공격속도 계산.
        /// baseAS * (1 + growthPctPerLevel * (level-1)) → 이후 보정(mods) → 최종 클램프.
        /// </summary>
        public static float AttackSpeedAtLevel(ChampionDefinition def, int level, List<StatModifier> mods)
        {
            level = Mathf.Clamp(level, 1, 18);
            float baseAS = def.AttackSpeed.baseValue;
            float growthPctPerLevel = def.AttackSpeed.perLevel; // 예: 0.025 = 2.5%/레벨
            float asFromLevel = baseAS * (1f + growthPctPerLevel * (level - 1));
            float value = Combine(asFromLevel, mods, StatType.AttackSpeed);
            return Mathf.Clamp(value, def.AttackSpeedMin, def.AttackSpeedMax);
        }

        /// <summary>
        /// 저항 파이프라인 적용.
        /// 입력: 원본 저항(rawResist), 대상 감소, 공격자 관통.
        /// 출력: 관통까지 반영된 "유효 저항".
        /// </summary>
        public static float ApplyResistPipeline(float rawResist, Reduction targetReduce, Penetration attackerPen)
        {
            float after = rawResist;

            // 1) 대상 %감소
            after *= (1f - Mathf.Clamp01(targetReduce.percentReduce));

            // 2) 대상 고정감소
            after -= targetReduce.flatReduce;

            // 3) 공격자 %관통
            after *= (1f - Mathf.Clamp01(attackerPen.percentPen));

            // 4) 공격자 고정관통
            after -= attackerPen.flatPen;

            return after;
        }

        /// <summary>
        /// 유효 저항으로부터 피해 경감 배율 계산.
        /// R >= 0: 100/(100+R)
        /// R < 0 : 2 - 100/(100-R)  → 음수 저항일 때 피해 증가 처리
        /// </summary>
        public static float MitigationMultiplier(float effectiveResist)
        {
            if (effectiveResist >= 0f) return 100f / (100f + effectiveResist);
            return 2f - (100f / (100f - effectiveResist));
        }

        /// <summary>
        /// 기대 치명타 배율. 독립 시도 가정.
        /// 1 + C*(M-1). C는 0~1 범위로 클램프.
        /// </summary>
        public static float ExpectedCritMultiplier(float critChance, float critMult)
        {
            critChance = Mathf.Clamp01(critChance);
            return 1f + critChance * (critMult - 1f);
        }
    }
}