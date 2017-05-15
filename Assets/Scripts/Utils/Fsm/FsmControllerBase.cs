/*
 * 
 * 有限状态机控制器
 * 
 * 注：T(枚举类型)表示不同的状态
 * */
using System.Collections.Generic;


namespace Util
{
    public interface IFsmController<T>
    {
        /// <summary>
        /// 初始化状态机
        /// </summary>
        void InitController();

        /// <summary>
        /// 初始默认状态
        /// </summary>
        void EnterDefaultState();

        /// <summary>
        /// 切换状态
        /// </summary>
        void SwitchState();

        /// <summary>
        /// 刷新当前状态，并不需要重新进入
        /// </summary>
        void UpdateState();
    }


    public abstract class FsmControllerBase<T> : IFsmController<T>
    {
        protected FsmStateBase<T> _currentState;

        protected Dictionary<T, FsmStateBase<T>> _allStates;

        public abstract void InitController();

        public abstract void EnterDefaultState();

        public abstract void SwitchState();

        public abstract void UpdateState();

    }
}
