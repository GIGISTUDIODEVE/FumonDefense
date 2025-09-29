// 목적: 특정 레벨 시점의 "최종 스탯 스냅샷" 데이터 컨테이너.
// 사용: UI 표시, 디버그 로그, 네트워크 전송 등.
using System;

namespace GIGISTUDIO
{
    [Serializable]
    public class Snapshot
    {
        public int level;

        // 생존/자원
        public float HP, HPRegen, Mana, ManaRegen;

        // 공격/방어
        public float AD, AP, Armor, MR;

        // 기타
        public float AttackSpeed, MoveSpeed, Range;
        public float CritChance, CritDamageMult;

        public override string ToString()
        {
            // 한국어 UI가 아니더라도 숫자 확인용 요약에 유용.
            return $"Lv {level} | HP {HP:F0}, AD {AD:F1}, Armor {Armor:F1}, MR {MR:F1}, AS {AttackSpeed:F3}, Crit {CritChance:P0}";
        }
    }
}