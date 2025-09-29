// File: BasicAttackController.cs
using System;
using UnityEngine;

namespace GIGISTUDIO
{
    [DisallowMultipleComponent]
    public sealed class BasicAttackController : MonoBehaviour
    {
        [Header("Refs")]
        public StatAgent agent;
        public Transform projectileSpawn;

        [Header("Gameplay")]
        [Range(1, 18)] public int level = 1;

        [Header("Timings (ratio of attack period)")]
        [Range(0.05f, 0.8f)] public float windupRatio = 0.3f;
        [Range(0.05f, 0.9f)] public float recoverRatio = 0.4f;
        [Range(0.0f, 0.3f)] public float hitWindow = 0.05f;

        [Header("Ranged / Melee")]
        public bool usesProjectile = true;
        [Range(0f, 1.0f)] public float projectileFlightTime = 0.15f;

        [Header("Target Defense (����, ������ Ÿ�� ������Ʈ���� �б�)")]
        public float targetArmor = 0f;
        public Reduction targetReduction = new Reduction(0f, 0f);
        public Penetration attackerPen = new Penetration(0f, 0f);

        enum State { Idle, Windup, Launch, HitPending, Recover }
        State _state = State.Idle;

        int _lateUpdateId = 0;
        float _attackPeriod;
        float _attackTimer;
        float _phaseTimer;
        float _lastNow = -1f;

        public event Action OnAttackStart;
        public event Action OnProjectileLaunch;
        public event Action OnMeleeContact;
        public event Action<float> OnHit;
        public event Action OnAttackEnd;

        void Awake()
        {
            if (agent == null) agent = GetComponent<StatAgent>();
            _lateUpdateId = LateUpdateManager.Instance.Register(OnLateTick);
        }

        void OnDestroy()
        {
            LateUpdateManager.Instance?.Unregister(_lateUpdateId);
        }

        void Start()
        {
            RecalculateAttackPeriod(Time.time);
            _attackTimer = 0f;
        }

        public void RecalculateAttackPeriod(float now)
        {
            var snap = agent.SnapshotAtLevel(level, now);
            // �������� ���� AttackSpeed�� �״�� ���
            float attackSpeed = Mathf.Max(0.1f, snap.AttackSpeed);
            _attackPeriod = 1f / attackSpeed;
        }

        public bool TryAttack(Transform target)
        {
            if (_state != State.Idle) return false;
            if (_attackTimer > 0f) return false;
            if (!target) return false;

            StartWindup();
            return true;
        }

        void StartWindup()
        {
            _state = State.Windup;
            _phaseTimer = _attackPeriod * windupRatio;
            OnAttackStart?.Invoke();
        }

        void StartLaunch()
        {
            _state = State.Launch;
            float launchPhase = Mathf.Max(0.0f, _attackPeriod * hitWindow);
            _phaseTimer = launchPhase;

            if (usesProjectile)
            {
                OnProjectileLaunch?.Invoke();
                _phaseTimer = Mathf.Max(_phaseTimer, projectileFlightTime);
            }
            else
            {
                OnMeleeContact?.Invoke();
            }
        }

        void StartHit(float now)
        {
            _state = State.HitPending;
            _phaseTimer = 0.02f;

            // === ���� ��� ===
            var snap = agent.SnapshotAtLevel(level, now);  // AD/ġ��/���� ��뿡 ���
            float dealt = agent.ExpectedPhysicalDamage(
                rawADDamage: snap.AD,
                level: level,
                now: now,
                targetArmor: targetArmor,
                targetReduction: targetReduction,
                attackerPen: attackerPen,
                includeCrit: true
            );

            OnHit?.Invoke(dealt);

            // ���� ���ݱ��� Ÿ�̸� ���
            _attackTimer = _attackPeriod;
        }

        void StartRecover()
        {
            _state = State.Recover;
            _phaseTimer = Mathf.Max(0f, _attackPeriod * recoverRatio);
        }

        void FinishAttack()
        {
            _state = State.Idle;
            OnAttackEnd?.Invoke();
        }

        void OnLateTick(float now)
        {
            // dt ��� (LateUpdateManager�� now�� ��)
            float dt = (_lastNow < 0f) ? 0f : Mathf.Max(0f, now - _lastNow);
            _lastNow = now;

            // ���� ���� ó��
            agent?.RemoveExpired(now);

            // ����/���� ���� ���� �� �ֱ� ����(�����ϸ� �� ���絵 ��)
            RecalculateAttackPeriod(now);

            // ���� Ÿ�̸�
            if (_attackTimer > 0f)
            {
                _attackTimer -= dt;
                if (_attackTimer < 0f) _attackTimer = 0f;
            }

            if (_state == State.Idle) return;

            _phaseTimer -= dt;
            if (_phaseTimer > 0f) return;

            switch (_state)
            {
                case State.Windup: StartLaunch(); break;
                case State.Launch: StartHit(now); break;
                case State.HitPending: StartRecover(); break;
                case State.Recover: FinishAttack(); break;
            }
        }
    }
}
