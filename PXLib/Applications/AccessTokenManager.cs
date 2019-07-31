using PXLib.Base.Entity;
using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.Application
{
    public class AccessTokenManager
    {
        public static string TokenKey = "AccessToken";
        #region 不使用缓存Token
        public static string CreateAccessToken(Token token)
        {
            if (token.UserId.IsNullEmpty())
                throw new ArgumentException("UserId不能为空", "UserId");
            return SecurityHelper.AESEncryptString(token.ToJson());
        }
        public static Token GetCurrent()
        {
            string tokenStr = WebHelper.GetHeadValue(TokenKey);
            if (tokenStr.IsNullEmpty())
                throw new Exception("401未登录");
            return GetCurrent(tokenStr);
        }
        public static Token GetCurrent(string tokenStr)
        {
            string json = SecurityHelper.AESDecryptString(tokenStr);
            return json.ToObject<Token>();
        }
        public static bool ValidateToken()
        {
            string tokenStr = WebHelper.GetHeadValue(TokenKey);
            return ValidateToken(tokenStr);
        }
        public static bool ValidateToken(string tokenStr)
        {
            if (tokenStr.IsNullEmpty())
                return false;
            try
            {
                return (DateTime.Now <= GetCurrent(tokenStr).ExpireTime) ? true : false;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
