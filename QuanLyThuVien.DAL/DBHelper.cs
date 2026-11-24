using System.Configuration;          // NuGet: System.Configuration.ConfigurationManager
using System.Data.Common;
using Microsoft.Data.SqlClient;      // NuGet: Microsoft.Data.SqlClient

namespace QuanLyThuVien.DAL
{
    /// <summary>
    /// Tạo kết nối SQL Server. Chuỗi kết nối đọc từ App.config của project GUI (mục "QuanLyThuVien"),
    /// không có thì dùng mặc định .\SQLEXPRESS. Máy ai khác server thì sửa App.config trên máy đó,
    /// không phải sửa code. Kiểu trả về là DbConnection nên BLL không phụ thuộc loại CSDL.
    /// Bản DBHelper của TV1 sẽ thay file mẫu này, chỉ cần giữ nguyên chữ ký GetConnection().
    /// </summary>
    public static class DBHelper
    {
        // TrustServerCertificate=True: bắt buộc với Microsoft.Data.SqlClient khi SQL Server tự cài trên máy
        private const string MacDinh =
            @"Server=.\SQLEXPRESS;Database=QuanLyThuVien;Integrated Security=True;TrustServerCertificate=True;";

        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings cfg = ConfigurationManager.ConnectionStrings["QuanLyThuVien"];
                return cfg != null ? cfg.ConnectionString : MacDinh;
            }
        }

        public static DbConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
