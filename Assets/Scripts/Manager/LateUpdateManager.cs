using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGISTUDIO
{
    /// <summary>
    /// ���� LateUpdate �ݹ� ����ó.
    /// - Register/Unregister�� Action<float now> �ݹ��� ����.
    /// - LateUpdate �� ���� �ϰ� ȣ��.
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

        /// <summary>�ݹ� ���. ��ȯ�� id�� ����.</summary>
        public int Register(Action<float> cb)
        {
            if (cb == null) return 0;
            var e = new Entry { id = _nextId++, cb = cb, alive = true };
            _entries.Add(e);
            return e.id;
        }

        /// <summary>�ݹ� ����. �������� �ʾƵ� ����.</summary>
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

            // ȣ�� + ���� ��Ʈ�� ����(�ڿ��� ������)
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
