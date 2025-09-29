using System;
using UnityEngine;

namespace GIGISTUDIO
{
    /// <summary>
    /// ��󿡰� �ο��Ǵ� ���� ���� ȿ��.
    /// percentReduce�� 0~1 ����, flatReduce�� ���� ����.
    /// </summary>
    [Serializable]
    public class Reduction
    {
        public float percentReduce; // ��: 0.24 = 24% ����
        public float flatReduce;    // ��: 12 = 12 ���� ����

        public Reduction(float percentReduce = 0, float flatReduce = 0)
        {
            this.percentReduce = Mathf.Clamp01(percentReduce);
            this.flatReduce = flatReduce;
        }
    }
}