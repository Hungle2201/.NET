# Quy tắc nghiệp vụ và ca kiểm thử (phần BLL – TV2)

Chạy lại `database_sqlserver.sql` trước mỗi lượt test để có dữ liệu mẫu sạch.

## 1. Dữ liệu mẫu dùng để test

| Độc giả | Tình trạng |
|---|---|
| DG1 (MaDG 1) | Hợp lệ, chưa mượn gì |
| DG2 (MaDG 2) | Thẻ đã hết hạn |
| DG3 (MaDG 3) | Đang trễ hạn 16 ngày, giữ sách 1 (loại Giáo trình) và sách 4 (loại Văn học) |
| DG4 (MaDG 4) | Đang giữ 4 cuốn (sách 2, 3, 5, 6), còn hạn trả |

| Sách | SoLuong / SoLuongCon | Ghi chú |
|---|---|---|
| 1 | 3 / 2 | DG3 đang giữ 1 |
| 2 | 3 / 2 | DG4 đang giữ 1 |
| 3 | 2 / 1 | |
| 4 | 1 / **0** | Hết sách |
| 5, 6 | 2 / 1 | |
| 7 | 5 / 5 | Còn đủ, chưa ai mượn |

Đơn giá phạt: Giáo trình 2.000đ/ngày, Văn học 1.000đ/ngày, Ngoại ngữ 1.500đ/ngày.

## 2. Quy tắc nghiệp vụ

| # | Quy tắc | Nguồn |
|---|---|---|
| R1 | Độc giả phải tồn tại và thẻ chưa hết hạn mới được mượn | Word (đăng ký thẻ, hạn thẻ) |
| R2 | Độc giả đang có sách quá hạn thì không được mượn thêm | Word |
| R3 | Một phiếu mượn được nhiều sách, mỗi đầu sách chỉ một lần trong phiếu | Word + đề xuất |
| R4 | Chỉ mượn được sách còn SoLuongCon > 0; mượn thì trừ 1, trả thì cộng 1 | Đề xuất |
| R5 | Mượn và trả là một giao dịch: lỗi giữa chừng thì hoàn tác tất cả | Đề xuất |
| R6 | Mỗi cuốn trong phiếu chỉ được trả một lần | Đề xuất |
| R7 | Trễ hạn thì phạt = số ngày trễ × đơn giá phạt của loại sách | Word (phạt theo loại sách) |
| R8 | Không xóa sách đã có trong phiếu mượn; không xóa độc giả đã có phiếu; không xóa loại còn sách | Đề xuất |
| R9 | Sửa tổng số lượng sách không được nhỏ hơn số đang cho mượn | Đề xuất |

**Con số tạm, cần nhóm/giảng viên chốt** (sửa trong `BLL/QuyDinh.cs`): số ngày mượn = 14, tối đa 5 cuốn chưa trả cùng lúc.

**Word có nhắc nhưng chưa làm, cần quyết định có làm không:** tiền cược khi mượn, phân biệt mượn tại chỗ và mượn về nhà, đền sách hỏng hoặc mất (hiện chỉ ghi tình trạng, không tính tiền), quy định nhắc trả tối đa 3 ngày.

## 3. Ca kiểm thử

### Mượn sách – `TaoPhieuMuon(maDG, dsMaSach, maNV)`

| Mã | Tình huống | Kết quả mong đợi |
|---|---|---|
| M01 | DG1 mượn [1] | Thành công; sách 1 còn 2 → 1; phiếu mới hẹn trả = hôm nay + 14 ngày |
| M02 | DG1 mượn [1, 2, 7] | Thành công; cả 3 sách giảm 1 |
| M03 | Mã độc giả 999 | Lỗi "Độc giả không tồn tại" |
| M04 | DG2 mượn [7] | Lỗi "Thẻ thư viện đã hết hạn" |
| M05 | DG3 mượn [7] | Lỗi "đang có sách quá hạn" |
| M06 | DG1 mượn [4] (hết sách) | Lỗi "đã hết"; không tạo phiếu nào |
| M07 | DG1 mượn [7, 4] | Lỗi vì sách 4 hết; **hoàn tác**: sách 7 vẫn 5/5, không có phiếu mới |
| M08 | DG4 mượn [7, 1] (4 + 2 = 6 > 5) | Lỗi "Vượt giới hạn mượn" |
| M09 | DG4 mượn [7] (4 + 1 = 5) | Thành công |
| M10 | Danh sách sách rỗng hoặc null | Lỗi "Chưa chọn sách nào" |
| M11 | DG1 mượn [7, 7] | Chỉ mượn 1 cuốn; sách 7 giảm đúng 1 |
| M12 | DG1 mượn [999] | Lỗi "không tồn tại"; không tạo phiếu |

