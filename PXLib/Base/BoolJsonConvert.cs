using Newtonsoft.Json;
using PXLib.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib
{
    /// <summary>
    ///  [JsonConverter(typeof(BoolJsonConvert))]
    /// </summary>
    public class BoolJsonConvert : JsonConverter
    {
        private string[] arrBString { get; set; }

        public BoolJsonConvert()
        {
            this.arrBString = "是,否".Split(',');
        }

        /// <summary>
        /// 构造函数 (用不来)
        /// </summary>
        /// <param name="BooleanString">将bool值转换成的字符串值</param>
        public BoolJsonConvert(string BooleanString)
        {
            if (string.IsNullOrEmpty(BooleanString))
            {
                throw new ArgumentNullException();
            }
            this.arrBString = BooleanString.Split(',');
            if (arrBString.Length != 2)
            {
                throw new ArgumentException("BooleanString格式不符合规定");
            }
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool isNullable = IsNullableType(objectType);
            Type t = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

            if (reader.TokenType == JsonToken.Null)
            {
                if (!IsNullableType(objectType))
                {
                    throw new Exception(string.Format("不能转换null value to {0}.", objectType));
                }

                return null;
            }

            try
            {
                if (reader.TokenType == JsonToken.String)
                {
                    string boolText = reader.Value.ToString();
                    if (boolText.Equals(arrBString[0], StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else if (boolText.Equals(arrBString[1], StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                if (reader.TokenType == JsonToken.Integer)
                {
                    //数值
                    return Convert.ToInt32(reader.Value) == 1;
                }
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Error converting value {0} to type '{1}'", reader.Value, objectType));
            }
            throw new Exception(string.Format("Unexpected token {0} when parsing enum", reader.TokenType));
        }

        /// <summary>
        /// 判断是否为Bool类型
        /// </summary>
        /// <param name="objectType">类型</param>
        /// <returns>为bool类型则可以进行转换</returns>
        public override bool CanConvert(Type objectType)
        {
            return true;
        }


        public bool IsNullableType(Type objectType)
        {
            return (ReflectionHelper.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType).IsEnum;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            bool bValue = (bool)value;

            if (bValue)
            {
                writer.WriteValue(arrBString[0]);
            }
            else
            {
                writer.WriteValue(arrBString[1]);
            }
        }
    }
}
