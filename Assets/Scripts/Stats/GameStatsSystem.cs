// 목적: 에디터/런타임에서 시스템 동작 확인을 위한 데모 컴포넌트.
// 사용법:
//  1) 빈 GameObject에 본 스크립트 부착
//  2) 재생 → 콘솔에서 스냅샷/피해 출력 확인
using UnityEngine;

namespace GIGISTUDIO
{
    public class GameStatsSystem : MonoBehaviour
    {
        [Header("Demo Champion")]
        public ChampionDefinition garen = new ChampionDefinition { championName = "Garen" };

        public StatAgent Agent { get; private set; }
        void Start()
        {
            // now는 Time.time 사용. 프로젝트에선 고정 틱 타이밍으로 대체 가능.
            var now = Time.time;


            // 기존 로컬 변수 유지하되, 공개 프로퍼티에 담아둔다
            var agent = new StatAgent(garen);
            Agent = agent;

            // 아이템/룬/버프 예시 ---------------------------------------------
            // Flat: 고정치 증가, PctAdd: % 가산, PctMult: 승수 곱
            agent.AddModifier(StatModifier.Flat("롱소드", StatType.AD, 1000f), now);
            agent.AddModifier(StatModifier.Flat("천갑옷", StatType.Armor, 1000f), now);
            agent.AddModifier(StatModifier.PctAdd("바미불씨", StatType.HP, 0.08f, 10f), now); // 10초 지속
            agent.AddModifier(StatModifier.PctAdd("공속 룬", StatType.AttackSpeed, 0.18f), now);
            agent.AddModifier(StatModifier.Flat("치확", StatType.CritChance, 0.25f,5f), now);
            agent.AddModifier(StatModifier.Flat("치피", StatType.CritDamageMult, 0.25f), now); // 2.25배

            // 레벨 지정 후 스냅샷 확인
            int level = 11;
            var snap = agent.SnapshotAtLevel(level, now);
            Debug.Log($"[스냅샷] {snap}");


            float ad = snap.AD;                 // _mods 반영된 최종 AD
            float scale = 1.0f;                 // 스킬/평타 계수(예: 1.0f=100%)
            float rawADDamage = ad * scale;     // ✨ 여기서 상수 대신 AD 기반 계산

            // 관통/감소 파이프라인 예시 ---------------------------------------
            // 대상 감소(예: 블클 중첩 24%)
            var targetReduction = new Reduction(percentReduce: 0.24f, flatReduce: 0);

            // 레탈리티는 레벨 스케일 적용 후 flatPen으로 반영.
            float lethality = 18f;
            float flatPenScaled = lethality * (0.6f + 0.4f * level / 18f); // 표준 스케일
            var attackerPen = new Penetration(percentPen: 0.35f, flatPen: flatPenScaled);

            // 물리 피해 기대값(치명 포함)
            float expectedHit = agent.ExpectedPhysicalDamage(
                rawADDamage,
                level: level,
                now: now,
                targetArmor: 120f,
                targetReduction: targetReduction,
                attackerPen: attackerPen,
                includeCrit: true
            );
            Debug.Log($"기대 물리 피해: {expectedHit:F1}");

            // 마법 피해 기대값(치명 제외)
            float expectedMagic = agent.ExpectedMagicDamage(
                rawAPDamage: 300f,
                level: level,
                now: now,
                targetMR: 80f,
                targetReduction: new Reduction(0.0f, 0f),
                attackerPen: new Penetration(percentPen: 0.40f, flatPen: 0f)
            );
            Debug.Log($"기대 마법 피해: {expectedMagic:F1}");

            // 만료 처리 예시: 주기적으로 RemoveExpired(Time.time) 호출 권장.
        }
    }
}
    