using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using QuanLyPhongGym.Model;
using QuanLyPhongGym.Areas;

namespace QuanLyPhongGym.Controller
{
    public class DBController : ControllerModel
    {
        protected string key = ConfigurationManager.AppSettings["KeyCrypt"];
        #region Sử dụng với Sql
        /// <summary>
        /// Đối tượng kết nối DB
        /// </summary>
        public SqlConnection Con;

        /// <summary>
        /// Đối tượng thực thi kết nối
        /// </summary>
        public SqlCommand Cmm;

        /// <summary>
        /// Đối tượng Transaction
        /// </summary>
        protected SqlTransaction Tran = null;

        protected static DateTime CacheTbSettingsExpire = default(DateTime);

        public static Dictionary<string, string> CacheTbSettingsDC = new Dictionary<string, string>();
        /// <summary>
        /// Mở kết nối DB
        /// </summary>
        /// <param name="strKeyConString"></param>
        public virtual void Open(string strKeyConString = "")
        {
            if (string.IsNullOrEmpty(strKeyConString)) strKeyConString = "DbDefault";
            string connection = ConfigurationManager.ConnectionStrings[strKeyConString] + string.Empty;
            if (Con != null)
            {
                if (Con.State == ConnectionState.Open)
                {
                    if (Con.ConnectionString.Equals(connection, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Cmm != null && Cmm.Parameters != null)
                        {
                            Cmm.CommandType = CommandType.Text;
                            Cmm.CommandText = string.Empty;
                            Cmm.Parameters.Clear();
                        }
                        return;
                    }
                    else
                    {
                        Con.Close();
                    }
                }
                else if (Con.State == ConnectionState.Closed)
                {
                    if (!Con.ConnectionString.Equals(connection, StringComparison.OrdinalIgnoreCase))
                    {
                        Con = new SqlConnection(connection);
                        Cmm = new SqlCommand();
                        Cmm.Connection = Con;
                    }
                    if (Cmm != null)
                    {
                        Cmm.CommandType = CommandType.Text;
                        Cmm.CommandText = string.Empty;
                        Cmm.Parameters.Clear();
                    }
                    else
                    {
                        Cmm = new SqlCommand();
                        Cmm.Connection = Con;
                    }

                    Con.Open();

                    return;
                }
            }
            Con = new SqlConnection(connection);
            Cmm = new SqlCommand();
            Cmm.Connection = Con;
            Con.Open();
        }

        /// <param name="storeName"></param>
        /// <param name="func"></param>
        /// <param name="paramsDictionary"></param>
        /// <param name="responseType"></param>
        /// <param name="langId"></param>
        /// <param name="curUserId"></param>
        /// <returns></returns>
        public object GetControllerWithFunc(string storeName, string func = null, DataTable tblParams = null, int iLCID = 1066, string userId = null)
        {
            object retData = new object();
            try
            {
                List<SqlParameter> lstPars = new List<SqlParameter>()
                {
                    new SqlParameter("@CurUserId", userId),
                    new SqlParameter("@LCID", iLCID),
                    new SqlParameter("@Func", func),
                };

                if (tblParams?.Rows.Count > 0)
                    lstPars.Add(new SqlParameter("@TBPars", value: tblParams));

                retData = Fill(storeName, lstPars);
            }
            catch (Exception ex)
            {
                CmmFunc.TrackLogSql("Method: GetControllerWithFunc - File: TT.WebApp_MyStore.Areas.DBBase", ex + string.Empty);
            }
            finally
            {
                Close();
            }
            return retData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="func"></param>
        /// <param name="paramsDictionary"></param>
        /// <param name="responseType"></param>
        /// <param name="langId"></param>
        /// <param name="curUserId"></param>
        /// <returns></returns>
        public object GetWithFunc(string storeName, string func = null, DataTable tblParams = null, string resDataType = "arr", int ILCD = 1066, string userId = null)
        {
            object retData = new object();
            try
            {
                List<SqlParameter> lstPars = new List<SqlParameter>()
                {
                    new SqlParameter("@CurUserId", userId),
                    new SqlParameter("@LCID", ILCD),
                    new SqlParameter("@Func", func),
                };

                if (tblParams?.Rows.Count > 0)
                    lstPars.Add(new SqlParameter("@TBPars", value: tblParams));

                switch (resDataType)
                {

                    case "multiarr":
                        retData = Fills(storeName, lstPars);
                        break;
                    case "arrtree":
                        {
                            DataTable tb = Fill(storeName, lstPars);
                            if (tb != null && tb.Rows.Count > 0)
                                retData = BuildDynamicTree(tb);
                        }
                        break;
                    case "objPaging":
                        {
                            DataTableCollection tbs = Fills(storeName, lstPars);
                            if (tbs != null && tbs.Count > 1)
                            {
                                DataTable dtData = tbs[0];
                                retData = new Dictionary<string, object> {
                                    { "DataItems", dtData ?? new DataTable() },
                                    { "TotalRecord", tbs.Count > 1 && tbs[1].Rows.Count > 0 ? tbs[1].Rows[0][0] : 0 }
                                };
                            }
                            else
                            {
                                retData = new Dictionary<string, object> {
                                    { "DataItems", new DataTable() },
                                    { "TotalRecord", 0 }
                                };
                            }
                        }
                        break;
                    default:
                        retData = Fill(storeName, lstPars);
                        break;
                }
            }
            catch (Exception ex)
            {
                CmmFunc.TrackLogSql("Method: GetWithFunc - File: TT.WebApp_MyStore.Areas.DBBase", ex + string.Empty);
            }
            finally
            {
                Close();
            }
            return retData;
        }

        public static List<object> BuildDynamicTree(DataTable dataTable)
        {
            // Tạo từ điển để lưu các node
            List<object> rootNodes = new List<object>();
            try
            {
                Dictionary<string, dynamic> lookup = new Dictionary<string, dynamic>(StringComparer.Ordinal);
                // Tạo node động cho mỗi hàng trong DataTable và thêm vào từ điển
                foreach (DataRow row in dataTable.Rows)
                {
                    dynamic node = new ExpandoObject();
                    var Item = (IDictionary<string, object>)node;
                    foreach (DataColumn column in row.Table.Columns)
                    {
                        Item[column.ColumnName] = row[column];
                    }
                    node.Items = new List<object>(); // Khởi tạo danh sách con động
                    lookup[node.ID + string.Empty] = node;
                }

                // Xây dựng cây
                foreach (DataRow row in dataTable.Rows)
                {
                    string parentId = row["ParentId"] != null ? row["ParentId"] + string.Empty : null;
                    string ID = row["ID"] + string.Empty;
                    if (!lookup.ContainsKey(ID))
                        continue;

                    if (string.IsNullOrEmpty(parentId) || parentId == "0")
                        // Nếu không có ParentId, đây là root
                        rootNodes.Add(lookup[ID]);
                    else
                    {
                        // Nếu có ParentId, tìm danh mục cha và thêm vào
                        var parentNode = lookup[parentId];
                        parentNode.Items.Add(lookup[ID]);
                    }
                }
            }
            catch (Exception ex)
            {
                CmmFunc.TrackLogSql("Method: BuildDynamicTree - File: TT.WebApp_MyStore.Areas.DBBase", ex + string.Empty);
            }
            return rootNodes;
        }

