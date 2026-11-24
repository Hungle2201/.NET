using System;
using System.Collections.Generic;
using QuanLyThuVien.DAL;
using QuanLyThuVien.DTO;

namespace QuanLyThuVien.BLL
{
    /// <summary>Nghiệp vụ quản lý sách: thêm, sửa, xóa, tìm kiếm, kiểm tra còn sách.</summary>
    public class SachBLL
    {
        private readonly ISachDAL _sachDal;
        private readonly ILoaiSachDAL _loaiDal;

        public SachBLL(ISachDAL sachDal, ILoaiSachDAL loaiDal)
        {
            _sachDal = sachDal;
            _loaiDal = loaiDal;
        }

        /// <summary>Tìm theo tên/tác giả. Từ khóa rỗng = lấy tất cả.</summary>
        public List<Sach> TimKiem(string keyword)
        {
            return _sachDal.Search(keyword == null ? "" : keyword.Trim());
        }

        /// <summary>checkAvailability: sách có tồn tại và còn ít nhất 1 bản trên kệ không.</summary>
        public bool KiemTraConSach(int maSach)
        {
            Sach s = _sachDal.GetById(maSach);
            return s != null && s.SoLuongCon > 0;
        }

        /// <summary>Thêm sách mới. Sách mới nhập thì số còn = tổng số.</summary>
        public int Them(Sach sach)
        {
            KiemTraHopLe(sach);
            sach.SoLuongCon = sach.SoLuong;
            return _sachDal.Insert(sach);
        }

        /// <summary>
        /// Sửa thông tin sách. Số đang cho mượn = SoLuong cũ - SoLuongCon cũ.
        /// Tổng số mới không được nhỏ hơn số đang cho mượn; SoLuongCon được tính lại.
        /// </summary>
        public void Sua(Sach sach)
        {
            Sach cu = _sachDal.GetById(sach.MaSach);
            if (cu == null)
                throw new BusinessException("Sách không tồn tại");

            KiemTraHopLe(sach);

            int dangMuon = cu.SoLuong - cu.SoLuongCon;
            if (sach.SoLuong < dangMuon)
                throw new BusinessException("Tổng số lượng không được nhỏ hơn số sách đang cho mượn (" + dangMuon + ")");

            sach.SoLuongCon = sach.SoLuong - dangMuon;
            _sachDal.Update(sach);
        }

        /// <summary>Xóa sách. Không xóa sách đã từng xuất hiện trong phiếu mượn.</summary>
        public void Xoa(int maSach)
        {
            if (_sachDal.GetById(maSach) == null)
                throw new BusinessException("Sách không tồn tại");

            if (_sachDal.CountLichSuMuon(maSach) > 0)
                throw new BusinessException("Không thể xóa: sách đã có trong phiếu mượn");

            _sachDal.Delete(maSach);
        }

        private void KiemTraHopLe(Sach s)
        {
            if (s == null || string.IsNullOrWhiteSpace(s.TenSach))
                throw new BusinessException("Tên sách không được để trống");
            if (s.SoLuong < 0)
                throw new BusinessException("Số lượng không được âm");
            if (s.NamXB.HasValue && (s.NamXB.Value < 1000 || s.NamXB.Value > DateTime.Today.Year))
                throw new BusinessException("Năm xuất bản không hợp lệ");
            if (_loaiDal.GetById(s.MaLoai) == null)
                throw new BusinessException("Loại sách không tồn tại");

            s.TenSach = s.TenSach.Trim();
        }
    }
}
