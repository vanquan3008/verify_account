using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using be_account.Services.IServices;
using be_account.Models;
using be_account.Response;
using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace be_account.Controllers
{
    public class OauthController : ControllerBase
    {
        private readonly IJwtService _jwtservice;

        public OauthController(IJwtService jwtService)
        {
            _jwtservice = jwtService;
        }
        [HttpGet]
        [Route("signin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public  IActionResult Login()
        {
            try
            {
                var redirectUrl = Url.Action(nameof(Callback), "Auth", null, Request.Scheme);
                var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
                return Challenge(properties, GoogleDefaults.AuthenticationScheme);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Message = "Message error : " + ex

                }); 

            }
        }

        [HttpGet]
        [Route("callback")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> Callback()
        {
            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                if (!authenticateResult.Succeeded)
                {
                    return BadRequest(new { Error = "Authentication failed", Details = authenticateResult });
                }

                var token = _jwtservice.GenerateJwtTokenGG(authenticateResult.Principal, ["User"]);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new CustomResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Message = "Message error : " + ex

                });

            }
        }
    }
}