        /// <summary>
        /// Đóng kết nối DB
        /// </summary>
        public virtual void Close()
        {
            if (Con != null && Con.State == ConnectionState.Open)
            {
                Con.Close();
            }
        }

        public DataTable GetDataByStore(string storeName = null, string dataPost = null, List<SqlParameter> retParaList = null, int limit = 0, string curUserId = "", int languageId = 1066)
        {
            DataTable retValue = null;
            try
            {
                Open();
                Cmm.CommandType = CommandType.StoredProcedure;
                Cmm.CommandText = storeName;
                Cmm.Parameters.AddWithValue("@UserId", curUserId);
                Cmm.Parameters.AddWithValue("@LanguageId", languageId);
                if (!string.IsNullOrEmpty(dataPost))
                {
                    JObject obj = JObject.Parse(dataPost);
                    foreach (JProperty jProperty in obj.Properties())
                    {
                        Cmm.Parameters.AddWithValue("@" + jProperty.Name, jProperty.Value + string.Empty);
                    }
                }

                SqlParameterCollection para = Cmm.Parameters;
                if (limit > 0)
                {
                    if (retParaList != null && retParaList.Count > 0)
                    {
                        Cmm.Parameters.AddRange(retParaList.ToArray());
                    }
                }

                SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    retValue = ds.Tables[0];
                }
            }
            finally
            {
                Close();
            }
            return retValue;
        }

        public DataTableCollection FillsByQuery(string strQuery, string strKeyConString = "", bool exeCon = true)
        {
            DataTableCollection retValue = null;

            try
            {
                if (exeCon) Open(strKeyConString);
                Cmm.CommandType = CommandType.Text;
                Cmm.Parameters.Clear();
                Cmm.CommandText = strQuery;
                SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    retValue = ds.Tables;
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (exeCon) Close();
            }
            return retValue;
        }

        /// <summary>
        /// Select dữ liệu với Store
        /// </summary>
        /// <param name="obj">Đối tượng truyền tham số</param>
        /// <param name="strStoreName">tên store</param>
        /// <param name="lstParaName">Danh sách tên biến nếu xác định được chính xác</param>
        /// <param name="strKeyConString"></param>
        /// <param name="exeCon"></param>
        /// <returns></returns>
        public DataTable Select(ControllerModel obj, string strStoreName, List<string> lstParaName = null, string strKeyConString = "", bool exeCon = true)
        {
            DataTable retValue = null;
            try
            {
                Type type = obj.GetType();
                if (exeCon) Open(strKeyConString);

                var cmmAttr = (FillAttribute)type.GetCustomAttributes(typeof(FillAttribute), true).ToList().Where(cAb => ((FillAttribute)cAb).StoreName.ToLower() == strStoreName.ToLower()).FirstOrDefault();
                Cmm.Parameters.Clear();
                if (cmmAttr != null)
                {
                    Cmm.CommandType = CommandType.StoredProcedure;
                    Cmm.CommandText = cmmAttr.StoreName;
                    if (lstParaName == null)
                    {
                        lstParaName = cmmAttr.GetLstCols();
                    }
                    foreach (string par in lstParaName)
                    {
                        object value = CmmFunc.GetPropertyValueByName(obj, par);
                        if (value == null) value = DBNull.Value;
                        Cmm.Parameters.AddWithValue("@" + par.Trim(), value);
                    }

                    SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                    DataSet ds = new DataSet();
                    dap.Fill(ds);
                    if (ds.Tables.Count > 0)
                    {
                        retValue = ds.Tables[0];
                    }
                }
            }
            finally
            {
                if (exeCon)
                {
                    Close();
                }
            }

            return retValue;
        }

        /// <summary>
        ///  Lấy dữ liệu các bảng Master
        /// </summary>
        /// <typeparam name="T">Đối tượng Bean</typeparam>
        /// <param name="obj">Đối tượng Bean</param>
        /// <param name="lstColsName">Danh sách cột muốn select. Mặc định null là lấy hết</param>
        /// <param name="limit">int?: Lấy giới hạn số dòng. Mặc định là lấy hết</param>
        /// <param name="strKeyConString">Key kết nối DB</param>
        /// <returns>Danh sách đối tượng Bean/></returns>
        public List<T> SelectAll<T>(ControllerModel obj, List<string> lstColsName = null, int? limit = null, string strKeyConString = "")
        {
            List<T> objList = null;
            try
            {
                Open(strKeyConString);
                Type type = obj.GetType();
                string tableName = obj.GetTableName(type);
                string strSelectCols;
                if (lstColsName != null && lstColsName.Count > 0)
                {
                    strSelectCols = "[" + string.Join("],[", lstColsName) + "]";
                }
                else strSelectCols = "*";

                string strTop = string.Empty;
                if (limit.HasValue) strTop = "Top " + limit.Value;

                Cmm.CommandType = CommandType.Text;
                Cmm.Parameters.Clear();
                Cmm.CommandText = string.Format("SELECT {0} {1} FROM [{2}]", strTop, strSelectCols, tableName);

                SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    DataTable tbResult = ds.Tables[0];
                    if (tbResult != null && tbResult.Rows.Count > 0)
                    {
                        ControllerModel.TryParse(tbResult.Rows, out objList);
                    }
                }
            }
            finally
            {
                Close();
            }
            return objList;
        }

