using System;
using UnityEngine;
/// <summary>
/// 관통 수치. 방어력/마저 공용.
/// percentPen은 0~1 비율, flatPen은 고정 차감 수치.
/// </summary>
namespace GIGISTUDIO
{
    [Serializable]
    public class Penetration
    {
        public float percentPen; // 예: 0.35 = 35% 관통
        public float flatPen;    // 예: 18 = 고정 관통

        public Penetration(float percentPen = 0, float flatPen = 0)
        {
            this.percentPen = Mathf.Clamp01(percentPen);
            this.flatPen = flatPen;
        }
    }
}