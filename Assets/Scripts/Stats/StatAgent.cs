// 목적: 한 챔피언 개체의 스탯 상태 관리 및 피해 기대값 연산.
// 역할:
//  - 보정치(아이템/룬/버프) 등록/만료 관리
//  - 레벨·시간(now) 기준 최종 스탯 스냅샷 제공
//  - 물리/마법 피해의 "기대" 값 계산(치명타 기대 포함 가능)
using System.Collections.Generic;
using UnityEngine;

namespace GIGISTUDIO
{
    public class StatAgent
    {
        public ChampionDefinition def;                 // 대상 챔피언 정의(불변 데이터)

      
        // 내부 보정 저장소
        readonly List<StatModifier> _mods = new List<StatModifier>();
        readonly List<StatModifier> _modsScratch = new List<StatModifier>(); // GC 감소용 임시 버퍼

        // 인스펙터 미러링용 읽기 전용 뷰
        public IReadOnlyList<StatModifier> Mods => _mods;
        public StatAgent(ChampionDefinition def) { this.def = def; }

        /// <summary>
        /// 보정 추가. now는 Time.time 등 외부 클럭을 그대로 전달.
        /// </summary>
        public void AddModifier(StatModifier mod, float now)
        {
            mod.timeApplied = now;
            _mods.Add(mod);
        }

        /// <summary>
        /// 만료된 보정 제거. 프레임 틱마다 호출 가능.
        /// </summary>
        public void RemoveExpired(float now)
        {
            _mods.RemoveAll(m => m.Expired(now));
        }

        /// <summary>
        /// 현재 시각 기준 유효한 보정 목록 반환. 내부 버퍼 재사용.
        /// </summary>
        public List<StatModifier> ActiveMods(float now)
        {
            _modsScratch.Clear();
            for (int i = 0; i < _mods.Count; i++)
            {
                if (!_mods[i].Expired(now))
                    _modsScratch.Add(_mods[i]);
            }
            return _modsScratch;
        }

        /// <summary>
        /// 레벨/시각(now) 기준 최종 스탯을 계산해 스냅샷으로 반환.
        /// 성장치 → 보정치(Flat → %Add → %Mult) → AS 클램프 순서.
        /// </summary>
        public Snapshot SnapshotAtLevel(int level, float now)
        {
            var mods = ActiveMods(now);

            // 성장치 반영 + 보정치 결합
            float HP = StatMath.Combine(def.HP.ValueAtLevel(level), mods, StatType.HP);
            float HPRegen = StatMath.Combine(def.HPRegen.ValueAtLevel(level), mods, StatType.HPRegen);

            float Mana = StatMath.Combine(def.Mana.ValueAtLevel(level), mods, StatType.Mana);
            float ManaRegen = StatMath.Combine(def.ManaRegen.ValueAtLevel(level), mods, StatType.ManaRegen);

            float AD = StatMath.Combine(def.AD.ValueAtLevel(level), mods, StatType.AD);
            float AP = StatMath.Combine(def.AP.ValueAtLevel(level), mods, StatType.AP);

            float Armor = StatMath.Combine(def.Armor.ValueAtLevel(level), mods, StatType.Armor);
            float MR = StatMath.Combine(def.MR.ValueAtLevel(level), mods, StatType.MR);

            float AS = StatMath.AttackSpeedAtLevel(def, level, mods); // 상한/하한 클램프 포함
            float MS = StatMath.Combine(def.MoveSpeed.ValueAtLevel(level), mods, StatType.MoveSpeed);
            float Range = StatMath.Combine(def.Range.ValueAtLevel(level), mods, StatType.Range);

            float critC = Mathf.Clamp01(StatMath.Combine(def.CritChance.ValueAtLevel(level), mods, StatType.CritChance));
            float critM = Mathf.Max(1f, StatMath.Combine(def.CritDamageMult.ValueAtLevel(level), mods, StatType.CritDamageMult));

            return new Snapshot
            {
                level = level,
                HP = HP,
                HPRegen = HPRegen,
                Mana = Mana,
                ManaRegen = ManaRegen,
                AD = AD,
                AP = AP,
                Armor = Armor,
                MR = MR,
                AttackSpeed = AS,
                MoveSpeed = MS,
                Range = Range,
                CritChance = critC,
                CritDamageMult = critM
            };
        }

        /// <summary>
        /// 물리 피해 기대값 계산.
        /// 입력: 원시 물리 피해(rawADDamage), 대상 방어력/감소, 공격자 관통, 치명타 반영 여부.
        /// 출력: 경감/치명 기대를 반영한 "평균적" 피해량.
        /// </summary>
        public float ExpectedPhysicalDamage(
            float rawADDamage, int level, float now,
            float targetArmor, Reduction targetReduction, Penetration attackerPen,
            bool includeCrit = true)
        {
            var snap = SnapshotAtLevel(level, now);

            // 유효 방어력 계산
            float effArmor = StatMath.ApplyResistPipeline(targetArmor, targetReduction, attackerPen);

            // 방어에 따른 경감 배율
            float mit = StatMath.MitigationMultiplier(effArmor);

            // 치명타 기대 배율(옵션)
            float critMult = includeCrit
                ? StatMath.ExpectedCritMultiplier(snap.CritChance, snap.CritDamageMult)
                : 1f;

            return rawADDamage * critMult * mit;
        }

        /// <summary>
        /// 마법 피해 기대값 계산. 치명타 제외(일반 LoL 룰 기반).
        /// 입력: 원시 마법 피해(rawAPDamage), 대상 MR/감소, 공격자 관통.
        /// </summary>
        public float ExpectedMagicDamage(
            float rawAPDamage, int level, float now,
            float targetMR, Reduction targetReduction, Penetration attackerPen)
        {
            float effMR = StatMath.ApplyResistPipeline(targetMR, targetReduction, attackerPen);
            float mit = StatMath.MitigationMultiplier(effMR);
            return rawAPDamage * mit;
        }
    }
}