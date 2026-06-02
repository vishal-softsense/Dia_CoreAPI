using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Repository;
using Dia_CoreAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Dia_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly MasterRepository _repository;

        public LoginController(
            JwtService jwtService,
            MasterRepository repository)
        {
            _jwtService = jwtService;
            _repository = repository;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginModel model)
        {
            try
            {
                string connString =
                    _repository.GetConnectionString(
                        "DefaultConnection");

                LoginResponseModel user = null;

                string encryptedPassword =
                    SecurityProcessor.EncryptPassword(
                        model.Password);

                using (SqlConnection con =
                    new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd =
                        new SqlCommand(@"
                        SELECT TOP 1 *
                        FROM TabUserMaster
                        WHERE UserName=@UserName
                        AND CompanyID=@CompanyID",
                        con);

                    cmd.Parameters.AddWithValue(
                        "@UserName",
                        model.Username);

                    cmd.Parameters.AddWithValue(
                        "@CompanyID",
                        model.CompanyId);

                    SqlDataReader dr =
                        cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        user =
                            new LoginResponseModel
                            {
                                UserID =
                                    Convert.ToInt32(
                                        dr["UserID"]),
                                UserName =
                                    dr["UserName"].ToString(),
                                FirstName =
                                    dr["FirstName"].ToString(),
                                LastName =
                                    dr["LastName"].ToString(),
                                Title =
                                    dr["Title"].ToString(),
                                UserTypeId =
                                    Convert.ToInt32(
                                        dr["UserTypeId"]),
                                CompanyID =
                                    Convert.ToInt32(
                                        dr["CompanyID"]),
                                ProfileImage =
                                    dr["ProfileImage"]
                                    .ToString()
                            };
                    }
                }

                if (user == null)
                {
                    return Unauthorized(
                        new
                        {
                            Success = false,
                            Message = "User not found"
                        });
                }

                string token =
                    _jwtService.GenerateToken(user);

                return Ok(new
                {
                    Success = true,
                    Token = token,
                    User = user
                });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = ex.Message
                    });
            }
        }
    }
}