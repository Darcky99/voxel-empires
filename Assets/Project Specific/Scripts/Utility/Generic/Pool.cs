using System.Collections.Generic;
using UnityEngine;

namespace VoxelUtilities.Pooling
{
    public class Pool<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Editor
        internal void PreBake()
        {
            ClearQueue();
            IncreaseThreshold();
        }
        internal void ClearObjects() => ClearQueue();
        #endregion

        #region Unity
        protected virtual void Awake()
        {
            InitializeQueue();
        }
        protected virtual void OnEnable()
        {
            s_Instance = this;
        }
        #endregion

        #region Specific
        public static Pool<T> s_Instance;

        public Queue<T> ObjectsQueue => m_ObjectsQueue;

        [SerializeField] protected Queue<T> m_ObjectsQueue;

        [SerializeField] private T m_Prefab;
        [SerializeField, Min(1)] private int m_Threshold = 5;

        private void InitializeQueue()
        {
            if (m_ObjectsQueue != null)
            {
                return;
            }
            m_ObjectsQueue = new Queue<T>();
            if (transform.childCount != 0)
            {
                T[] i_ObjectsArray = GetComponentsInChildren<T>(true);
                foreach (T i_object in i_ObjectsArray)
                {
                    m_ObjectsQueue.Enqueue(i_object);
                }
            }
            else
            {
                return;
            }
        }
        private void ClearQueue()
        {
            InitializeQueue();
            if (m_ObjectsQueue == null || m_ObjectsQueue.Count == 0)
            {
                return;
            }
            foreach (T i_object in m_ObjectsQueue)
            {
                DestroyImmediate(i_object.gameObject);
            }
            m_ObjectsQueue.Clear();
        }

        private void IncreaseThreshold()
        {
            for (int i = 0; i < m_Threshold; i++)
            {
                Queue(Instantiate(m_Prefab, transform));
            }
        }

        public void Queue(T i_Object)
        {
            i_Object.gameObject.SetActive(false);

            i_Object.transform.SetParent(transform);

            i_Object.transform.localPosition = Vector3.zero;
            i_Object.transform.rotation = Quaternion.identity;

            m_ObjectsQueue.Enqueue(i_Object);
        }
        public T DeQueue()
        {
            if (m_ObjectsQueue.Count == 0)
                IncreaseThreshold();

            T i_object = m_ObjectsQueue.Dequeue();
            i_object.gameObject.SetActive(true);
            return i_object;
        }
        public T DeQueue(Transform i_Parent)
        {
            T i_object = DeQueue();
            i_object.transform.parent = i_Parent;
            return i_object;
        }

        #endregion
    }
}