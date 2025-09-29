// ����: ���� �ռ�/����/ġ��Ÿ �� �ٽ� ���� ���� ���.
// �ٽ� ��Ģ ���:
// 1) ���� �ռ� ����: Flat �� PercentAdd(�ջ�) �� PercentMult(������)
// 2) ���� ����������: ���%���� �� ���������� �� ������%���� �� �����ڰ�������
// 3) ���� �氨: ���� ���� 100/(100+R), ���� ���� 2 - 100/(100-R)
// 4) ��� ġ�� ����: 1 + CritChance*(CritMult-1)
using System.Collections.Generic;
using UnityEngine;

namespace GIGISTUDIO
{
    public static class StatMath
    {
        /// <summary>
        /// ���� �ռ�. ���� StatType�� ���� �� ModOp�� ��Ģ�� ���� ����.
        /// PercentAdd�� ��� ���� 1ȸ ����. PercentMult�� ��Һ� ��.
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
        /// ���� ��� ���ݼӵ� ���.
        /// baseAS * (1 + growthPctPerLevel * (level-1)) �� ���� ����(mods) �� ���� Ŭ����.
        /// </summary>
        public static float AttackSpeedAtLevel(ChampionDefinition def, int level, List<StatModifier> mods)
        {
            level = Mathf.Clamp(level, 1, 18);
            float baseAS = def.AttackSpeed.baseValue;
            float growthPctPerLevel = def.AttackSpeed.perLevel; // ��: 0.025 = 2.5%/����
            float asFromLevel = baseAS * (1f + growthPctPerLevel * (level - 1));
            float value = Combine(asFromLevel, mods, StatType.AttackSpeed);
            return Mathf.Clamp(value, def.AttackSpeedMin, def.AttackSpeedMax);
        }

        /// <summary>
        /// ���� ���������� ����.
        /// �Է�: ���� ����(rawResist), ��� ����, ������ ����.
        /// ���: ������� �ݿ��� "��ȿ ����".
        /// </summary>
        public static float ApplyResistPipeline(float rawResist, Reduction targetReduce, Penetration attackerPen)
        {
            float after = rawResist;

            // 1) ��� %����
            after *= (1f - Mathf.Clamp01(targetReduce.percentReduce));

            // 2) ��� ��������
            after -= targetReduce.flatReduce;

            // 3) ������ %����
            after *= (1f - Mathf.Clamp01(attackerPen.percentPen));

            // 4) ������ ��������
            after -= attackerPen.flatPen;

            return after;
        }

        /// <summary>
        /// ��ȿ �������κ��� ���� �氨 ���� ���.
        /// R >= 0: 100/(100+R)
        /// R < 0 : 2 - 100/(100-R)  �� ���� ������ �� ���� ���� ó��
        /// </summary>
        public static float MitigationMultiplier(float effectiveResist)
        {
            if (effectiveResist >= 0f) return 100f / (100f + effectiveResist);
            return 2f - (100f / (100f - effectiveResist));
        }

        /// <summary>
        /// ��� ġ��Ÿ ����. ���� �õ� ����.
        /// 1 + C*(M-1). C�� 0~1 ������ Ŭ����.
        /// </summary>
        public static float ExpectedCritMultiplier(float critChance, float critMult)
        {
            critChance = Mathf.Clamp01(critChance);
            return 1f + critChance * (critMult - 1f);
        }
    }
}