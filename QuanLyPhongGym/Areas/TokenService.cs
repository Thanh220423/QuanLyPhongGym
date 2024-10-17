using Microsoft.IdentityModel.Tokens;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService
{
    protected string SecretKey = ConfigurationManager.AppSettings["KeyCrypt"];
    private const int ExpirationMinutes = 10; // Thời gian hết hạn của token

    public string GenerateToken(string UserId, string UserName)
    {
        // Tạo một khóa từ chuỗi bí mật
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Tạo các yêu cầu (claims)
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, UserName),
            new Claim("UserId", UserId) // Thêm bất kỳ yêu cầu tùy chỉnh nào khác nếu cần
        };

        // Tạo token
        var token = new JwtSecurityToken(
            issuer: "MiniBank",
            audience: "MiniBank",
            claims: claims,
            expires: DateTime.Now.AddMinutes(ExpirationMinutes),
            signingCredentials: creds);

        // Trả về token dưới dạng chuỗi
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Kiểm tra tính hợp lệ của token
    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = "MiniBank",
                ValidateAudience = true,
                ValidAudience = "MiniBank",
                ClockSkew = TimeSpan.Zero // Đặt độ trễ về 0
            }, out SecurityToken validatedToken);

            // Kiểm tra thời gian hết hạn
            var jwtToken = validatedToken as JwtSecurityToken;
            if (jwtToken != null)
            {
                var expiryDate = jwtToken.ValidTo;
                if (expiryDate < DateTime.UtcNow) // Kiểm tra xem token đã hết hạn chưa
                {
                    return false; // Token đã hết hạn
                }
            }

            return true; // Token hợp lệ và chưa hết hạn
        }
        catch
        {
            return false; // Token không hợp lệ hoặc đã hết hạn
        }
    }

}
