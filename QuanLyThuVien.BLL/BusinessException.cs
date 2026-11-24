using System;

namespace QuanLyThuVien.BLL
{
    /// <summary>
    /// Lỗi nghiệp vụ (vi phạm quy tắc mượn trả, dữ liệu không hợp lệ...).
    /// Message là câu tiếng Việt có thể hiển thị thẳng cho người dùng.
    /// GUI chỉ cần: catch (BusinessException ex) { MessageBox.Show(ex.Message); }
    /// Các lỗi khác (mất kết nối SQL Server...) là lỗi hệ thống, không phải BusinessException.
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }
}