### Trả sách – `TraSach(maPM, maSach, tinhTrang)`

| Mã | Tình huống | Kết quả mong đợi |
|---|---|---|
| T01 | Phiếu 2, sách 2 (còn hạn) | Trả về null (không phạt); sách 2 còn 2 → 3; NgayTra = hôm nay |
| T02 | Trả lại phiếu 2, sách 2 lần nữa | Lỗi "đã được trả"; SoLuongCon không tăng thêm |
| T03 | Phiếu 1, sách 1 (trễ 16 ngày, Giáo trình) | Phạt 16 ngày × 2.000 = **32.000đ**; có dòng trong bảng Phat |
| T04 | Phiếu 1, sách 4 (trễ 16 ngày, Văn học) | Phạt 16 × 1.000 = **16.000đ** |
| T05 | Phiếu 1, sách 2 (không thuộc phiếu 1) | Lỗi "không có trong phiếu mượn" |
| T06 | Trả hết sách của DG3 (T03 + T04) rồi DG3 mượn [7] | Mượn thành công (hết bị chặn quá hạn) |
| T07 | Tình trạng bỏ trống | Ghi "Bình thường" |

### Tính phạt – `MuonTraBLL.TinhTienPhat(hẹn, trả, đơn giá, out soNgayTre)` (hàm thuần, test không cần CSDL)

| Mã | Hẹn trả | Ngày trả | Đơn giá | Kết quả |
|---|---|---|---|---|
| P01 | 10/11 | 10/11 | 2.000 | 0đ, 0 ngày |
| P02 | 10/11 | 09/11 (sớm) | 2.000 | 0đ, 0 ngày |
| P03 | 10/11 | 11/11 | 2.000 | 2.000đ, 1 ngày |
| P04 | 10/11 | 20/11 | 1.500 | 15.000đ, 10 ngày |

### Sách – `SachBLL`

| Mã | Tình huống | Kết quả mong đợi |
|---|---|---|
| S01 | Thêm sách hợp lệ, SoLuong = 4 | Thành công; SoLuongCon = 4 |
| S02 | Tên sách rỗng | Lỗi "Tên sách không được để trống" |
| S03 | SoLuong = -1 | Lỗi "Số lượng không được âm" |
| S04 | Loại sách 999 | Lỗi "Loại sách không tồn tại" |
| S05 | Sửa sách 1 (đang mượn 1) thành SoLuong = 1 | Thành công; SoLuongCon = 0 |
| S06 | Sửa sách 1 thành SoLuong = 0 | Lỗi "nhỏ hơn số sách đang cho mượn (1)" |
| S07 | Xóa sách 4 (đã có trong phiếu) | Lỗi "đã có trong phiếu mượn" |
| S08 | Xóa sách 7 (chưa ai mượn) | Thành công |
| S09 | `KiemTraConSach(4)` / `(7)` / `(999)` | false / true / false |

### Độc giả – `DocGiaBLL` và Loại sách – `LoaiSachBLL`

| Mã | Tình huống | Kết quả mong đợi |
|---|---|---|
| D01 | Thêm độc giả hợp lệ (thẻ hạn trong tương lai) | Thành công |
| D02 | Thêm độc giả có HanThe ở quá khứ | Lỗi "Hạn thẻ không được ở quá khứ" |
| D03 | Email sai định dạng ("abc") | Lỗi "Email không đúng định dạng" |
| D04 | Sửa DG2 (thẻ hết hạn) để gia hạn thẻ | Thành công |
| D05 | Xóa DG3 (đã có phiếu) | Lỗi "đã có phiếu mượn" |
| D06 | Xóa độc giả mới thêm ở D01 (chưa có phiếu) | Thành công |
| L01 | Xóa loại 1 (còn sách) | Lỗi "còn N đầu sách thuộc loại này" |
| L02 | Thêm loại có đơn giá phạt âm | Lỗi "Đơn giá phạt không được âm" |
| L03 | Thêm loại tên rỗng | Lỗi "Tên loại sách không được để trống" |
