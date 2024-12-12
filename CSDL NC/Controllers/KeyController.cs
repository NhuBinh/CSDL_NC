using Microsoft.AspNetCore.Mvc;
using CSDL_NC.Models;
using System.Collections.Generic;
using System.Linq;

namespace CSDL_NC.Controllers
{
    [ApiController]
    [Route("api/keys")] // Đặt route cho KeyController
    public class KeyController : ControllerBase
    {
        [HttpPost]
        public IActionResult FindKeys([FromBody] KeyRequest model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Attributes) || string.IsNullOrWhiteSpace(model.FunctionalDependencies))
                {
                    return BadRequest(new { message = "Vui lòng nhập đầy đủ các thông tin." });
                }

                // Tách và chuẩn hóa các thuộc tính ban đầu
                var attributes = model.Attributes.Split(',').Select(a => a.Trim().ToUpper()).ToList();
                var dependencies = ParseDependencies(model.FunctionalDependencies);

                // Bước 1: Tìm tập TN và TG
                var tn = new HashSet<string>(); // Tập tối thiểu (TN)
                var tg = new HashSet<string>(attributes); // Tập tổng (TG)

                // Bước 2: Nếu TG rỗng hoặc TN bao gồm tập rỗng thì khóa = TN.
                if (tg.Count == 0 || tn.Contains("Q+"))
                {
                    return Ok(new
                    {
                        superKeys = new List<string> { string.Join("", tn) }, // Không có dấu phẩy
                        keys = new List<string> { string.Join("", tn) } // Không có dấu phẩy
                    });
                }

                // Bước 3: Tìm tất cả tập con Xi của TG
                var subsets = GenerateSubsets(attributes);

                // Bước 4: Tạo TK và kiểm tra các tập con
                var tk = new List<List<string>>();

                foreach (var subset in subsets)
                {
                    if (subset.Count == 0) continue; // Bỏ qua tập rỗng

                    // Tính bao đóng của tập con
                    var closure = CalculateAttributeClosure(subset, dependencies);

                    // Nếu bao đóng bao phủ toàn bộ thuộc tính
                    if (closure.SetEquals(tg))
                    {
                        // Thêm siêu khóa mới
                        tk.Add(new List<string>(subset));
                    }
                }

                // Bước 5: Lọc TK để loại bỏ các tập con
                var keys = new List<List<string>>();

                foreach (var ki in tk)
                {
                    bool isKey = true;
                    // Kiểm tra xem có khóa khác nào là tập con của ki không
                    foreach (var kj in keys)
                    {
                        if (kj.All(ki.Contains))
                        {
                            isKey = false;
                            break;
                        }
                    }

                    // Nếu ki là khóa mới
                    if (isKey)
                    {
                        // Loại bỏ các khóa cũ là tập con của ki
                        keys.RemoveAll(existingKey => ki.All(existingKey.Contains));
                        keys.Add(ki);
                    }
                }

                return Ok(new
                {
                    superKeys = tk.Select(sk => string.Join("", sk)).ToList(), // Không có dấu phẩy
                    keys = keys.Select(key => string.Join(", ", key)).ToList() // Giữ dấu phẩy cho khóa chính
                });
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi để dễ dàng theo dõi
                return StatusCode(500, new { message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        private List<FunctionalDependency> ParseDependencies(string dependenciesString)
        {
            return dependenciesString.Split(',')
                .Select(d =>
                {
                    var parts = d.Split("->");
                    return new FunctionalDependency
                    {
                        Left = parts[0].Trim().ToUpper().Split(',').Select(attr => attr.Trim()).ToList(),
                        Right = parts[1].Trim().ToUpper().Split(',').Select(attr => attr.Trim()).ToList()
                    };
                }).ToList();
        }

        private HashSet<string> CalculateAttributeClosure(List<string> attributes, List<FunctionalDependency> dependencies)
        {
            var closure = new HashSet<string>(attributes);
            int previousCount;

            do
            {
                previousCount = closure.Count;
                foreach (var dep in dependencies)
                {
                    if (dep.Left.All(attr => closure.Contains(attr)))
                    {
                        foreach (var attr in dep.Right)
                        {
                            closure.Add(attr);
                        }
                    }
                }
            } while (closure.Count > previousCount);

            return closure;
        }

        // Phương thức tạo tập con
        private List<List<string>> GenerateSubsets(List<string> set)
        {
            var subsets = new List<List<string>> { new List<string>() };
            foreach (var element in set)
            {
                var newSubsets = subsets.Select(subset => new List<string>(subset) { element }).ToList();
                subsets.AddRange(newSubsets);
            }
            return subsets;
        }
    }
}
