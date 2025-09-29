// File: Unit.cs
using System;
using UnityEngine;

namespace GIGISTUDIO
{
    /// <summary>
    /// 유닛(챔피언/몬스터 등)의 최소 구현:
    /// - StatAgent(비 Mono)를 내부에서 생성/보관
    /// - 레벨/스냅샷/버프만료/체력/사망 이벤트 관리
    /// - 물리/마법 피해 적용 유틸 제공 (StatMath 파이프라인 일관 유지)
    /// - LateUpdateManager를 통해 주기 갱신(버프 만료 등)
    /// 
    /// 주의:
    /// - BasicAttackController.cs가 GetComponent&lt;StatAgent&gt;를 시도하고 있는데,
    ///   StatAgent는 Mono가 아니므로 컴파일 에러가 날 수 있음.
    ///   해결책: BasicAttackController에서 agent 필드를 인스펙터로 비워두고,
    ///   본 컴포넌트(Unit)가 런타임에 주입(setter)하도록 하거나
    ///   BasicAttackController의 해당 라인을 안전하게 수정 필요.
    /// </summary>
    [DisallowMultipleComponent]
    public class Unit : MonoBehaviour
    {
        public enum Team { Neutral, TeamA, TeamB }

        [Header("Definition & Level")]
        public ChampionDefinition definition;
        [Range(1, 18)] public int level = 1;

        [Header("Team & Identity")]
        public Team team = Team.Neutral;
        public string unitName = "Unit";

        [Header("Runtime (ReadOnly)")]
        [SerializeField] private float currentHP;
        [SerializeField] private bool isDead;

        // 내부 StatAgent (비 Mono)
        private StatAgent _agent;

        // 외부에서 읽을 수 있게 프로퍼티 노출
        public StatAgent Agent => _agent;

        // LateUpdateManager 등록 id
        private int _lateId = 0;
        private float _lastNow = -1f;

        // 이벤트
        public event Action<Unit> OnSpawned;
        public event Action<Unit> OnDied;
        public event Action<Unit, float> OnDamaged;      // (unit, finalDamage)
        public event Action<Unit, float> OnHealed;       // (unit, amount)

        void Awake()
        {
            if (definition == null)
            {
                // 안전장치: 기본 정의 생성
                definition = new ChampionDefinition { championName = unitName };
            }

            // StatAgent 생성
            _agent = new StatAgent(definition);

            // 초기 체력은 스냅샷 기준 HP 최대치
            var snap = _agent.SnapshotAtLevel(level, Time.time);
            currentHP = snap.HP;
            isDead = false;

            _lateId = LateUpdateManager.Instance.Register(LateTick);

            // 만약 같은 오브젝트에 BasicAttackController/SkillBase 파생이 있다면 여기서 agent 주입 권장
            // 단, BasicAttackController.cs의 GetComponent<StatAgent>() 라인은 컴파일 이슈가 될 수 있으니
            // 해당 스크립트에서 그 라인을 제거/주석 처리하는 것을 권함.
            // TryInjectAgentToSameGameObject();
        }

        void Start()
        {
            OnSpawned?.Invoke(this);
        }

        void OnDestroy()
        {
            LateUpdateManager.Instance?.Unregister(_lateId);
        }

        private void LateTick(float now)
        {
            float dt = (_lastNow < 0f) ? 0f : Mathf.Max(0f, now - _lastNow);
            _lastNow = now;

            // 버프 만료 처리
            _agent.RemoveExpired(now);

            // 죽은 유닛은 추가 처리 생략
            if (isDead) return;

            // (선택) 체력재생/마나재생 등 패시브 회복 처리 예시
            var snap = _agent.SnapshotAtLevel(level, now);
            if (snap.HPRegen > 0)
            {
                Heal(snap.HPRegen * dt);
            }
            // 마나도 필요하다면 비슷하게 처리 가능
        }

        /// <summary>
        /// 현재 레벨 기준 스냅샷 반환(버프 반영).
        /// </summary>
        public StatSnapshot Snapshot(float now)
        {
            return new StatSnapshot(_agent.SnapshotAtLevel(level, now));
        }

        /// <summary>
        /// 외부에서 스탯 보정 추가(아이템/버프 등).
        /// now는 Time.time 등 외부 클럭 그대로.
        /// </summary>
        public void AddModifier(StatModifier mod, float now)
        {
            _agent.AddModifier(mod, now);
        }

        /// <summary>
        /// 즉시 체력 회복.
        /// </summary>
        public void Heal(float amount)
        {
            if (isDead || amount <= 0f) return;
            float maxHP = _agent.SnapshotAtLevel(level, Time.time).HP;
            float prev = currentHP;
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            float gained = currentHP - prev;
            if (gained > 0f) OnHealed?.Invoke(this, gained);
        }

