using System;
using UnityEngine;
// ����: è�Ǿ� ���� �⺻������ ���� ����.
// ����: AttackSpeed.perLevel�� "����/����" �Է�(��: 0.025 = 2.5%/����)
namespace GIGISTUDIO
{
    [Serializable]
    public class ChampionDefinition
    {
        public string championName;

        // �ٽ� ����
        public StatGrowth HP = new StatGrowth(600, 100);
        public StatGrowth HPRegen = new StatGrowth(6, 0.5f);
        public StatGrowth Mana = new StatGrowth(0, 0);         // ���/�г� è�Ǿ��� 0
        public StatGrowth ManaRegen = new StatGrowth(0, 0);

        public StatGrowth AD = new StatGrowth(60, 4.5f);
        public StatGrowth AP = new StatGrowth(0, 0);
        public StatGrowth Armor = new StatGrowth(30, 4.0f);
        public StatGrowth MR = new StatGrowth(32, 2.05f);

        // ���ݼӵ�: LoL�� ���� AS�� ����� ����. ����ȭ�ؼ� perLevel�� %�� ���.
        public StatGrowth AttackSpeed = new StatGrowth(0.658f, 0.025f); // baseAS, perLevelGrowth%
        public float AttackSpeedMin = 0.1f;
        public float AttackSpeedMax = 2.5f;

        public StatGrowth MoveSpeed = new StatGrowth(340, 0);
        public StatGrowth Range = new StatGrowth(175, 0);

        public StatGrowth CritChance = new StatGrowth(0, 0);
        public StatGrowth CritDamageMult = new StatGrowth(2.0f, 0); // �⺻ 200%
    }
}