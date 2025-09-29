using System;
using UnityEngine;
/// <summary>
/// ���� ��ġ. ����/���� ����.
/// percentPen�� 0~1 ����, flatPen�� ���� ���� ��ġ.
/// </summary>
namespace GIGISTUDIO
{
    [Serializable]
    public class Penetration
    {
        public float percentPen; // ��: 0.35 = 35% ����
        public float flatPen;    // ��: 18 = ���� ����

        public Penetration(float percentPen = 0, float flatPen = 0)
        {
            this.percentPen = Mathf.Clamp01(percentPen);
            this.flatPen = flatPen;
        }
    }
}