using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DeportHub.DTOs;
using DeportHub.Models;

namespace DeportHub.Services;

public class AuthService
{

    private readonly FirebaseService _firebaseService;

    private readonly IConfiguration _configuration;

    public AuthService(FirebaseService firebaseService, IConfiguration configuration)
    {
        _firebaseService = firebaseService;
        _configuration = configuration;
    }

    public async Task<Miembro> Register(Register dto)
    {
        var collection = _firebaseService.GetCollection("Miembro");


        var existing = await collection
            .WhereEqualTo("Email", dto.Email)
            .GetSnapshotAsync();

        if (existing.Count > 0)
            throw new Exception("Ya existe un usuario con ese correo");

        var Miembro = new Miembro
        {
            Id = Guid.NewGuid().ToString(),
            Username = dto.Fullname,
            Email = dto.Email,
            PasswordHash= HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };


        await collection.Document(Miembro.Id).SetAsync(new Dictionary<string, object>
        {
            { "Id", Miembro.Id },
            { "Fullname", Miembro.Username },
            { "Email", Miembro.Email },
            { "PasswordHash", Miembro.PasswordHash },
            { "CreatedAt", Miembro.CreatedAt }
        });

        return Miembro;
    }

    public async Task<string> Login(Login dto)
    {
        var collection = _firebaseService.GetCollection("Miembro");

        var snapshot = await collection
            .WhereEqualTo("Email", dto.Email)
            .GetSnapshotAsync();

        
        if (snapshot.Count == 0)
            throw new Exception("Credenciales invalidas");

        
        var data = snapshot.Documents[0].ToDictionary();

        var Miembro = new Miembro
        {
            Id = data["Id"].ToString()!,
            Username = data["Fullname"].ToString()!,
            Email = data["Email"].ToString()!,
            PasswordHash = data["PasswordHash"].ToString()!,
            CreatedAt = ((Google.Cloud.Firestore.Timestamp)data["CreatedAt"]).ToDateTime()
        };

        if (!VerifyPassword(dto.Password, Miembro.PasswordHash))
            throw new Exception("Credenciales invalidas");

        return GenerateToken(Miembro);
    }

    private string GenerateToken(Miembro user)
    {
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };
        
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],     // quien genera el token
            audience: _configuration["Jwt:Audience"], // para quien es el token
            claims: claims,                           // los datos del usuario
            expires: DateTime.UtcNow.AddHours(8),     // cuanto dura el token
            signingCredentials: creds                 // la firma de seguridad
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}