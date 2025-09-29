// ����: �� è�Ǿ� ��ü�� ���� ���� ���� �� ���� ��밪 ����.
// ����:
//  - ����ġ(������/��/����) ���/���� ����
//  - �������ð�(now) ���� ���� ���� ������ ����
//  - ����/���� ������ "���" �� ���(ġ��Ÿ ��� ���� ����)
using System.Collections.Generic;
using UnityEngine;

namespace GIGISTUDIO
{
    public class StatAgent
    {
        public ChampionDefinition def;                 // ��� è�Ǿ� ����(�Һ� ������)

      
        // ���� ���� �����
        readonly List<StatModifier> _mods = new List<StatModifier>();
        readonly List<StatModifier> _modsScratch = new List<StatModifier>(); // GC ���ҿ� �ӽ� ����

        // �ν����� �̷����� �б� ���� ��
        public IReadOnlyList<StatModifier> Mods => _mods;
        public StatAgent(ChampionDefinition def) { this.def = def; }

        /// <summary>
        /// ���� �߰�. now�� Time.time �� �ܺ� Ŭ���� �״�� ����.
        /// </summary>
        public void AddModifier(StatModifier mod, float now)
        {
            mod.timeApplied = now;
            _mods.Add(mod);
        }

        /// <summary>
        /// ����� ���� ����. ������ ƽ���� ȣ�� ����.
        /// </summary>
        public void RemoveExpired(float now)
        {
            _mods.RemoveAll(m => m.Expired(now));
        }

        /// <summary>
        /// ���� �ð� ���� ��ȿ�� ���� ��� ��ȯ. ���� ���� ����.
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
        /// ����/�ð�(now) ���� ���� ������ ����� ���������� ��ȯ.
        /// ����ġ �� ����ġ(Flat �� %Add �� %Mult) �� AS Ŭ���� ����.
        /// </summary>
        public Snapshot SnapshotAtLevel(int level, float now)
        {
            var mods = ActiveMods(now);

            // ����ġ �ݿ� + ����ġ ����
            float HP = StatMath.Combine(def.HP.ValueAtLevel(level), mods, StatType.HP);
            float HPRegen = StatMath.Combine(def.HPRegen.ValueAtLevel(level), mods, StatType.HPRegen);

            float Mana = StatMath.Combine(def.Mana.ValueAtLevel(level), mods, StatType.Mana);
            float ManaRegen = StatMath.Combine(def.ManaRegen.ValueAtLevel(level), mods, StatType.ManaRegen);

            float AD = StatMath.Combine(def.AD.ValueAtLevel(level), mods, StatType.AD);
            float AP = StatMath.Combine(def.AP.ValueAtLevel(level), mods, StatType.AP);

            float Armor = StatMath.Combine(def.Armor.ValueAtLevel(level), mods, StatType.Armor);
            float MR = StatMath.Combine(def.MR.ValueAtLevel(level), mods, StatType.MR);

            float AS = StatMath.AttackSpeedAtLevel(def, level, mods); // ����/���� Ŭ���� ����
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
        /// ���� ���� ��밪 ���.
        /// �Է�: ���� ���� ����(rawADDamage), ��� ����/����, ������ ����, ġ��Ÿ �ݿ� ����.
        /// ���: �氨/ġ�� ��븦 �ݿ��� "�����" ���ط�.
        /// </summary>
        public float ExpectedPhysicalDamage(
            float rawADDamage, int level, float now,
            float targetArmor, Reduction targetReduction, Penetration attackerPen,
            bool includeCrit = true)
        {
            var snap = SnapshotAtLevel(level, now);

            // ��ȿ ���� ���
            float effArmor = StatMath.ApplyResistPipeline(targetArmor, targetReduction, attackerPen);

            // �� ���� �氨 ����
            float mit = StatMath.MitigationMultiplier(effArmor);

            // ġ��Ÿ ��� ����(�ɼ�)
            float critMult = includeCrit
                ? StatMath.ExpectedCritMultiplier(snap.CritChance, snap.CritDamageMult)
                : 1f;

            return rawADDamage * critMult * mit;
        }

        /// <summary>
        /// ���� ���� ��밪 ���. ġ��Ÿ ����(�Ϲ� LoL �� ���).
        /// �Է�: ���� ���� ����(rawAPDamage), ��� MR/����, ������ ����.
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