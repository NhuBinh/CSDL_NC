$(document).ready(function () {
    $('#closureForm').on('submit', function (e) {
        e.preventDefault();
        const attributes = $('#attributes').val().trim();
        const functionalDependencies = $('#functionalDependencies').val().trim();
        const closureToCalculate = $('#closureToCalculate').val().trim();
        if (!attributes || !functionalDependencies || !closureToCalculate) {
            alert('Vui lòng nhập đầy đủ thông tin.');
            return;
        }
        $.ajax({
            url: '/api/closure',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Attributes: attributes,
                FunctionalDependencies: functionalDependencies,
                ClosureToCalculate: closureToCalculate
            }),
            success: function (response) {
                $('#closureResult').text(`Bao đóng của ${closureToCalculate}: ${response.closure}`);
            },
            error: function (xhr) {
                const errorMessage = xhr.responseJSON?.message || 'Đã xảy ra lỗi!';
                alert(errorMessage);
            }
        });
    });

    $('#keyForm').on('submit', function (e) {
        e.preventDefault();

        // Lấy và làm sạch dữ liệu đầu vào
        const attributes = $('#keysAttributes').val().trim();
        const functionalDependencies = $('#keyFunctionalDependencies').val().trim();

        // Validate đầu vào
        if (!attributes || !functionalDependencies) {
            alert('Vui lòng nhập đầy đủ thông tin.');
            return;
        }

        // Hiển thị trạng thái loading
        $('#keyResult').removeClass('d-none');
        $('#keyOutput').html('<div class="text-center"><div class="spinner-border text-success" role="status"><span class="visually-hidden">Đang tải...</span></div><p>Đang xử lý...</p></div>');

        // Gọi API tìm khóa
        $.ajax({
            url: '/api/keys',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Attributes: attributes,
                FunctionalDependencies: functionalDependencies
            }),
            success: function (response) {
                // Hiển thị kết quả chi tiết
                let resultHtml = `
                <div class="mt-3">
                    <div class="mb-2">
                        <strong>Các Thuộc Tính:</strong> ${attributes}
                    </div>
                    <div class="mb-2">
                        <strong>Siêu Khóa:</strong>
                        <p class="text-primary">${response.superKeys.join('; ')}</p>
                    </div>
                    <div>
                        <strong>Khóa Chính:</strong>
                        <p class="text-success">${response.keys.join('; ')}</p>
                    </div>
                </div>
            `;

                // Hiển thị kết quả
                $('#keyOutput').html(resultHtml);
            },
            error: function (xhr) {
                // Xử lý lỗi
                const errorMessage = xhr.responseJSON?.message || 'Đã xảy ra lỗi không xác định!';
                $('#keyOutput').html(`<div class="text-danger"><strong>Lỗi:</strong> ${errorMessage}</div>`);
            }
        });
    });

    // Thêm các tính năng hỗ trợ người dùng
    $(document).ready(function () {
        // Thêm ví dụ mẫu
        $('#btnExample').on('click', function () {
            $('#keysAttributes').val('A, B, C, D');
            $('#keyFunctionalDependencies').val('A->B, B->C, A,B->D');
        });

        // Nút xóa form
        $('#btnClear').on('click', function () {
            $('#keysAttributes').val('');
            $('#keyFunctionalDependencies').val('');
            $('#keyResult').addClass('d-none');
            $('#keyOutput').empty();
        });
    });
});