        /// <summary>
        /// Lấy thông tin đầy đủ Bean Object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="strKeyConString"></param>
        /// <param name="listCols">Null lấy tất cả; Ngược lại, chỉ định cột cần lấy</param>
        /// <param name="flgIgnoreAttr">True bo qua thuoc tinh store select</param>
        /// <param name="exeCon"></param>
        /// <returns></returns>
        public virtual T Select<T>(ControllerModel obj, string strKeyConString = "", List<string> listCols = null, bool flgIgnoreAttr = false, bool exeCon = true)
        {
            T retValue = default(T);
            try
            {
                Type type = obj.GetType();
                string tableName = obj.GetTableName(type, false);
                if (exeCon)
                {
                    Open(strKeyConString);
                }

                var cmmAttr = flgIgnoreAttr ? null : (SelectAttribute)type.GetCustomAttributes(typeof(SelectAttribute), true).FirstOrDefault();
                Cmm.Parameters.Clear();
                Cmm.CommandText = string.Empty;
                string DatabaseName = obj.GetDataBaseName();
                string SchemaName = obj.GetSchemaName();

                if (cmmAttr != null && !string.IsNullOrEmpty(cmmAttr.StoreName))
                {
                    string StoreName = cmmAttr.StoreName;
                    if (!string.IsNullOrEmpty(DatabaseName))
                    {
                        StoreName = string.Concat("[", DatabaseName, "].[dbo].[", cmmAttr.StoreName, "]");
                    }
                    Cmm.CommandType = CommandType.StoredProcedure;
                    Cmm.CommandText = StoreName;
                    foreach (string par in cmmAttr.GetLstCols())
                    {
                        PropertyInfo propKey = CmmFunc.GetProperty(obj, par);
                        object value = CmmFunc.TryGetPropertyValueByName(obj, par);
                        if (propKey.PropertyType == typeof(int) && (int)value == 0)
                            continue;

                        if (value == null) value = DBNull.Value;
                        Cmm.Parameters.AddWithValue("@" + par.Trim(), value);
                    }
                }
                else
                {
                    List<string> lstPriKey = obj.GetPriKey(type);

                    Cmm.CommandType = CommandType.Text;
                    Cmm.Parameters.Clear();

                    List<string> lstSelKey = new List<string>();
                    foreach (string priKeyItem in lstPriKey)
                    {
                        PropertyInfo propKey = CmmFunc.GetProperty(obj, priKeyItem);
                        object propValue = propKey.GetValue(obj);
                        if (propKey.PropertyType == typeof(int) && (int)propValue == 0)
                        {
                            continue;
                        }
                        else if (propKey.PropertyType == typeof(string) && string.IsNullOrEmpty(propValue + ""))
                        {
                            continue;
                        }
                        lstSelKey.Add(priKeyItem);
                        Cmm.Parameters.AddWithValue("@" + priKeyItem, propValue);
                    }

                    if (Cmm.Parameters.Count == 0)
                    {
                        List<string> lstExtraId = obj.GetExtraId(type);
                        foreach (string extraItem in lstExtraId)
                        {
                            PropertyInfo propKey = CmmFunc.GetProperty(obj, extraItem);
                            object propValue = propKey.GetValue(obj);
                            if (propKey.PropertyType == typeof(int) && (int)propValue == 0)
                            {
                                continue;
                            }
                            else if (propKey.PropertyType == typeof(string) && string.IsNullOrEmpty(propValue + ""))
                            {
                                continue;
                            }
                            lstSelKey.Add(extraItem);
                            Cmm.Parameters.AddWithValue("@" + extraItem, propValue);
                        }
                    }

                    string strWhere = string.Join(" AND ", (from w in lstSelKey select "[" + w + "] = @" + w + " ").ToArray());

                    string TableFullName = GetTableFullName(tableName, DatabaseName, SchemaName);

                    if (listCols == null || listCols.Count == 0)
                        Cmm.CommandText = string.Format("SELECT * FROM {0} WITH (NOLOCK) WHERE {1}", TableFullName, strWhere);
                    else
                    {
                        string strSelectCols = "[" + string.Join("],[", listCols) + "]";
                        Cmm.CommandText = string.Format("SELECT {2} FROM {0} WITH (NOLOCK) WHERE {1}", TableFullName, strWhere, strSelectCols);
                    }
                }

                SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    DataTable tbResult = ds.Tables[0];
                    if (tbResult != null && tbResult.Rows.Count > 0)
                        ControllerModel.TryParse(tbResult.Rows[0], out retValue);
                }
            }
            finally
            {
                if (exeCon)
                {
                    Close();
                }
            }

