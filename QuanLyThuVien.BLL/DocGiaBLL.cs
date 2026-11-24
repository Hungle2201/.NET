using System;
using System.Collections.Generic;
using System.Net.Mail;
using QuanLyThuVien.DAL;
using QuanLyThuVien.DTO;

namespace QuanLyThuVien.BLL
{
    /// <summary>Nghiệp vụ quản lý độc giả.</summary>
    public class DocGiaBLL
    {
        private readonly IDocGiaDAL _dal;

        public DocGiaBLL(IDocGiaDAL dal)
        {
            _dal = dal;
        }

        public List<DocGia> TimKiem(string keyword)
        {
            return _dal.Search(keyword == null ? "" : keyword.Trim());
        }

        /// <summary>Thêm độc giả. Thẻ mới phải có hạn từ hôm nay trở đi.</summary>
        public int Them(DocGia dg)
        {
            KiemTraHopLe(dg);
            if (dg.HanThe.Date < DateTime.Today)
                throw new BusinessException("Hạn thẻ không được ở quá khứ");
            return _dal.Insert(dg);
        }

        /// <summary>Sửa độc giả. Cho phép sửa cả khi thẻ đã hết hạn (để gia hạn).</summary>
        public void Sua(DocGia dg)
        {
            if (_dal.GetById(dg.MaDG) == null)
                throw new BusinessException("Độc giả không tồn tại");
            KiemTraHopLe(dg);
            _dal.Update(dg);
        }

        /// <summary>Xóa độc giả. Không xóa độc giả đã có phiếu mượn (giữ lịch sử).</summary>
        public void Xoa(int maDG)
        {
            if (_dal.GetById(maDG) == null)
                throw new BusinessException("Độc giả không tồn tại");

            if (_dal.CountPhieuMuon(maDG) > 0)
                throw new BusinessException("Không thể xóa: độc giả đã có phiếu mượn");

            _dal.Delete(maDG);
        }

        private static void KiemTraHopLe(DocGia dg)
        {
            if (dg == null || string.IsNullOrWhiteSpace(dg.TenDG))
                throw new BusinessException("Tên độc giả không được để trống");

            if (!string.IsNullOrWhiteSpace(dg.Email))
            {
                try { new MailAddress(dg.Email.Trim()); }
                catch (FormatException) { throw new BusinessException("Email không đúng định dạng"); }
            }

            if (dg.NgaySinh.HasValue && dg.NgaySinh.Value.Date > DateTime.Today)
                throw new BusinessException("Ngày sinh không hợp lệ");

            dg.TenDG = dg.TenDG.Trim();
        }
    }
}
