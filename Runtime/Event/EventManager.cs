using System.Collections.Generic;
using ToolKit.Singleton;
using Unity.VisualScripting;
using UnityEngine;

namespace ToolKit.Event
{
    public class EventManager : SingletonBase<EventManager>
    {
        // 事件表
        private readonly Dictionary<EventEnum, System.Delegate> _eventTable = new Dictionary<EventEnum, System.Delegate>();
        // 添加监听器前置操作
        private void AddingListenerEvent(EventEnum eventEnum, System.Delegate callBack)
        {
            if (!_eventTable.ContainsKey(eventEnum) && !callBack.IsUnityNull())
            {
                _eventTable.Add(eventEnum,null);
            }
        }
        
        // 移除监听器前置操作
        private void RemovingListener(EventEnum eventEnum, System.Delegate callBack)
        {
            // Do nothing because of X
        }
        
        // 移除监听器后置操作
        private void RemovedListener(EventEnum eventEnum)
        {
            if (_eventTable[eventEnum] == null)
            {
                _eventTable.Remove(eventEnum);
            }
        }
        
        // 广播事件
        public void BroadCast(EventEnum eventEnum)
        {
            if (!_eventTable.TryGetValue(eventEnum, out var handler))
            {
                Debug.LogWarning($"未找到对应的0参数事件: {eventEnum.ToString()}");
                return;
            }

            if (handler is CallBack callBack)
            {
                callBack();
            }
        }
        
        public void BroadCast<T>(EventEnum eventEnum, T value)
        {
            if (!_eventTable.TryGetValue(eventEnum, out var handler))
            {
                Debug.LogWarning($"未找到对应的1参数事件: {eventEnum.ToString()}");
                return;
            }

            if (handler is CallBack<T> callBack)
            {
                callBack(value);
            }
        }
        
        public void BroadCast<T1, T2>(EventEnum eventEnum, T1 value1, T2 value2)
        {
            if (!_eventTable.TryGetValue(eventEnum, out var handler))
            {
                Debug.LogWarning($"未找到对应的2参数事件: {eventEnum.ToString()}");
                return;
            }

            if (handler is CallBack<T1, T2> callBack)
            {
                callBack(value1, value2);
            }
        }
        
        public void BroadCast<T1, T2, T3>(EventEnum eventEnum, T1 value1, T2 value2, T3 value3)
        {
            if (!_eventTable.TryGetValue(eventEnum, out var handler))
            {
                Debug.LogWarning($"未找到对应的3参数事件: {eventEnum.ToString()}");
                return;
            }

            if (handler is CallBack<T1, T2, T3> callBack)
            {
                callBack(value1, value2, value3);
            }
        }
        
        public void BroadCast<T1, T2, T3,T4>(EventEnum eventEnum, T1 value1, T2 value2, T3 value3,T4 value4)
        {
            if (!_eventTable.TryGetValue(eventEnum, out var handler))
            {
                Debug.LogWarning($"未找到对应的3参数事件: {eventEnum.ToString()}");
                return;
            }

            if (handler is CallBack<T1, T2, T3,T4> callBack)
            {
                callBack(value1, value2, value3,value4);
            }
        }
        
        // 添加监听器
        public void OnAddListener(EventEnum eventEnum, CallBack callBack)
        {
            AddingListenerEvent(eventEnum, callBack);
            _eventTable[eventEnum] = (CallBack)_eventTable[eventEnum] + callBack;
        }
        
        public void OnAddListener<T>(EventEnum eventEnum, CallBack<T> callBack)
        {
            AddingListenerEvent(eventEnum, callBack);
            _eventTable[eventEnum] = (CallBack<T>)_eventTable[eventEnum] + callBack;
        }
        
        public void OnAddListener<T1, T2>(EventEnum eventEnum, CallBack<T1, T2> callBack)
        {
            AddingListenerEvent(eventEnum, callBack);
            _eventTable[eventEnum] = (CallBack<T1, T2>)_eventTable[eventEnum] + callBack;
        }
        
        public void OnAddListener<T1, T2, T3>(EventEnum eventEnum, CallBack<T1, T2, T3> callBack)
        {
            AddingListenerEvent(eventEnum, callBack);
            _eventTable[eventEnum] = (CallBack<T1, T2, T3>)_eventTable[eventEnum] + callBack;
        }

        public void OnAddListener<T1, T2, T3,T4>(EventEnum eventEnum, CallBack<T1, T2, T3,T4> callBack)
        {
            AddingListenerEvent(eventEnum, callBack);
            _eventTable[eventEnum] = (CallBack<T1, T2, T3,T4>)_eventTable[eventEnum] + callBack;
        }
        

