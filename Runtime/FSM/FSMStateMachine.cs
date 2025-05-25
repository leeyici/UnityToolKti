using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToolKit.FSM
{
    /// <summary>
    /// 通用有限状态机（FSM）系统。
    /// T: 状态标识符（推荐使用 enum），Owner: 状态机拥有者类型。
    /// </summary>
    public class FsmStateMachine<T, TOwner>
    {
        private readonly Dictionary<T, FsmState<T, TOwner>> _states = new();
        private FsmState<T, TOwner> _currentFsmState;
        private T _currentStateKey;
        private readonly TOwner _owner;

        /// <summary>
        /// 当前状态枚举值。
        /// </summary>
        public T CurrentState => _currentStateKey;

        /// <summary>
        /// 当前状态实例。
        /// </summary>
        public FsmState<T, TOwner> CurrentFsmState => _currentFsmState;

        /// <summary>
        /// 状态切换事件：从旧状态 -> 新状态。
        /// </summary>
        public event Action<T, T> OnStateChanged;

        public FsmStateMachine(TOwner owner)
        {
            this._owner = owner;
            _currentStateKey = default;
        }

        /// <summary>
        /// 添加一个状态。
        /// </summary>
        public void AddState(T stateKey, FsmState<T, TOwner> state)
        {
            if (state == null)
            {
                Debug.LogError($"FSMState for [{stateKey}] is null.");
                return;
            }

            _states[stateKey] = state;
        }

        /// <summary>
        /// 设置初始状态（只调用一次）。
        /// </summary>
        public void SetInitialState(T stateKey)
        {
            if (!_states.TryGetValue(stateKey, out var state))
            {
                Debug.LogError($"FSMState not found: {stateKey}");
                return;
            }

            _currentStateKey = stateKey;
            _currentFsmState = state;

            LogEnterState(stateKey);
            _currentFsmState.Enter(_owner);
        }

        /// <summary>
        /// 切换状态。
        /// </summary>
        public void ChangeState(T newStateKey)
        {
            // 不重复切换相同状态
            if (EqualityComparer<T>.Default.Equals(_currentStateKey, newStateKey))
            {
                Debug.LogWarning($"FSM: Already in state {newStateKey}, ignored.");
                return;
            }

            if (!_states.TryGetValue(newStateKey, out var newState))
            {
                Debug.LogError($"FSMState not found: {newStateKey}");
                return;
            }

            // 退出旧状态
            _currentFsmState?.Exit(_owner);

            var previousStateKey = _currentStateKey;

            // 切换状态
            _currentStateKey = newStateKey;
            _currentFsmState = newState;

            // 回调事件
            OnStateChanged?.Invoke(previousStateKey, _currentStateKey);

            // 进入新状态
            LogEnterState(newStateKey);
            _currentFsmState.Enter(_owner);
        }

        /// <summary>
        /// 状态机更新。
        /// </summary>
        public void Update()
        {
            _currentFsmState?.Update(_owner);
        }

        private void LogEnterState(T stateKey)
        {
            Debug.Log($"{_owner.GetType().Name} entered state: {stateKey}");
        }
    }

    /// <summary>
    /// FSM 状态抽象类。用户需要继承并实现 Enter、Update、Exit。
    /// </summary>
    public abstract class FsmState<T, TOwner>
    {
        /// <summary>
        /// 状态进入时调用。
        /// </summary>
        public abstract void Enter(TOwner owner);

        /// <summary>
        /// 状态更新时调用。
        /// </summary>
        public abstract void Update(TOwner owner);

        /// <summary>
        /// 状态退出时调用。
        /// </summary>
        public abstract void Exit(TOwner owner);

        /// <summary>
        /// 可选：用于调试输出状态信息。
        /// </summary>
        public virtual void PrintData() { }
    }
}
