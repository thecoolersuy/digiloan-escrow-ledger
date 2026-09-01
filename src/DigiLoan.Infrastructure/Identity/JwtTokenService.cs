using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DigiLoan.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace DigiLoan.Infrastructure.Identity;

public class JwtTokenGenerator : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Guid userId, string email)
    {
        var claims = new[]
        {
         new Claim(JwtRegisteredClaimNames.Sub , userId.ToString()),
         new Claim(JwtRegisteredClaimNames.Email, email),
         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
           issuer: _configuration["Jwt:Issuer"],
           audience: _configuration["Jwt:Audience"],
           claims: claims,
           expires: DateTime.UtcNow.AddMinutes(60),
           signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}