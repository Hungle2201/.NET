using System.Collections.Generic;
using QuanLyThuVien.DAL;
using QuanLyThuVien.DTO;

namespace QuanLyThuVien.BLL
{
    /// <summary>Nghiệp vụ quản lý loại (thể loại) sách.</summary>
    public class LoaiSachBLL
    {
        private readonly ILoaiSachDAL _dal;

        public LoaiSachBLL(ILoaiSachDAL dal)
        {
            _dal = dal;
        }

        public List<LoaiSach> LayTatCa()
        {
            return _dal.GetAll();
        }

        public int Them(LoaiSach loai)
        {
            KiemTraHopLe(loai);
            return _dal.Insert(loai);
        }

        public void Sua(LoaiSach loai)
        {
            if (_dal.GetById(loai.MaLoai) == null)
                throw new BusinessException("Loại sách không tồn tại");
            KiemTraHopLe(loai);
            _dal.Update(loai);
        }

        public void Xoa(int maLoai)
        {
            if (_dal.GetById(maLoai) == null)
                throw new BusinessException("Loại sách không tồn tại");

            // Ràng buộc: không xóa loại còn sách
            int soSach = _dal.CountSach(maLoai);
            if (soSach > 0)
                throw new BusinessException("Không thể xóa: còn " + soSach + " đầu sách thuộc loại này");

            _dal.Delete(maLoai);
        }

        private static void KiemTraHopLe(LoaiSach loai)
        {
            if (loai == null || string.IsNullOrWhiteSpace(loai.TenLoai))
                throw new BusinessException("Tên loại sách không được để trống");
            if (loai.DonGiaPhat < 0)
                throw new BusinessException("Đơn giá phạt không được âm");
            loai.TenLoai = loai.TenLoai.Trim();
        }
    }
}
