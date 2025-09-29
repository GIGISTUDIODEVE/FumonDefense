using System;
using UnityEngine;

namespace GIGISTUDIO
{
    /// <summary>
    /// 대상에게 부여되는 저항 감소 효과.
    /// percentReduce는 0~1 비율, flatReduce는 고정 차감.
    /// </summary>
    [Serializable]
    public class Reduction
    {
        public float percentReduce; // 예: 0.24 = 24% 감소
        public float flatReduce;    // 예: 12 = 12 고정 감소

        public Reduction(float percentReduce = 0, float flatReduce = 0)
        {
            this.percentReduce = Mathf.Clamp01(percentReduce);
            this.flatReduce = flatReduce;
        }
    }
}