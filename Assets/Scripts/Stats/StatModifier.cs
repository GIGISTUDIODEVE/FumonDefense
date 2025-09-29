using System;
using UnityEngine;

// 목적: 아이템/룬/버프 등 외부 요인에 의한 스탯 보정 효과를 표현.
// 지속시간(duration) 0은 무한 지속을 의미.

namespace GIGISTUDIO
{
    [Serializable]
    public class StatModifier
    {
        public string source;     // 출처(아이템명/버프명 등, 디버깅/툴팁용)
        public StatType stat;     // 대상 스탯
        public ModOp op;          // 연산 방식 (Flat / PercentAdd / PercentMult)
        public float value;       // 보정값 (연산 방식에 따라 해석)
        public float duration;    // 지속시간(초). 0 이하 -> 영구
        public float timeApplied;     // 내부용

        public bool Expired(float now)
        {
            return duration > 0 && now >= timeApplied + duration;
        }

        /// <summary>
        /// 고정(Flat) 보정을 생성한다.
        /// 예) AD +10, HP +200.
        /// </summary>
        /// <param name="src">보정 출처(아이템/버프/스킬 이름). 디버깅 및 툴팁 표기에 사용.</param>
        /// <param name="s">대상 스탯(예: <see cref="StatType.AttackDamage"/>).</param>
        /// <param name="v">고정 증가/감소량. 단위는 스탯과 동일(예: AD면 '공격력' 단위).</param>
        /// <param name="duration">
        /// 지속시간(초). 0 이하이면 영구로 간주.
        /// 실제 만료 판정은 StatAgent가 now(시간)와 함께 관리한다.
        /// </param>
        /// <returns>Flat 보정이 설정된 <see cref="StatModifier"/>.</returns>
        public static StatModifier Flat(string src, StatType s, float v, float duration = 0) =>
            new StatModifier { source = src, stat = s, op = ModOp.Flat, value = v, duration = duration };

        /// <summary>
        /// 가산(PercentAdd) 보정을 생성한다. 동일 타입끼리는 “합산” 후 한 번에 적용된다.
        /// 예) +15% 공격력, -30% 이동속도(슬로우).
        /// </summary>
        /// <param name="src">보정 출처(아이템/버프/스킬 이름). 디버깅 및 툴팁 표기에 사용.</param>
        /// <param name="s">대상 스탯.</param>
        /// <param name="v">
        /// 가산 비율. 0.15 = +15%, -0.3 = -30%.
        /// 여러 개가 함께 존재하면 서로 더해진다(예: +10%와 +20% → 총 +30%).
        /// </param>
        /// <param name="duration">
        /// 지속시간(초). 0 이하이면 영구.
        /// 실제 시간 경과/만료는 StatAgent가 처리한다.
        /// </param>
        /// <returns>PercentAdd 보정이 설정된 <see cref="StatModifier"/>.</returns>
        public static StatModifier PctAdd(string src, StatType s, float v, float duration = 0) =>
            new StatModifier { source = src, stat = s, op = ModOp.PercentAdd, value = v, duration = duration };

        /// <summary>
        /// 승수(PercentMult) 보정을 생성한다. 동일 타입끼리는 “서로 곱”으로 결합된다.
        /// 예) 최종 피해 ×(1+0.2) ×(1+0.1) = ×1.32.
        /// </summary>
        /// <param name="src">보정 출처(아이템/버프/스킬 이름). 디버깅 및 툴팁 표기에 사용.</param>
        /// <param name="s">대상 스탯.</param>
        /// <param name="v">
        /// 승수 비율. 0.1 = ×1.1(= +10%), -0.2 = ×0.8(= -20%).
        /// PercentAdd와 달리 여러 개가 함께 존재하면 곱해진다.
        /// </param>
        /// <param name="duration">
        /// 지속시간(초). 0 이하이면 영구.
        /// 실제 만료/갱신은 StatAgent가 now(시간) 기준으로 판단한다.
        /// </param>
        /// <returns>PercentMult 보정이 설정된 <see cref="StatModifier"/>.</returns>
        public static StatModifier PctMult(string src, StatType s, float v, float duration = 0) =>
            new StatModifier { source = src, stat = s, op = ModOp.PercentMult, value = v, duration = duration };
    }
}