using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using QuanLyPhongGym.Controller;
using Newtonsoft.Json;
using QuanLyPhongGym.Model;
using System.Web;
using OfficeOpenXml;
using System.Web.UI;
using QuanLyPhongGym.Pages;

namespace QuanLyPhongGym.Areas
{
    public class CmmFunc
    {
        protected string key = ConfigurationManager.AppSettings["KeyCrypt"];
        DBController DbBase = new DBController();

        public static DataTable ConvertToTableParams(Dictionary<string, object> objectData)
        {
            DataTable retTable = new DataTable();
            try
            {
                retTable.Columns.Add("ID", typeof(int));
                retTable.Columns.Add("Name", typeof(string));
                retTable.Columns.Add("DataType", typeof(string));
                retTable.Columns.Add("Value", typeof(string));

                if (objectData != null)
                {
                    int index = 1;
                    foreach (KeyValuePair<string, object> keyVal in objectData)
                    {
                        DataRow dr = retTable.NewRow();
                        dr["ID"] = index;
                        dr["Name"] = keyVal.Key;
                        dr["DataType"] = string.Empty;
                        object value = keyVal.Value;
                        if (IsList(value))
                            dr["Value"] = JsonConvert.SerializeObject(value);
                        else
                            dr["Value"] = value + string.Empty;
                        retTable.Rows.Add(dr);
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                CmmFunc.TrackLogSql($"Method: ConvertToTableParams - File: TT.WF_DCMOP.Areas.Areas.CmmFunc", ex + string.Empty);
            }

            return retTable;
        }

        public static ExpandoObject CreateExpandoObject(params (string Key, object Value)[] properties)
        {
            var expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)expando;

            foreach (var (key, value) in properties)
            {
                dictionary[key] = value;
            }

            return expando;
        }

        public bool FieldRequire(List<dynamic> lstRequire)
        {
            foreach (dynamic obj in lstRequire)
            {
                if (string.IsNullOrEmpty(obj.KeyObj))
                {
                    MessageBox.Show($"Trường '{obj.Key}' là bắt buộc nhập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    return false;
                }
                else if (obj.IsValidate != null && !obj.IsValidate)
                {
                    MessageBox.Show(obj.ValidateText, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    return false;
                }
            }
            return true;
        }

        public static bool IsList(object value)
        {
            if (value == null) return false;
            return (value is Dictionary<string, object> || value is List<object>);
        }

        /// <summary>
        ///  Set giá trị cho thuộc tính của Object
        /// </summary>
        /// <param name="obj">Object muốn set giá trị</param>
        /// <param name="propInfo">Thuộc tính propertyInfo thuộc Class Object</param>
        /// <param name="value">Giá trị muốn set</param>
        /// <param name="isTry">Có sử dụng try catch trong hàm</param>
        /// <returns></returns>
        public static bool SetPropertyValue(object obj, PropertyInfo propInfo, object value, bool isTry = true)
        {
            // có sử dụng try catch
            if (isTry)
            {
                try
                {
                    Type t = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;

                    object safeValue = (value == null) ? null : Convert.ChangeType(value, t);
                    propInfo.SetValue(obj, safeValue, null);
                    return true;
                }
                catch (Exception ex)
                {
                    CmmFunc.TrackLogSql($"Method: SetPropertyValue - File: TT.WF_DCMOP.Areas.Areas.CmmFunc", ex + string.Empty);
                }
            }
            else
            {
                Type t = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;

                object safeValue = (value == null) ? null : Convert.ChangeType(value, t);
                propInfo.SetValue(obj, safeValue, null);
                return true;
            }
            return false;
        }

        public static bool ToBoolean(string value)
        {
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }

            return false;
        }

        /// <summary>
        /// Lấy giá trị Property bằng Tên Property
        /// </summary>
        /// <param name="obj">Object chứa property muốn lấy giá trị</param>
        /// <param name="proName">Tên thuộc tính muốn lấy giá trị</param>
        /// <returns></returns>
        public static object GetPropertyValueByName(object obj, string proName)
        {
            object retValue = null;
            Type type = obj.GetType();
            var perInfo = type.GetProperty(proName);
            if (perInfo != null)
            {
                retValue = perInfo.GetValue(obj);
            }
            return retValue;
        }

        /// <summary>
        /// Get Property của Object dựa vào tên
        /// </summary>
        /// <param name="obj">Đối tường chứa property</param>
        /// <param name="strPropName">Tên Property</param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(object obj, string strPropName)
        {
            Type type = obj.GetType();
            return type.GetProperty(strPropName);
        }

        /// <summary>
        /// Lấy giá trị Property bằng Property
        /// </summary>
        /// <param name="obj">Object chứa property muốn lấy giá trị</param>
        /// <param name="perInfo">Tên thuộc tính muốn lấy giá trị</param>
        /// <returns>Giá trị của Property trong Object</returns>
        public static object GetPropertyValue(object obj, PropertyInfo perInfo)
        {
            object retValue = null;
            if (perInfo != null)
            {
                retValue = perInfo.GetValue(obj);
            }
            return retValue;
        }

        /// <summary>
        /// Get Setting với Default Value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetConfigWithDefault(string key, string defaultValue = "")
        {
            string retValue;
            try
            {
                retValue = ConfigurationManager.AppSettings[key] ?? defaultValue;
            }
            catch
            {
                retValue = defaultValue;
            }
            return retValue;
        }

        /// <summary>
        /// Lấy giá trị Property bằng Tên Property
        /// </summary>
        /// <param name="obj">Object chứa property muốn lấy giá trị</param>
        /// <param name="strPropName">Tên thuộc tính muốn lấy giá trị</param>
        /// <returns></returns>
        public static object TryGetPropertyValueByName(object obj, string propName, bool getChoiceText = false)
        {
            object Value = null;
            try
            {
                object CurrentObj = obj;
                Type type = null;
                Type PropertyType = null;
                DataSourceAttribute attr = null;

                if (string.IsNullOrEmpty(propName)) return Value;

                // Danh sách Property
                string[] Properties = propName.Split('.');

                string FieldName = Properties[0];

                // Kiểm tra có chưa ngoặc vuông không Nếu có thì tính Value bằng obj Index trong ngoặc vuông. Ex: ListAppCustomerAddress[0].CIF
                System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(@"\[\d+\]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                System.Text.RegularExpressions.MatchCollection matches = rgx.Matches(FieldName);

                // Có ngoặc vuông
                if (matches != null && matches.Count > 0)
                {
                    // Matches[0] chính là [số]. Ex: [0]
                    var firstMatch = matches[0];
                    // Substring để lấy tên thuộc tính. Ex: ListAppCustomerAddress
                    FieldName = FieldName.Substring(0, FieldName.Length - firstMatch.Length);

                    // Lấy Object obj.ListAppCustomerAddress ra
                    CurrentObj = TryGetPropertyValueByName(CurrentObj, FieldName);

                    if (CurrentObj == null) return null;

                    Type CurrentObj_Type = CurrentObj.GetType();

                    // Nếu Object này không phải là List thì return null luôn
                    if (!CurrentObj_Type.GetInterfaces().Contains(typeof(IList))) return null;

                    // Lấy số bên trong [số] ra để lấy (Int) của ItemIndex. Ex: 0
                    rgx = new System.Text.RegularExpressions.Regex(@"\d+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    matches = rgx.Matches(firstMatch.Value);
                    int ItemIndex = int.Parse(matches[0].Value);

                    // Gán kiểu về kiểu List và lấy Item thứ ItemIndex ra làm Value trả về
                    IList ListValues = ((IList)CurrentObj);
                    if (ListValues == null || ListValues.Count < (ItemIndex + 1)) return Value;
                    Value = ListValues[ItemIndex];
                    PropertyType = Value.GetType();
                }
                else
                {
                    //  Không có ngoặc vuông
                    type = CurrentObj.GetType();
                    System.Reflection.PropertyInfo perInfo = type.GetProperty(FieldName);

                    if (perInfo != null)
                    {
                        PropertyType = perInfo.PropertyType;
                        Value = perInfo.GetValue(CurrentObj);
                        attr = perInfo.GetCustomAttribute<DataSourceAttribute>();
                    }
                    else
                    {
                        return null;
                    }
                    // propName chỉ có 1 Property thì return hoặc Value get được bằng null thì return lun
                    if (Properties.Length == 1 && attr == null)
                        return Value;
                }
                if (PropertyType == null) return Value;

                // Lấy tên thuộc tính còn lại. Ex: CIF.
                string extendPropName = string.Join(".", Properties, 1, Properties.Length - 1);

                // Default kiểu là object không kế thừa từ BeanBase
                int ObjValueType = 3;
                // Value là 1 List
                if (PropertyType.GetInterfaces().Contains(typeof(IList))) ObjValueType = 1;
                // Value là Bean kế thừa từ BeanBase
                else if (PropertyType.BaseType.Equals(typeof(ControllerModel))) ObjValueType = 2;

                switch (ObjValueType)
                {
                    case 1: //List
                        {
                            IList ListValues = ((IList)Value);
                            List<object> ValueReturn = new List<object>();
                            if (ListValues == null) return null;

                            // Lấy danh sách giá trị của 1 danh sách đối tượng theo 1 thuộc tính
                            foreach (var item in ListValues)
                            {
                                object itemValue = TryGetPropertyValueByName(item, extendPropName, getChoiceText);
                                ValueReturn.Add(itemValue);
                            }
                            Value = ValueReturn;
                            break;
                        }
                    case 2: //Bean kế thừa từ BeanBase
                        {
                            // Lấy giá trị mặc định của đối tượng bị null
                            if (Value == null && Properties.Length > 1)
                            {
                                object ChildObj = Activator.CreateInstance(PropertyType);
                                Value = TryGetPropertyValueByName(ChildObj, extendPropName, getChoiceText);
                                break;
                            }

                            Value = TryGetPropertyValueByName(Value, extendPropName, getChoiceText);
                            break;
                        }
                }
            }
            catch { }
            return Value;
        }

        public static void TrackLogSql(string title = "", string message = "", string category = "")
        {
            try
            {
                List<SqlParameter> lstPara = new List<SqlParameter>
                {
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Error", message),
                    new SqlParameter("@Category", category)
                };
                new DBController().Execute("DCMOP_TrackingError_Insert", lstPara);
            }
            catch { }
        }

        public void RedirectUrl(string urlRedirect, HttpContext context, bool endResonse = true)
        {
            if (!urlRedirect.StartsWith("http"))
            {
                if (context.Request.QueryString["IsDlg"] + string.Empty != string.Empty)
                {
                    if (urlRedirect.IndexOf('?') > -1)
                    {
                        urlRedirect += "&IsDlg=1";
                    }
                    else
                    {
                        urlRedirect += "?IsDlg=1";
                    }
                }
                context.Response.Redirect(urlRedirect, endResonse);
            }
            else
            {
                context.Response.Redirect("~/AccessDenied.aspx", endResonse);
            }
        }

        public T ConvertToModel<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var obj = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            if (pro.PropertyType.Name == "Boolean" && row[pro.Name] + string.Empty == "1")
                                pro.SetValue(obj, true);
                            else
                                pro.SetValue(obj, row[pro.Name]);
                        }
                        catch (Exception ex)
                        {
                            CmmFunc.TrackLogSql($"Method: ConvertToModel - File: TT.WF_DCMOP.Areas.Areas.CmmFunc", ex + string.Empty);
                        }
                    }
                }
                return obj;
            }).First();
        }

        /// Lấy ngày từ điều kiện Search
        /// </summary>
        /// <param name="strDate">ngày kiểu text</param>
        /// <param name="outDate">dữ liệu ngày convert thành công</param>
        /// <returns></returns>
        public static bool GetDateSearch(string strDate, ref DateTime outDate)
        {
            bool retValue = false;
            try
            {
                if (strDate.ToLower().Contains("day"))
                {
                    string[] arrDate = strDate.Split(' ');
                    if (arrDate.Length == 2)
                    {
                        int num = int.Parse(arrDate[0]);
                        num -= 1;
                        if (num > 0)
                        {
                            outDate = DateTime.Now.AddDays(num * -1);
                        }
                        else
                        {
                            outDate = DateTime.Now;
                        }
                        retValue = true;
                    }
                }
                else if (strDate.ToLower().Contains("week"))
                {
                    string[] arrDate = strDate.Split(' ');
                    if (arrDate.Length == 2)
                    {
                        int num = int.Parse(arrDate[0]);
                        num = num * 7;
                        num -= 1;
                        outDate = num > 0 ? DateTime.Now.AddDays(num * -1) : DateTime.Now;
                        retValue = true;
                    }
                }
                else if (strDate.ToLower().Contains("month"))
                {
                    string[] arrDate = strDate.Split(' ');
                    if (arrDate.Length == 2)
                    {
                        int num = int.Parse(arrDate[0]);
                        outDate = num > 0 ? DateTime.Now.AddMonths(num * -1) : DateTime.Now;
                        retValue = true;
                    }
                }
                else if (strDate.ToLower().Contains("year"))
                {
                    string[] arrDate = strDate.Split(' ');
                    if (arrDate.Length == 2)
                    {
                        int num = int.Parse(arrDate[0]);
                        outDate = num > 0 ? DateTime.Now.AddYears(num * -1) : DateTime.Now;
                        retValue = true;
                    }
                }
                else
                {
                    retValue = DateTime.TryParse(strDate, out outDate);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return retValue;
        }

        /// <summary>
        /// Convert DataTable to List Model
        /// </summary>
        public List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var obj = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            pro.SetValue(obj, row[pro.Name]);
                        }
                        catch (Exception ex)
                        {
                            CmmFunc.TrackLogSql($"Method: ConvertToList - File: TT.WF_DCMOP.Areas.Areas.CmmFunc", ex + string.Empty);
                        }
                    }
                }
                return obj;
            }).ToList();
        }

        /// <<summary>
        /// Giải mã chuỗi Json thành Object và bỏ qua Catch
        /// </summary>
        /// <typeparam name="T">Kiểu đối tượng muốn chuyển đổi thành</typeparam>
        /// <param name="value">Giá trị chuỗi Json</param>
        /// <returns></returns>
        public static T TryDeserializeObject<T>(string value)
        {
            T retValue = default(T);
            try
            {
                retValue = JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                CmmFunc.TrackLogSql($"Method: TryDeserializeObject - File: TT.WF_DCMOP.Areas.Areas.CmmFunc", ex + string.Empty);
            }
            return retValue;
        }

        public void ExportExcelByAPI(string strFileTemplate, List<DataTable> DataTables, string Options = "", string extension = ".xlsx", string fileName = "", HttpContext context = null, bool IsLinkUrl = true, int LanguageId = 1066)
        {
            try
            {
                List<SqlParameter> lstParam = new List<SqlParameter>();
                lstParam.Add(new SqlParameter("LanguageId", "1066"));
                DataTable retData = DbBase.Fill("Bos_MySQL_MenuFrontendSettings_Controller_get", lstParam);

                DataTables.Add(retData);

                var templateFilePath = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath + "/Scripts/ExcelTemplate/" + strFileTemplate + ".xlsx");
                HttpRequest request = HttpContext.Current.Request;
                byte[] fileBinary;
                using (Stream stream = File.OpenRead(templateFilePath))
                {
                    ExcelPackage pkg = new ExcelPackage(stream);
                    var wsSource = pkg.Workbook.Worksheets[0];
                    if (DataTables != null && DataTables.Count > 0)
                    {
                        var IndexStart = 0;
                        var IsIndex = true;
                        DataTable dataTable = DataTables[0];
                        for (var i = 0; i < dataTable.Rows.Count; i++)
                        {
                            var item = dataTable.Rows[i];
                            var cells = wsSource.Cells;
                            var dictionary = cells
                                .GroupBy(c => new { c.Start.Row, c.Start.Column })
                                .ToDictionary(
                                    rcg => new KeyValuePair<int, int>(rcg.Key.Row, rcg.Key.Column),
                                    rcg => cells[rcg.Key.Row, rcg.Key.Column].Value
                                 )
                                .Where(rcg => rcg.Value.ToString().StartsWith("[") && rcg.Value.ToString().EndsWith("]"));


                            foreach (var kvp in dictionary)
                            {
                                if (i < dataTable.Rows.Count - 1)
                                    wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Copy(wsSource.Cells[kvp.Key.Key + 1, kvp.Key.Value]);

                                var itemColumn = kvp.Value.ToString().Replace("[", "").Replace("]", "");

                                if (itemColumn.Contains("|"))
                                {
                                    if (IsIndex)
                                    {
                                        IndexStart = kvp.Key.Key;
                                        IsIndex = false;
                                    }

                                    var StrArr = itemColumn;
                                    var ArrValue = StrArr.Split('|');
                                    for (var t = 0; t < ArrValue.Length - 1; t++)
                                    {
                                        var ItemValue = item[ArrValue[0]] != null ? item[ArrValue[0]] + "" : ""; // default value column
                                        var StrValue = (ArrValue[t + 1] + "").Split('#');
                                        var Func = StrValue[0];
                                        switch (Func)
                                        {
                                            case "AddLink":
                                                {
                                                    var ValueFormat = item[StrValue[1]] != null ? item[StrValue[1]] : "";
                                                    wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Value = ItemValue;
                                                    wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Hyperlink = new Uri(request.Url.Scheme + @"://" + request.Url.Authority + ValueFormat);
                                                    wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Style.Font.UnderLine = true;
                                                    wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#049BEB"));
                                                    // Replace Đ thành Đ
                                                    wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Value = (wsSource.Cells[kvp.Key.Key, kvp.Key.Value] + "").Replace("Ð", "Đ");
                                                    break;
                                                };
                                            case "Merge": // merge theo điều kiện của văn bản (đặc thù)
                                                {
                                                    if (i > 0)
                                                    {
                                                        var ValueFormat = item[StrValue[1]] != null ? item[StrValue[1]] : "";
                                                        var CellBeforeVal = wsSource.Cells[kvp.Key.Key - 1, kvp.Key.Value].Value + "";
                                                        var dataBefore = dataTable.Rows[i - 1][StrValue[1] + ""] + "";
                                                        if (kvp.Key.Key > IndexStart && CellBeforeVal.Equals(ItemValue) && ValueFormat.Equals(dataBefore))
                                                            wsSource.Cells[kvp.Key.Key - 1, kvp.Key.Value, kvp.Key.Key, kvp.Key.Value].Merge = true;
                                                        else
                                                            wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Value = ItemValue;
                                                    }
                                                    break;
                                                };
                                            case "FormatDate":
                                                {
                                                    if (LanguageId == 1066)
                                                        wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Value = !string.IsNullOrEmpty(ItemValue + string.Empty) ? DateTime.Parse(ItemValue + string.Empty, CultureInfo.CurrentUICulture, DateTimeStyles.None).ToString(StrValue[1] + "") : string.Empty;
                                                    else
                                                        wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Value = !string.IsNullOrEmpty(ItemValue + string.Empty) ? DateTime.Parse(ItemValue + string.Empty, CultureInfo.CurrentUICulture, DateTimeStyles.None).ToString(StrValue[1] + "") : string.Empty;
                                                    break;
                                                };
                                        }
                                        continue;
                                    }
                                }
                                if ((!dataTable.Columns.Contains(itemColumn) && !itemColumn.Equals("STT")) || itemColumn.Contains("|"))
                                {
                                    continue;
                                }
                                if (itemColumn.Equals("STT"))
                                {
                                    wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Value = i + 1;
                                    continue;
                                }
                                wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Value = item[itemColumn];
                            }
                        }
                    }
                    pkg.Save();
                    fileBinary = ReadToEnd(pkg.Stream);
                }

                if (fileBinary != null)
                {
                    if (string.IsNullOrEmpty(fileName))
                        fileName = DateTime.Today.Year + "." + DateTime.Today.Month.ToString("00") + "." + DateTime.Today.Day.ToString("00") + " ExportExcel";

                    HttpResponse response = HttpContext.Current.Response;
                    response.Clear();
                    response.Charset = string.Empty;
                    response.BinaryWrite(fileBinary);
                    response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    response.AddHeader("Content-Disposition", "attachment;filename=\"" + fileName + extension + "\"");
                    response.Flush();
                    response.End();
                }
            }
            catch (Exception ex)
            {
                CmmFunc.TrackLogSql($"Method: ExportExcelByAPI - File: TT.WF_DCMOP.Areas.Areas.CmmFunc", ex + string.Empty);
            }
        }

        public void ExportExcel(string strFileTemplate, List<DataTable> DataTables, string Options = "", string extension = ".xlsx", string fileName = "", bool IsLinkUrl = true, int LanguageId = 1066)
        {
            try
            {
                var templateFilePath = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath + "/Scripts/ExcelTemplate/" + strFileTemplate + ".xlsx");
                HttpRequest request = HttpContext.Current.Request;
                byte[] fileBinary;
                using (Stream stream = File.OpenRead(templateFilePath))
                {
                    ExcelPackage pkg = new ExcelPackage(stream);
                    var wsSource = pkg.Workbook.Worksheets[0];
                    if (DataTables != null && DataTables.Count > 0)
                    {
                        DataTable dataTable = DataTables[0];
                        for (var i = 0; i < dataTable.Rows.Count; i++)
                        {
                            var item = dataTable.Rows[i];
                            var cells = wsSource.Cells;
                            var dictionary = cells
                                .GroupBy(c => new { c.Start.Row, c.Start.Column })
                                .ToDictionary(
                                    rcg => new KeyValuePair<int, int>(rcg.Key.Row, rcg.Key.Column),
                                    rcg => cells[rcg.Key.Row, rcg.Key.Column].Value
                                 )
                                .Where(rcg => rcg.Value.ToString().StartsWith("[") && rcg.Value.ToString().EndsWith("]"));


                            foreach (var kvp in dictionary)
                            {
                                if (i < dataTable.Rows.Count - 1)
                                    wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Copy(wsSource.Cells[kvp.Key.Key + 1, kvp.Key.Value]);

                                var itemColumn = kvp.Value.ToString().Replace("[", "").Replace("]", "");

                                if (itemColumn.Equals("STT"))
                                {
                                    wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Value = i + 1;
                                    continue;
                                }
                                wsSource.Cells[kvp.Key.Key, kvp.Key.Value].Value = item[itemColumn];
                            }
                        }
                    }
                    pkg.Save();
                    fileBinary = ReadToEnd(pkg.Stream);
                }

                if (fileBinary != null)
                {
                    if (string.IsNullOrEmpty(fileName))
                        fileName = DateTime.Today.Year + "." + DateTime.Today.Month.ToString("00") + "." + DateTime.Today.Day.ToString("00") + " ExportExcel";

                    HttpResponse response = HttpContext.Current.Response;
                    response.Clear();
                    response.Charset = string.Empty;
                    response.BinaryWrite(fileBinary);
                    response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    response.AddHeader("Content-Disposition", "attachment;filename=\"" + fileName + extension + "\"");
                    response.Flush();
                    response.End();
                }
            }
            catch (Exception ex)
            {
                CmmFunc.TrackLogSql($"Method: ExportExcel - File: TT.WF_DCMOP.Areas.Areas.CmmFunc", ex + string.Empty);
            }
        }

        /// <param name="key"></param>
        /// <param name="defaultValue">Giá trị default</param>
        /// <returns></returns>
        public string GetSettingByKey(string key, string defaultValue = null)
        {
            string retValue = defaultValue;
            try
            {
                DataTable data = DbBase.Select(@"SELECT TOP 1 [Value] FROM dbo.Settings WHERE [Key] = '" + key + "'");
                if (data.Rows.Count > 0)
                    retValue = data.Rows[0]["Value"] + string.Empty;
            }
            catch (Exception ex)
            {
                CmmFunc.TrackLogSql($"Method: GetSettingByKey - File: TT.WF_DCMOP.Areas.Areas.CmmFunc", ex + string.Empty);
            }

            return retValue;
        }

        public static string ServerMapPath(string path)
        {
            string retValue = path.TrimStart('/');
            if (path.IndexOf(":", StringComparison.Ordinal) < 0 && !path.StartsWith("\\\\"))
            {
                retValue = HttpContext.Current.Server.MapPath("~/" + retValue);
            }
            return retValue;
        }

        private byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            System.Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            System.Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    System.Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public void ShowMessageBox(string message)
        {
            MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public BeanEmployee getDataLogin(Form formCurrent, Form form)
        {
            BeanEmployee employee = new BeanEmployee();
            try
            {

                if (!UserSession.Instance.IsLoggedIn)
                {
                    form.Show();
                    formCurrent.Hide();
                }
                else
                {
                    TokenService tokenService = new TokenService();
                    if (!tokenService.ValidateToken(UserSession.Instance.Token))
                    {
                        UserSession.Instance.Logout();
                        form.Show();
                        formCurrent.Hide();
                    }

                    employee.Id = UserSession.Instance.Id;
                    employee.UserName = UserSession.Instance.UserName;
                    employee.Role = UserSession.Instance.Role;
                }
            }
            catch (Exception ex)
            {
                CmmFunc.TrackLogSql($"Method: getDataLogin - File: TT.WF_DCMOP.Areas.Areas.CmmFunc", ex + string.Empty);
            }
            return employee;
        }

        public string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public string Utf8RemoveAccents(string str, bool toLowerCase = false)
        {
            if (!String.IsNullOrEmpty(str) && toLowerCase)
                str = str.ToLower();

            string stFormD = str.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < stFormD.Length; i++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[i]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[i]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            return (sb.ToString().Normalize(NormalizationForm.FormD));
        }

        /// <summary>
        /// type default = success, type = 1 Warning, type = 2 error, type = 3 info
        /// </summary>
        public void Notify(string Content, int type = 0, Page page = null)
        {
            var TypeNotify = "success";
            if (type == 1)
            {
                TypeNotify = "warn";
            }
            else if (type == 2)
            {
                TypeNotify = "error";
            }
            else if (type == 3)
            {
                TypeNotify = "info";
            }

            page.Controls.Add(new LiteralControl(@"<script>$.notify('" + Content + @"','" + TypeNotify + @"');</script>"));
        }


        /// <summary>
        /// isActive = true Loading
        /// </summary>
        public void Loading(bool isActive = false, Page page = null)
        {
            var strHTML = "";
            if (isActive)
                strHTML = @"$('#isLoading').show()";
            else
                strHTML = @"$('#isLoading').hide()";
            page.Controls.Add(new LiteralControl(@"<script>" + strHTML + @";</script>"));
        }

        /// <summary>
        /// Set giá trị cho thuộc tính của Object dựa vào Tên thuộc tính có tên
        /// </summary>
        /// <param name="obj">Object muốn set giá trị</param>
        /// <param name="strPropName">Tên thuộc tính muốn set giá trị</param>
        /// <param name="value">Giá trị muốn set</param>
        /// <param name="isTry">Có sử dụng try catch trong hàm</param>
        public static void SetPropertyValueByName(object obj, string strPropName, object value, bool isTry = true)
        {
            // có sử dụng try catch
            if (isTry)
            {
                try
                {
                    PropertyInfo propInfo = GetProperty(obj, strPropName);
                    if (propInfo != null)
                    {
                        Type t = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;

                        object safeValue = (value == null) ? null : Convert.ChangeType(value, t);

                        propInfo.SetValue(obj, safeValue, null);
                    }
                }
                catch (Exception ex)
                {
                    CmmFunc.TrackLogSql($"Method: SetPropertyValueByName - File: TT.WF_DCMOP.Areas.Areas.CmmFunc", ex + string.Empty);
                }
            }
            else
            {

                PropertyInfo propInfo = GetProperty(obj, strPropName);
                if (propInfo != null)
                {
                    Type t = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;

                    object safeValue = (value == null) ? null : Convert.ChangeType(value, t);

                    propInfo.SetValue(obj, safeValue, null);
                }


            }
        }

        public static dynamic ConvertBeanToDynamic(object model, List<string> columnConvert = null)
        {
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var property in model.GetType().GetProperties())
            {
                if (columnConvert != null && !columnConvert.Contains(property.Name)) continue;

                expando.Add(property.Name, property.GetValue(model));
            }

            return expando;
        }

        public static List<dynamic> ConvertListBeanToDynamic<T>(List<T> list, List<string> columnConvert = null)
        {
            List<dynamic> dynamicList = new List<dynamic>();

            foreach (var item in list)
            {
                IDictionary<string, object> expando = new ExpandoObject();
                foreach (var property in item.GetType().GetProperties())
                {
                    if (columnConvert != null && !columnConvert.Contains(property.Name)) continue;

                    expando.Add(property.Name, property.GetValue(item));
                }
                dynamicList.Add(expando);
            }

            return dynamicList;
        }

        public void CloseForm(FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có muốn đóng Form ?",
                                                "Xác nhận thoát",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Question);

            // Nếu người dùng chọn No, ngăn chặn việc đóng Form
            if (result == DialogResult.Yes)
            {
                Index form = new Index();
                form.Show();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}