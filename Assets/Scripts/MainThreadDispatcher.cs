// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using System.Collections.Generic;
using System;
using UnityEngine;

// This singleton class is used to queue actions that need to be run on the main thread
// like UI updates that need to be triggered by threaded AWS events
public class MainThreadDispatcher : MonoBehaviour
{
    static public void Q(Action fn)
    {
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(() => { fn(); });
        }
    }

    static public void Q<T1>(Action<T1> fn, T1 p1)
    {
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(() => { fn(p1); });
        }
    }

    static public void Q<T1, T2>(Action<T1, T2> fn, T1 p1, T2 p2)
    {
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(() => { fn(p1, p2); });
        }
    }

    static public void Q<T1, T2, T3>(Action<T1, T2, T3> fn, T1 p1, T2 p2, T3 p3)
    {
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(() => { fn(p1, p2, p3); });
        }
    }

    private static Queue<Action> _mainThreadQueue = new Queue<Action>();
    private static MainThreadDispatcher _instance;

    private void RunMainThreadQueueActions()
    {
        lock (_mainThreadQueue)
        {
            while (_mainThreadQueue.Count > 0)
            {
                _mainThreadQueue.Dequeue().Invoke();
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if(_instance == null)
        {
            _instance = new GameObject("MainThreadDispatcher").AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(_instance.gameObject);
        }
    }

    private void Update()
    {
        RunMainThreadQueueActions();
    }


}
