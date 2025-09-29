// File: CombatTags.cs
using System;

namespace GIGISTUDIO
{
    /// <summary>
    /// 전투 판정 태그. 하나의 액션(평타/스킬)이 어떤 규칙을 탈지 선언적으로 표기.
    /// </summary>
    [Flags]
    public enum CombatFlags
    {
        None = 0,

        // 기본 공격인지(= 공격 타이머 사용, 치명/온히트 기본 허용)
        IsBasicAttack = 1 << 0,

        // 공격으로 "간주"되는지(온-어택 트리거, 정복자/흡혈 등)
        CountsAsAttack = 1 << 1,

        // 온히트 효과(라이프스틸/온히트 추가피해/아이템 효과) 적용 허용
        AppliesOnHit = 1 << 2,

        // 치명타 판정 허용
        CanCrit = 1 << 3,

        // 시전 종료 시 공격 타이머 리셋(공격-리셋 스킬용)
        ResetsAttackTimer = 1 << 4,

        // 투사체 사용(원거리) vs 근접 접촉
        UsesProjectile = 1 << 5,

        // "다음 기본 공격 강화(대체)" 버퍼를 소비/적용하는 행위인지
        IsEmpoweredNext = 1 << 6,
    }
}
