// File: Unit.cs
using System;
using UnityEngine;

namespace GIGISTUDIO
{
    /// <summary>
    /// ����(è�Ǿ�/���� ��)�� �ּ� ����:
    /// - StatAgent(�� Mono)�� ���ο��� ����/����
    /// - ����/������/��������/ü��/��� �̺�Ʈ ����
    /// - ����/���� ���� ���� ��ƿ ���� (StatMath ���������� �ϰ� ����)
    /// - LateUpdateManager�� ���� �ֱ� ����(���� ���� ��)
    /// 
    /// ����:
    /// - BasicAttackController.cs�� GetComponent&lt;StatAgent&gt;�� �õ��ϰ� �ִµ�,
    ///   StatAgent�� Mono�� �ƴϹǷ� ������ ������ �� �� ����.
    ///   �ذ�å: BasicAttackController���� agent �ʵ带 �ν����ͷ� ����ΰ�,
    ///   �� ������Ʈ(Unit)�� ��Ÿ�ӿ� ����(setter)�ϵ��� �ϰų�
    ///   BasicAttackController�� �ش� ������ �����ϰ� ���� �ʿ�.
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

        // ���� StatAgent (�� Mono)
        private StatAgent _agent;

        // �ܺο��� ���� �� �ְ� ������Ƽ ����
        public StatAgent Agent => _agent;

        // LateUpdateManager ��� id
        private int _lateId = 0;
        private float _lastNow = -1f;

        // �̺�Ʈ
        public event Action<Unit> OnSpawned;
        public event Action<Unit> OnDied;
        public event Action<Unit, float> OnDamaged;      // (unit, finalDamage)
        public event Action<Unit, float> OnHealed;       // (unit, amount)

        void Awake()
        {
            if (definition == null)
            {
                // ������ġ: �⺻ ���� ����
                definition = new ChampionDefinition { championName = unitName };
            }

            // StatAgent ����
            _agent = new StatAgent(definition);

            // �ʱ� ü���� ������ ���� HP �ִ�ġ
            var snap = _agent.SnapshotAtLevel(level, Time.time);
            currentHP = snap.HP;
            isDead = false;

            _lateId = LateUpdateManager.Instance.Register(LateTick);

            // ���� ���� ������Ʈ�� BasicAttackController/SkillBase �Ļ��� �ִٸ� ���⼭ agent ���� ����
            // ��, BasicAttackController.cs�� GetComponent<StatAgent>() ������ ������ �̽��� �� �� ������
            // �ش� ��ũ��Ʈ���� �� ������ ����/�ּ� ó���ϴ� ���� ����.
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

            // ���� ���� ó��
            _agent.RemoveExpired(now);

            // ���� ������ �߰� ó�� ����
            if (isDead) return;

            // (����) ü�����/������� �� �нú� ȸ�� ó�� ����
            var snap = _agent.SnapshotAtLevel(level, now);
            if (snap.HPRegen > 0)
            {
                Heal(snap.HPRegen * dt);
            }
            // ������ �ʿ��ϴٸ� ����ϰ� ó�� ����
        }

        /// <summary>
        /// ���� ���� ���� ������ ��ȯ(���� �ݿ�).
        /// </summary>
        public StatSnapshot Snapshot(float now)
        {
            return new StatSnapshot(_agent.SnapshotAtLevel(level, now));
        }

        /// <summary>
        /// �ܺο��� ���� ���� �߰�(������/���� ��).
        /// now�� Time.time �� �ܺ� Ŭ�� �״��.
        /// </summary>
        public void AddModifier(StatModifier mod, float now)
        {
            _agent.AddModifier(mod, now);
        }

        /// <summary>
        /// ��� ü�� ȸ��.
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
        /// ��� ���� ����(������) ����. (��� ����)
        /// </summary>
        public void ApplyTrueDamage(float trueDamage)
        {
            if (isDead || trueDamage <= 0f) return;
            DealDamageInternal(trueDamage);
        }

        /// <summary>
        /// ���� ���� ���� ��ƿ.
        /// - rawADDamage�� '��ų/��Ÿ �⺻���*AD' ������ ����� �� ����.
        /// - targetReduction/attackerPen�� ����/�ܺ� �ý��ۿ��� ������ �Ѱܵ� ��.
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
            float targetArmor = snap.Armor; // �ڱ� �ڽ��� ���� �޴� ��Ȳ(���� ��õ�� �ܺ��� ���� ���ʿ��� �� �� ���� ����)
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
        /// ���� ���� ���� ��ƿ.
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
            // TODO: ������/�ı�/�ִϸ��̼� Ʈ���� �� ���� ��å�� �°� ��ó��
        }

        /// <summary>
        /// (����) ���� GO�� ������Ʈ�� StatAgent ���� �õ�.
        /// BasicAttackController/SkillBase���� public StatAgent agent �ʵ带 �����ߴٸ�
        /// �� �޼��带 Awake���� ȣ���Ͽ� ���� ����.
        /// </summary>
        private void TryInjectAgentToSameGameObject()
        {
            // BasicAttackController ����
            var basic = GetComponent<BasicAttackController>();
            if (basic != null)
            {
                // ������ �̽� ������ ����: BasicAttackController �� GetComponent<StatAgent>() ���� ���� ����
                basic.GetType().GetField("agent")?.SetValue(basic, _agent);
            }

            // SkillBase �Ļ��� ����
            var skills = GetComponents<SkillBase>();
            foreach (var s in skills)
            {
                s.GetType().GetField("agent")?.SetValue(s, _agent);
            }
        }
    }

    /// <summary>
    /// �ܺο� ����� �����ֱ� ���� ������ DTO (ToString ����)
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
            return $"Lv{level} | HP {HP:F0} / AD {AD:F1} / AS {AttackSpeed:F3} / Armor {Armor:F1} / MR {MR:F1} / Crit {CritChance:P0} ��{CritDamageMult:F2}";
        }
    }
}
