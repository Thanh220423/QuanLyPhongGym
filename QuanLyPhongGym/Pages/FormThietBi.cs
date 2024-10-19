using QuanLyPhongGym.Areas;
using QuanLyPhongGym.Controller;
using QuanLyPhongGym.Model;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuanLyPhongGym.Pages
{
    public partial class FormThietBi : Form
    {
        private DBController _dbController = new DBController();
        private CmmFunc _cmmFunc = new CmmFunc();
        private string _ImgTBPath;
        private string _MaTB;

        public FormThietBi(string MaTB)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(MaTB))
            {
                _MaTB = MaTB;
                ThietBiModel thietBi = new ThietBiModel { MaTB = MaTB };
                thietBi = _dbController.Select<ThietBiModel>(thietBi);
                if (thietBi != null)
                {
                    txt_TenTB.Text = thietBi.Ten;
                    _cmmFunc.SelectCbbByText(cbb_LoaiTB, thietBi.Loai);
                    txt_SoLuongTB.Text = thietBi.SoLuong.HasValue ? thietBi.SoLuong.ToString() : string.Empty;
                    txt_HangSXTB.Text = thietBi.HangSX;
                    _cmmFunc.SelectCbbByText(cbb_TinhTrangTB, thietBi.TinhTrang);
                    txt_SoLuongHuTB.Text = thietBi.SoLuongHu.HasValue ? thietBi.SoLuongHu.ToString() : string.Empty;
                    txt_GhiChuTB.Text = thietBi.GhiChu;

                    if (thietBi.HinhAnh != null && thietBi.HinhAnh.Length > 0)
                    {
                        MemoryStream image = new MemoryStream(thietBi.HinhAnh);
                        picbox_TB.Image = Image.FromStream(image);
                    }
                    else
                        picbox_TB.Image = null;
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
            picbox_TB.ImageLocation = null;
            cbb_LoaiTB.SelectedIndex = 0;
            cbb_TinhTrangTB.SelectedIndex = 0;
        }

        private Byte[] ImageToByteArray(string _ImgTB)
        {
            Byte[] img = null;
            FileStream fs = new FileStream(_ImgTB, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            img = br.ReadBytes((int)fs.Length);

            return img;
        }

        private void btn_Luu_Click(object sender, EventArgs e)
        {
            try
            {
                ThietBiModel thietBi = null;
                if (!string.IsNullOrEmpty(_MaTB))
                {
                    thietBi = new ThietBiModel { MaTB = _MaTB };
                    thietBi = _dbController.Select<ThietBiModel>(thietBi);
                }
                else
                {
                    thietBi = new ThietBiModel();
                    thietBi.MaTB = _dbController.CreateStrNewID("TB", thietBi);
                }

                thietBi.Ten = txt_TenTB.Text;
                thietBi.Loai = cbb_LoaiTB.SelectedItem.ToString();
                thietBi.SoLuong = !string.IsNullOrEmpty(txt_SoLuongTB.Text) ? int.Parse(txt_SoLuongTB.Text) : 0;
                thietBi.HangSX = txt_HangSXTB.Text;
                thietBi.TinhTrang = cbb_TinhTrangTB.SelectedItem.ToString();
                thietBi.GhiChu = txt_GhiChuTB.Text;
                thietBi.SoLuongHu = !string.IsNullOrEmpty(txt_SoLuongHuTB.Text) ? int.Parse(txt_SoLuongHuTB.Text) : 0;

                if (picbox_TB.Image != null && _ImgTBPath != null)
                    thietBi.HinhAnh = ImageToByteArray(_ImgTBPath);

                if (!string.IsNullOrEmpty(_MaTB))
                    _dbController.Update(thietBi);
                else
                    _dbController.Insert(thietBi);

                if (MessageBox.Show(!string.IsNullOrEmpty(_MaTB) ? "Cập nhật thành công!" : "Thêm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Thông báo");
            }
        }

        private void picbox_TB_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Chọn ảnh đại diện";
                dlg.Filter = "JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|All Files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _ImgTBPath = dlg.FileName;
                    picbox_TB.ImageLocation = _ImgTBPath;
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
