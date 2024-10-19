using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using FontStyle = System.Drawing.FontStyle;
using QuanLyPhongGym.Controller;
using QuanLyPhongGym.Areas;
using QuanLyPhongGym.Model;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyPhongGym.Pages
{
    public partial class Index : Form
    {
        private DBController _dbController = new DBController();
        private CmmFunc _cmmFunc = new CmmFunc();
        private List<HoiVienModel> _DS_HoiVien;
        private List<SanPhamModel> _DS_SanPham;
        private List<ThietBiModel> _DS_ThietBi;

        public Index()
        {
            InitializeComponent();
            tab_Ctrl.DrawItem += new DrawItemEventHandler(tabCtrl_DrawItem);
        }

        #region Controller Innit
        private void Index_Load(object sender, EventArgs e)
        {
            // Hoi Vien
            tbl_HoiVien.Columns[0].HeaderText = "Mã học viên";
            tbl_HoiVien.Columns[1].HeaderText = "Họ tên";
            tbl_HoiVien.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // San Pham
            tbl_SanPham.Columns[0].HeaderText = "Mã sản phẩm";
            tbl_SanPham.Columns[1].HeaderText = "Tên";
            tbl_SanPham.Columns[2].HeaderText = "Loại";
            tbl_SanPham.Columns[3].HeaderText = "Ngày nhập";
            tbl_SanPham.Columns[4].HeaderText = "Số lượng";
            tbl_SanPham.Columns[5].HeaderText = "Đơn giá";
            tbl_SanPham.Columns[6].HeaderText = "Trọng lượng";
            tbl_SanPham.Columns[7].HeaderText = "Hãng sản xuất";
            tbl_SanPham.Columns[8].HeaderText = "Tình trạng";
            tbl_SanPham.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;


            // Thiet Bi
            tbl_ThietBi.Columns[0].HeaderText = "Mã thiết bị";
            tbl_ThietBi.Columns[1].HeaderText = "Tên";
            tbl_ThietBi.Columns[2].HeaderText = "Loại";
            tbl_ThietBi.Columns[3].HeaderText = "Số lượng";
            tbl_ThietBi.Columns[4].HeaderText = "Số lượng hư";
            tbl_ThietBi.Columns[5].HeaderText = "Tình trạng";
            tbl_ThietBi.Columns[6].HeaderText = "Hãng sản xuất";
            tbl_ThietBi.Columns[7].HeaderText = "Ghi chú";
            tbl_ThietBi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            load_HoiVien();
            load_SanPham();
            load_ThietBi();
        }

        private void tabCtrl_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            TabPage _tabPage = tab_Ctrl.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = tab_Ctrl.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {
                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.White);
                g.FillRectangle(Brushes.DarkCyan, e.Bounds);
            }
            else
            {
                _textBrush = new SolidBrush(e.ForeColor);
                //e.DrawBackground();
                //_textBrush = new SolidBrush(Color.White);
                g.FillRectangle(Brushes.LightBlue, e.Bounds);
            }

            // Use our own font.
            Font _tabFont = new Font("Arial", (float)13.0, FontStyle.Regular, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Near;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));

            // Draw image if available
            int indent = 3;
            Rectangle rect = new Rectangle(e.Bounds.X, e.Bounds.Y + indent, e.Bounds.Width, e.Bounds.Height - indent);
            if (tab_Ctrl.TabPages[e.Index].ImageIndex >= 0)
            {
                Image img = tab_Ctrl.ImageList.Images[tab_Ctrl.TabPages[e.Index].ImageIndex];
                float _x = (rect.X + rect.Width) - img.Width - 2 * indent;
                float _y = ((rect.Height - img.Height) / 2.0f) + rect.Y;
                e.Graphics.DrawImage(img, _x, _y);
            }
        }
        #endregion

        #region Search boxes placeholders
        private void txt_HoiVienSearch_Enter(object sender, EventArgs e)
        {
            if (txt_SearchHV.Text == "Search...")
            {
                txt_SearchHV.Text = "";
                txt_SearchHV.ForeColor = Color.Black;
            }
        }

        private void txt_HoiVienSearch_Leave(object sender, EventArgs e)
        {
            if (txt_SearchHV.Text == "")
            {
                txt_SearchHV.Text = "Search...";
                txt_SearchHV.ForeColor = Color.Gray;
            }
        }

        private void btn_HoiVienSearch_Click(object sender, EventArgs e)
        {
            load_HoiVien(txt_SearchHV.Text);
        }

        private void txt_HoiVienSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                load_HoiVien(txt_SearchHV.Text);
        }

        private void txt_SanPhamSearch_Enter(object sender, EventArgs e)
        {
            if (txt_SearchSP.Text == "Search...")
            {
                txt_SearchSP.Text = "";
                txt_SearchSP.ForeColor = Color.Black;
            }
        }

        private void txt_SanPhamSearch_Leave(object sender, EventArgs e)
        {
            if (txt_SearchSP.Text == "")
            {
                txt_SearchSP.Text = "Search...";
                txt_SearchSP.ForeColor = Color.Gray;
            }
        }

        private void txt_SanPhamSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                load_SanPham(txt_SearchSP.Text);
        }

        private void btn_SanPhamSearch_Click(object sender, EventArgs e)
        {
            load_SanPham(txt_SearchSP.Text);
        }

        private void txt_ThietBiSearch_Enter(object sender, EventArgs e)
        {
            if (txt_SearchTB.Text == "Search...")
            {
                txt_SearchTB.Text = "";
                txt_SearchTB.ForeColor = Color.Black;
            }
        }

        private void txt_ThietBiSearch_Leave(object sender, EventArgs e)
        {
            if (txt_SearchTB.Text == "")
            {
                txt_SearchTB.Text = "Search...";
                txt_SearchTB.ForeColor = Color.Gray;
            }
        }

        private void txt_ThietBiSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                load_ThietBi(txt_SearchTB.Text);
        }

        private void btn_ThietBiSearch_Click(object sender, EventArgs e)
        {
            load_ThietBi(txt_SearchTB.Text);
        }
        #endregion

        #region Search And Load data By keyword
        private void load_HoiVien(string keyword = null)
        {
            tbl_HoiVien.Rows.Clear();
            _DS_HoiVien = _dbController.SelectAll<HoiVienModel>(new HoiVienModel());
            if (!string.IsNullOrEmpty(keyword) && keyword != "Search...")
            {
                string strKeyword = keyword.ToLower().Trim();
                _DS_HoiVien = _DS_HoiVien.Where(item =>
                        item.MaHV.ToLower().Contains(strKeyword)
                    ||
                        item.HoTen.ToLower().Contains(strKeyword)
                    ||
                        item.SDT.ToLower().Contains(strKeyword)
                ).ToList();
            }

            if (_DS_HoiVien != null)
            {
                foreach (HoiVienModel hoiVien in _DS_HoiVien)
                {
                    // Tạo một mảng các giá trị cho từng cột
                    string[] row = new string[]
                    {
                        hoiVien.MaHV,
                        hoiVien.HoTen
                    };
                    tbl_HoiVien.Rows.Add(row);
                }
            }
        }

        private void load_SanPham(string keyword = null)
        {
            tbl_SanPham.Rows.Clear();
            _DS_SanPham = _dbController.SelectAll<SanPhamModel>(new SanPhamModel());
            _DS_SanPham = _DS_SanPham.OrderByDescending(item => item.NgayNhap).ToList();

            if (!string.IsNullOrEmpty(keyword) && keyword != "Search...")
            {
                string strKeyword = keyword.ToLower().Trim();
                _DS_SanPham = _DS_SanPham.Where(item =>
                        item.MaSP.ToLower().Contains(strKeyword)
                    ||
                        item.Ten.ToLower().Contains(strKeyword)
                    ||
                        item.Loai.ToLower().Contains(strKeyword)
                    ||
                        item.HangSX.ToLower().Contains(strKeyword)
                    ||
                        item.TinhTrang.ToLower().Contains(strKeyword)
                ).ToList();
            }

            if (_DS_SanPham != null)
            {
                foreach (SanPhamModel sanPham in _DS_SanPham)
                {
                    // Tạo một mảng các giá trị cho từng cột
                    string[] row = new string[]
                    {
                        sanPham.MaSP,
                        sanPham.Ten,
                        sanPham.Loai,
                        sanPham.NgayNhap.HasValue ? sanPham.NgayNhap.Value.ToString("dd-MM-yyyy HH:mm") : string.Empty,
                        sanPham.SoLuong.HasValue ? sanPham.SoLuong.ToString() : string.Empty,
                        sanPham.DonGia.HasValue ? sanPham.DonGia.Value.ToString("N2", new CultureInfo("de-DE")) : string.Empty,
                        sanPham.TrongLuong,
                        sanPham.HangSX,
                        sanPham.TinhTrang
                    };
                    tbl_SanPham.Rows.Add(row);
                }
            }
        }

        private void load_ThietBi(string keyword = null)
        {
            tbl_ThietBi.Rows.Clear();
            _DS_ThietBi = _dbController.SelectAll<ThietBiModel>(new ThietBiModel());
            if (!string.IsNullOrEmpty(keyword) && keyword != "Search...")
            {
                string strKeyword = keyword.ToLower().Trim();
                _DS_ThietBi = _DS_ThietBi.Where(item =>
                        item.MaTB.ToLower().Contains(strKeyword)
                    ||
                        item.Ten.ToLower().Contains(strKeyword)
                    ||
                        item.Loai.ToLower().Contains(strKeyword)
                    ||
                        item.HangSX.ToLower().Contains(strKeyword)
                ).ToList();
            }
            if (_DS_ThietBi != null)
            {
                foreach (ThietBiModel thietBi in _DS_ThietBi)
                {
                    // Tạo một mảng các giá trị cho từng cột
                    string[] row = new string[]
                    {
                        thietBi.MaTB,
                        thietBi.Ten,
                        thietBi.Loai,
                        thietBi.SoLuong.HasValue ? thietBi.SoLuong.ToString() : string.Empty,
                        thietBi.SoLuongHu.HasValue ? thietBi.SoLuongHu.ToString() : string.Empty,
                        thietBi.TinhTrang,
                        thietBi.HangSX,
                        thietBi.GhiChu,

                    };
                    tbl_ThietBi.Rows.Add(row);
                }
            }
        }
        #endregion

        #region Method Hoi Vien
        private void get_HoiVien_ByRowTable()
        {
            DataGridViewRow row = tbl_HoiVien.CurrentRow;
            string strMaHV = row.Cells[0].Value + string.Empty;
            if (!string.IsNullOrEmpty(strMaHV))
            {
                HoiVienModel hoiVien = new HoiVienModel { MaHV = strMaHV };
                hoiVien = _dbController.Select<HoiVienModel>(hoiVien);
                if (hoiVien != null)
                {
                    txt_MaHV.Text = hoiVien.MaHV;
                    txt_HoTen.Text = hoiVien.HoTen;
                    _cmmFunc.SelectCbbByText(cbb_GioiTinh, hoiVien.GioiTinh);
                    _cmmFunc.SelectCbbByText(cbb_GoiTap, hoiVien.GoiTap);
                    txt_SDT.Text = hoiVien.SDT;
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
        }

        private void btn_SuaHoiVien_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = tbl_HoiVien.CurrentRow;
            string strMaHV = row.Cells[0].Value + string.Empty;
            FormHoiVien fadd = new FormHoiVien(strMaHV);
            fadd.ShowDialog();
            load_HoiVien();
        }

        private void btn_ThemHoiVien_Click(object sender, EventArgs e)
        {
            FormHoiVien fadd = new FormHoiVien(null);
            fadd.ShowDialog();
            load_HoiVien();
        }

        private void btn_XoaHoiVien_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn chác chắn muốn xóa dữ liệu?", "Thông báo", MessageBoxButtons.OKCancel) != DialogResult.OK)
                return;

            try
            {
                DataGridViewRow row = tbl_HoiVien.CurrentRow;
                string strMaHV = row.Cells[0].Value + string.Empty;
                if (!string.IsNullOrEmpty(strMaHV))
                {
                    HoiVienModel hoiVien = new HoiVienModel { MaHV = strMaHV };
                    hoiVien = _dbController.Select<HoiVienModel>(hoiVien);
                    if (hoiVien != null)
                    {
                        _dbController.Delete(hoiVien);
                        MessageBox.Show("Xóa thành công!");
                    }
                }    
                load_HoiVien();
            }
            catch (Exception)
            {
                MessageBox.Show("Xóa thất bại!");
            }
        }

        private void btn_GiaHanHoiVien_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = tbl_HoiVien.CurrentRow;
            string strMaHV = row.Cells[0].Value + string.Empty;
            FormGiaHanHV fGiaHan = new FormGiaHanHV(strMaHV);
            fGiaHan.ShowDialog();
            load_HoiVien();
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
                case "Thường":
                    dt = dt.AddMonths(7);
                    break;
                case "VIP":
                    dt = dt.AddMonths(13);
                    break;
            }
            lbl_HetHan.Text = dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        private void cell_HoiVien_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            btn_GiaHanHV.Enabled = true;
            get_HoiVien_ByRowTable();
        }
        #endregion

        #region Method Sản Phẩm
        private void get_SanPham_ByRowTable()
        {
            DataGridViewRow row = tbl_SanPham.CurrentRow;
            string strMaSP = row.Cells[0].Value + string.Empty;
            if (!string.IsNullOrEmpty(strMaSP))
            {
                SanPhamModel sanPham = new SanPhamModel { MaSP = strMaSP };
                sanPham = _dbController.Select<SanPhamModel>(sanPham);
                if (sanPham != null)
                {
                    txt_TenSP.Text = sanPham.Ten;
                    txt_LoaiSP.Text = sanPham.Loai;
                    txt_SoLuongSP.Text = sanPham.SoLuong.HasValue ? sanPham.SoLuong.ToString() : string.Empty;
                    txt_DonGiaSP.Text = sanPham.DonGia.HasValue ? sanPham.DonGia.Value.ToString("N2", new CultureInfo("de-DE")) : string.Empty;
                    lbl_TinhTrangSP.Text = sanPham.TinhTrang;
                    if (sanPham.HinhAnh != null && sanPham.HinhAnh.Length > 0)
                    {
                        MemoryStream image = new MemoryStream(sanPham.HinhAnh);
                        picbox_SP.Image = Image.FromStream(image);
                    }
                    else
                        picbox_SP.Image = null;
                }
            }
        }

        private void cell_SanPham_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            get_SanPham_ByRowTable();
        }

        private void btn_ThemSP_Click(object sender, EventArgs e)
        {
            FormSanPham fadd = new FormSanPham(null);
            fadd.ShowDialog();
            load_SanPham();
        }

        private void btn_XoaSP_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn chác chắn muốn xóa dữ liệu?", "Thông báo", MessageBoxButtons.OKCancel) != DialogResult.OK)
                return;

            try
            {
                DataGridViewRow row = tbl_SanPham.CurrentRow;
                string strMaSP = row.Cells[0].Value + string.Empty;
                if (!string.IsNullOrEmpty(strMaSP))
                {
                    SanPhamModel sanPham = new SanPhamModel { MaSP = strMaSP };
                    sanPham = _dbController.Select<SanPhamModel>(sanPham);
                    if (sanPham != null)
                    {
                        _dbController.Delete(sanPham);
                        MessageBox.Show("Xóa thành công!");
                    }
                }
                load_SanPham();
            }
            catch (Exception)
            {
                MessageBox.Show("Xóa thất bại!");
            }
        }

        private void btn_SuaSP_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = tbl_SanPham.CurrentRow;
            string strMaSP = row.Cells[0].Value + string.Empty;
            FormSanPham fadd = new FormSanPham(strMaSP);
            fadd.ShowDialog();
            load_SanPham();
        }
        #endregion

        #region Method Thiết Bi
        private void get_ThietBi_ByRowTable()
        {
            DataGridViewRow row = tbl_ThietBi.CurrentRow;
            string strMaTB = row.Cells[0].Value + string.Empty;
            if (!string.IsNullOrEmpty(strMaTB))
            {
                ThietBiModel thietBi = new ThietBiModel { MaTB = strMaTB };
                thietBi = _dbController.Select<ThietBiModel>(thietBi);
                if (thietBi != null)
                {
                    txt_TenTB.Text = thietBi.Ten;
                    txt_LoaiTB.Text = thietBi.Loai;
                    txt_SoLuongTB.Text = thietBi.SoLuong.HasValue ? thietBi.SoLuong.ToString() : string.Empty;
                    txt_HangSXTB.Text = thietBi.HangSX;
                    lbl_TinhTrangTB.Text = thietBi.TinhTrang;
                    if (thietBi.HinhAnh != null && thietBi.HinhAnh.Length > 0)
                    {
                        MemoryStream image = new MemoryStream(thietBi.HinhAnh);
                        picbox_TB.Image = Image.FromStream(image);
                    }
                    else
                        picbox_TB.Image = null;
                }
            }
        }

        private void cell_ThietBi_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            get_ThietBi_ByRowTable();
        }

        private void btn_ThemTB_Click(object sender, EventArgs e)
        {
            FormThietBi fadd = new FormThietBi(null);
            fadd.ShowDialog();
            load_ThietBi();
        }

        private void btn_XoaTB_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn chác chắn muốn xóa dữ liệu?", "Thông báo", MessageBoxButtons.OKCancel) != DialogResult.OK)
                return;

            try
            {
                DataGridViewRow row = tbl_ThietBi.CurrentRow;
                string strMaTB = row.Cells[0].Value + string.Empty;
                if (!string.IsNullOrEmpty(strMaTB))
                {
                    ThietBiModel thietBi = new ThietBiModel { MaTB = strMaTB };
                    thietBi = _dbController.Select<ThietBiModel>(thietBi);
                    if (thietBi != null)
                    {
                        _dbController.Delete(thietBi);
                        MessageBox.Show("Xóa thành công!");
                    }
                }
                load_ThietBi();
            }
            catch (Exception)
            {
                MessageBox.Show("Xóa thất bại!");
            }
        }

        private void btn_SuaTB_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = tbl_ThietBi.CurrentRow;
            string strMaTB = row.Cells[0].Value + string.Empty;
            FormThietBi fadd = new FormThietBi(strMaTB);
            fadd.ShowDialog();
            load_ThietBi();
        }
        #endregion
    }
}
