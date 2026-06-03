ĐỀ KIỂM TRA GIỮA KỲ
Hệ thống đăng ký học phần cho sinh viên

Mô tả hệ thống
Xây dựng ứng dụng Course Registration Application sử dụng:
ASP.NET Core MVC
Entity Framework Core
ASP.NET Core Identity
Hệ thống cho phép sinh viên đăng ký học phần và quản lý tài khoản.

Database gợi ý
Sử dụng ASP.NET Core Identity cho quản lý người dùng và phân quyền.
🔹 Các bảng có sẵn từ Identity:
AspNetUsers (lưu thông tin người dùng)
AspNetRoles (ADMIN, STUDENT)
AspNetUserRoles

🔹 Các bảng cần xây dựng thêm:
Course
Id
Name
Image
Credits
Lecturer
CategoryId
Category
Id
Name
Enrollment
Id
UserId (FK → AspNetUsers.Id)
CourseId (FK → Course.Id)
EnrollDate

Câu hỏi

Câu 1 (2,5 điểm)
Xây dựng trang Home hiển thị danh sách tất cả học phần (Course) gồm:
Tên học phần
Số tín chỉ
Giảng viên
Hình ảnh minh họa
Yêu cầu:
Hiển thị bằng View (Razor) (2điểm)
Có phân trang, mỗi trang 5 học phần (0,5đ)

Câu 2 (1,5 điểm)
Xây dựng chức năng CRUD học phần cho ADMIN:
Create Course
Edit Course
Delete Course
Yêu cầu:
Sử dụng Controller + View
Áp dụng [Authorize(Roles = "Admin")]

Câu 3 (1 điểm)
Xây dựng chức năng đăng ký tài khoản (Register) sử dụng ASP.NET Core Identity, gồm:
Username
Password
Email
Yêu cầu:
Sau khi đăng ký → lưu vào AspNetUsers
Gán role mặc định: STUDENT

Câu 4 (0,5 điểm)
Cấu hình Authorization:
/admin/** → chỉ ADMIN truy cập
/courses → tất cả người dùng truy cập
/enroll/** → chỉ STUDENT

Câu 5 (0,5 điểm)
Xây dựng chức năng đăng nhập (Login):
Sử dụng ASP.NET Core Identity
Sau khi đăng nhập thành công → chuyển về /home

Câu 6 (1 điểm)
Xây dựng chức năng Đăng ký học phần (Enroll Course):
Hiển thị nút Enroll trong danh sách học phần
Chỉ STUDENT được phép đăng ký
Cho phép sinh viên hủy đăng ký

Câu 7 (1 điểm)
Xây dựng trang My Courses:
Hiển thị danh sách học phần mà người dùng đã đăng ký

Câu 8 (0,5 điểm)
Thực hiện chức năng Tìm kiếm học phần:
Tìm theo tên học phần (Course Name)
Hiển thị đúng kết quả chứa từ khóa

Câu 9 (1 điểm)
Tích hợp chức năng đăng nhập bằng Google:
Sử dụng External Login (Google Authentication) trong ASP.NET Core

Câu 10 (0,5 điểm)
Thiết kế giao diện:
Responsive
Sử dụng Bootstrap hoặc CSS framework
--hết--
