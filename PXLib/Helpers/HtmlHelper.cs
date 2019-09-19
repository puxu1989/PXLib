using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PXLib.Helpers
{
   public class HtmlHelper
    {
        #region HTML加解码

        /// <summary>
        /// 转换成 HTML code
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>string</returns>
        public static string EncodeHTML(string str)
        {
            str = str.Replace("&", "&amp;");
            str = str.Replace("'", "''");
            str = str.Replace("\"", "&quot;");
            str = str.Replace(" ", "&nbsp;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            str = str.Replace("\n", "<br>");
            return str;
        }
        /// <summary>
        ///解析html成 普通文本
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>string</returns>
        public static string DecodeHTML(string str)
        {
            str = str.Replace("<br>", "\n");
            str = str.Replace("&gt;", ">");
            str = str.Replace("&lt;", "<");
            str = str.Replace("&nbsp;", " ");
            str = str.Replace("&quot;", "\"");
            return str;
        }
        #endregion
        #region HTML转行成TEXT
        /// <summary>
        /// HTML转行成TEXT
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        public static string HtmlToText(string strHtml)
        {
            string[] aryReg ={
            @"<script[^>]*?>.*?</script>",
            @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
            @"([\r\n])[\s]+",
            @"&(quot|#34);",
            @"&(amp|#38);",
            @"&(lt|#60);",
            @"&(gt|#62);", 
            @"&(nbsp|#160);", 
            @"&(iexcl|#161);",
            @"&(cent|#162);",
            @"&(pound|#163);",
            @"&(copy|#169);",
            @"&#(\d+);",
            @"-->",
            @"<!--.*\n"
            };

            string newReg = aryReg[0];
            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, string.Empty);
            }

            strOutput.Replace("<", "");
            strOutput.Replace(">", "");
            strOutput.Replace("\r\n", "");


            return strOutput;
        }


        #endregion
        #region 去除HTML标记
        /// <summary>
        /// 去除HTML标记
        /// </summary>
        /// <param name="NoHTML">包括HTML的源码 </param>
        /// <returns>已经去除后的文字</returns>
        public static string NoHtml(string Htmlstring)
        {
            //删除脚本
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            //删除HTML
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&hellip;", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&mdash;", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&ldquo;", "", RegexOptions.IgnoreCase);
            Htmlstring.Replace("<", "");
            Htmlstring = Regex.Replace(Htmlstring, @"&rdquo;", "", RegexOptions.IgnoreCase);
            Htmlstring.Replace(">", "");
            Htmlstring.Replace("\r\n", "");
            Htmlstring = System.Web.HttpContext.Current.Server.HtmlEncode(Htmlstring).Trim();
            return Htmlstring;

        }
        #endregion
        #region 去掉所有HTML标签
        /// <summary>
        /// 去掉所有HTML标签并获取指定长度
        /// </summary>
        /// <param name="html"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ReplaceHtmlTag(string html, int length = 0)
        {
            string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");

            if (length > 0 && strText.Length > length)
                return strText.Substring(0, length);

            return strText;
        }
        #endregion
        #region 转换HTML为纯文本

        private static readonly string html2TextPattern =
                              @"(?<script><script[^>]*?>.*?</script>)|(?<style><style>.*?</style>)|(?<comment><!--.*?-->)" +
                              @"|(?<html><[^>]+>)" + // HTML标记
                              @"|(?<quot>&(quot|#34);)" + // 符号: "
                              @"|(?<amp>&(amp|#38);)" + // 符号: &
                              @"|(?<lt>&(lt|#60);)" + // 符号: <
                              @"|(?<gt>&(gt|#62);)" + // 符号: >
                              @"|(?<iexcl>&(iexcl|#161);)" + // 符号: (char)161
                              @"|(?<cent>&(cent|#162);)" + // 符号: (char)162
                              @"|(?<pound>&(pound|#163);)" + // 符号: (char)163
                              @"|(?<copy>&(copy|#169);)" + // 符号: (char)169
                              @"|(?<others>&(\d+);)" + // 符号: 其他
                              @"|(?<space>&nbsp;|&#160;)"; // 空格
        /// <summary>
        /// 转换HTML为纯文本
        /// </summary>
        /// <param name="html">HTML字符串</param>
        /// <param name="keepFormat">是否保留换行格式</param>
        /// <returns></returns>
        public static string HtmlToText(string html, bool keepFormat)
        {
            string pattern = html2TextPattern;
            if (!keepFormat) pattern += "|(?<control>[\r\n\\s])"; // 换行字符

            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;
            string txt = Regex.Replace(html, pattern, new MatchEvaluator(Html2Text_Match), options);

            if (!keepFormat)
                return Regex.Replace(txt.Trim(), "[\u0020]+", "\u0020", options); // 替换多个连续空格
            else
                return txt;
        }

        private static string Html2Text_Match(Match m)
        {
            if (m.Groups["quot"].Value != string.Empty)
                return "\"";
            else if (m.Groups["amp"].Value != string.Empty)
                return "&";
            else if (m.Groups["lt"].Value != string.Empty)
                return "<";
            else if (m.Groups["gt"].Value != string.Empty)
                return ">";
            else if (m.Groups["iexcl"].Value != string.Empty)
                return "\xa1";
            else if (m.Groups["cent"].Value != string.Empty)
                return "\xa2";
            else if (m.Groups["pound"].Value != string.Empty)
                return "\xa3";
            else if (m.Groups["copy"].Value != string.Empty)
                return "(c)";
            else if (m.Groups["space"].Value != string.Empty)
                return "\u0020";
            else if (m.Groups["control"].Value != string.Empty)
                return "\u0020";
            else
                return string.Empty;
        }
        #endregion
        #region 获得html的body部分
        /// <summary>
        /// 获得html的body部分
        /// </summary>
        public static string GetBodyContentFromHtml(string html)
        {
            Regex re = new Regex(@"[\s\S]*?<\bbody\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            html = re.Replace(html, "");

            re = new Regex(@"</\bbody\b[^>]*>\s*</html>", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.RightToLeft);
            html = re.Replace(html, "");

            return html;
        }
        #endregion
        #region 生成分页HTML代码
        /// <summary>
        /// 生成分页HTML代码
        /// </summary>
        /// <param name="format">当前URL格式其中分页位置用@代替</param>
        /// <param name="totalPages">总页数</param>
        /// <param name="current">当前页码</param>
        /// <param name="showCount">显示几个页码</param>
        /// <param name="ulContainerClass">分页HTML中外层ul容器的class</param>
        /// <param name="activeLiClass">当前激活状态的li的class</param>
        /// <param name="separator">模版中的页码占位符</param>
        /// <example>
        /// <code>
        ///     <div><%=PaginationHelper.GetPagination("http://www.wedn.net/cat=1&page=@&other=something",10,1,11)%></div>
        /// </code>
        /// </example>
        /// <returns>分页HTML代码</returns>
        public static string BuildPaginationHtml(string format, int totalPages, int current = 1, int showCount = 9, string ulContainerClass = "pagination", string activeLiClass = "active", char separator = '@')
        {
            var tempFormats = format.Split(separator);
            // url 前缀
            var prefix = tempFormats[0];
            // url 后缀
            var suffix = tempFormats.Length > 1 ? tempFormats[1] : string.Empty;
            // var totalPages = Math.Max((totalCount + pageSize - 1) / pageSize, 1); //总页数
            // 左右区间
            var region = (int)Math.Floor(showCount / 2.0);
            // 开始页码数
            var beginNum = current - region <= 0 ? 1 : current - region;
            // 结束页码数
            var endNum = beginNum + showCount;
            if (endNum > totalPages)
            {
                endNum = totalPages + 1;
                beginNum = endNum - showCount;
                beginNum = beginNum < 1 ? 1 : beginNum;
            }
            var pager = new StringBuilder(string.Format("<ul class=\"{0}\">\r\n", ulContainerClass));
            if (current != 1)
            {
                pager.AppendFormat("\t<li><a href=\"{1}{0}{2}\">上一页</a></li>\r\n", current - 1, prefix, suffix);
            }
            if (beginNum != 1)
            {
                pager.Append("\t<li><span>&hellip;</span></li>\r\n");
            }
            for (var i = beginNum; i < endNum; i++)
            {
                if (i != current)
                {
                    pager.AppendFormat("\t<li><a href=\"{1}{0}{2}\">{0}</a></li>\r\n", i, prefix, suffix);
                }
                else
                {
                    pager.AppendFormat("\t<li class=\"active\"><span>{0}</span></li>\r\n", current);
                }
            }
            if (endNum != totalPages + 1)
            {
                pager.Append("\t<li><span>&hellip;</span></li>\r\n");
            }
            if (current != totalPages)
            {
                pager.AppendFormat("\t<li><a href=\"{1}{0}{2}\">下一页</a></li>\r\n", current + 1, prefix, suffix);
            }
            pager.Append("</ul>");
            return pager.ToString();
        }

        public static string BuildPaginationHtml(int totalcount, int pagesize, int pageindex,int showCount, string url)//未使用测试
        {
            if (totalcount <= 0)
                return string.Empty;
            int pagecount = totalcount % pagesize == 0 ? totalcount / pagesize : totalcount / pagesize + 1;////如果能除尽就总条数除以每页数 否则+1页
            if (pagecount == 1)
                return string.Empty;

            StringBuilder sb = new StringBuilder("");

            //首页,上一页
            if (pageindex > 1)
            {
                sb.Append(@"<a href='" + string.Format(url, 1) + "'>首页</a> ");
                sb.Append(string.Format(@"<a href='" + url + "'>上一页</a> ", pageindex - 1));
            }

            int pstart = pageindex - 2;
            int pend = pageindex + 5;

            if (pend > pagecount)
                pend = pagecount;

            if (pend - pstart < showCount)
                pstart = pend - showCount;

            if (pstart < 1)
                pstart = 1;

            for (int p = pstart; p <= pend; p++)
            {
                if (p == pageindex)
                {
                    sb.Append(string.Format(@"<span>{0}</span> ", p));
                }
                else
                {
                    sb.Append(string.Format(@"<a href='" + url + "'>{1}</a> ", p, p));
                }
            }

            //下一页,尾页
            if (pageindex < pagecount)
            {
                if (pagecount - pageindex > showCount-1)
                    sb.Append(string.Format(@"<a href='" + url + "'>...{1}</a> ", pagecount, pagecount));

                sb.Append(string.Format(@"<a href='" + url + "'>下一页</a> ", pageindex + 1));
                sb.Append(string.Format(@"<a href='" + url + "'>尾页</a> ", pagecount));
            }

            return sb.ToString();
        }
        #endregion

    }
}
