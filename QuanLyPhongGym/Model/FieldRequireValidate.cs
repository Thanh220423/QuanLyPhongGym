namespace QuanLyPhongGym.Model
{
    public class FieldRequireValidate
    {
        public string ObjText { get; set; }
        public string ObjValue { get; set; }
        public bool IsRequire { get; set; } = false;
        public bool IsValidate { get; set; } = true;
        public string ValidateText { get; set; }
    }
}
