namespace QuanLyThuVien.BLL
{
    /// <summary>
    /// Các con số quy định của thư viện. File Word KHÔNG nêu rõ các số này,
    /// nên đây là GIÁ TRỊ TẠM - cả nhóm/giảng viên chốt rồi sửa tại một chỗ duy nhất này.
    /// (Đơn giá phạt không nằm ở đây mà nằm ở cột LoaiSach.DonGiaPhat, vì Word nói phạt theo loại sách.)
    /// </summary>
    public static class QuyDinh
    {
        /// <summary>Số ngày được mượn mỗi lần (NgayHenTra = NgayMuon + số này).</summary>
        public const int SoNgayMuon = 14;

        /// <summary>Số cuốn chưa trả tối đa một độc giả được giữ cùng lúc.</summary>
        public const int ToiDaSachMuon = 5;

        /// <summary>Tình trạng ghi mặc định khi trả sách mà không nhập gì.</summary>
        public const string TinhTrangMacDinh = "Bình thường";
    }
}
