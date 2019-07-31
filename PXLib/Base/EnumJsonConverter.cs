
using Newtonsoft.Json;
using PXLib.Attributes;
using PXLib.Reflection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PXLib
{
    /// <summary>
    /// 序列化转换Enum的描述信息  如果是中文枚举键 使用[StringEnumConverter]
    /// </summary>
    public class EnumJsonConverter : JsonConverter
    {
        public bool CamelCaseText
        {
            get;
            set;
        }

        public bool AllowIntegerValues
        {
            get;
            set;
        }

        public EnumJsonConverter()
        {
            this.AllowIntegerValues = true;
        }

        public EnumJsonConverter(bool camelCaseText)
            : this()
        {
            this.CamelCaseText = camelCaseText;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            Enum @enum = (Enum)value;
            string text = @enum.ToString("G");
            if (char.IsNumber(text[0]) || text[0] == '-')
            {
                writer.WriteValue(value);
                return;
            }
            //string value2 = ToEnumName(@enum.GetType(), text, this.CamelCaseText);
            string value2 = GetEnumDescription(@enum.GetType(), text);
            writer.WriteValue(value2);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Null)
            {
                bool flag = ReflectionHelper.IsNullableType(objectType);
                Type type = flag ? Nullable.GetUnderlyingType(objectType) : objectType;
                try
                {
                    if (reader.TokenType == JsonToken.String)
                    {
                        //object result = EnumUtils.ParseEnumName(reader.Value.ToString(), flag, type);
                        object result = reader.Value.ToString();
                        return result;
                    }
                    if (reader.TokenType == JsonToken.Integer)
                    {
                        if (!this.AllowIntegerValues)
                        {
                            //throw new Exception("Integer value {0} is not allowed.".FormatWith(CultureInfo.InvariantCulture, reader.Value));
                        }
                        //object result = ConvertUtils.ConvertOrCast(reader.Value, CultureInfo.InvariantCulture, type);
                        object result = reader.Value;
                        return result;
                    }
                }
                catch (Exception)
                {
                    //throw new Exception("Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, FormatValueForPrint(reader.Value), objectType), ex);
                }
                //throw new Exception("Unexpected token {0} when parsing enum.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }
            if (!ReflectionHelper.IsNullableType(objectType))
            {
                //throw new Exception("Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
            }
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return (ReflectionHelper.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType).IsEnum;
        }
        internal static string FormatValueForPrint(object value)
        {
            if (value == null)
            {
                return "{null}";
            }
            if (value is string)
            {
                return "\"" + value + "\"";
            }
            return value.ToString();
        }
        private string GetEnumDescription(Type type, string value)
        {
            FieldInfo field = type.GetField(value);
            if (field == null)
            {
                return value;
            }
            var desc = Attribute.GetCustomAttribute(field, typeof(EnumText)) as EnumText;
            if (desc != null)
                return desc.Description;
            return value;

        }

    }
}
