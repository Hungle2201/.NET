using System;

namespace QuanLyThuVien.DTO
{
    // Các lớp dữ liệu thuần (không chứa logic). Tên thuộc tính khớp tên cột trong CSDL.
    // BẢN ĐỀ XUẤT - TV1 và nhóm thống nhất lần cuối, sau đó không đổi tên nữa.

    public class LoaiSach
    {
        public int MaLoai { get; set; }
        public string TenLoai { get; set; }
        public decimal DonGiaPhat { get; set; }   // tiền phạt mỗi ngày trễ
    }

    public class Sach
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; }
        public string TacGia { get; set; }
        public int? NamXB { get; set; }
        public int? MaNXB { get; set; }
        public int MaLoai { get; set; }
        public int SoLuong { get; set; }       // tổng số bản
        public int SoLuongCon { get; set; }    // số bản còn trên kệ
    }

    public class DocGia
    {
        public int MaDG { get; set; }
        public string TenDG { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string DiaChi { get; set; }
        public string Email { get; set; }
        public string GioiTinh { get; set; }
        public DateTime HanThe { get; set; }   // hạn sử dụng thẻ
    }

    public class PhieuMuon
    {
        public int MaPM { get; set; }
        public int MaDG { get; set; }
        public int? MaNV { get; set; }
        public DateTime NgayMuon { get; set; }
        public DateTime NgayHenTra { get; set; }
    }

    public class ChiTietMuon
    {
        public int MaPM { get; set; }
        public int MaSach { get; set; }
        public DateTime? NgayTra { get; set; }   // null = chưa trả
        public string TinhTrang { get; set; }
    }

    public class Phat
    {
        public int MaPhat { get; set; }
        public int MaPM { get; set; }
        public int MaSach { get; set; }
        public int SoNgayTre { get; set; }
        public decimal SoTien { get; set; }
    }

    // Kết quả của truy vấn "hạn trả + đơn giá phạt" cho một cuốn sách trong một phiếu
    public class ThongTinPhat
    {
        public DateTime NgayHenTra { get; set; }
        public decimal DonGiaPhat { get; set; }
    }
}
