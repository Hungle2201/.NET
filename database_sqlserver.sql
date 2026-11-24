/* =====================================================================
   CSDL SQL Server cho đề tài Quản lý mượn trả sách thư viện
   Chạy cả file trong SSMS (có các lệnh GO). Chạy lại = xóa và tạo lại sạch.
   Đây là bản ĐỀ XUẤT để TV1 đối chiếu; TV1 có thể chỉnh nhưng phải báo
   tên bảng/cột cuối cùng để BLL khớp.
   ===================================================================== */
USE master;
GO
IF DB_ID(N'QuanLyThuVien') IS NOT NULL
BEGIN
    ALTER DATABASE QuanLyThuVien SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyThuVien;
END
GO
CREATE DATABASE QuanLyThuVien;
GO
USE QuanLyThuVien;
GO

CREATE TABLE LoaiSach (
    MaLoai      INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai     NVARCHAR(100) NOT NULL,
    DonGiaPhat  DECIMAL(18,0) NOT NULL DEFAULT 2000   -- tiền phạt mỗi ngày trễ, theo loại sách
);

CREATE TABLE NhaXuatBan (
    MaNXB   INT IDENTITY(1,1) PRIMARY KEY,
    TenNXB  NVARCHAR(100) NOT NULL
);

CREATE TABLE QuyenSach (
    MaSach       INT IDENTITY(1,1) PRIMARY KEY,
    TenSach      NVARCHAR(200) NOT NULL,
    TacGia       NVARCHAR(100) NULL,
    NamXB        INT NULL,
    MaNXB        INT NULL REFERENCES NhaXuatBan(MaNXB),
    MaLoai       INT NOT NULL REFERENCES LoaiSach(MaLoai),
    SoLuong      INT NOT NULL,          -- tổng số bản thư viện sở hữu
    SoLuongCon   INT NOT NULL,          -- số bản còn trên kệ (có thể cho mượn)
    CONSTRAINT CK_QuyenSach_SoLuong CHECK (SoLuong >= 0 AND SoLuongCon >= 0 AND SoLuongCon <= SoLuong)
);

CREATE TABLE DocGia (
    MaDG      INT IDENTITY(1,1) PRIMARY KEY,
    TenDG     NVARCHAR(100) NOT NULL,
    NgaySinh  DATE NULL,
    DiaChi    NVARCHAR(200) NULL,
    Email     NVARCHAR(100) NULL,
    GioiTinh  NVARCHAR(10) NULL,
    HanThe    DATE NOT NULL            -- hạn sử dụng thẻ thư viện
);

CREATE TABLE NhanVien (
    MaNV       INT IDENTITY(1,1) PRIMARY KEY,
    TenNV      NVARCHAR(100) NOT NULL,
    DienThoai  VARCHAR(15) NULL
);

CREATE TABLE TaiKhoan (
    TenDN    VARCHAR(50) PRIMARY KEY,
    MatKhau  VARCHAR(64) NOT NULL,     -- SHA-256 dạng hex CHỮ HOA, KHÔNG lưu chữ thường
    Quyen    NVARCHAR(20) NOT NULL,    -- 'Admin' hoặc 'NhanVien'
    MaNV     INT NULL REFERENCES NhanVien(MaNV)
);

CREATE TABLE PhieuMuon (
    MaPM         INT IDENTITY(1,1) PRIMARY KEY,
    MaDG         INT NOT NULL REFERENCES DocGia(MaDG),
    MaNV         INT NULL REFERENCES NhanVien(MaNV),
    NgayMuon     DATE NOT NULL,
    NgayHenTra   DATE NOT NULL
);

-- Trả sách được ghi ngay trên chi tiết mượn (trả theo từng cuốn)
CREATE TABLE ChiTietMuon (
    MaPM       INT NOT NULL REFERENCES PhieuMuon(MaPM),
    MaSach     INT NOT NULL REFERENCES QuyenSach(MaSach),
    NgayTra    DATE NULL,              -- NULL = chưa trả
    TinhTrang  NVARCHAR(100) NULL,     -- tình trạng sách lúc trả
    PRIMARY KEY (MaPM, MaSach)
);

