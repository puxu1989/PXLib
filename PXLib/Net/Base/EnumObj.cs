using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Net
{
    /// <summary>
    /// 客户端设备的类型。
    /// </summary>
    public enum ClientType : byte
    {
        DotNET,
        Silverlight,
        WindowsPhone,
        WebSocket,
        IOS,
        Android,
        Xamarin,
        Others = 8
    }
    /// <summary>
    /// 客户端连接断开的原因分类。
    /// </summary>
    public enum DisconnectedType
    {
        /// <summary>
        /// 网络连接中断。
        /// </summary>
        NetworkInterrupted,
        /// <summary>
        /// 无效的消息。
        /// </summary>
        InvalidMessage,
        /// <summary>
        /// 消息中的UserID与当前连接的OwnerID不一致。
        /// </summary>
        MessageWithWrongUserID,
        /// <summary>
        /// 心跳超时。
        /// </summary>
        HeartBeatTimeout,
        /// <summary>
        /// 被同名用户挤掉线。（发生于RelogonMode为ReplaceOld）
        /// </summary>
        BeingPushedOut,
        /// <summary>
        /// 当已经有同名用户在线时，新的连接被忽略。（发生于RelogonMode为IgnoreNew） 
        /// </summary>
        NewConnectionIgnored,
        /// <summary>
        /// 等待发送以及正在发送的消息个数超过了MaxChannelCacheSize的设定值。
        /// </summary>
        ChannelCacheOverflow,
        /// <summary>
        /// 未授权的客户端类型
        /// </summary>
        UnauthorizedClientType,
        /// <summary>
        /// 已达到最大连接数限制
        /// </summary>
        MaxConnectionCountLimitted
    }
}
