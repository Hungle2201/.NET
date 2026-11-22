using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using QuanLyThuVien.DTO;

namespace QuanLyThuVien.DAL
{
    // =====================================================================
    // HỢP ĐỒNG GIỮA TV2 (BLL) VÀ TV1 (DAL)
    // TV1 viết các lớp DocGiaDAL, SachDAL, LoaiSachDAL, MuonTraDAL, PhatDAL
    // cài đặt (implements) đúng các interface dưới đây.
    //
    // QUY ƯỚC BẮT BUỘC:
    //  1. Hàm có tham số `tran` PHẢI dùng tran.Connection và gán Transaction = tran
    //     cho câu lệnh (cmd.Transaction = tran). KHÔNG tự mở kết nối mới, KHÔNG tự Commit/Rollback.
    //  2. Mọi giá trị đưa vào câu SQL phải qua tham số (Parameters), không nối chuỗi.
    //  3. Hàm GetById trả về null khi không tìm thấy (không ném lỗi).
    //  4. Insert trả về khóa vừa tạo (SQL Server: thêm OUTPUT INSERTED.<khóa> vào câu INSERT rồi ExecuteScalar).
    //  5. Interface này KHÔNG phụ thuộc loại CSDL: nhóm dùng SQL Server (System.Data.SqlClient),
    //     nhưng BLL chỉ biết DbConnection/DbTransaction nên đổi CSDL sau này không phải sửa BLL.
    // =====================================================================

    public interface ILoaiSachDAL
    {
        List<LoaiSach> GetAll();
        LoaiSach GetById(int maLoai);
        int Insert(LoaiSach loai);
        bool Update(LoaiSach loai);
        bool Delete(int maLoai);
        int CountSach(int maLoai);                 // số sách thuộc loại này
    }

    public interface ISachDAL
    {
        Sach GetById(int maSach);
        List<Sach> Search(string keyword);         // tìm theo tên / tác giả; keyword rỗng = lấy tất cả
        int Insert(Sach sach);
        bool Update(Sach sach);                    // cập nhật cả SoLuong và SoLuongCon
        bool Delete(int maSach);
        int CountLichSuMuon(int maSach);           // số dòng ChiTietMuon tham chiếu tới sách

        // UPDATE QuyenSach SET SoLuongCon = SoLuongCon - 1 WHERE MaSach=@id AND SoLuongCon > 0
        // Trả về true nếu trừ được (còn sách), false nếu hết sách.
        bool GiamSoLuongCon(int maSach, DbTransaction tran);

        // UPDATE QuyenSach SET SoLuongCon = SoLuongCon + 1 WHERE MaSach=@id
        void TangSoLuongCon(int maSach, DbTransaction tran);
    }

    public interface IDocGiaDAL
    {
        DocGia GetById(int maDG);
        List<DocGia> Search(string keyword);       // tìm theo tên / email / mã; rỗng = tất cả
        int Insert(DocGia dg);
        bool Update(DocGia dg);
        bool Delete(int maDG);
        int CountPhieuMuon(int maDG);              // số phiếu mượn của độc giả (kể cả đã trả)
    }

    public interface IMuonTraDAL
    {
        // Có chi tiết chưa trả (NgayTra IS NULL) mà phiếu có NgayHenTra < ngay không?
        bool CoSachQuaHan(int maDG, DateTime ngay, DbTransaction tran);

        // Số cuốn độc giả đang giữ chưa trả
        int CountSachChuaTra(int maDG, DbTransaction tran);

        // INSERT PhieuMuon, trả về MaPM mới
        int InsertPhieu(PhieuMuon pm, DbTransaction tran);

        void InsertChiTiet(int maPM, int maSach, DbTransaction tran);

        // UPDATE ChiTietMuon SET NgayTra=@nt, TinhTrang=@tt
        //   WHERE MaPM=@pm AND MaSach=@s AND NgayTra IS NULL
        // Trả về số dòng bị ảnh hưởng: 0 nghĩa là đã trả rồi hoặc không có trong phiếu.
        int DanhDauDaTra(int maPM, int maSach, DateTime ngayTra, string tinhTrang, DbTransaction tran);

        // SELECT p.NgayHenTra, l.DonGiaPhat FROM PhieuMuon p, QuyenSach s JOIN LoaiSach l ...
        // Trả về null nếu không tìm thấy.
        ThongTinPhat GetThongTinPhat(int maPM, int maSach, DbTransaction tran);

        // Danh sách sách độc giả đang giữ chưa trả, để hiển thị lên form Trả sách.
        // Cột gợi ý: MaPM, MaSach, TenSach, NgayMuon, NgayHenTra
        DataTable GetChuaTraTheoDocGia(int maDG);
    }

    public interface IPhatDAL
    {
        int Insert(Phat phat, DbTransaction tran);   // trả về MaPhat mới
    }
}
