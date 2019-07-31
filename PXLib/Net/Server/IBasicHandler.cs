using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Net.Server
{
    public interface IBasicHandler
    {
        /// <summary>
        /// 客户端登陆验证。
        /// </summary>        
        /// <param name="userID">登陆用户账号</param>
        /// <param name="systemToken">系统标志。用于验证客户端是否与服务端属于同一系统。</param>
        /// <param name="password">登陆密码</param>
        /// <param name="failureCause">如果登录失败，该out参数指明失败的原因</param>
        /// <returns>如果密码和系统标志都正确则返回true；否则返回false。</returns>
        bool CheckLoginUser(string systemToken, string userID, string password, out string failureCause);

        /// <summary>
        /// 直接在从服务端发出相关控制指令（如踢人等）。将目标用户从当前AS中踢出，并关闭对应的连接。
        /// </summary>        
        void KickOut(string targetUserID);

    }
}
