// ����: Ư�� ���� ������ "���� ���� ������" ������ �����̳�.
// ���: UI ǥ��, ����� �α�, ��Ʈ��ũ ���� ��.
using System;

namespace GIGISTUDIO
{
    [Serializable]
    public class Snapshot
    {
        public int level;

        // ����/�ڿ�
        public float HP, HPRegen, Mana, ManaRegen;

        // ����/���
        public float AD, AP, Armor, MR;

        // ��Ÿ
        public float AttackSpeed, MoveSpeed, Range;
        public float CritChance, CritDamageMult;

        public override string ToString()
        {
            // �ѱ��� UI�� �ƴϴ��� ���� Ȯ�ο� ��࿡ ����.
            return $"Lv {level} | HP {HP:F0}, AD {AD:F1}, Armor {Armor:F1}, MR {MR:F1}, AS {AttackSpeed:F3}, Crit {CritChance:P0}";
        }
    }
}