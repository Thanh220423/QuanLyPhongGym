﻿using System;
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
        private string imgLoc;
        private bool isAnotherImage = false;

        public Index()
        {
            InitializeComponent();
            tab_Ctrl.DrawItem += new DrawItemEventHandler(tabCtrl_DrawItem);
        }

        // Search boxes placeholders
        #region Search
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

        private void btn_SearchHV_Click(object sender, EventArgs e)
        {
            load_HoiVien(txt_SearchHV.Text);
        }

        private void txt_SearchHV_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                load_HoiVien(txt_SearchHV.Text);
        }

        private void txtSearchSP_Enter(object sender, EventArgs e)
        {
            if (txt_SearchSP.Text == "Search...")
            {
                txt_SearchSP.Text = "";
                txt_SearchSP.ForeColor = Color.Black;
            }
        }

        private void txtSearchSP_Leave(object sender, EventArgs e)
        {
            if (txt_SearchSP.Text == "")
            {
                txt_SearchSP.Text = "Search...";
                txt_SearchSP.ForeColor = Color.Gray;
            }
        }

        private void txtSearchTB_Enter(object sender, EventArgs e)
        {
            if (txt_SearchTB.Text == "Search...")
            {
                txt_SearchTB.Text = "";
                txt_SearchTB.ForeColor = Color.Black;
            }
        }

        private void txtSearchTB_Leave(object sender, EventArgs e)
        {
            if (txt_SearchTB.Text == "")
            {
                txt_SearchTB.Text = "Search...";
                txt_SearchTB.ForeColor = Color.Gray;
            }
        }
        #endregion

        // Support functions
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

        public Byte[] ImageToByteArray(string imgLocation)
        {
            Byte[] img = null;
            FileStream fs = new FileStream(imgLocation, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            img = br.ReadBytes((int)fs.Length);
            return img;
        }

        private void getCurrentRowSPInfo()
        {
            DataGridViewRow row = tbl_SanPham.CurrentRow;
            //sanPhamDTO.ID_SP = row.Cells["id_sp"].Value.ToString();
            //sanPhamDTO.Ten = row.Cells["ten"].Value.ToString();
            //sanPhamDTO.Loai = row.Cells["loai"].Value.ToString();
            //sanPhamDTO.NgayNhap = (DateTime)row.Cells["ngaynhap"].Value;
            //sanPhamDTO.SoLuong = Convert.ToInt32(row.Cells["soluong"].Value);
            //sanPhamDTO.DonGia = row.Cells["dongia"].Value.ToString();
            //sanPhamDTO.TrongLuong = row.Cells["trongluong"].Value.ToString();
            //sanPhamDTO.HangSX = row.Cells["hangsx"].Value.ToString();
            //sanPhamDTO.TinhTrang = row.Cells["tinhtrang"].Value.ToString();
            //sanPhamDTO.HinhAnh = (Byte[])row.Cells["hinhanh"].Value;
        }

        private void getCurrentRowTBInfo()
        {
            DataGridViewRow row = tbl_ThietBi.CurrentRow;
            //thietBiDTO.ID_TB = row.Cells["id_tb"].Value.ToString();
            //thietBiDTO.Ten = row.Cells["tentb"].Value.ToString();
            //thietBiDTO.Loai = row.Cells["loaitb"].Value.ToString();
            //thietBiDTO.SoLuong = Convert.ToInt32(row.Cells["soluongtb"].Value);
            //thietBiDTO.TinhTrang = row.Cells["tinhtrangtb"].Value.ToString();
            //thietBiDTO.SoLuongHu = Convert.ToInt32(row.Cells["soluonghu"].Value);
            //thietBiDTO.HangSX = row.Cells["hangsxtb"].Value.ToString();
            //thietBiDTO.GhiChu = row.Cells["ghichu"].Value.ToString();
            //thietBiDTO.HinhAnh = (Byte[])row.Cells["hinhanh"].Value;
        }

        private void getSPRowToTxtBOX(DataGridViewRow row)
        {
            txt_TenSP.Text = row.Cells["ten"].Value.ToString();
            txt_LoaiSP.Text = row.Cells["loai"].Value.ToString();
            txt_SoLuongSP.Text = row.Cells["soluong"].Value.ToString();
            txt_DonGiaSP.Text = row.Cells["dongia"].Value.ToString();

            string tt = row.Cells["tinhtrang"].Value.ToString();
            lbl_TinhTrangSP.Text = tt;
            if (tt == "Còn hàng")
                lbl_TinhTrangSP.ForeColor = Color.MediumBlue;
            else if (tt == "Hết hàng")
                lbl_TinhTrangSP.ForeColor = Color.Red;

            Byte[] data = new Byte[0];
            data = (Byte[])(row.Cells["hinhanh"].Value);
            MemoryStream mem = new MemoryStream(data);
            picbox_SP.Image = Image.FromStream(mem);
        }

        private void getTBRowToTxtBOX(DataGridViewRow row)
        {
            txt_TenTB.Text = row.Cells[1].Value.ToString();
            txt_LoaiTB.Text = row.Cells[2].Value.ToString();
            txt_SoLuongTB.Text = row.Cells[3].Value.ToString();
            txt_HangSXTB.Text = row.Cells[4].Value.ToString();

            string tt = row.Cells[5].Value.ToString();
            lbl_TinhTrangTB.Text = tt;
            if (tt == "Mới")
                lbl_TinhTrangTB.ForeColor = Color.MediumBlue;
            else if (tt == "Tốt")
                lbl_TinhTrangTB.ForeColor = Color.Green;
            else if (tt == "Hư")
                lbl_TinhTrangTB.ForeColor = Color.Red;

            Byte[] data = new Byte[0];
            data = (Byte[])(row.Cells["hinhanh"].Value);
            MemoryStream mem = new MemoryStream(data);
            picbox_TB.Image = Image.FromStream(mem);
        }

        // Events
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
            if (MessageBox.Show("Bạn chác chắn muốn xóa dữ liệu?", "Thông báo", MessageBoxButtons.OKCancel)!= DialogResult.OK)
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
                        MessageBox.Show("Xóa THÀNH CÔNG!");
                    }
                }    
                load_HoiVien();
            }
            catch (Exception)
            {
                MessageBox.Show("Xóa THẤT BẠI!");
            }
        }

        private void btn_GiaHanHoiVien_Click(object sender, EventArgs e)
        {
            DataGridViewRow curRow = tbl_HoiVien.CurrentRow;
            FormGiaHanHV fGiaHan = new FormGiaHanHV(curRow);
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
                case "VIP":
                    dt = dt.AddMonths(13);
                    break;
                case "Thường":
                    dt = dt.AddMonths(7);
                    break;
            }
            lbl_HetHan.Text = dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        private void txt_SDT_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void btn_CapNhatHoiVien_Click(object sender, EventArgs e)
        {

        }

        private void cell_HoiVien_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            btn_GiaHanHV.Enabled = true;
            get_HoiVien_ByRowTable();
        }

        private void dtgvSanPham_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = tbl_SanPham.CurrentRow;
            getSPRowToTxtBOX(row);
        }

        private void btnThemSP_Click(object sender, EventArgs e)
        {
            DataGridViewRow lastRow = tbl_SanPham.Rows[tbl_SanPham.Rows.Count - 1];
            string lastRowID = lastRow.Cells["id_sp"].Value.ToString();
            FormSanPham fadd = new FormSanPham(lastRow, lastRowID);
            fadd.ShowDialog();
            load_SanPham();
        }

        private void btnXoaSP_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Chấp nhận xóa dữ liệu ?", "Thông báo", MessageBoxButtons.OKCancel)
                != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                getCurrentRowSPInfo();
                //sanPhamCTL.SanPham = sanPhamDTO;
                //sanPhamCTL.delete();

                MessageBox.Show("Xóa THÀNH CÔNG!");
                load_SanPham();
            }
            catch (Exception)
            {
                MessageBox.Show("Xóa THẤT BẠI!");
            }
        }

        private void btnSuaSP_Click(object sender, EventArgs e)
        {
            DataGridViewRow curRow = tbl_SanPham.CurrentRow;
            FormSanPham fEdit = new FormSanPham(curRow, null);
            fEdit.ShowDialog();
            load_SanPham();
        }

        private void txtSearchSP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                load_SanPham(txt_SearchSP.Text);
        }

        private void btnSearchSP_Click(object sender, EventArgs e)
        {
            load_SanPham(txt_SearchSP.Text);
        }

        private void dtgvThietBi_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = tbl_ThietBi.CurrentRow;
            getTBRowToTxtBOX(row);
        }

        private void btnThemTB_Click(object sender, EventArgs e)
        {
            //int soMay = thietBiCTL.countTBType("Máy");
            //int soTa = thietBiCTL.countTBType("Tạ");
            //fThemTB fadd = new fThemTB(soMay, soTa);
            //fadd.ShowDialog();
            load_ThietBi();
        }

        private void btnXoaTB_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Chấp nhận xóa dữ liệu ?", "Thông báo", MessageBoxButtons.OKCancel)
                != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                getCurrentRowTBInfo();
                //thietBiCTL.ThietBi = thietBiDTO;
                //thietBiCTL.delete();

                MessageBox.Show("Xóa THÀNH CÔNG!");
                load_ThietBi();
            }
            catch (Exception)
            {
                MessageBox.Show("Xóa THẤT BẠI!");
            }
        }

        private void btnSuaTB_Click(object sender, EventArgs e)
        {
            DataGridViewRow curRow = tbl_ThietBi.CurrentRow;
            //fSuaTB fEdit = new fSuaTB(curRow);
            //fEdit.ShowDialog();
            load_ThietBi();
        }

        private void txtSearchTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                load_ThietBi(txt_SearchTB.Text);
        }

        private void btnSearchTB_Click(object sender, EventArgs e)
        {
            load_ThietBi(txt_SearchTB.Text);
        }

        private void picBoxHV_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Chọn ảnh đại diện";
                dlg.Filter = "JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|All Files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imgLoc = dlg.FileName;
                    picbox_HV.ImageLocation = imgLoc;
                    //hoiVienDTO.HinhAnh = ImageToByteArray(imgLoc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
