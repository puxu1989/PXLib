using System;
using System.Collections.Generic;
using System.Text;

namespace PXLib.Helpers
{
    /// <summary>
    /// 使用Random类生成伪随机数和字符串
    /// </summary>
  public  class RandomHelper
    {
        private static string randomString = "0123456789ABCDEFGHIJKMLNOPQRSTUVWXYZ";
        private static Random random = new Random(DateTime.Now.Second);

        #region 生成一个指定范围的随机整数
        /// <summary>
        /// 生成一个指定范围的随机整数，该随机数范围包括最小值，但不包括最大值
        /// </summary>
        /// <param name="minNum">最小值</param>
        /// <param name="maxNum">最大值</param>
        public static int GetRandomInt(int minNum, int maxNum)
        {
            return random.Next(minNum, maxNum);
        }
        #endregion

        #region 生成一个0.0到1.0的随机小数
        /// <summary>
        /// 生成一个0.0到1.0的随机小数
        /// </summary>
        public static double GetRandomDouble()
        {
            return random.NextDouble();
        }
        #endregion
        #region 从字符串里随机得到，规定个数的字符串
        /// <summary>
        /// 从字符串里随机得到，规定个数的字符串.
        /// </summary>
        /// <param name="allChar"></param>
        /// <param name="CodeCount"></param>
        /// <returns></returns>
        public static string GetRandomCode(string allChar, int CodeCount)
        {
            //string allChar = "1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,i,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z"; 
            string[] allCharArray = allChar.Split(',');
            string RandomCode = "";
            int temp = -1;
            Random rand = new Random();
            for (int i = 0; i < CodeCount; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(temp * i * ((int)DateTime.Now.Ticks));
                }

                int t = rand.Next(allCharArray.Length - 1);

                while (temp == t)
                {
                    t = rand.Next(allCharArray.Length - 1);
                }

                temp = t;
                RandomCode += allCharArray[t];
            }
            return RandomCode;
        }

        #endregion
        #region 产生随机字符串（数字和字母混和）
        /// <summary>
        /// 产生随机字符串（数字和字母混和）
        /// </summary>
        /// <returns>字符串</returns>
        public static string GetRandomString(int length)
        {
            string returnValue = string.Empty;
            for (int i = 0; i < length; i++)
            {
                int r = random.Next(0, randomString.Length - 1);
                returnValue += randomString[r];
            }
            return returnValue;
        }
        #endregion
        #region 生成只有数字的验证码 （如短信验证码）
        public static string GetVerificationCode(int length)
        {
            int num;
            char code;
            string checkcode = String.Empty;
            Random random = new Random();
            //用i设置验证码的字数
            for (int i = 0; i < length; i++)
            {
                num = random.Next();
                code = (char)('0' + (char)(num % 10)); //只返回数值型校验码
                //num=偶数时，用数字表示；num=奇数时，用字母表示
                //if (num % 2 == 0) { code = (char)('0' + (char)(num % 10)); }  //num%10，是为了让得的数字在0~9
                //else { code = (char)('A' + (char)(num % 26)); }  //num%26，是为了让得到的数字在0~25，满足字母A到Z的要示
                checkcode += code.ToString();
            }
            return checkcode;
        }
        #endregion
    }
}
