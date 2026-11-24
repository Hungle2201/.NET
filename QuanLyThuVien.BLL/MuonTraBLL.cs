using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Common;
using QuanLyThuVien.DAL;
using QuanLyThuVien.DTO;

namespace QuanLyThuVien.BLL
{
    /// <summary>
    /// Nghiệp vụ mượn - trả sách và tính phạt.
    /// Mượn và trả mỗi thao tác là MỘT giao dịch (transaction): lỗi ở bất kỳ bước nào
    /// thì rollback toàn bộ, dữ liệu không bị lệch (ví dụ đã trừ kho mà chưa ghi phiếu).
    /// </summary>
    public class MuonTraBLL
    {
        private readonly IDocGiaDAL _docGiaDal;
        private readonly ISachDAL _sachDal;
        private readonly IMuonTraDAL _muonTraDal;
        private readonly IPhatDAL _phatDal;

        public MuonTraBLL(IDocGiaDAL docGiaDal, ISachDAL sachDal, IMuonTraDAL muonTraDal, IPhatDAL phatDal)
        {
            _docGiaDal = docGiaDal;
            _sachDal = sachDal;
            _muonTraDal = muonTraDal;
            _phatDal = phatDal;
        }

        /// <summary>
        /// Tạo phiếu mượn cho độc giả với danh sách sách. Trả về mã phiếu (MaPM).
        /// Ném BusinessException nếu: danh sách rỗng, độc giả không tồn tại, thẻ hết hạn,
        /// đang có sách quá hạn, vượt giới hạn số cuốn, sách không tồn tại hoặc đã hết.
        /// </summary>
        public int TaoPhieuMuon(int maDG, List<int> dsMaSach, int? maNV)
        {
            if (dsMaSach == null || dsMaSach.Count == 0)
                throw new BusinessException("Chưa chọn sách nào để mượn");

            // Loại mã trùng: một phiếu không mượn hai lần cùng một đầu sách
            List<int> ds = dsMaSach.Distinct().ToList();

            DocGia dg = _docGiaDal.GetById(maDG);
            if (dg == null)
                throw new BusinessException("Độc giả không tồn tại");

            DateTime homNay = DateTime.Today;
            if (dg.HanThe.Date < homNay)
                throw new BusinessException("Thẻ thư viện đã hết hạn");

            using (DbConnection conn = DBHelper.GetConnection())
            {
                conn.Open();
                using (DbTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        if (_muonTraDal.CoSachQuaHan(maDG, homNay, tran))
                            throw new BusinessException("Độc giả đang có sách quá hạn chưa trả");

                        int dangGiu = _muonTraDal.CountSachChuaTra(maDG, tran);
                        if (dangGiu + ds.Count > QuyDinh.ToiDaSachMuon)
                            throw new BusinessException(string.Format(
                                "Vượt giới hạn mượn: đang giữ {0} cuốn, mượn thêm {1} cuốn thì quá tối đa {2} cuốn",
                                dangGiu, ds.Count, QuyDinh.ToiDaSachMuon));

                        PhieuMuon pm = new PhieuMuon
                        {
                            MaDG = maDG,
                            MaNV = maNV,
                            NgayMuon = homNay,
                            NgayHenTra = homNay.AddDays(QuyDinh.SoNgayMuon)
                        };
                        int maPM = _muonTraDal.InsertPhieu(pm, tran);

                        foreach (int maSach in ds)
                        {
                            Sach s = _sachDal.GetById(maSach);
                            if (s == null)
                                throw new BusinessException("Sách mã " + maSach + " không tồn tại");

                            // Trừ kho kèm điều kiện còn sách trong cùng một câu lệnh,
                            // tránh âm kho khi hai người cùng mượn cuốn cuối cùng
                            if (!_sachDal.GiamSoLuongCon(maSach, tran))
                                throw new BusinessException("Sách \"" + s.TenSach + "\" đã hết");

                            _muonTraDal.InsertChiTiet(maPM, maSach, tran);
                        }

                        tran.Commit();
                        return maPM;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Trả MỘT cuốn sách của một phiếu. Trả về khoản phạt nếu trễ hạn, ngược lại trả về null.
        /// Ném BusinessException nếu sách đã trả rồi hoặc không có trong phiếu.
        /// </summary>
        public Phat TraSach(int maPM, int maSach, string tinhTrang)
        {
            if (string.IsNullOrWhiteSpace(tinhTrang))
                tinhTrang = QuyDinh.TinhTrangMacDinh;

            DateTime homNay = DateTime.Today;

            using (DbConnection conn = DBHelper.GetConnection())
            {
                conn.Open();
                using (DbTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Điều kiện NgayTra IS NULL nằm trong DAL: chặn trả hai lần
                        int soDong = _muonTraDal.DanhDauDaTra(maPM, maSach, homNay, tinhTrang.Trim(), tran);
                        if (soDong == 0)
                            throw new BusinessException("Sách này đã được trả hoặc không có trong phiếu mượn");

                        _sachDal.TangSoLuongCon(maSach, tran);

                        ThongTinPhat tt = _muonTraDal.GetThongTinPhat(maPM, maSach, tran);
                        if (tt == null)
                            throw new BusinessException("Không tìm thấy thông tin phiếu mượn để tính phạt");

                        int soNgayTre;
                        decimal tien = TinhTienPhat(tt.NgayHenTra, homNay, tt.DonGiaPhat, out soNgayTre);

                        Phat phat = null;
                        if (tien > 0)
                        {
                            phat = new Phat
                            {
                                MaPM = maPM,
                                MaSach = maSach,
                                SoNgayTre = soNgayTre,
                                SoTien = tien
                            };
                            phat.MaPhat = _phatDal.Insert(phat, tran);
                        }

                        tran.Commit();
                        return phat;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// calculateFine: tiền phạt = số ngày trễ x đơn giá phạt (theo loại sách).
        /// Hàm thuần (không đụng CSDL) nên kiểm thử rất dễ.
        /// Trả đúng hạn hoặc sớm thì soNgayTre = 0 và tiền = 0.
        /// </summary>
        public static decimal TinhTienPhat(DateTime ngayHenTra, DateTime ngayTra, decimal donGiaPhat, out int soNgayTre)
        {
            soNgayTre = (ngayTra.Date - ngayHenTra.Date).Days;
            if (soNgayTre <= 0)
            {
                soNgayTre = 0;
                return 0;
            }
            return soNgayTre * donGiaPhat;
        }

        /// <summary>Danh sách sách độc giả đang giữ chưa trả (cho form Trả sách).</summary>
        public DataTable LayDanhSachChuaTra(int maDG)
        {
            if (_docGiaDal.GetById(maDG) == null)
                throw new BusinessException("Độc giả không tồn tại");
            return _muonTraDal.GetChuaTraTheoDocGia(maDG);
        }
    }
}
