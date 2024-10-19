using QuanLyPhongGym.Areas;
using QuanLyPhongGym.Controller;
using QuanLyPhongGym.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace QuanLyPhongGym.Pages
{
    public partial class FormHoiVien : Form
    {
        private DBController _dbController = new DBController();
        private CmmFunc _cmmFunc = new CmmFunc();
        private string _ImgHVPath;
        private string _MaHV;

        public FormHoiVien(string MaHV)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(MaHV))
            {
                groupbox_HV.Text = "CẬP NHẬT HỘI VIÊN";
                _MaHV = MaHV;
                HoiVienModel hoiVien = new HoiVienModel { MaHV = MaHV };
                hoiVien = _dbController.Select<HoiVienModel>(hoiVien);
                if (hoiVien != null)
                {
                    txt_HoTen.Text = hoiVien.HoTen;
                    txt_SDT.Text = hoiVien.SDT;
                    _cmmFunc.SelectCbbByText(cbb_GioiTinh, hoiVien.GioiTinh);
                    _cmmFunc.SelectCbbByText(cbb_GoiTap, hoiVien.GoiTap);
                    lbl_HetHan.Text = hoiVien.NgayHetHan.HasValue ? hoiVien.NgayHetHan.Value.ToString("dd/MM/yyyy") : string.Empty;

                    if (hoiVien.HinhAnh != null && hoiVien.HinhAnh.Length > 0)
                    {
                        MemoryStream image = new MemoryStream(hoiVien.HinhAnh);
                        picbox_HV.Image = Image.FromStream(image);
                    }
                    else
                        picbox_HV.Image = null;
                }
            }
            else
            {
                groupbox_HV.Text = "THÊM HỘI VIÊN";
                ClearField();
            }
        }

        private void ClearField()
        {
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                    if (control is TextBox)
                        (control as TextBox).Clear();
                    else
                        func(control.Controls);
            };
            func(Controls);
            picbox_HV.ImageLocation = null;
            cbb_GoiTap.SelectedIndex = 0;
            cbb_GioiTinh.SelectedIndex = 0;
            lbl_HetHan.Text = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        private Byte[] ImageToByteArray(string _ImgHV)
        {
            Byte[] img = null;
            FileStream fs = new FileStream(_ImgHV, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            img = br.ReadBytes((int)fs.Length);

            return img;
        }

        private void btn_Luu_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_cmmFunc.FieldRequire(AddFieldValidate(new List<string> { "hoten", "sdt" })))
                    return;

                HoiVienModel hoiVien = null;
                if (!string.IsNullOrEmpty(_MaHV))
                {
                    hoiVien = new HoiVienModel { MaHV = _MaHV };
                    hoiVien = _dbController.Select<HoiVienModel>(hoiVien);
                }
                else
                {
                    hoiVien = new HoiVienModel();
                    hoiVien.MaHV = _dbController.CreateStrNewID("KH", hoiVien);
                }

                hoiVien.HoTen = txt_HoTen.Text;
                hoiVien.GioiTinh = cbb_GioiTinh.Text;
                hoiVien.SDT = txt_SDT.Text;
                hoiVien.GoiTap = cbb_GoiTap.Text;

                DateTime date;
                DateTime.TryParseExact(lbl_HetHan.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                hoiVien.NgayHetHan = date;

                if (picbox_HV.Image != null && _ImgHVPath != null)
                    hoiVien.HinhAnh = ImageToByteArray(_ImgHVPath);

                if (!string.IsNullOrEmpty(_MaHV))
                    _dbController.Update(hoiVien, new List<string> { "HoTen", "GioiTinh", "SDT", "GoiTap", "NgayHetHan", "HinhAnh" });
                else
                    _dbController.Insert(hoiVien);

                if (MessageBox.Show(!string.IsNullOrEmpty(_MaHV) ? "Cập nhật thành công!" : "Thêm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Thông báo");
            }
        }


        private void cbb_GoiTap_TextChanged(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            switch (cbb_GoiTap.Text)
            {
                case "1 tháng":
                    dt = dt.AddMonths(1);
                    break;
                case "3 tháng":
                    dt = dt.AddMonths(3);
                    break;
                case "VIP":
                    dt = dt.AddMonths(13);
                    break;
                case "Thường":
                    dt = dt.AddMonths(7);
                    break;
            }
            lbl_HetHan.Text = dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        private void picbox_HV_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Chọn ảnh sản phẩm";
                dlg.Filter = "JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|All Files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _ImgHVPath = dlg.FileName;
                    picbox_HV.ImageLocation = _ImgHVPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_RefeshField_Click(object sender, EventArgs e)
        {
            ClearField();
        }

        private List<FieldRequireValidate> AddFieldValidate(List<string> lstKey)
        {
            List<FieldRequireValidate> fields = new List<FieldRequireValidate>();
            FieldRequireValidate field = null;
            foreach (string strKey in lstKey)
            {
                var Key = strKey.ToLower();
                switch (Key)
                {
                    case "hoten":
                        {
                            field = new FieldRequireValidate();
                            field.ObjText = "Họ tên";
                            field.ObjValue = txt_HoTen.Text;
                            field.IsRequire = true;
                            fields.Add(field);
                            break;
                        }
                    case "sdt":
                        {
                            field = new FieldRequireValidate();
                            field.ObjText = "SĐT";
                            field.ObjValue = txt_SDT.Text;
                            field.IsRequire = true;
                            field.IsValidate = _cmmFunc.Validate_SDT(txt_SDT.Text);
                            field.ValidateText = "Số điện thoại không đúng, vui lòng kiểm tra lại.";
                            fields.Add(field);
                            break;
                        }
                }
            }
            return fields;
        }
    }
}
