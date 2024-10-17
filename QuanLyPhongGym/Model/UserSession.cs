public class UserSession
{
    // Biến tĩnh giữ thể hiện duy nhất của lớp
    private static UserSession _instance;

    // Thuộc tính để lưu trữ token và username
    public string Token { get; private set; }
    public string UserName { get; private set; }
    public string Id { get; private set; }
    public string Role { get; private set; }
    public string Branch_id { get; private set; }

    // Constructor riêng tư để ngăn việc tạo đối tượng từ bên ngoài
    private UserSession() { }

    // Phương thức để lấy thể hiện duy nhất
    public static UserSession Instance
    {
        get
        {
            // Nếu chưa có thể hiện nào, tạo mới
            if (_instance == null)
            {
                _instance = new UserSession();
            }
            return _instance;
        }
    }

    // Phương thức để đăng nhập
    public void Login(string token, string userId, string userName, string role, string BranchId)
    {
        Token = token;
        UserName = userName;
        Id = userId;
        Role = role;
        Branch_id = BranchId;
    }

    // Phương thức để đăng xuất
    public void Logout()
    {
        Token = null;
        UserName = null;
        Id = null;
        Role = null;
        Branch_id = null;
    }

    // Thuộc tính để kiểm tra xem người dùng đã đăng nhập chưa
    public bool IsLoggedIn => !string.IsNullOrEmpty(Token);
}
