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

                var attributes = model.Attributes.Split(',').Select(a => a.Trim().ToUpper()).ToList();
                var dependencies = ParseDependencies(model.FunctionalDependencies);

                var tn = new HashSet<string>(); // Tập tối thiểu (TN)
                var tg = new HashSet<string>(attributes); // Tập tổng (TG)

                if (tg.Count == 0 || tn.Contains("Q+"))
                {
                    return Ok(new
                    {
                        superKeys = new List<string> { string.Join("", tn) },
                        keys = new List<string> { string.Join("", tn) }
                    });
                }

                var subsets = GenerateSubsets(attributes);
                var superKeys = new List<List<string>>(); // Danh sách siêu khóa
                var keys = new List<List<string>>(); // Danh sách khóa chính

                foreach (var subset in subsets)
                {
                    if (subset.Count == 0) continue;

                    // Tính bao đóng của tập con
                    var closure = CalculateAttributeClosure(subset, dependencies);

                    // Nếu bao đóng bao phủ toàn bộ thuộc tính
                    if (closure.SetEquals(tg))
                    {
                        superKeys.Add(new List<string>(subset)); // Thêm vào danh sách siêu khóa

                        // Kiểm tra xem tập con này có phải là khóa chính không
                        bool isKey = true;
                        foreach (var existingKey in keys)
                        {
                            if (existingKey.All(subset.Contains))
                            {
                                isKey = false;
                                break;
                            }
                        }

                        // Nếu subset là khóa chính mới
                        if (isKey)
                        {
                            // Loại bỏ các khóa cũ là tập con của subset
                            keys.RemoveAll(existingKey => subset.All(existingKey.Contains));
                            keys.Add(new List<string>(subset));
                        }
                    }
                }

                return Ok(new
                {
                    superKeys = superKeys.Select(sk => string.Join("", sk)).ToList(),
                    keys = keys.Select(key => string.Join("", key)).ToList()
                });
            }
            catch (Exception ex)
            {
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