        /// <summary>
        /// 单播注册，只会接收最新的注册结果
        /// </summary>
        /// <param name="eventEnum"></param>
        /// <param name="callBack"></param>
        /// <typeparam name="T"></typeparam>
        public void OnAddAction<T>(EventEnum eventEnum, CallBackWithReturn<T> callBack)
        {
            AddingListenerEvent(eventEnum,callBack);
            _eventTable[eventEnum] = callBack;
        }

        public void OnAddAction(EventEnum eventEnum, CallBack callBack)
        {
            AddingListenerEvent(eventEnum,callBack);
            _eventTable[eventEnum] = callBack;
        }
        
        public CallBackWithReturn<T> GetAction<T>(EventEnum eventEnum)
        {
            if (!_eventTable.TryGetValue(eventEnum, out var handler))
            {
                Debug.LogWarning("未找到对应的单播！");
                return null;
            }
        
            if (handler is CallBackWithReturn<T> callBack)
            {
                return callBack;
            }

            return null;
        }

        public CallBack GetAction(EventEnum eventEnum)
        {
            if (!_eventTable.TryGetValue(eventEnum, out var handler))
            {
                Debug.LogWarning("未找到对应的单播！");
                return null;
            }
        
            if (handler is CallBack callBack)
            {
                return callBack;
            }

            return null;
        }
        
        public void OnRemoveAction<T>(EventEnum eventEnum,CallBackWithReturn<T> callBackWithReturn)
        {
            RemovingListener(eventEnum, callBackWithReturn);
            _eventTable[eventEnum] = (CallBackWithReturn<T>)_eventTable[eventEnum] - callBackWithReturn;
            RemovedListener(eventEnum);
        }

        public void OnRemoveAction(EventEnum eventEnum, CallBack callBack)
        {
            RemovingListener(eventEnum, callBack);
            if (_eventTable.ContainsKey(eventEnum))
            {
                _eventTable[eventEnum] = (CallBack)_eventTable[eventEnum] - callBack;
                RemovedListener(eventEnum);
            }
        }
        
        //移除监听器
        public void OnRemoveListener(EventEnum eventEnum, CallBack callBack)
        { 
            if (!_eventTable.ContainsKey(eventEnum))
            {
                return;
            }
            RemovingListener(eventEnum, callBack);
            _eventTable[eventEnum] = (CallBack)_eventTable[eventEnum] - callBack;
            RemovedListener(eventEnum);
        }
        
        public void OnRemoveListener<T>(EventEnum eventEnum, CallBack<T> callBack)
        {
            RemovingListener(eventEnum, callBack);
            if (_eventTable.ContainsKey(eventEnum))
            {
                _eventTable[eventEnum] = (CallBack<T>)_eventTable[eventEnum] - callBack;
                RemovedListener(eventEnum);
            }
        }
    
        public void OnRemoveListener<T1, T2>(EventEnum eventEnum, CallBack<T1, T2> callBack)
        {
            RemovingListener(eventEnum, callBack);
            if (_eventTable.ContainsKey(eventEnum))
            {
                _eventTable[eventEnum] = (CallBack<T1, T2>)_eventTable[eventEnum] - callBack;
                RemovedListener(eventEnum);
            }
        }
        
        public void OnRemoveListener<T1, T2, T3>(EventEnum eventEnum, CallBack<T1, T2, T3> callBack)
        {
            RemovingListener(eventEnum, callBack);
            _eventTable[eventEnum] = (CallBack<T1, T2, T3>)_eventTable[eventEnum] - callBack;
            RemovedListener(eventEnum);
        }
        
        public void OnRemoveListener<T1, T2, T3,T4>(EventEnum eventEnum, CallBack<T1, T2, T3,T4> callBack)
        {
            RemovingListener(eventEnum, callBack);
            _eventTable[eventEnum] = (CallBack<T1, T2, T3,T4>)_eventTable[eventEnum] - callBack;
            RemovedListener(eventEnum);
        }
        
        // // 清空所有监听
        // public void OnRemoveAllListener()
        // {
        //     eventTable.Clear();
        // }
        
        //清空当前选择的eventEnum的全部监听
        public void OnRemoveCurrentListener(EventEnum eventEnum)
        {
            if (!_eventTable.ContainsKey(eventEnum))
            {
                return;
            }
            _eventTable[eventEnum] = null;
        }
    }
}
