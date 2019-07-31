
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Net.Server
{
    // 摘要: 
    //     自定义信息处理器接口。
    public interface ICustomizeHandler
    {
        void ClientClose(UserTokenEventArgs userToken, string currentUserId);
        void HandleInformation(UserTokenEventArgs userToken);
        void HandleInformation(UserTokenEventArgs userToken, string currentUserId, int informationType, byte[] info);
        void SeverShutDown();
    }
}
