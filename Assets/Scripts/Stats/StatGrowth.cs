using System;
using UnityEngine;
namespace GIGISTUDIO
{
    [Serializable]
    public struct StatGrowth
    {
        public float baseValue;   // 챔피언 고유 기본값
        public float perLevel;    // 레벨당 증가치

        public StatGrowth(float baseValue, float perLevel)
        {
            this.baseValue = baseValue;
            this.perLevel = perLevel;
        }

        // LoL 성장 보정: base + growth*(lvl-1)*(0.7025 + 0.0175*(lvl-1))
        public float ValueAtLevel(int level)
        {
            level = Mathf.Clamp(level, 1, 18);
            float n = level - 1;
            float curve = 0.7025f + 0.0175f * n;
            return baseValue + perLevel * n * curve;
        }
    }
}