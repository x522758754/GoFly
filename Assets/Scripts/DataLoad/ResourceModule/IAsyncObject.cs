
namespace AssetLoad
{
    /// <summary>
    /// 异步操作返回结果，参照：IAsyncResult，AsyncOperation
    /// </summary>
    public interface IAsyncObject
    {
        /// <summary>
        /// 最终加载结果的资源
        /// </summary>
        object RetResult { get; }

        /// <summary>
        /// 操作是否完成
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// 操作中出错
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// 过程信息
        /// </summary>
        string AysncMessage { get; }
    }
}
