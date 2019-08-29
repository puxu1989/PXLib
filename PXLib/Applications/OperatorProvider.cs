using PXLib.Base.Entity;
using PXLib.Caches;
using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Application
{
    /// <summary>
    /// 用于消息会话
    /// </summary>
    public class OperatorProvider : IOperatorProvider
    {
        #region 静态实例
        /// <summary>
        /// 当前提供者
        /// </summary>
        public static IOperatorProvider Provider
        {
            get { return new OperatorProvider(); }
        }
        /// <summary>
        /// 给app调用
        /// </summary>
        public static string AppUserId
        {
            set;
            get;
        }
        #endregion
        /// <summary>
        /// 登录会话模式
        /// </summary>
        private string LoginProvider = ConfigHelper.GetConfigAppSettingsValue("LoginProvider");
        /// <summary>
        /// 会话关键字
        /// </summary>
        private string LoginUserKey = "loginUserkey";

        public virtual void AddCurrent(Operator user)
        {
            try
            {
                string userDate = SecurityHelper.DESEncrypt(user.ToJson());
                if (this.LoginProvider == "Cookie")
                {
                    WebHelper.WriteCookie(this.LoginUserKey, userDate);
                }
                else
                {
                    WebHelper.WriteSession(this.LoginUserKey, userDate);
                }
                CacheFactory.Cache().WriteCache(user.Token, user.UserId, user.LogTime.AddHours(12));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Operator Current()
        {
            try
            {
                Operator user = new Operator();

                if (LoginProvider == "Cookie")
                {
                    user = SecurityHelper.DESDecrypt(WebHelper.GetCookie(LoginUserKey).ToString()).ToObject<Operator>();
                }
                else if (LoginProvider == "AppClient")
                {
                    user = CacheFactory.Cache().GetCache<Operator>(AppUserId);
                }
                else
                {
                    user = SecurityHelper.DESDecrypt(WebHelper.GetSession(LoginUserKey).ToString()).ToObject<Operator>();
                }
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void RemoveCurrent()
        {
            if (LoginProvider == "Cookie")
            {
                WebHelper.RemoveCookie(LoginUserKey.Trim());
            }
            else
            {
                WebHelper.RemoveSession(LoginUserKey.Trim());
            }
        }
        /// <summary>
        /// 是否过期了
        /// </summary>
        /// <returns></returns>
        public bool IsOverdue()
        {
            try
            {
                object str = "";
                if (LoginProvider == "Cookie")
                {
                    str = WebHelper.GetCookie(LoginUserKey);
                }
                else
                {
                    str = WebHelper.GetSession(LoginUserKey);
                }
                if (str != null && str.ToString() != "")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return true;
            }
        }

        public int IsOnLine()
        {
            Operator user = new Operator();
            if (LoginProvider == "Cookie")
            {
                user =SecurityHelper.DESDecrypt(WebHelper.GetCookie(LoginUserKey).ToString()).ToObject<Operator>();
            }
            else
            {
                user = SecurityHelper.DESDecrypt(WebHelper.GetSession(LoginUserKey).ToString()).ToObject<Operator>();
            }
            object token = CacheFactory.Cache().GetCache<string>(user.UserId);
            if (token == null)
            {
                return -1;//过期
            }
            if (user.Token == token.ToString())
            {
                return 1;//正常
            }
            else
            {
                return 0;//已登录
            }
        }
    }
    public interface IOperatorProvider
    {
        /// <summary>
        /// 写入登录信息
        /// </summary>
        /// <param name="user">成员信息</param>
        void AddCurrent(Operator user);
        /// <summary>
        /// 获取当前用户
        /// </summary>
        /// <returns></returns>
        Operator Current();
        /// <summary>
        /// 删除当前用户
        /// </summary>
        void RemoveCurrent();
        /// <summary>
        /// 是否过期
        /// </summary>
        /// <returns></returns>
        bool IsOverdue();
        /// <summary>
        /// 是否已登录
        /// </summary>
        /// <returns></returns>
        int IsOnLine();
    }
}
