using QuanLyPhongGym.Areas;
using QuanLyPhongGym.Controller;
using QuanLyPhongGym.Model;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuanLyPhongGym.Pages
{
    public partial class FormSanPham : Form
    {
        private DBController _dbController = new DBController();
        private CmmFunc _cmmFunc = new CmmFunc();
        private string _ImgSPPath;
        private string _MaSP;

        public FormSanPham(string MaSP)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(MaSP))
            {
                _MaSP = MaSP;
                SanPhamModel sanPham = new SanPhamModel { MaSP = MaSP };
                sanPham = _dbController.Select<SanPhamModel>(sanPham);
                if (sanPham != null)
                {
                    txt_TenSP.Text = sanPham.Ten;
                    _cmmFunc.SelectCbbByText(cbb_LoaiSP, sanPham.Loai);
                    date_NgayNhap.Value = sanPham.NgayNhap.Value;
                    txt_SoLuongSP.Text = sanPham.SoLuong.HasValue ? sanPham.SoLuong.ToString() : string.Empty;
                    txt_DonGiaSP.Text = sanPham.DonGia.HasValue ? sanPham.DonGia.Value.ToString() : string.Empty;
                    txt_TrongLuongSP.Text = sanPham.TrongLuong;
                    txt_HangSXSP.Text = sanPham.HangSX;
                    _cmmFunc.SelectCbbByText(cbb_TinhTrangSP, sanPham.TinhTrang);

                    if (sanPham.HinhAnh != null && sanPham.HinhAnh.Length > 0)
                    {
                        MemoryStream image = new MemoryStream(sanPham.HinhAnh);
                        picbox_SP.Image = Image.FromStream(image);
                    }
                    else
                        picbox_SP.Image = null;
                }
            }
            else
                ClearField();
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
            picbox_SP.ImageLocation = null;
            cbb_LoaiSP.SelectedIndex = 0;
            cbb_TinhTrangSP.SelectedIndex = 0;
            date_NgayNhap.Value = DateTime.Now;
        }

        private Byte[] ImageToByteArray(string _ImgSP)
        {
            Byte[] img = null;
            FileStream fs = new FileStream(_ImgSP, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            img = br.ReadBytes((int)fs.Length);

            return img;
        }

        private void btn_Luu_Click(object sender, EventArgs e)
        {
            try
            {
                SanPhamModel sanPham = null;
                if (!string.IsNullOrEmpty(_MaSP))
                {
                    sanPham = new SanPhamModel { MaSP = _MaSP };
                    sanPham = _dbController.Select<SanPhamModel>(sanPham);
                }
                else
                {
                    sanPham = new SanPhamModel();
                    sanPham.MaSP = _dbController.CreateStrNewID("SP", sanPham);
                }

                sanPham.Ten = txt_TenSP.Text;
                sanPham.Loai = cbb_LoaiSP.SelectedItem.ToString();
                sanPham.TrongLuong = txt_TrongLuongSP.Text;
                sanPham.HangSX = txt_HangSXSP.Text;
                sanPham.TinhTrang = cbb_TinhTrangSP.SelectedItem.ToString();
                sanPham.NgayNhap = date_NgayNhap.Value;
                sanPham.SoLuong = !string.IsNullOrEmpty(txt_SoLuongSP.Text) ? int.Parse(txt_SoLuongSP.Text) : 0;

                if (!string.IsNullOrEmpty(txt_DonGiaSP.Text))
                    sanPham.DonGia = decimal.Parse(txt_DonGiaSP.Text);

                if (picbox_SP.Image != null && _ImgSPPath != null)
                    sanPham.HinhAnh = ImageToByteArray(_ImgSPPath);

                if (!string.IsNullOrEmpty(_MaSP))
                    _dbController.Update(sanPham);
                else
                    _dbController.Insert(sanPham);

                if (MessageBox.Show(!string.IsNullOrEmpty(_MaSP) ? "Cập nhật thành công!" : "Thêm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Thông báo");
            }
        }

        private void picbox_SP_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Chọn ảnh đại diện";
                dlg.Filter = "JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|All Files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _ImgSPPath = dlg.FileName;
                    picbox_SP.ImageLocation = _ImgSPPath;
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
    }
}
