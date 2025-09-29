// File: CombatTags.cs
using System;

namespace GIGISTUDIO
{
    /// <summary>
    /// ���� ���� �±�. �ϳ��� �׼�(��Ÿ/��ų)�� � ��Ģ�� Ż�� ���������� ǥ��.
    /// </summary>
    [Flags]
    public enum CombatFlags
    {
        None = 0,

        // �⺻ ��������(= ���� Ÿ�̸� ���, ġ��/����Ʈ �⺻ ���)
        IsBasicAttack = 1 << 0,

        // �������� "����"�Ǵ���(��-���� Ʈ����, ������/���� ��)
        CountsAsAttack = 1 << 1,

        // ����Ʈ ȿ��(��������ƿ/����Ʈ �߰�����/������ ȿ��) ���� ���
        AppliesOnHit = 1 << 2,

        // ġ��Ÿ ���� ���
        CanCrit = 1 << 3,

        // ���� ���� �� ���� Ÿ�̸� ����(����-���� ��ų��)
        ResetsAttackTimer = 1 << 4,

        // ����ü ���(���Ÿ�) vs ���� ����
        UsesProjectile = 1 << 5,

        // "���� �⺻ ���� ��ȭ(��ü)" ���۸� �Һ�/�����ϴ� ��������
        IsEmpoweredNext = 1 << 6,
    }
}
