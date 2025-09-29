// StatAgentDebugView.cs  (�� ����, namespace GIGISTUDIO)
using System.Collections.Generic;
using UnityEngine;

namespace GIGISTUDIO
{
    /// <summary>
    /// ����: ��Ÿ���� StatAgent.Mods�� �ν����Ϳ� "���� ����"���� �̷���.
    /// - ���� ���̱⸸ �ϸ� ��.
    /// - ���� ������Ʈ�� GameStatsSystem�� ������ �ڵ����� Agent�� ����.
    /// - ������ sourceSystem�� ���� ���� �־ ��.
    /// </summary>
    [ExecuteAlways]
    public class StatAgentDebugView : MonoBehaviour
    {
        [Tooltip("����θ� ���� GameObject���� GameStatsSystem�� �ڵ����� ã���ϴ�.")]
        public GameStatsSystem sourceSystem;

        [Tooltip("���� ������ ����������, ������ sourceSystem.Agent�� �ڵ����� ���ϴ�.")]
        public StatAgent agent;

        // �ν����� ǥ�ÿ� �̷� ����(�б� ����ó�� ���)
        [SerializeField]
        List<StatModifier> runtimeView = new List<StatModifier>();

        void Awake()
        {
            if (sourceSystem == null)
                sourceSystem = GetComponent<GameStatsSystem>();
        }

        void LateUpdate()
        {
            // 1) Agent ����
            if (agent == null)
                agent = sourceSystem != null ? sourceSystem.Agent : agent;

            // 2) �̷���
            runtimeView.Clear();
            var mods = agent?.Mods;
            if (mods != null)
            {
                // ���� ����: �ν����Ϳ��� ������ �뵵
                for (int i = 0; i < mods.Count; i++)
                    runtimeView.Add(mods[i]);
            }

            // 3) ������ ���� (�÷���/���� �� �� �� ���̰�)
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