            return retValue;
        }

        /// <summary>
        /// Lấy thông tin đầy đủ Bean Object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="strKeyConString"></param>
        /// <param name="listCols">Null lấy tất cả; Ngược lại, chỉ định cột cần lấy</param>
        /// <param name="flgIgnoreAttr">True bo qua thuoc tinh store select</param>
        /// <param name="exeCon"></param>
        /// <returns></returns>
        public object SelectReturnObject(ControllerModel obj, string strKeyConString = "", List<string> listCols = null, bool flgIgnoreAttr = false, bool exeCon = true)
        {
            object retValue = null;
            try
            {
                if (exeCon)
                {
                    Open(strKeyConString);
                }
                Type type = obj.GetType();
                var cmmAttr = flgIgnoreAttr ? null : (SelectAttribute)type.GetCustomAttributes(typeof(SelectAttribute), true).FirstOrDefault();
                Cmm.Parameters.Clear();
                Cmm.CommandText = string.Empty;
                string DatabaseName = obj.GetDataBaseName();

                if (cmmAttr != null && !string.IsNullOrEmpty(cmmAttr.StoreName))
                {
                    string StoreName = cmmAttr.StoreName;
                    if (!string.IsNullOrEmpty(DatabaseName))
                    {
                        StoreName = string.Concat("[", DatabaseName, "].[dbo].[", cmmAttr.StoreName, "]");
                    }
                    Cmm.CommandType = CommandType.StoredProcedure;
                    Cmm.CommandText = StoreName;
                    foreach (string par in cmmAttr.GetLstCols())
                    {
                        object value = CmmFunc.TryGetPropertyValueByName(obj, par);
                        if (value == null) value = DBNull.Value;
                        Cmm.Parameters.AddWithValue("@" + par.Trim(), value);
                    }
                }
                else
                {
                    string tableName = obj.GetTableName(type);
                    List<string> lstPriKey = obj.GetPriKey(type);

                    Cmm.CommandType = CommandType.Text;
                    Cmm.Parameters.Clear();

                    List<string> lstSelKey = new List<string>();
                    foreach (string priKeyItem in lstPriKey)
                    {
                        PropertyInfo propKey = CmmFunc.GetProperty(obj, priKeyItem);
                        object propValue = propKey.GetValue(obj);
                        if (propKey.PropertyType == typeof(int) && (int)propValue != 0)
                        {
                            continue;
                        }
                        else if (propKey.PropertyType == typeof(string) && !string.IsNullOrEmpty(propValue + ""))
                        {
                            continue;
                        }
                        lstSelKey.Add(priKeyItem);
                        Cmm.Parameters.AddWithValue("@" + priKeyItem, propValue);
                    }

                    if (Cmm.Parameters.Count == 0)
                    {
                        List<string> lstExtraId = obj.GetExtraId(type);
                        foreach (string extraItem in lstExtraId)
                        {
                            PropertyInfo propKey = CmmFunc.GetProperty(obj, extraItem);
                            object propValue = propKey.GetValue(obj);
                            if (propKey.PropertyType == typeof(int) && (int)propValue != 0)
                            {
                                continue;
                            }
                            else if (propKey.PropertyType == typeof(string) && !string.IsNullOrEmpty(propValue + ""))
                            {
                                continue;
                            }
                            lstSelKey.Add(extraItem);
                            Cmm.Parameters.AddWithValue("@" + extraItem, propValue);
                        }
                    }
                    string strWhere = string.Join(" AND ", (from w in lstSelKey select "[" + w + "] = @" + w + " ").ToArray());

                    string TableFullName = string.Concat("[", tableName, "]");
                    if (!string.IsNullOrEmpty(DatabaseName))
                    {
                        TableFullName = string.Concat("[", DatabaseName, "].[dbo].[", tableName, "]");
                    }

                    if (listCols == null || listCols.Count == 0)
                        Cmm.CommandText = string.Format("SELECT * FROM {0} WITH (NOLOCK) WHERE {1}", TableFullName, strWhere);
                    else
                    {
                        string strSelectCols = "[" + string.Join("],[", listCols) + "]";
                        Cmm.CommandText = string.Format("SELECT {2} FROM {0} WITH (NOLOCK) WHERE {1}", TableFullName, strWhere, strSelectCols);
                    }
                }

                SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    DataTable tbResult = ds.Tables[0];
                    if (tbResult != null && tbResult.Rows.Count > 0)
                    {
                        retValue = Activator.CreateInstance(type);
                        ControllerModel.TryParse(tbResult.Rows[0], ref retValue);
                    }
                }
            }
            finally
            {
                if (exeCon)
                {
                    Close();
                }
            }

            return retValue;
        }

        /// <summary>
        /// Select Dữ liệu với Query String
        /// </summary>
        /// <param name="strQuery">Câu Query SQL</param>
        /// <param name="strKeyConString"></param>
        /// <returns></returns>
        public DataTable Select(string strQuery, string strKeyConString = "")
        {
            DataTable retValue = null;
            try
            {
                Open(strKeyConString);
                Cmm.Parameters.Clear();
                Cmm.CommandText = strQuery;
                SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    retValue = ds.Tables[0];
                }
            }
            finally
            {
                Close();
            }

            return retValue;
        }

        /// <summary>
        /// Lấy dữ liệu với Store và điều kiện Parameter
        /// </summary>
        /// <param name="storeName">Tên Store</param>
        /// <param name="retParaList"></param>
        /// <param name="strKeyConString"></param>
        /// <returns></returns>
        public DataTable Fill(string storeName, List<SqlParameter> retParaList = null, string strKeyConString = "")
        {
            DataTable retValue = null;
            try
            {
                Open(strKeyConString);

                Cmm.Parameters.Clear();
                Cmm.CommandType = CommandType.StoredProcedure;
                Cmm.CommandText = storeName;
                if (retParaList != null && retParaList.Count > 0)
                {
                    Cmm.Parameters.AddRange(retParaList.ToArray());
                }
                SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    retValue = ds.Tables[0];
                }
            }
            finally
            {
                Close();
            }

            return retValue;
        }

        /// <summary>
        /// Lấy dữ liệu với Store nhiều Table và điều kiện Parameter
        /// </summary>
        /// <param name="storeName">Tên Store</param>
        /// <param name="retParaList"></param>
        /// <param name="strKeyConString"></param>
        /// <returns></returns>
        public DataTableCollection Fills(string storeName, List<SqlParameter> retParaList = null, string strKeyConString = "")
        {
            DataTableCollection retValue = null;
            try
            {
                Open(strKeyConString);

                Cmm.Parameters.Clear();
                Cmm.CommandType = CommandType.StoredProcedure;
                Cmm.CommandText = storeName;
                if (retParaList != null) Cmm.Parameters.AddRange(retParaList.ToArray());
                SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    retValue = ds.Tables;
                }
            }
            finally
            {
                Close();
            }

            return retValue;
        }

        /// <summary>
        /// Xóa một đối tượng dưới sql
        /// </summary>
        /// <param name="obj">đối tượng muốn xóa</param>
        /// <param name="strKeyConString"></param>
        /// <param name="exeCon"></param>
        /// <param name="flgIgnoreAttr">True bo qua thuoc tinh store select</param>
        /// <returns></returns>
        public virtual int Delete(ControllerModel obj, string strKeyConString = "", bool exeCon = true, bool flgIgnoreAttr = false)
        {
            int retValue = -1;
            try
            {
                string tableName = null;
                if (exeCon)
                {
                    Open(strKeyConString);
                }

                Type type = obj.GetType();
                List<string> lstPriKey = obj.GetPriKey(type);
                var cmmAttr = flgIgnoreAttr ? null : (DeleteAttribute)type.GetCustomAttributes(typeof(DeleteAttribute), true).FirstOrDefault();
                SqlParameter retPara = null;
                Cmm.Parameters.Clear();
                Cmm.CommandText = string.Empty;
                Cmm.CommandType = CommandType.Text;
                string DatabaseName = obj.GetDataBaseName();
                string SchemaName = obj.GetSchemaName();

                if (cmmAttr != null)
                {
                    string StoreName = GetStoreFullName(cmmAttr.StoreName, SchemaName, DatabaseName);

                    Cmm.CommandType = CommandType.StoredProcedure;
                    Cmm.CommandText = StoreName;
                    foreach (string par in cmmAttr.GetLstCols())
                    {
                        Cmm.Parameters.AddWithValue("@" + par.Trim(), CmmFunc.GetPropertyValueByName(obj, par));
                    }

                    retPara = new SqlParameter();
                    retPara.ParameterName = "@retId";
                    retPara.DbType = DbType.Int16;
                    retPara.Size = 100;
                    retPara.Direction = ParameterDirection.ReturnValue;
                    Cmm.Parameters.Add(retPara);
                }
                else
                {
                    tableName = obj.GetTableName(type);
                    string strWhere = string.Join(" AND ", (from w in lstPriKey select "[" + w + "] = @" + w + " ").ToArray());

                    string TableFullName = GetTableFullName(tableName, DatabaseName, SchemaName);

                    Cmm.CommandText = string.Format("DELETE FROM {0} WHERE {1}", TableFullName, strWhere);
                    foreach (string priKeyItem in lstPriKey)
                    {
                        Cmm.Parameters.AddWithValue("@" + priKeyItem, CmmFunc.TryGetPropertyValueByName(obj, priKeyItem));
                    }
                }

                retValue = Cmm.ExecuteNonQuery();
                if (retPara != null)
                {
                    int.TryParse(retPara.Value + "", out retValue);
                }
            }
            finally
            {
                if (exeCon)
                {
                    Close();
                }
            }

            return retValue;
        }

        ///  <summary>
        /// Cập nhật thông tin
        ///  </summary>
        ///  <param name="obj"></param>
        ///  <param name="flgIgnoreNullVal"></param>
        ///  <param name="strKeyConString"></param>
        ///  <param name="colsUpdate"></param>
        ///  <param name="exeCon"></param>
        ///  <param name="flgIgnoreAttr"></param>
        ///  <returns></returns>
        public virtual int Update(ControllerModel obj, List<string> colsUpdate = null, bool flgIgnoreNullVal = true, string strKeyConString = "", bool exeCon = true, bool flgIgnoreAttr = false)
        {
            int retValue = -1;
            try
            {
                string tableName = null;
                if (exeCon)
                {
                    Open(strKeyConString);
                }

                Type type = obj.GetType();
                List<string> lstPriKey = obj.GetPriKey(type);
                var cmmAttr = flgIgnoreAttr ? null : (UpdateAttribute)type.GetCustomAttributes(typeof(UpdateAttribute), true).FirstOrDefault();
                Cmm.Parameters.Clear();
                Cmm.CommandText = string.Empty;
                Cmm.CommandType = CommandType.Text;
                string DatabaseName = obj.GetDataBaseName();
                string SchemaName = obj.GetSchemaName();
                if (colsUpdate == null && cmmAttr != null && !string.IsNullOrEmpty(cmmAttr.StoreName))
                {
                    string StoreName = GetStoreFullName(cmmAttr.StoreName, SchemaName, DatabaseName);

                    Cmm.CommandType = CommandType.StoredProcedure;
                    Cmm.CommandText = StoreName;

                    #region lấy danh sách field

                    if (cmmAttr.GetLstCols() != null && cmmAttr.GetLstCols().Count > 0)
                    {
                        foreach (string par in cmmAttr.GetLstCols())
                        {
                            object value = CmmFunc.GetPropertyValueByName(obj, par);
                            if (value == null) value = DBNull.Value;
                            Cmm.Parameters.AddWithValue("@" + par.Trim(), value);
                        }
                    }
                    else
                    {
                        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                        //Ko truyền danh sách column, lấy tất cả các field except Ignore
                        foreach (var p in props)
                        {
                            // Nếu là cột Ignore thì bỏ qua không xử lý
                            var ignore = (p.GetCustomAttributes(typeof(IgnoreAttribute), true).Length > 0 || p.GetCustomAttributes(typeof(IgnoreSAttribute), true).Length > 0);
                            if (ignore) continue;
                            object itemValue = CmmFunc.GetPropertyValue(obj, p);
                            if (flgIgnoreNullVal && itemValue == null) continue;
                            Type pType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                            if (itemValue != null && pType == typeof(DateTime) && (DateTime)itemValue == default(DateTime))
                            {
                                if (flgIgnoreNullVal)
                                {
                                    continue;
                                }
                                else
                                {
                                    itemValue = DBNull.Value;
                                }
                            }
                            if (itemValue == null)
                            {
                                itemValue = DBNull.Value;
                            }
                            Cmm.Parameters.AddWithValue("@" + p.Name, itemValue);
                        }
                    }

                    #endregion lấy danh sách field
                }
                else
                {
                    Cmm.CommandType = CommandType.Text;
                    tableName = obj.GetTableName(type);

                    string strWhere = string.Empty;
                    string clos = string.Empty;

                    if (cmmAttr != null)
                    {
                        foreach (string par in cmmAttr.GetLstCols())
                        {
                            string formatPar = par.Trim();
                            if (lstPriKey.Contains(formatPar)) continue;
                            if (colsUpdate != null && !colsUpdate.Contains(formatPar)) continue;

                            PropertyInfo p = type.GetProperty(formatPar);
                            if (p == null) continue;

                            object itemValue = CmmFunc.GetPropertyValue(obj, p);
                            if (flgIgnoreNullVal && itemValue == null) continue;

                            Type pType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                            if (itemValue != null && pType == typeof(DateTime) && (DateTime)itemValue == default(DateTime))
                            {
                                if (flgIgnoreNullVal)
                                {
                                    continue;
                                }
                                else
                                {
                                    itemValue = DBNull.Value;
                                }
                            }

                            if (itemValue == null)
                            {
                                itemValue = DBNull.Value;
                            }

                            if (!string.IsNullOrEmpty(clos))
                            {
                                clos += ",";
                            }
                            clos += "[" + formatPar + "]=@" + formatPar;
                            Cmm.Parameters.AddWithValue("@" + formatPar, itemValue);
                        }
                    }
                    else
                    {
                        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                        foreach (var p in props)
                        {
                            // Nếu là cột Ignore thì bỏ qua không xử lý
                            var ignore = p.GetCustomAttributes(typeof(IgnoreAttribute), true).Length > 0;
                            if (ignore) continue;
                            if (lstPriKey.Contains(p.Name)) continue;
                            if (colsUpdate != null && !colsUpdate.Contains(p.Name)) continue;
                            object itemValue = CmmFunc.GetPropertyValue(obj, p);
                            Type pType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                            if (itemValue != null && pType == typeof(DateTime) && (DateTime)itemValue == default(DateTime))
                            {
                                if (flgIgnoreNullVal)
                                {
                                    continue;
                                }
                                else
                                {
                                    itemValue = DBNull.Value;
                                }
                            }

                            if (colsUpdate == null && flgIgnoreNullVal && itemValue == null) continue;

                            if (itemValue == null)
                            {
                                itemValue = DBNull.Value;
                            }

                            if (!string.IsNullOrEmpty(clos))
                            {
                                clos += ",";
                            }
                            clos += "[" + p.Name + "]=@" + p.Name;
                            Cmm.Parameters.AddWithValue("@" + p.Name, itemValue);
                        }
                    }
                    foreach (string priKeyItem in lstPriKey)
                    {
                        if (!string.IsNullOrEmpty(strWhere))
                        {
                            strWhere += " AND ";
                        }
                        strWhere += "[" + priKeyItem + "]=@" + priKeyItem;
                        Cmm.Parameters.AddWithValue("@" + priKeyItem, CmmFunc.GetPropertyValueByName(obj, priKeyItem));
                    }

                    string TableFullName = GetTableFullName(tableName, DatabaseName, SchemaName);
                    Cmm.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2}", TableFullName, clos, strWhere);
                }
                // Nếu có sử dụng transation thì
                if (!exeCon && Tran != null)
                {
                    Cmm.Transaction = Tran;
                }
                Cmm.ExecuteNonQuery();
                retValue = 1;
            }
            finally
            {
                if (exeCon)
                {
                    Close();
                }
            }

            return retValue;
        }

        /// <summary>
        /// Execute Dữ liệu vào DataBase
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="strStoreName">string: Tên store</param>
        /// <param name="retParaList"></param>
        /// <param name="strKeyConString"></param>
        /// <param name="exeCon"></param>
        /// <returns>int: -1 là chạy lỗi; Ngược lại là chạy thành công.</returns>
        public virtual int Execute(ControllerModel obj, string strStoreName, List<SqlParameter> retParaList = null, string strKeyConString = "", bool exeCon = true)
        {
            int retValue = -1;
            try
            {
                if (exeCon)
                {
                    Open(strKeyConString);
                }
                Type type = obj.GetType();
                var cmmAttr = (ExecuteAttribute)type.GetCustomAttributes(typeof(ExecuteAttribute), true).ToList().FirstOrDefault(cAb => ((ExecuteAttribute)cAb).StoreName.ToLower() == strStoreName.ToLower());
                Cmm.Parameters.Clear();

                if (cmmAttr != null)
                {
                    string DatabaseName = obj.GetDataBaseName();
                    string SchemaName = obj.GetSchemaName();
                    string StoreName = GetStoreFullName(cmmAttr.StoreName, SchemaName, DatabaseName);
                    Cmm.CommandType = CommandType.StoredProcedure;
                    Cmm.CommandText = StoreName;
                    foreach (string par in cmmAttr.GetLstCols())
                    {
                        object value = CmmFunc.GetPropertyValueByName(obj, par);
                        if (value == null) value = DBNull.Value;
                        Cmm.Parameters.AddWithValue("@" + par.Trim(), value);
                    }
                    if (retParaList != null && retParaList.Count > 0)
                    {
                        Cmm.Parameters.AddRange(retParaList.ToArray());
                    }

                    // Nếu có sử dụng transation thì
                    if (!exeCon && Tran != null)
                    {
                        Cmm.Transaction = Tran;
                    }
                    retValue = Cmm.ExecuteNonQuery();
                }
            }
            finally
            {
                if (exeCon)
                {
                    Close();
                }
            }

            return retValue;
        }

        /// <summary>
        /// Execute Dữ liệu vào DataBase
        /// </summary>
        /// <param name="strStoreName">string: Tên store</param>
        /// <param name="retParaList"></param>
        /// <param name="strKeyConString"></param>
        /// <param name="exeCon"></param>
        /// <returns>int: -1 là chạy lỗi; Ngược lại là chạy thành công.</returns>
        public virtual int Execute(string strStoreName, List<SqlParameter> retParaList = null, string strKeyConString = "", bool exeCon = true)
        {
            int retValue = -1;
            try
            {
                if (exeCon)
                {
                    Open(strKeyConString);
                }
                Cmm.Parameters.Clear();
                Cmm.CommandType = CommandType.StoredProcedure;
                Cmm.CommandText = strStoreName;

                if (retParaList != null && retParaList.Count > 0)
                {
                    Cmm.Parameters.AddRange(retParaList.ToArray());
                }

                // Nếu có sử dụng transation thì
                if (!exeCon && Tran != null)
                {
                    Cmm.Transaction = Tran;
                }
                retValue = Cmm.ExecuteNonQuery();
            }
            finally
            {
                if (exeCon)
                {
                    Close();
                }
            }

            return retValue;
        }

        /// <summary>
        /// Tự động tạo Id dự trên kiểu dữ liệu
        /// </summary>
        /// <param name="type">Kiển dữ liệu tương ứng với ID muốn tạo</param>
        /// <param name="strKeyConString"></param>
        /// <returns></returns>
        public string CreateNewId(Type type, string strKeyConString = "", bool addSchema = false)
        {
            ControllerModel obj = new ControllerModel();
            bool flgOpenConn = false;
            string retValue = string.Empty;
            string tableName = obj.GetTableName(type, addSchema);

            List<string> lstPriKey = obj.GetPriKey(type);

            try
            {
                if (Con == null || Con.State != ConnectionState.Open)
                {
                    Open(strKeyConString);
                    flgOpenConn = true;
                }

                if (!string.IsNullOrEmpty(tableName) && lstPriKey.Count == 1)
                {
                    Cmm.Parameters.Clear();
                    Cmm.CommandType = CommandType.StoredProcedure;
                    Cmm.CommandText = "QuanLyPhongGym_Create_ExtraId";
                    Cmm.Parameters.AddWithValue("@TableName", tableName);
                    Cmm.Parameters.AddWithValue("@ColName", lstPriKey[0]);
                    SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                    DataSet ds = new DataSet();
                    dap.Fill(ds);
                    if (ds.Tables.Count > 0)
                    {
                        DataTable tbResult = ds.Tables[0];
                        if (tbResult != null && tbResult.Rows.Count > 0)
                        {
                            retValue = tbResult.Rows[0]["Value"].ToString();
                        }
                    }
                }
            }
            finally
            {
                if (flgOpenConn) Close();
            }
            return retValue;
        }

        /// <summary>
        /// Tự động tạo Id mở rộng
        /// </summary>
        /// <param name="type">Kiển dữ liệu tương ứng với ID muốn tạo</param>
        /// <param name="strKeyConString"></param>
        /// <returns></returns>
        public string CreateExtraId(Type type, string strKeyConString = "", bool addSchema = false)
        {
            ControllerModel obj = new ControllerModel();
            string retValue;
            string tableName = obj.GetTableName(type, addSchema);

            List<string> lstPriKey = obj.GetExtraId(type);

            retValue = CreateExtraId(tableName, strKeyConString);
            return retValue;
        }

        /// <summary>
        /// Tự động tạo Id mở rộng
        /// </summary>
        /// <param name="tableName">Table với ID muốn tạo</param>
        /// <param name="strKeyConString"></param>
        /// <returns></returns>
        public string CreateExtraId(string tableName, string strKeyConString = "")
        {
            bool flgOpenConn = false;
            string retValue = string.Empty;

            try
            {
                if (Con == null || Con.State != ConnectionState.Open)
                {
                    Open(strKeyConString);
                    flgOpenConn = true;
                }

                if (!string.IsNullOrEmpty(tableName))
                {
                    Cmm.Parameters.Clear();
                    Cmm.CommandType = CommandType.StoredProcedure;
                    Cmm.CommandText = "WF_DCMOP_JOB_IMPORT_IDMSResetExtractId";
                    Cmm.Parameters.AddWithValue("@TableName", tableName);
                    SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                    DataSet ds = new DataSet();
                    dap.Fill(ds);
                    if (ds.Tables.Count > 0)
                    {
                        DataTable tbResult = ds.Tables[0];
                        if (tbResult != null && tbResult.Rows.Count > 0)
                        {
                            retValue = tbResult.Rows[0]["Value"].ToString();
                        }
                    }
                }
            }
            finally
            {
                if (flgOpenConn) Close();
            }
            return retValue;
        }

        private string GetTableFullName(string table, string database = "", string schema = "")
        {
            string retValue = table;

            if (String.IsNullOrEmpty(schema))
                schema = "dbo";

            retValue = retValue.Trim(new char[] { '[', ']' });

            retValue = string.Concat("[", schema, "].[", retValue, "]");
            if (!string.IsNullOrEmpty(database))
            {
                retValue = string.Concat("[", database, "].", retValue);
            }

            return retValue;
        }

        private string GetStoreFullName(string store, string schema = "", string database = "")
        {
            string retValue = store;
            if (String.IsNullOrEmpty(schema))
                schema = "dbo";

            retValue = retValue.Trim(new char[] { '[', ']' });

            retValue = string.Concat("[", schema, "].[", retValue, "]");
            if (!string.IsNullOrEmpty(database))
            {
                retValue = string.Concat("[", database, "].", retValue);
            }
            return retValue;
        }

        /// <summary>
        /// Lấy tổng số lượng dữ liệu lấy được
        /// </summary>
        /// <param name="limit">Giới hạn số dòng muốn lấy</param>
        /// <param name="offset">Vị trí bắt đầu lấy tin</param>
        /// <param name="tbData">Table dữ liệu đã lấy được</param>
        /// <param name="cmm">Sqlcommand của câu Sql trước</param>
        /// <returns></returns>
        public int GetTotalRecord(int limit, int offset, DataTable tbData, SqlCommand cmm)
        {
            int retValue;
            if (limit < 1 || tbData.Rows.Count < limit)
            {
                retValue = offset + tbData.Rows.Count;
            }
            else
            {
                SqlParameter retPara = new SqlParameter();
                retPara.ParameterName = "@TotalRecord";
                retPara.DbType = DbType.Int32;
                retPara.Size = 100;
                retPara.Value = 0;
                retPara.Direction = ParameterDirection.InputOutput;

                cmm.CommandType = CommandType.StoredProcedure;
                cmm.Parameters.Add(retPara);
                cmm.ExecuteNonQuery();
                retValue = Convert.ToInt32(retPara.Value);
            }
            return retValue;
        }

        public string CreateStrNewID(string StrCode, ControllerModel obj)
        {
            try
            {
                Open();
                Type type = obj.GetType();
                SqlParameter retPara = new SqlParameter();
                retPara.ParameterName = "@StrNewID";
                retPara.DbType = DbType.String;
                retPara.Size = 255;
                retPara.Value = string.Empty;
                retPara.Direction = ParameterDirection.InputOutput;

                Cmm.CommandText = "QuanLyPhongGym_CreateStringNewID";
                Cmm.CommandType = CommandType.StoredProcedure;
                Cmm.Parameters.Add(retPara);
                Cmm.Parameters.AddWithValue("@Key", StrCode);
                Cmm.Parameters.AddWithValue("@TableName", obj.GetTableName(type));
                Cmm.ExecuteNonQuery();
                return (retPara.Value + string.Empty);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            finally
            {
                Close();
            }
        }

        public bool ReturnBool(string functionName, List<SqlParameter> retParaList = null, string strKeyConString = "")
        {
            bool retValue = false;

            bool flgOpenConn = false;
            try
            {

                if (Con == null || Con.State != ConnectionState.Open)
                {
                    Open(strKeyConString);
                    flgOpenConn = true;
                }
                SqlParameter retPara = new SqlParameter();
                retPara.DbType = DbType.Boolean;
                retPara.Size = 100;
                retPara.Value = 0;
                retPara.Direction = ParameterDirection.ReturnValue;

                Cmm.Parameters.Clear();
                Cmm.CommandType = CommandType.StoredProcedure;
                Cmm.CommandText = functionName;
                Cmm.Parameters.Add(retPara);
                if (retParaList != null && retParaList.Count > 0)
                {
                    Cmm.Parameters.AddRange(retParaList.ToArray());
                }
                Cmm.ExecuteNonQuery();
                if (retPara.Value != null)
                    retValue = Convert.ToBoolean(retPara.Value);
            }
            finally
            {
                if (flgOpenConn) Close();
            }

            return retValue;
        }

        /// <summary>
        /// Lấy thông tin đầy đủ Bean Object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="strKeyConString"></param>
        /// <param name="exeCon"></param>
        /// <returns></returns>
        public object Select(ControllerModel obj, string strKeyConString = "", bool exeCon = true)
        {
            object retValue = null;
            try
            {
                if (exeCon)
                {
                    Open(strKeyConString);
                }
                Type type = obj.GetType();
                var cmmAttr = (SelectAttribute)type.GetCustomAttributes(typeof(SelectAttribute), true).FirstOrDefault();
                Cmm.Parameters.Clear();

                if (cmmAttr != null && !string.IsNullOrEmpty(cmmAttr.StoreName))
                {
                    Cmm.CommandType = CommandType.StoredProcedure;
                    Cmm.CommandText = cmmAttr.StoreName;
                    foreach (string par in cmmAttr.GetLstCols())
                    {
                        object value = CmmFunc.GetPropertyValueByName(obj, par);
                        if (value == null) value = DBNull.Value;
                        Cmm.Parameters.AddWithValue("@" + par.Trim(), value);
                    }
                }
                else
                {
                    string tableName = obj.GetTableName(type);
                    List<string> lstPriKey = obj.GetPriKey(type);

                    Cmm.CommandType = CommandType.Text;
                    Cmm.Parameters.Clear();
                    string strWhere = string.Join(" AND ", (from w in lstPriKey select "[" + w + "] = @" + w + " ").ToArray());

                    Cmm.CommandText = string.Format("SELECT * FROM [{0}] WHERE {1}", tableName, strWhere);
                    foreach (string priKeyItem in lstPriKey)
                    {
                        PropertyInfo propKey = CmmFunc.GetProperty(obj, priKeyItem);
                        object propValue = propKey.GetValue(obj);
                        if (propKey.PropertyType == typeof(int) && (int)propValue != 0)
                        {
                            continue;
                        }
                        else if (propKey.PropertyType == typeof(string) && !string.IsNullOrEmpty(propValue + ""))
                        {
                            continue;
                        }

                        Cmm.Parameters.AddWithValue("@" + priKeyItem, propValue);
                    }

                    if (Cmm.Parameters.Count == 0)
                    {
                        List<string> lstExtraId = obj.GetExtraId(type);
                        foreach (string extraItem in lstExtraId)
                        {
                            PropertyInfo propKey = CmmFunc.GetProperty(obj, extraItem);
                            object propValue = propKey.GetValue(obj);
                            if (propKey.PropertyType == typeof(int) && (int)propValue != 0)
                            {
                                continue;
                            }
                            else if (propKey.PropertyType == typeof(string) && !string.IsNullOrEmpty(propValue + ""))
                            {
                                continue;
                            }
                            Cmm.Parameters.AddWithValue("@" + extraItem, propValue);
                        }
                    }
                }

                SqlDataAdapter dap = new SqlDataAdapter(Cmm);
                DataSet ds = new DataSet();
                dap.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    DataTable tbResult = ds.Tables[0];
                    if (tbResult != null && tbResult.Rows.Count > 0)
                    {
                        retValue = Activator.CreateInstance(type);
                        ControllerModel.TryParse(tbResult.Rows[0], ref retValue);
                    }
                }
            }
            finally
            {
                if (exeCon)
                {
                    Close();
                }
            }

            return retValue;
        }

        /// <summary>
        /// Insert Dữ liệu vào DataBase
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="flgIgnoreNullVal"></param>
        /// <param name="strKeyConString"></param>
        /// <param name="retPara"></param>
        /// <param name="exeCon"></param>
        /// <returns>ID (Guid, int) của item vừa insert </returns>
        public virtual int Insert(ControllerModel obj, SqlParameter retPara = null, bool flgIgnoreNullVal = true, string strKeyConString = "", bool exeCon = true)
        {
            int retValue = -1;
            try
            {
                if (exeCon)
                {
                    Open(strKeyConString);
                }
                Type type = obj.GetType();
                var cmmAttr = (InsertAttribute)type.GetCustomAttributes(typeof(InsertAttribute), true).FirstOrDefault();
                Cmm.Parameters.Clear();
                if (cmmAttr != null)
                {
                    Cmm.CommandType = CommandType.StoredProcedure;
                    Cmm.CommandText = cmmAttr.StoreName;

                    #region lấy danh sách field

                    if (cmmAttr.GetLstCols() != null && cmmAttr.GetLstCols().Count > 0)
                    {
                        foreach (string par in cmmAttr.GetLstCols())
                        {
                            object value = CmmFunc.GetPropertyValueByName(obj, par);
                            if (value == null) value = DBNull.Value;
                            Cmm.Parameters.AddWithValue("@" + par.Trim(), value);
                        }
                    }
                    else
                    {
                        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                        //Ko truyền danh sách column, lấy tất cả các field except Ignore
                        foreach (var p in props)
                        {
                            // Nếu là cột Ignore thì bỏ qua không xử lý
                            var ignore = (p.GetCustomAttributes(typeof(IgnoreAttribute), true).Length > 0 || p.GetCustomAttributes(typeof(IgnoreSAttribute), true).Length > 0);
                            if (ignore) continue;
                            object itemValue = CmmFunc.GetPropertyValue(obj, p);
                            if (flgIgnoreNullVal && itemValue == null) continue;
                            Type pType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                            if (itemValue != null && pType == typeof(DateTime) && (DateTime)itemValue == default(DateTime))
                            {
                                if (flgIgnoreNullVal)
                                {
                                    continue;
                                }
                                else
                                {
                                    itemValue = DBNull.Value;
                                }
                            }
                            if (itemValue == null)
                            {
                                itemValue = DBNull.Value;
                            }
                            Cmm.Parameters.AddWithValue("@" + p.Name, itemValue);
                        }
                    }

                    #endregion lấy danh sách field

                    if (retPara != null)
                        Cmm.Parameters.Add(retPara);
                }
                else
                {
                    string tableName = obj.GetTableName(type);
                    string clos = string.Empty;
                    string values = string.Empty;
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                    List<string> lstPriKey = obj.GetPriKey(type);

                    if (lstPriKey != null && lstPriKey.Count > 0)
                    {
                        bool flgNewId = true;
                        PropertyInfo propKey = CmmFunc.GetProperty(obj, lstPriKey[0]);
                        object propValue = CmmFunc.GetPropertyValueByName(obj, lstPriKey[0]);
                        if (propKey.PropertyType == typeof(int) && (int)propValue != 0)
                        {
                            flgNewId = false;
                        }
                        else if (propKey.PropertyType == typeof(string) && !string.IsNullOrEmpty(propValue + ""))
                        {
                            flgNewId = false;
                        }

                        if (flgNewId)
                        {
                            string newId = CreateNewId(type, strKeyConString);
                            if (!string.IsNullOrEmpty(newId))
                            {
                                CmmFunc.SetPropertyValueByName(obj, lstPriKey[0], newId);
                                if (retPara != null)
                                {
                                    retPara.Value = newId;
                                }
                                // mở lại sau khi đóng tại store get NewId
                                Open(strKeyConString);
                            }
                        }
                    }

                    List<string> lstExtraId = obj.GetExtraId(type);
                    if (lstExtraId != null && lstExtraId.Count > 0)
                    {
                        bool flgNewId = true;
                        PropertyInfo propKey = CmmFunc.GetProperty(obj, lstExtraId[0]);
                        object propValue = CmmFunc.GetPropertyValueByName(obj, lstExtraId[0]);
                        if (propKey.PropertyType == typeof(int) && (int)propValue != 0)
                        {
                            flgNewId = false;
                        }
                        else if (propKey.PropertyType == typeof(string) && !string.IsNullOrEmpty(propValue + ""))
                        {
                            flgNewId = false;
                        }

                        if (flgNewId)
                        {
                            string newId = CreateExtraId(type, strKeyConString);
                            if (!string.IsNullOrEmpty(newId))
                            {
                                CmmFunc.SetPropertyValueByName(obj, lstExtraId[0], newId);
                                if (retPara != null)
                                {
                                    retPara.Value = newId;
                                }
                                // mở lại sau khi đóng tại store get NewId
                                Open(strKeyConString);
                            }
                        }
                    }

                    foreach (var p in props)
                    {
                        // Nếu là cột Ignore thì bỏ qua không xử lý
                        var ignore = (p.GetCustomAttributes(typeof(IgnoreAttribute), true).Length > 0 || p.GetCustomAttributes(typeof(IgnoreSAttribute), true).Length > 0);
                        if (ignore) continue;

                        object itemValue = CmmFunc.GetPropertyValue(obj, p);
                        if (flgIgnoreNullVal && itemValue == null) continue;

                        Type pType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                        if (itemValue != null && pType == typeof(DateTime) && (DateTime)itemValue == default(DateTime))
                        {
                            if (flgIgnoreNullVal)
                            {
                                continue;
                            }
                            else
                            {
                                itemValue = DBNull.Value;
                            }
                        }

                        if (itemValue == null)
                        {
                            itemValue = DBNull.Value;
                        }

                        if (!string.IsNullOrEmpty(clos))
                        {
                            clos += ",";
                            values += ",";
                        }

                        clos += "[" + p.Name + "]";
                        values += "@" + p.Name;

                        Cmm.Parameters.AddWithValue("@" + p.Name, itemValue);
                    }

                    Cmm.CommandText = string.Format("INSERT INTO [{0}]({1}) VALUES({2})", tableName, clos, values);
                }
                // Nếu có sử dụng transation thì
                if (!exeCon && Tran != null)
                {
                    Cmm.Transaction = Tran;
                }
                retValue = Cmm.ExecuteNonQuery();
            }
            finally
            {
                if (exeCon)
                {
                    Close();
                }
            }

            return retValue;
        }

        /// <summary>
        /// Kiểm tra user tồn tại trong group
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="groupId">group id</param>
        /// <param name="groupName">tên nhóm </param>
        /// <param name="strKeyConString"></param>
        /// <returns>True là user tồn tại trong nhóm</returns>
        public bool CheckUserInGroup(string userId, string groupId = "", string groupName = "", bool exeCon = true, string strKeyConString = "")
        {
            bool IsContainUser = false;
            try
            {
                if (string.IsNullOrEmpty(groupName) && string.IsNullOrEmpty(groupId)) throw new Exception("Group is null.");

                if (exeCon) Open(strKeyConString);
                Cmm.Parameters.Clear();
                Cmm.CommandType = CommandType.StoredProcedure;
                Cmm.CommandText = "DCMOP_UserGroup_ContainUser";

                Cmm.Parameters.AddWithValue("@UserId", userId);
                if (!string.IsNullOrEmpty(groupName)) Cmm.Parameters.AddWithValue("@GroupName", groupName);
                if (!string.IsNullOrEmpty(groupId)) Cmm.Parameters.AddWithValue("@GroupId", groupId);

                SqlParameter RetPar = Cmm.Parameters.Add("@RetValue", SqlDbType.Bit);
                RetPar.Direction = ParameterDirection.ReturnValue;

                Cmm.ExecuteNonQuery();
                if (RetPar.Value != null)
                {
                    IsContainUser = Convert.ToBoolean(RetPar.Value);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (exeCon) Close();
            }
            return IsContainUser;
        }

        #endregion Sử dụng với Sql

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
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FillAttribute : Attribute
    {
        public string StoreName { get; set; }
        public string Cols { get; set; }

        public FillAttribute(string storeName)
        {
            StoreName = storeName;
        }

        public List<string> GetLstCols()
        {
            if (string.IsNullOrEmpty(Cols)) return null;
            List<string> retValue = Cols.Split(',').ToList();
            retValue = retValue.Select(i => i.Trim()).ToList();
            return retValue;
        }
    }
}