CREATE TABLE Phat (
    MaPhat      INT IDENTITY(1,1) PRIMARY KEY,
    MaPM        INT NOT NULL,
    MaSach      INT NOT NULL,
    SoNgayTre   INT NOT NULL,
    SoTien      DECIMAL(18,0) NOT NULL,
    FOREIGN KEY (MaPM, MaSach) REFERENCES ChiTietMuon(MaPM, MaSach)
);
GO

/* =====================================================================
   DỮ LIỆU MẪU (ngày tính theo hôm nay nên chạy lúc nào cũng đúng kịch bản)
   ===================================================================== */
DECLARE @homNay DATE = CAST(GETDATE() AS DATE);

INSERT INTO LoaiSach (TenLoai, DonGiaPhat) VALUES
 (N'Giáo trình', 2000), (N'Văn học', 1000), (N'Ngoại ngữ', 1500);

INSERT INTO NhaXuatBan (TenNXB) VALUES (N'NXB Giáo dục'), (N'NXB Trẻ');

INSERT INTO QuyenSach (TenSach, TacGia, NamXB, MaNXB, MaLoai, SoLuong, SoLuongCon) VALUES
 (N'Lập trình C# căn bản',    N'Nhiều tác giả',  2020, 1, 1, 3, 2),  -- MaSach 1: DG3 đang giữ 1
 (N'Cơ sở dữ liệu',           N'Nhiều tác giả',  2019, 1, 1, 3, 2),  -- MaSach 2: DG4 đang giữ 1
 (N'Truyện Kiều',             N'Nguyễn Du',      2015, 2, 2, 2, 1),  -- MaSach 3: DG4 đang giữ 1
 (N'Nhà giả kim',             N'Paulo Coelho',   2018, 2, 2, 1, 0),  -- MaSach 4: HẾT SÁCH (DG3 giữ)
 (N'English Grammar in Use',  N'Raymond Murphy', 2017, 1, 3, 2, 1),  -- MaSach 5: DG4 đang giữ 1
 (N'Từ điển Anh - Việt',      N'Nhiều tác giả',  2016, 1, 3, 2, 1),  -- MaSach 6: DG4 đang giữ 1
 (N'Cấu trúc dữ liệu',        N'Nhiều tác giả',  2021, 1, 1, 5, 5);  -- MaSach 7: còn đủ

INSERT INTO DocGia (TenDG, NgaySinh, DiaChi, Email, GioiTinh, HanThe) VALUES
 (N'DG1 - Hợp lệ',       '2002-01-01', N'Hà Nội', 'dg1@example.com', N'Nam', DATEADD(DAY, 365, @homNay)),
 (N'DG2 - Thẻ hết hạn',  '2002-02-02', N'Hà Nội', 'dg2@example.com', N'Nữ',  DATEADD(DAY, -10, @homNay)),
 (N'DG3 - Đang trễ hạn', '2002-03-03', N'Hà Nội', 'dg3@example.com', N'Nam', DATEADD(DAY, 365, @homNay)),
 (N'DG4 - Đang mượn 4',  '2002-04-04', N'Hà Nội', 'dg4@example.com', N'Nữ',  DATEADD(DAY, 365, @homNay));

INSERT INTO NhanVien (TenNV, DienThoai) VALUES (N'Nguyễn Văn A', '0900000000');
-- Mật khẩu mẫu: admin123 (băm SHA-256, hex chữ hoa)
INSERT INTO TaiKhoan (TenDN, MatKhau, Quyen, MaNV)
 VALUES ('admin', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2), N'Admin', 1);

-- Phiếu 1: DG3 mượn cách đây 30 ngày, hẹn trả cách đây 16 ngày -> đang TRỄ 16 ngày (sách 1, sách 4)
INSERT INTO PhieuMuon (MaDG, MaNV, NgayMuon, NgayHenTra)
 VALUES (3, 1, DATEADD(DAY, -30, @homNay), DATEADD(DAY, -16, @homNay));
-- Phiếu 2: DG4 mượn 5 ngày trước, hẹn trả 9 ngày nữa -> còn hạn (sách 2, 3, 5, 6)
INSERT INTO PhieuMuon (MaDG, MaNV, NgayMuon, NgayHenTra)
 VALUES (4, 1, DATEADD(DAY, -5, @homNay), DATEADD(DAY, 9, @homNay));

INSERT INTO ChiTietMuon (MaPM, MaSach) VALUES (1,1), (1,4), (2,2), (2,3), (2,5), (2,6);
GO