        /// <summary>
        /// 즉시 고정 피해(실피해) 적용. (방어 무시)
        /// </summary>
        public void ApplyTrueDamage(float trueDamage)
        {
            if (isDead || trueDamage <= 0f) return;
            DealDamageInternal(trueDamage);
        }

        /// <summary>
        /// 물리 피해 적용 유틸.
        /// - rawADDamage는 '스킬/평타 기본계수*AD' 등으로 산출된 값 전달.
        /// - targetReduction/attackerPen은 로컬/외부 시스템에서 구성해 넘겨도 됨.
        /// </summary>
        public void ApplyPhysicalDamage(
            float rawADDamage,
            Reduction targetReduction,
            Penetration attackerPen,
            bool includeCrit = true)
        {
            if (isDead || rawADDamage <= 0f) return;

            float now = Time.time;
            var snap = _agent.SnapshotAtLevel(level, now);
            float targetArmor = snap.Armor; // 자기 자신의 방어로 받는 상황(피해 원천이 외부일 때는 그쪽에서 이 값 지정 가능)
            float final = _agent.ExpectedPhysicalDamage(
                rawADDamage: rawADDamage,
                level: level,
                now: now,
                targetArmor: targetArmor,
                targetReduction: targetReduction ?? new Reduction(0, 0),
                attackerPen: attackerPen ?? new Penetration(0, 0),
                includeCrit: includeCrit
            );
            DealDamageInternal(final);
        }

        /// <summary>
        /// 마법 피해 적용 유틸.
        /// </summary>
        public void ApplyMagicDamage(
            float rawAPDamage,
            Reduction targetReduction,
            Penetration attackerPen)
        {
            if (isDead || rawAPDamage <= 0f) return;

            float now = Time.time;
            var snap = _agent.SnapshotAtLevel(level, now);
            float targetMR = snap.MR;
            float final = _agent.ExpectedMagicDamage(
                rawAPDamage: rawAPDamage,
                level: level,
                now: now,
                targetMR: targetMR,
                targetReduction: targetReduction ?? new Reduction(0, 0),
                attackerPen: attackerPen ?? new Penetration(0, 0)
            );
            DealDamageInternal(final);
        }

        private void DealDamageInternal(float finalDamage)
        {
            if (finalDamage <= 0f) return;
            float prev = currentHP;
            currentHP = Mathf.Max(0f, currentHP - finalDamage);
            float took = prev - currentHP;
            if (took > 0f) OnDamaged?.Invoke(this, took);

            if (currentHP <= 0f && !isDead)
                Die();
        }

        private void Die()
        {
            isDead = true;
            OnDied?.Invoke(this);
            // TODO: 리스폰/파괴/애니메이션 트리거 등 게임 정책에 맞게 후처리
        }

        /// <summary>
        /// (선택) 같은 GO의 컴포넌트에 StatAgent 주입 시도.
        /// BasicAttackController/SkillBase에서 public StatAgent agent 필드를 노출했다면
        /// 이 메서드를 Awake에서 호출하여 주입 가능.
        /// </summary>
        private void TryInjectAgentToSameGameObject()
        {
            // BasicAttackController 연결
            var basic = GetComponent<BasicAttackController>();
            if (basic != null)
            {
                // 컴파일 이슈 방지용 가드: BasicAttackController 내 GetComponent<StatAgent>() 라인 수정 권장
                basic.GetType().GetField("agent")?.SetValue(basic, _agent);
            }

            // SkillBase 파생들 연결
            var skills = GetComponents<SkillBase>();
            foreach (var s in skills)
            {
                s.GetType().GetField("agent")?.SetValue(s, _agent);
            }
        }
    }

    /// <summary>
    /// 외부에 깔끔히 보여주기 위한 스냅샷 DTO (ToString 포함)
    /// </summary>
    [Serializable]
    public struct StatSnapshot
    {
        public int level;
        public float HP, HPRegen, Mana, ManaRegen;
        public float AD, AP, Armor, MR;
        public float AttackSpeed, MoveSpeed, Range;
        public float CritChance, CritDamageMult;

        public StatSnapshot(Snapshot s)
        {
            level = s.level;
            HP = s.HP; HPRegen = s.HPRegen; Mana = s.Mana; ManaRegen = s.ManaRegen;
            AD = s.AD; AP = s.AP; Armor = s.Armor; MR = s.MR;
            AttackSpeed = s.AttackSpeed; MoveSpeed = s.MoveSpeed; Range = s.Range;
            CritChance = s.CritChance; CritDamageMult = s.CritDamageMult;
        }

        public override string ToString()
        {
            return $"Lv{level} | HP {HP:F0} / AD {AD:F1} / AS {AttackSpeed:F3} / Armor {Armor:F1} / MR {MR:F1} / Crit {CritChance:P0} ×{CritDamageMult:F2}";
        }
    }
}
