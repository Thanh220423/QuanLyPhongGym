using Newtonsoft.Json;
using QuanLyPhongGym.Areas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace QuanLyPhongGym.Model
{
    public class ControllerModel : ICloneable
    {
        /// Danh sách cột cần cập nhật
        /// </summary>
        [Ignore]
        public List<string> UpdateCols { get; set; }

        /// <summary>
        /// Kiểm tra giá trị có xóa hay không
        /// </summary>
        [Ignore]
        public bool Deleting { get; set; }

        /// <summary>
        /// Lấy tên Table ứng với đối tượng ánh xạ
        /// </summary>
        /// <param name="type">Kiểu dữ liệu (kiển bean ánh xạ table)</param>
        /// <returns></returns>
        public virtual string GetTableName(Type type)
        {
            var tableAttr = (TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
            string tableName = tableAttr != null ? tableAttr.Name : type.Name.Replace("Bean", "");
            return tableName;
        }

        /// <summary>
        /// Lấy tên Table ứng với đối tượng ánh xạ voi schema
        /// </summary>
        /// <param name="type">Kiểu dữ liệu (kiển bean ánh xạ table)</param>
        /// <returns></returns>
        public virtual string GetTableName(Type type, bool addSchema = false)
        {

            var tableAttr = (TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
            string tableName = tableAttr != null ? tableAttr.Name : type.Name.Replace("Bean", "");
            if (addSchema)
            {
                var schemaAttr = (SchemaAttribute)type.GetCustomAttributes(typeof(SchemaAttribute), true).FirstOrDefault();
                if (schemaAttr != null) tableName = string.Format("[{0}].[{1}]", schemaAttr.Name, tableName);
                else tableName = string.Format("[{0}]", tableName);
            }
            return tableName;
        }

        public virtual string GetDataBaseName(Type type = null)
        {
            string DatabaseName = string.Empty;
            type = type ?? this.GetType();
            var DatabaseAttr = type.GetCustomAttribute<DatabaseAttribute>(true);
            if (DatabaseAttr != null) DatabaseName = DatabaseAttr.Name;
            return DatabaseName;
        }

        public virtual string GetSchemaName(Type type = null)
        {
            string retValue = string.Empty;
            type = type ?? this.GetType();
            var schemaAttr = (SchemaAttribute)type.GetCustomAttributes(typeof(SchemaAttribute), true).FirstOrDefault();
            if (schemaAttr != null) retValue = schemaAttr.Name;
            return retValue;
        }

        /// <summary>
        /// Lấy các khóa chính trong table
        /// </summary>
        /// <param name="type">Kiểu dữ liệu (kiển bean ánh xạ table)</param>
        /// <returns></returns>
        public List<string> GetPriKey(Type type)
        {
            List<string> retValue = new List<string>();

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            foreach (var p in props)
            {
                if (p.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).Length > 0)
                {
                    retValue.Add(p.Name);
                }
            }
            return retValue;
        }

        /// <summary>
        /// Lấy các khóa chính trong table
        /// </summary>
        /// <param name="type">Kiểu dữ liệu (kiển bean ánh xạ table)</param>
        /// <returns></returns>
        public List<string> GetExtraId(Type type)
        {
            List<string> retValue = new List<string>();

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            foreach (var p in props)
            {
                if (p.GetCustomAttributes(typeof(ExtraIdAttribute), true).Length > 0)
                {
                    retValue.Add(p.Name);
                }
            }
            return retValue;
        }

        public static bool TryParse<T>(DataRow rowData, out T outData)
        {
            outData = default(T);
            try
            {
                Type type = typeof(T);
                if (rowData != null)
                {

                    object obj = Activator.CreateInstance(type);
                    foreach (DataColumn col in rowData.Table.Columns)
                    {
                        if (rowData[col.ColumnName] == DBNull.Value) continue;
                        var perInfo = type.GetProperty(col.ColumnName);
                        if (perInfo != null)
                        {
                            CmmFunc.SetPropertyValue(obj, perInfo,
                                rowData[col.ColumnName] is Guid
                                    ? rowData[col.ColumnName].ToString()
                                    : rowData[col.ColumnName]);
                        }
                    }
                    outData = (T)obj;
                    return true;
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public static bool TryParse(DataRow rowData, ref object outData)
        {
            try
            {
                if (rowData != null)
                {
                    Type type = outData.GetType();
                    foreach (DataColumn col in rowData.Table.Columns)
                    {
                        if (rowData[col.ColumnName] == DBNull.Value) continue;
                        var perInfo = type.GetProperty(col.ColumnName);
                        if (perInfo != null)
                        {
                            CmmFunc.SetPropertyValue(outData, perInfo,
                                rowData[col.ColumnName] is Guid
                                    ? rowData[col.ColumnName].ToString()
                                    : rowData[col.ColumnName]);
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        /// <summary>
        /// Chuyển dổi dữ liệu từ Table dưới DB thành Danh sách các Object
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu (kiển bean ánh xạ table)</typeparam>
        /// <param name="rowsData">mảng các dữ liệu dạng DataRow</param>
        /// <param name="outData">Danh sách đối tượng convert được</param>
        /// <returns>True: Convert thành công, False: Convert không thành công</returns>
        public static bool TryParse<T>(DataRow[] rowsData, out List<T> outData)
        {
            outData = new List<T>();
            try
            {
                //Type type = typeof(T);
                foreach (DataRow rowData in rowsData)
                {
                    T itemData;
                    if (TryParse(rowData, out itemData))
                    {
                        outData.Add(itemData);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        /// <summary>
        /// Chuyển dổi dữ liệu từ Table dưới DB thành Danh sách các Object
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu (kiển bean ánh xạ table)</typeparam>
        /// <param name="rowsData">mảng các dữ liệu dạng DataRow</param>
        /// <param name="outData">Danh sách đối tượng convert được</param>
        /// <returns>True: Convert thành công, False: Convert không thành công</returns>
        public static bool TryParse<T>(DataRowCollection rowsData, out List<T> outData)
        {
            outData = new List<T>();
            try
            {
                //Type type = typeof(T);
                foreach (DataRow rowData in rowsData)
                {
                    T itemData;
                    if (TryParse(rowData, out itemData))
                    {
                        outData.Add(itemData);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        /// <summary>
        /// Chuyển dổi dữ liệu từ Table dưới DB thành Danh sách các Object
        /// </summary>
        /// <param name="table">mảng các dữ liệu dạng DataRow</param>
        /// <param name="outData">Danh sách đối tượng convert được</param>
        /// <param name="lstColName">Danh sách column muốn Parse</param>
        /// <returns>True: Convert thành công, False: Convert không thành công</returns>
        public static bool TryParse(DataTable table, out List<object> outData, List<string> lstColName = null)
        {
            return TryParse(table.Select(), out outData, lstColName);
        }

        /// <summary>
        /// Chuyển dổi dữ liệu từ Danh sách DataRow dưới DB thành Danh sách các Object
        /// </summary>
        /// <param name="rowsData">mảng các dữ liệu dạng DataRow</param>
        /// <param name="outData">Danh sách đối tượng convert được</param>
        /// <param name="lstColName">Danh sách column muốn Parse</param>
        /// <returns>True: Convert thành công, False: Convert không thành công</returns>
        public static bool TryParse(DataRow[] rowsData, out List<object> outData, List<string> lstColName = null)
        {
            outData = new List<object>();
            try
            {
                if (rowsData == null || rowsData.Length == 0) return true;

                List<string> cols = new List<string>();
                if (lstColName != null && lstColName.Count > 0)
                {
                    foreach (string colItem in lstColName)
                    {

                        // Nếu Field không tồn tại thì cho qua
                        if (!rowsData[0].Table.Columns.Contains(colItem)) continue;
                        cols.Add(colItem);

                    }

                }
                else
                {
                    foreach (DataColumn col in rowsData[0].Table.Columns)
                    {
                        cols.Add(col.ColumnName);
                    }
                }

                foreach (DataRow row in rowsData)
                {
                    dynamic obj = new ExpandoObject();
                    outData.Add(obj);

                    foreach (string colName in cols)
                    {
                        var dic = (IDictionary<string, object>)obj;
                        dic[colName] = row[colName];
                    }

                }
                return true;

            }
            catch (Exception ex)
            {
            }
            return false;
        }

        /// <summary>
        /// Chuyển dổi dữ liệu từ Danh sách DataRow dưới DB thành Danh sách các Object
        /// </summary>
        /// <param name="rowData">DataRow chứa dữ liệu muốn chuyển đổi</param>
        /// <param name="outData">Đối tượng convert được</param>
        /// <param name="lstColName">Danh sách column muốn Parse</param>
        /// <returns>True: Convert thành công, False: Convert không thành công</returns>
        public static bool TryParse(DataRow rowData, out object outData, List<string> lstColName = null)
        {
            outData = null;
            try
            {
                if (rowData == null) return true;

                dynamic obj = new ExpandoObject();
                outData = obj;

                foreach (DataColumn col in rowData.Table.Columns)
                {
                    // Nếu Field không tồn tại thì cho qua
                    if (lstColName != null && !lstColName.Contains(col.ColumnName)) continue;

                    var dic = (IDictionary<string, object>)obj;
                    dic[col.ColumnName] = rowData[col.ColumnName];
                }
                return true;

            }
            catch (Exception ex)
            {
            }
            return false;
        }

        /// <summary>
        /// Sao chép thành đối tượng mới, thuộc tính phức tạp vẫn bị tham chiếu
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Thuộc tính khóa chính của bảng
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class PrimaryKeyAttribute : Attribute { }
        /// <summary>
        /// Thuộc tính trong bảng SQL để tự tăng, định nghĩa để bỏ qua hàm tự tạo khóa
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class ExtraIdAttribute : Attribute { }
        [AttributeUsage(AttributeTargets.Property)]
        public class PersonIdAttribute : Attribute { }
        /// <summary>
        /// Thuộc tính bỏ qua khi insert, update SQL
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class IgnoreAttribute : Attribute { }
        public class PlainTextAttribute : Attribute { }
        public class IgnoreLAttribute : Attribute { }
        public class IgnoreSAttribute : Attribute { }

        /// <summary>
        /// Thuộc tính bảng dữ liệu
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class TableAttribute : Attribute
        {
            public string Name { get; set; }

            public TableAttribute(string name)
            {
                Name = name;
            }
        }
        /// <summary>
        /// Thuoc tinh database name
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class DatabaseAttribute : Attribute
        {
            /// <summary>
            /// Key AppSetting để lấy tên DB
            /// </summary>
            public string AppSettingName { get; set; }

            private string _Name = "";
            public string Name
            {
                get
                {
                    if (string.IsNullOrEmpty(_Name)) _Name = System.Configuration.ConfigurationManager.AppSettings[AppSettingName];
                    return _Name;
                }
                set { _Name = value; }
            }

            public DatabaseAttribute() { }

            public DatabaseAttribute(string name = "", string appSettingName = "")
            {
                Name = name;
                AppSettingName = appSettingName;
            }
        }
        /// <summary>
        /// Thuộc tính encode html
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class HtmlEncodeAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class ListAttribute : Attribute
        {
            public string Name { get; set; }

            public ListAttribute(string name)
            {
                Name = name;
            }
        }
        /// <summary>
        /// Thuộc tính định nghĩa store insert
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class InsertAttribute : Attribute
        {
            public string StoreName { get; set; }
            public string Cols { get; set; }

            public InsertAttribute(string storeName)
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
        /// <summary>
        /// Thuộc tính định nghĩa store select
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class SelectAttribute : Attribute
        {
            public string StoreName { get; set; }
            public string Cols { get; set; }

            public SelectAttribute(string storeName)
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
        /// <summary>
        /// Thuộc tính định nghĩa store update
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class UpdateAttribute : Attribute
        {
            public string StoreName { get; set; }
            public string Cols { get; set; }

            public UpdateAttribute(string storeName)
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
        /// <summary>
        /// Thuộc tính định nghĩa store delete
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class DeleteAttribute : Attribute
        {
            public string StoreName { get; set; }
            public string Cols { get; set; }

            public DeleteAttribute(string storeName)
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
        /// <summary>
        /// Thuộc tính định nghĩa store execute
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public class ExecuteAttribute : Attribute
        {
            public string StoreName { get; set; }
            public string Cols { get; set; }

            public ExecuteAttribute(string storeName)
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
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public class FillAttribute : Attribute
        {
            public string StoreName { get; set; }
            public string Cols { get; set; }

            public FillAttribute(string storeName)
            {
                StoreName = storeName;
            }

            public List<string> getLstCols()
            {
                if (string.IsNullOrEmpty(Cols)) return null;
                List<string> retValue = Cols.Split(',').ToList();
                retValue.ForEach(i => i = i.Trim());
                return retValue;
            }

            internal List<string> GetLstCols()
            {
                throw new NotImplementedException();
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class SchemaAttribute : Attribute
        {
            public string Name { get; set; }

            public SchemaAttribute(string name)
            {
                Name = name;
            }
        }

        public List<string> getProperties()
        {
            List<string> Value = new List<string>();
            var props = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            foreach (var p in props)
            {
                Value.Add(p.Name);
            }
            return Value;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DataSourceAttribute : Attribute
    {
        public int ID { get; set; }

        public DataSourceAttribute(int id)
        { this.ID = id; }
    }

    /// <summary>
    /// Cho phép mở rộng các phương thức xử lý của đối tượng
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// Sao chép thành đối tượng mới, thuộc tính phức tạp vẫn bị tham chiếu
        /// </summary>
        /// <returns></returns>
        public static IList<T> Clone<T>(this IList<T> list) where T : ICloneable
        {
            return list.Select(i => (T)i.Clone()).ToList();
        }

        /// <summary>
        /// Sao chép thành đối tượng mới, thuộc tính phức tạp vẫn bị tham chiếu
        /// </summary>
        /// <returns></returns>
        public static T Clone<T>(this ControllerModel obj)
        {
            return (T)obj.Clone();
        }

        /// <summary>
        /// Sao chép thành đối tượng mới, thuộc tính phức tạp không bị tham chiếu
        /// </summary>
        /// <returns></returns>
        public static T DeepClone<T>(this T obj, JsonSerializerSettings jsonSetting = null)
        {
            string JsonData = JsonConvert.SerializeObject(obj, jsonSetting);
            return JsonConvert.DeserializeObject<T>(JsonData, jsonSetting);
        }

        /// <summary>
        /// Sao chép thành đối tượng mới, thuộc tính phức tạp không bị tham chiếu
        /// </summary>
        /// <returns></returns>
        public static IList<T> DeepClone<T>(this IList<T> obj, JsonSerializerSettings jsonSetting = null)
        {
            string JsonData = JsonConvert.SerializeObject(obj, jsonSetting);
            return JsonConvert.DeserializeObject<IList<T>>(JsonData, jsonSetting);
        }
    }
}
