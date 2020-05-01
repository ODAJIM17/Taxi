using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Common.Enums;
using Taxi.Common.Models;
using Taxi.Web.Data;
using Taxi.Web.Data.Entities;
using Taxi.Web.Helpers;

namespace Taxi.Web.Controllers.API
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class UserGroupsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public UserGroupsController(
            DataContext context,
            IConverterHelper converterHelper,
            IUserHelper userHelper,
            IMailHelper mailHelper)
        {
            _context = context;
            _converterHelper = converterHelper;
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }
        [HttpPost]
        public async Task<IActionResult> PostUserGroup([FromBody] AddUserGroupRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //CultureInfo cultureInfo = new CultureInfo(request.CultureInfo);
            //Resource.Culture = cultureInfo;

            UserEntity proposalUser = await _userHelper.GetUserAsync(request.UserId);
            if (proposalUser == null)
            {
                return BadRequest(new Response
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            UserEntity requiredUser = await _userHelper.GetUserAsync(request.Email);
            if (requiredUser == null)
            {
                return BadRequest(new Response
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            UserGroupEntity userGroup = await _context.UserGroups
                .Include(ug => ug.Users)
                .ThenInclude(u => u.User)
                .FirstOrDefaultAsync(ug => ug.User.Id == request.UserId.ToString());
            if (userGroup != null)
            {
                UserGroupDetailEntity user = userGroup.Users.FirstOrDefault(u => u.User.Email == request.Email);
                if (user != null)
                {
                    return BadRequest(new Response
                    {
                        IsSuccess = false,
                        Message = "User already belongs to a group"
                    });
                }
            }

            UserGroupRequestEntity userGroupRequest = new UserGroupRequestEntity
            {
                ProposalUser = proposalUser,
                RequiredUser = requiredUser,
                Status = UserGroupStatus.Pending,
                Token = Guid.NewGuid()
            };

            try
            {
                _context.UserGroupRequests.Add(userGroupRequest);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            string linkConfirm = Url.Action("ConfirmUserGroup", "Account", new
            {
                requestId = userGroupRequest.Id,
                token = userGroupRequest.Token
            }, protocol: HttpContext.Request.Scheme);

            string linkReject = Url.Action("RejectUserGroup", "Account", new
            {
                requestId = userGroupRequest.Id,
                token = userGroupRequest.Token
            }, protocol: HttpContext.Request.Scheme);

            Response response = _mailHelper.SendMail(request.Email,"Request To Join Group", $"<h1>{"Invitation to join a user group"}</h1>" +
                $"{"User"}: {proposalUser.FullName} ({proposalUser.Email}), {" Has Invite you to join a user group for the Taxi Survey Application."}" +
                $"</hr></br></br>{"If you wish to accept, clik here"} <a href = \"{linkConfirm}\">{"Confirm"}</a>" +
                $"</hr></br></br>{"If you wish To Reject, clck here"} <a href = \"{linkReject}\">{"Not Inerested"}</a>");

            if (!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }

            return Ok("Request To Join a Group EmailSent");
        }




        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserGroup([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            UserGroupEntity userGroup = await _context.UserGroups
                .Include(ug => ug.Users)
                .ThenInclude(u => u.User)
                .FirstOrDefaultAsync(u => u.User.Id == id);

            if (userGroup == null || userGroup?.Users == null)
            {
                return Ok();
            }

            return Ok(_converterHelper.ToUserGroupResponse(userGroup.Users.ToList()));
        }
    }
}
