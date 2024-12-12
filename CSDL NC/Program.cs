var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Thêm dịch vụ MVC
builder.Services.AddControllers(); // Đảm bảo đã đăng ký dịch vụ cho controller

var app = builder.Build();

// Các cấu hình khác
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Hiển thị lỗi trong môi trường phát triển
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Xử lý lỗi trong môi trường sản xuất
    app.UseHsts(); // Bật HSTS
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting(); // Bật định tuyến

app.MapControllers(); // Ánh xạ các controller

app.Run(); // Chạy ứng dụng
