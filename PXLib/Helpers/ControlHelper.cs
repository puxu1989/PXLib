using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PXLib.Helpers
{
    public class ControlHelper
    {
        public static void SetWebControls<T>(Control page, T model)
        {
            SetWebControls(page, DataConvertHelper.ModelToHashtable(model));
        }
        public static void SetWebControls(Control page, Hashtable ht)
        {
            if (ht != null && ht.Count != 0)
            {
                int size = ht.Keys.Count;
                foreach (string key in ht.Keys)
                {
                    object val = ht[key];
                    if (val != null)
                    {
                        Control control = page.FindControl(key);
                        if (control == null) continue;
                        if (control is HtmlInputText)
                        {
                            HtmlInputText txt = (HtmlInputText)control;
                            txt.Value = val.ToString().Trim();
                        }
                        if (control is TextBox)//新加
                        {
                            TextBox txt = (TextBox)control;
                            txt.Text = val.ToString().Trim();
                        }
                        if (control is HtmlSelect)
                        {
                            HtmlSelect txt = (HtmlSelect)control;
                            txt.Value = val.ToString().Trim();
                        }
                        if (control is HtmlInputHidden)
                        {
                            HtmlInputHidden txt = (HtmlInputHidden)control;
                            txt.Value = val.ToString().Trim();
                        }
                        if (control is HtmlInputPassword)
                        {
                            HtmlInputPassword txt = (HtmlInputPassword)control;
                            txt.Value = val.ToString().Trim();
                        }
                        if (control is Label)
                        {
                            Label txt = (Label)control;
                            txt.Text = val.ToString().Trim();
                        }
                        if (control is HtmlInputCheckBox)
                        {
                            HtmlInputCheckBox chk = (HtmlInputCheckBox)control;
                            chk.Checked = val.ToInt() == 1 ? true : false;
                        }
                        if (control is HtmlTextArea)
                        {
                            HtmlTextArea area = (HtmlTextArea)control;
                            area.Value = val.ToString().Trim();
                        }
                    }
                }
            }
        }
        public static Hashtable GetWebControls(Control page)
        {
            Hashtable ht = new Hashtable(System.StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, true));//不区分大小写，便于模型绑定
            int size = HttpContext.Current.Request.Params.Count;
            for (int i = 0; i < size; i++)
            {
                string id = HttpContext.Current.Request.Params.GetKey(i);
                Control control = page.FindControl(id);
                if (control == null) continue;

                if (control is HtmlInputText)
                {
                    HtmlInputText txt = (HtmlInputText)control;
                    ht[txt.ID] = txt.Value.Trim();
                }
                if (control is TextBox) //新加服务端控件 还有其他控件
                {
                    TextBox txt = (TextBox)control;
                    ht[txt.ID] = txt.Text.Trim();
                }
                if (control is HtmlSelect)
                {
                    HtmlSelect txt = (HtmlSelect)control;
                    //for (int j = 0; j < txt.Items.Count; j++)
                    //{
                    //    if (txt.Items[j].Selected)
                    //    {
                    //        ht[txt.ID] =txt.Items[j].Text;
                    //    }
                    //}
                    ht[txt.ID] = txt.Value.Trim();
                }

                if (control is HtmlInputHidden)
                {
                    HtmlInputHidden txt = (HtmlInputHidden)control;
                    ht[txt.ID] = txt.Value.Trim();
                }
                if (control is HtmlInputPassword)
                {
                    HtmlInputPassword txt = (HtmlInputPassword)control;
                    ht[txt.ID] = txt.Value.Trim();
                }
                if (control is HtmlInputCheckBox)
                {
                    HtmlInputCheckBox chk = (HtmlInputCheckBox)control;
                    ht[chk.ID] = chk.Checked ? 1 : 0;
                }
                if (control is HtmlTextArea)
                {
                    HtmlTextArea area = (HtmlTextArea)control;
                    ht[area.ID] = area.Value.Trim();
                }
            }
            return ht;
        }
        public static T GetWebControls<T>(Control page)
        {

            Hashtable ht = GetWebControls(page);
            T model = Activator.CreateInstance<T>();
            Type type = model.GetType();
            //遍历每一个属性
            foreach (PropertyInfo prop in type.GetProperties())
            {
                object value = ht[prop.Name];
                if (prop.PropertyType.ToString() == "System.Nullable`1[System.DateTime]")
                {
                    if (value != null && value.ToString() != "")
                        value = DateTime.Parse(value.ToString());
                }
                prop.SetValue(model, DataConvertHelper.ChangeType(value, prop.PropertyType), null);
            }
            return model;
        }

        #region 下拉绑定

        /// <summary>
        /// 绑定IList:HtmlSelect下拉列表框 
        /// </summary>
        /// <param name="list">list</param>
        /// <param name="select">控件名称</param>
        /// <param name="_Name">绑定字段名称</param>
        /// <param name="_ID">绑定字段主键</param>
        /// <param name="_Memo">默认显示值</param>
        public static void BindSelectHtml(IList list, HtmlSelect select, string _Name, string _ID, string _Memo)
        {
            select.DataSource = list;
            select.DataTextField = _Name;
            select.DataValueField = _ID;
            select.DataBind();
            if (!string.IsNullOrEmpty(_Memo.Trim()))
            {
                select.Items.Insert(0, new ListItem(_Memo, ""));
            }
        }
        /// <summary>
        /// 将枚举类型绑定到下拉框
        /// </summary>
        /// <param name="select">控件id</param>
        /// <param name="enumType">枚举类型</param>
        /// <param name="_Memo">默认</param>
        public static void BindSelectHtml(HtmlSelect select, Type enumType, string _Memo)
        {
            foreach (int index in Enum.GetValues(enumType))
            {
                string name = Enum.GetName(enumType, index);//枚举名字   
                string value = index.ToString();//枚举值 
                //ListBox 也可用这个方法  
                ListItem item = new ListItem();
                item.Text = name;
                item.Value = value;
                select.Items.Add(item);
            }
            if (!string.IsNullOrEmpty(_Memo.Trim()))
            {
                select.Items.Insert(0, new ListItem(_Memo, "-1"));
            }
        }
        /// <summary>
        /// 绑定下拉最新版 需配置T_Itme数据表
        /// </summary>
        /// <param name="select"></param>
        /// <param name="_Memo"></param>
        public static void BindSelectHtml(DataTable dt, HtmlSelect select, string _Memo)
        {
            if (dt == null || dt.Rows.Count <= 0)
            {
                return;
            }
            DataRow[] drArr = dt.Select("ControlsId='" + select.ID + "'");
            if (drArr != null && drArr.Length > 0)
            {
                for (int i = 0; i < drArr.Length; i++)
                {
                    ListItem item = new ListItem();
                    item.Text = drArr[i]["ItemValue"].ToString();
                    item.Value = drArr[i]["ItemValue"].ToString(); ;
                    select.Items.Add(item);
                }
            }
            if (!string.IsNullOrEmpty(_Memo.Trim()))
                select.Items.Insert(0, new ListItem(_Memo, "-1"));
        }
        /// <summary>
        /// 根据sql查询高效的DataReader绑定下拉 
        /// </summary>
        //public static void BindSelect(HtmlSelect select,string sql,string textStr,string valueStr, string _Memo)
        //{
        //    OracleDataReader myReader = OrclDBHelper.ExecuteDataReader(sql);
        //    select.Items.Clear();
        //    while (myReader.Read())
        //    {
        //        ListItem item = new ListItem();
        //        item.Text = myReader[textStr].ToString();
        //        item.Value = myReader[valueStr].ToString();
        //        select.Items.Add(item);
        //    }
        //    myReader.Close();
        //    if (!string.IsNullOrEmpty(_Memo.Trim()))
        //        select.Items.Insert(0, new ListItem(_Memo, "-1"));
        //}
        #endregion
    }
}
