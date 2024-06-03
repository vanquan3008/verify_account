//Import project
using be_account.DTOs.Requests;
using be_account.DTOs.Respones;
using be_account.Models;
using be_account.Response;
using be_account.Services;
using be_account.Services.IServices;
using Microsoft.AspNetCore.Authorization;

//Import library
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace be_account.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IAccountService _accountService;
        private readonly IJwtService _jwtservice;
        public AccountController(UserManager<User> userManager, IAccountService accountService, IJwtService jwtService)
        {
            _userManager = userManager;
            _accountService = accountService;
            _jwtservice = jwtService;
        }
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register_User([FromBody] UserRegisterDTO userRegister)
        {
            try
            {
                if (userRegister.Password.CompareTo(userRegister.ConfirmPassword) != 0)
                {

                    return StatusCode(StatusCodes.Status400BadRequest, new CustomResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Message = "Can't comfirm password"
                    });

                }
                if (await _accountService.FindUserbyUserName(userRegister.UserName) != null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new CustomResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Message = "Username is aready exists."
                    });
                }
                if (await _accountService.FindUserbyEmail(userRegister.Email) != null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new CustomResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Message = "Email is aready exists."
                    });
                }
                var IdentityResult = await _accountService.CreateUser(userRegister);
                if (IdentityResult.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK, new CustomResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = "Create account successfully",
                        Result = IdentityResult
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new CustomResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Message = "Some thing wrong!"
                    });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Message = "Internal Server Error: " + ex.Message,

                });
            }
        }
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> Login([FromBody] UserLoginDTO userLogin)
        {
            try
            {
                var user = await _accountService.FindUserbyUserName(userLogin.Username);
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new CustomResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        IsSuccess = false,
                        Message = "Username is not exists",
                    });
                }
                var verifyPassword = await _userManager.CheckPasswordAsync(user, userLogin.Password);
                if (verifyPassword)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles != null)
                    {
                        var tokenUser = _jwtservice.CreateJWTToken(user, roles.ToList());
                        var refreshToken = _jwtservice.CreateJWTRefresh(user, roles.ToList());
                        //Set cookie
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true, // Đảm bảo cookie chỉ có thể truy cập qua HTTP
                            Secure = true, // Chỉ truyền cookie qua HTTPS
                            Expires = DateTime.Now.AddDays(364) // Thiết lập thời gian hết hạn cho cookie
                        };

                        Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);

                        return StatusCode(StatusCodes.Status200OK, new CustomResponse
                        {
                            StatusCode = HttpStatusCode.OK,
                            Message = "Login user is successfully",
                            Result = new LoginRespone
                            {
                                Token = tokenUser,
                                Username = user.UserName,
                                FullName = user.FullName
                            }
                        });
                    }
                }
                return StatusCode(StatusCodes.Status400BadRequest, new CustomResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Message = "Password is wrong!",

                });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Message = "Internal Server Error: " + ex.Message,

                });
            }
        }
        [HttpGet]
        [Route("verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Verify()
        {
            var token = HttpContext.Request.Headers["JwtToken"].FirstOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                return StatusCode(StatusCodes.Status404NotFound, new CustomResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Message = "Token not found",
                });
            }
            else
            {
                bool CheckToken = _jwtservice.ExpriedToken(token);
                if (CheckToken == true)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new CustomResponse
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        IsSuccess = false,
                        Message = "Access token is expried"
                    });
                }
                else
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    List<string> Claims = [];
                    List<string> Values = [];
                    foreach (var claim in jwtToken.Claims)
                    {
                        Claims.Add(claim.Type);
                        Values.Add(claim.Value);
                    }

                    var user = await _accountService.FindUserbyUserName(Values[0]);

                    return StatusCode(StatusCodes.Status200OK, new CustomResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = "Verify access token is successfully",
                        Result = user
                    });
                }
            }
        }

        [HttpGet]
        [Route("refreshToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var token = Request.Cookies["RefreshToken"];

                if (token == null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new CustomResponse
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        IsSuccess = false,
                        Message = "Can not verify token refresh"

                    });
                }
                bool checkexp = _jwtservice.ExpriedToken(token);
                if (checkexp == true)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new CustomResponse
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        IsSuccess = false,
                        Message = "Token Refresh is Expired"

                    });
                }
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                List<string> Claims = [];
                List<string> Values = [];
                foreach (var claim in jwtToken.Claims)
                {
                    Claims.Add(claim.Type);
                    Values.Add(claim.Value);
                }

                var user = await _accountService.FindUserbyUserName(Values[0]);
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new CustomResponse
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        IsSuccess = false,
                        Message = "Can not verify token refresh"

                    });
                }
                var roles = await _userManager.GetRolesAsync(user);
                var tokenUser = _jwtservice.CreateJWTToken(user, roles.ToList());
                var refreshToken = _jwtservice.CreateJWTRefresh(user, roles.ToList());
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // Đảm bảo cookie chỉ có thể truy cập qua HTTP
                    Secure = true, // Chỉ truyền cookie qua HTTPS
                    Expires = DateTime.Now.AddDays(364) // Thiết lập thời gian hết hạn cho cookie
                };

                Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);

                return StatusCode(StatusCodes.Status200OK, new CustomResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Result = new RefreshTokenRespone
                    {
                        TokenAccess = tokenUser
                    }
                    ,
                    Message = "Refresh token successfully"

                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Message = "Internal Server Error: " + ex.Message,

                });
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("Getuser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUSer()
        {
            var user = await _accountService.GetAll();
            Console.WriteLine(user);
            return StatusCode(StatusCodes.Status200OK, new CustomResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Result = user,
                Message = "Refresh token successfully"

            });

        }
    }
}
