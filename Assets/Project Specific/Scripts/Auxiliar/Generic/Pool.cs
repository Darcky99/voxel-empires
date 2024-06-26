using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Pool<T> : MonoBehaviour where T : MonoBehaviour
{
    #region Editor
    [Button]
    private void preBake()
    {
        clearQueue();

        increaseThreshold();
    }
    [Button]
    private void clearObjects() => clearQueue();
    #endregion

    #region Unity
    protected virtual void Awake()
    {
        initializeQueue();
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

    private void initializeQueue()
    {
        if (m_ObjectsQueue != null)
            return;

        m_ObjectsQueue = new Queue<T>();

        if (transform.childCount != 0)
        {
            T[] i_ObjectsArray = GetComponentsInChildren<T>(true);

            foreach (T i_object in i_ObjectsArray)
                m_ObjectsQueue.Enqueue(i_object);
        }
        else return;
    }
    private void clearQueue()
    {
        initializeQueue();

        if (m_ObjectsQueue == null || m_ObjectsQueue.Count == 0)
            return;

        foreach (T i_object in m_ObjectsQueue)
            DestroyImmediate(i_object.gameObject);

        m_ObjectsQueue.Clear();
    }

    private void increaseThreshold()
    {
        for (int i = 0; i < m_Threshold; i++)
        {
            Queue(Instantiate(m_Prefab, transform));
        }
    }

    private void queue(T i_Object)
    {
        i_Object.gameObject.SetActive(false);

        i_Object.transform.SetParent(transform);

        i_Object.transform.localPosition = Vector3.zero;
        i_Object.transform.rotation = Quaternion.identity;

        m_ObjectsQueue.Enqueue(i_Object);
    }
    private T deQueue()
    {
        if (m_ObjectsQueue.Count == 0)
            increaseThreshold();

        T i_object =  m_ObjectsQueue.Dequeue();
        i_object.gameObject.SetActive(true);
        return i_object;
    }
    private T deQueue(Transform i_Parent)
    {
        T i_object = deQueue();
        i_object.transform.parent = i_Parent;
        return i_object;
    }

    public void Queue(T i_Object) => queue(i_Object);
    public T DeQueue() => deQueue();
    public T DeQueue(Transform i_Parent) => deQueue(i_Parent);
    #endregion
}