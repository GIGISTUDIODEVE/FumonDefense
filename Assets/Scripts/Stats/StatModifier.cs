using System;
using UnityEngine;

// ����: ������/��/���� �� �ܺ� ���ο� ���� ���� ���� ȿ���� ǥ��.
// ���ӽð�(duration) 0�� ���� ������ �ǹ�.

namespace GIGISTUDIO
{
    [Serializable]
    public class StatModifier
    {
        public string source;     // ��ó(�����۸�/������ ��, �����/������)
        public StatType stat;     // ��� ����
        public ModOp op;          // ���� ��� (Flat / PercentAdd / PercentMult)
        public float value;       // ������ (���� ��Ŀ� ���� �ؼ�)
        public float duration;    // ���ӽð�(��). 0 ���� -> ����
        public float timeApplied;     // ���ο�

        public bool Expired(float now)
        {
            return duration > 0 && now >= timeApplied + duration;
        }

        /// <summary>
        /// ����(Flat) ������ �����Ѵ�.
        /// ��) AD +10, HP +200.
        /// </summary>
        /// <param name="src">���� ��ó(������/����/��ų �̸�). ����� �� ���� ǥ�⿡ ���.</param>
        /// <param name="s">��� ����(��: <see cref="StatType.AttackDamage"/>).</param>
        /// <param name="v">���� ����/���ҷ�. ������ ���Ȱ� ����(��: AD�� '���ݷ�' ����).</param>
        /// <param name="duration">
        /// ���ӽð�(��). 0 �����̸� ������ ����.
        /// ���� ���� ������ StatAgent�� now(�ð�)�� �Բ� �����Ѵ�.
        /// </param>
        /// <returns>Flat ������ ������ <see cref="StatModifier"/>.</returns>
        public static StatModifier Flat(string src, StatType s, float v, float duration = 0) =>
            new StatModifier { source = src, stat = s, op = ModOp.Flat, value = v, duration = duration };

        /// <summary>
        /// ����(PercentAdd) ������ �����Ѵ�. ���� Ÿ�Գ����� ���ջꡱ �� �� ���� ����ȴ�.
        /// ��) +15% ���ݷ�, -30% �̵��ӵ�(���ο�).
        /// </summary>
        /// <param name="src">���� ��ó(������/����/��ų �̸�). ����� �� ���� ǥ�⿡ ���.</param>
        /// <param name="s">��� ����.</param>
        /// <param name="v">
        /// ���� ����. 0.15 = +15%, -0.3 = -30%.
        /// ���� ���� �Բ� �����ϸ� ���� ��������(��: +10%�� +20% �� �� +30%).
        /// </param>
        /// <param name="duration">
        /// ���ӽð�(��). 0 �����̸� ����.
        /// ���� �ð� ���/����� StatAgent�� ó���Ѵ�.
        /// </param>
        /// <returns>PercentAdd ������ ������ <see cref="StatModifier"/>.</returns>
        public static StatModifier PctAdd(string src, StatType s, float v, float duration = 0) =>
            new StatModifier { source = src, stat = s, op = ModOp.PercentAdd, value = v, duration = duration };

        /// <summary>
        /// �¼�(PercentMult) ������ �����Ѵ�. ���� Ÿ�Գ����� ������ �������� ���յȴ�.
        /// ��) ���� ���� ��(1+0.2) ��(1+0.1) = ��1.32.
        /// </summary>
        /// <param name="src">���� ��ó(������/����/��ų �̸�). ����� �� ���� ǥ�⿡ ���.</param>
        /// <param name="s">��� ����.</param>
        /// <param name="v">
        /// �¼� ����. 0.1 = ��1.1(= +10%), -0.2 = ��0.8(= -20%).
        /// PercentAdd�� �޸� ���� ���� �Բ� �����ϸ� ��������.
        /// </param>
        /// <param name="duration">
        /// ���ӽð�(��). 0 �����̸� ����.
        /// ���� ����/������ StatAgent�� now(�ð�) �������� �Ǵ��Ѵ�.
        /// </param>
        /// <returns>PercentMult ������ ������ <see cref="StatModifier"/>.</returns>
        public static StatModifier PctMult(string src, StatType s, float v, float duration = 0) =>
            new StatModifier { source = src, stat = s, op = ModOp.PercentMult, value = v, duration = duration };
    }
}