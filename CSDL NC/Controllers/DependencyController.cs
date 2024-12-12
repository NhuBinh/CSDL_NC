using Microsoft.AspNetCore.Mvc;
using CSDL_NC.Models;
using System.Collections.Generic;
using System.Linq;

namespace CSDL_NC.Controllers
{
    [ApiController]
    [Route("api")]
    public class DependencyController : ControllerBase
    {
        // API Tính Bao Đóng
        [HttpPost("closure")]
        public IActionResult CalculateClosure([FromBody] ClosureRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Attributes) || string.IsNullOrWhiteSpace(model.FunctionalDependencies))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ các thông tin." });
            }

            // Tách và chuẩn hóa các thuộc tính ban đầu
            var attributes = model.Attributes.Split(',').Select(a => a.Trim().ToUpper()).ToList();
            var closureToCalculate = model.ClosureToCalculate.Split(',').Select(a => a.Trim().ToUpper()).ToList();

            // Xử lý các phụ thuộc hàm
            var dependencies = ParseDependencies(model.FunctionalDependencies);

            // Tính bao đóng
            var closureResult = CalculateAttributeClosure(closureToCalculate, dependencies);

            return Ok(new { closure = string.Join(", ", closureResult) });
        }

        
        // API Chứng Minh Armstrong
        [HttpPost("armstrong")]
        public IActionResult ValidateArmstrong([FromBody] ArmstrongRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.FunctionalDependencies) || string.IsNullOrWhiteSpace(model.Proof))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ các thông tin." });
            }

            // Tách và xử lý dữ liệu
            var dependencies = ParseDependencies(model.FunctionalDependencies);
            var proof = ParseSingleDependency(model.Proof);

            // Kiểm tra chứng minh
            var isValid = ValidateDependencyUsingArmstrong(dependencies, proof);

            return Ok(new { isValid });
        }

        // Phương thức xử lý phụ thuộc hàm
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

        private FunctionalDependency ParseSingleDependency(string dependencyString)
        {
            var parts = dependencyString.Split("->");
            return new FunctionalDependency
            {
                Left = parts[0].Trim().ToUpper().Split(',').Select(attr => attr.Trim()).ToList(),
                Right = parts[1].Trim().ToUpper().Split(',').Select(attr => attr.Trim()).ToList()
            };
        }

        // Phương thức tính bao đóng
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

        // Phương thức tìm tập khóa
        private List<List<string>> FindCandidateKeys(List<string> attributes, List<FunctionalDependency> dependencies)
        {
            var allAttributes = new HashSet<string>(attributes);
            var subsets = GenerateSubsets(attributes);

            // Lọc tập con là khóa
            var candidateKeys = new List<List<string>>();
            foreach (var subset in subsets)
            {
                var closure = CalculateAttributeClosure(subset, dependencies);
                if (closure.SetEquals(allAttributes))
                {
                    if (!candidateKeys.Any(key => key.All(subset.Contains) && subset.All(key.Contains)))
                    {
                        candidateKeys.Add(subset);
                    }
                }
            }
            return candidateKeys;
        }

        // Phương thức chứng minh dựa trên Armstrong
        private bool ValidateDependencyUsingArmstrong(List<FunctionalDependency> dependencies, FunctionalDependency proof)
        {
            var closure = CalculateAttributeClosure(proof.Left, dependencies);
            return proof.Right.All(attr => closure.Contains(attr));
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
