// StatAgentDebugView.cs  (새 파일, namespace GIGISTUDIO)
using System.Collections.Generic;
using UnityEngine;

namespace GIGISTUDIO
{
    /// <summary>
    /// 목적: 런타임의 StatAgent.Mods를 인스펙터에 "보기 전용"으로 미러링.
    /// - 씬에 붙이기만 하면 됨.
    /// - 같은 오브젝트에 GameStatsSystem이 있으면 자동으로 Agent를 참조.
    /// - 없으면 sourceSystem에 직접 참조 넣어도 됨.
    /// </summary>
    [ExecuteAlways]
    public class StatAgentDebugView : MonoBehaviour
    {
        [Tooltip("비워두면 같은 GameObject에서 GameStatsSystem을 자동으로 찾습니다.")]
        public GameStatsSystem sourceSystem;

        [Tooltip("직접 지정도 가능하지만, 보통은 sourceSystem.Agent를 자동으로 씁니다.")]
        public StatAgent agent;

        // 인스펙터 표시용 미러 버퍼(읽기 전용처럼 사용)
        [SerializeField]
        List<StatModifier> runtimeView = new List<StatModifier>();

        void Awake()
        {
            if (sourceSystem == null)
                sourceSystem = GetComponent<GameStatsSystem>();
        }

        void LateUpdate()
        {
            // 1) Agent 결정
            if (agent == null)
                agent = sourceSystem != null ? sourceSystem.Agent : agent;

            // 2) 미러링
            runtimeView.Clear();
            var mods = agent?.Mods;
            if (mods != null)
            {
                // 얕은 복사: 인스펙터에만 보여줄 용도
                for (int i = 0; i < mods.Count; i++)
                    runtimeView.Add(mods[i]);
            }

            // 3) 에디터 갱신 (플레이/에딧 둘 다 잘 보이게)
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
