/*
 * 有限状态机基状态
 * 注：T以枚举类型表示不同的状态
 * */

namespace Util
{

    public interface IFsmState<T>
    {
        /// <summary>
        /// 状态进入时执行的动作
        /// </summary>
        void Enter();

        /// <summary>
        /// 常态
        /// </summary>
        void Update();

        /// <summary>
        /// 状态退出时执行的动作
        /// </summary>
        void Exit();

    }


    public abstract class FsmStateBase<T> : IFsmState<T>
    {
        protected readonly T _stateType;

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
    }
}
