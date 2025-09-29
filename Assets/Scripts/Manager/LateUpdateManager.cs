using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGISTUDIO
{
    /// <summary>
    /// 전역 LateUpdate 콜백 디스패처.
    /// - Register/Unregister로 Action<float now> 콜백을 관리.
    /// - LateUpdate 한 번에 일괄 호출.
    /// </summary>
    public class LateUpdateManager : Singleton<LateUpdateManager>
    {
        struct Entry
        {
            public int id;
            public Action<float> cb;
            public bool alive;
        }

        int _nextId = 1;
        readonly List<Entry> _entries = new List<Entry>(256);

        /// <summary>콜백 등록. 반환된 id로 해제.</summary>
        public int Register(Action<float> cb)
        {
            if (cb == null) return 0;
            var e = new Entry { id = _nextId++, cb = cb, alive = true };
            _entries.Add(e);
            return e.id;
        }

        /// <summary>콜백 해제. 존재하지 않아도 안전.</summary>
        public void Unregister(int id)
        {
            if (id == 0) return;
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].id == id)
                {
                    var e = _entries[i];
                    e.alive = false;
                    e.cb = null;
                    _entries[i] = e;
                    break;
                }
            }
        }

        void LateUpdate()
        {
            float now = Time.time;

            // 호출 + 죽은 엔트리 정리(뒤에서 앞으로)
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                if (!e.alive || e.cb == null) continue;
                try { e.cb(now); }
                catch (Exception ex) { Debug.LogException(ex); }
            }

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (!_entries[i].alive || _entries[i].cb == null)
                    _entries.RemoveAt(i);
            }
        }
    }
}
