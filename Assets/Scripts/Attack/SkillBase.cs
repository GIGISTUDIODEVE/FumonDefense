// File: SkillBase.cs
using System;
using UnityEngine;

namespace GIGISTUDIO
{
    public abstract class SkillBase : MonoBehaviour
    {
        [Header("Refs")]
        public StatAgent agent;

        [Header("Gameplay")]
        [Range(1, 18)] public int level = 1;

        [Header("Costs & Cooldown")]
        public float manaCost = 0f;
        public float cooldown = 6f;
        public float castTime = 0.25f;

        [Header("Targeting")]
        public float range = 8f;
        public bool requiresTarget = true;

        [Header("Rule Flags")]
        public CombatFlags flags = CombatFlags.None;

        [Header("Default Target Mitigation (예시)")]
        public float targetArmor = 0f;
        public float targetMR = 0f;
        public Reduction targetReduction = new Reduction(0f, 0f);
        public Penetration attackerPen = new Penetration(0f, 0f);

        float _cdTimer = 0f;
        float _castTimer = 0f;
        bool _casting = false;
        int _lateId = 0;
        float _lastNow = -1f;

        public event Action OnCastStart;
        public event Action OnCastEnd;
        public event Action OnSpellHit;

        protected virtual void Awake()
        {
            if (agent == null) agent = GetComponent<StatAgent>();
            _lateId = LateUpdateManager.Instance.Register(LateTick);
        }

        protected virtual void OnDestroy()
        {
            LateUpdateManager.Instance?.Unregister(_lateId);
        }

        public bool TryCast(Transform target = null)
        {
            if (_casting) return false;
            if (_cdTimer > 0f) return false;
            if (requiresTarget && !target) return false;

            _casting = true;
            _castTimer = castTime;
            OnCastStart?.Invoke();
            return true;
        }

        void LateTick(float now)
        {
            float dt = (_lastNow < 0f) ? 0f : Mathf.Max(0f, now - _lastNow);
            _lastNow = now;

            agent?.RemoveExpired(now);

            if (_cdTimer > 0f)
            {
                _cdTimer -= dt;
                if (_cdTimer < 0f) _cdTimer = 0f;
            }

            if (_casting)
            {
                _castTimer -= dt;
                if (_castTimer <= 0f)
                {
                    _casting = false;
                    ResolveCast(now);   // now를 넘겨 실제 효과 처리
                    OnCastEnd?.Invoke();

                    if ((flags & CombatFlags.ResetsAttackTimer) != 0)
                        SendMessageUpwards("ResetAttackTimer", SendMessageOptions.DontRequireReceiver);

                    _cdTimer = cooldown;
                }
            }
        }

        protected abstract void ResolveCast(float now);

        // === 피해 유틸 ===
        protected float CalcPhysical(float rawDamage, float now)
        {
            return agent.ExpectedPhysicalDamage(
                rawADDamage: rawDamage,
                level: level,
                now: now,
                targetArmor: targetArmor,
                targetReduction: targetReduction,
                attackerPen: attackerPen,
                includeCrit: ((flags & CombatFlags.CanCrit) != 0)
            );
        }

        protected float CalcMagic(float rawDamage, float now)
        {
            return agent.ExpectedMagicDamage(
                rawAPDamage: rawDamage,
                level: level,
                now: now,
                targetMR: targetMR,
                targetReduction: targetReduction,
                attackerPen: attackerPen
            );
        }
    }
}
