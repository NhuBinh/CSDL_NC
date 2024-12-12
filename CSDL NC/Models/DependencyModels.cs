using System.Collections.Generic;

namespace CSDL_NC.Models
{
    // Model tính bao đóng
    public class ClosureRequest
    {
        public string? Attributes { get; set; } // Tất cả thuộc tính
        public string? FunctionalDependencies { get; set; } // Các phụ thuộc hàm
        public string? ClosureToCalculate { get; set; } // Tập cần tính bao đóng
    }

    // Model tìm khóa
    public class KeyRequest
    {
        public string? Attributes { get; set; } // Tất cả thuộc tính
        public string? FunctionalDependencies { get; set; } // Các phụ thuộc hàm
    }

    // Model chứng minh Armstrong
    public class ArmstrongRequest
    {
        public string? FunctionalDependencies { get; set; } // Các phụ thuộc hàm
        public string? Proof { get; set; } // Phụ thuộc hàm cần chứng minh
    }

    // Phụ thuộc hàm
    public class FunctionalDependency
    {
        public List<string>? Left { get; set; } // Tập bên trái
        public List<string>? Right { get; set; } // Tập bên phải
    }
}
