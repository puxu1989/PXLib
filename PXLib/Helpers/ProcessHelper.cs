using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PXLib.Helpers
{
   public  class ProcessHelper
    {
       public static string RunCmd(string cmdText)
       {
           //备份Oracle数据库命令 
           //string cmd = "D:&CD D:\\oracleDataBase\\product\\11.2.0\\dbhome_1\\BIN&Exp userid=GDWCMS/GDWCMS@ZFKJORCL owner=GDWCMS file=D:\\DATA666.dmp";
           Process process = new Process();
           process.StartInfo.FileName = "cmd.exe";
           process.StartInfo.Arguments = "/c " + cmdText;
           process.StartInfo.UseShellExecute = false;
           process.StartInfo.RedirectStandardInput = true;
           process.StartInfo.RedirectStandardOutput = true;
           process.StartInfo.RedirectStandardError = true;
           process.StartInfo.CreateNoWindow = true;
           process.Start();
           return process.StandardOutput.ReadToEnd();
       }
    }
}
