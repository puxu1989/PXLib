using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PXLib.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PXLib.Api
{
    //http://wthrcdn.etouch.cn/weather_mini?city=%E6%AF%95%E8%8A%82%E5%B8%82
    //https://api.seniverse.com/v3/weather/now.json?key=ibcgo1cf0f9ysvyu&location=%E8%B4%B5%E9%98%B3&language=zh-Hans&unit=c
   public class WeatherApi
    {
       public static string GetWeatherInfo(string cityName) 
       {
          string serverUrl = string.Format("https://api.seniverse.com/v3/weather/now.json?key=ibcgo1cf0f9ysvyu&location={0}&language=zh-Hans&unit=c", cityName);
           //string serverUrl = string.Format("http://wthrcdn.etouch.cn/weather_mini?city={0}", cityName);

           string res = RequestHelper.GetWebRequest(serverUrl);
           return res;
       }
       public static string Dome1(string cityName) 
       {
           var jobj = GetWeatherInfo(cityName).ToJObject();
           JArray ja = (JArray)jobj.GetValue("results");
           string mowtext = ja[0]["now"].ToString();
           var cloudysky = mowtext.ToJObject().GetValue("text").ToString();
           var temperature = mowtext.ToJObject().GetValue("temperature").ToString();
           string texts = string.Format("{0}，{1}天气预报，气温{2}℃，{3}", DateTime.Now.ToString("yyyy年MM月dd日"), cityName, temperature, cloudysky);
           return texts;
       }
    }
}
