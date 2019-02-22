using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using UserRegistrationApi.Models;

namespace UserRegistrationApi.Controllers
{
    public class AccountController : ApiController
    {
        [Route("api/User/Register")]
        [HttpPost]
        [AllowAnonymous]
        public IdentityResult Register(AccountModel model)
        {
            var userStore = new UserStore<ApplicationUser>(new ApplicationDBContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            var user = new ApplicationUser() { UserName = model.UserName, Email = model.Email };
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Time = DateTime.Now.ToString();
            //user.ImageUrl = model.ImageUrl;

            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 3
            };
            IdentityResult result = manager.Create(user, model.Password);
            ApplicationDBContext db = new ApplicationDBContext();
            Image img = new Image { AId = user.Id, ImageUrl = "download.jpg" };
            db.images.Add(img);
            db.SaveChanges();

            return result;
        }

        [Authorize]
        [HttpGet]
        [Route("api/GetUserClaims")]

        public AccountModel GetUserClaims()
        {
            var identityClaims = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identityClaims.Claims;
            AccountModel model = new AccountModel()
            {
                UserName = identityClaims.FindFirst("Username").Value,
                Email = identityClaims.FindFirst("Email").Value,
                FirstName = identityClaims.FindFirst("FirstName").Value,
                LastName = identityClaims.FindFirst("LastName").Value,
                PhoneNumber = identityClaims.FindFirst("PhoneNumber").Value,
                LoggedOn = identityClaims.FindFirst("LoggedOn").Value,
                Time = identityClaims.FindFirst("Time").Value,

            };

            return model;
        }



        [HttpPost]
        [AllowAnonymous]
        [Route("api/UploadImage")]
        public HttpResponseMessage UploadImage()
        {
            string imageName = null;
            var httpRequest = HttpContext.Current.Request;

            //upload
            var postedFile = httpRequest.Files["Image"];
            var UserName = httpRequest.Form["Username"];
            imageName = new string(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
            imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
            var filePath = HttpContext.Current.Server.MapPath("~/Image/" + imageName);
            postedFile.SaveAs(filePath);

            var userStore = new UserStore<ApplicationUser>(new ApplicationDBContext());
            var manager = new UserManager<ApplicationUser>(userStore);

            var user = manager.FindByName(UserName);
            System.Diagnostics.Debug.WriteLine(user.FirstName);

            ApplicationDBContext db = new ApplicationDBContext();
            Image img = db.images.FirstOrDefault(a => a.AId == user.Id);
            File.Delete(img.ImageUrl);
            img.ImageUrl = imageName;
            db.Entry(img).State = EntityState.Modified;
            db.SaveChanges();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        [HttpPost]
        [Route("api/UName")]
        [AllowAnonymous]
        public string UName()
        {
            var httpRequest = HttpContext.Current.Request;
            var userStore = new UserStore<ApplicationUser>(new ApplicationDBContext());
            var manager = new UserManager<ApplicationUser>(userStore);

            var jsonContent = Request.Content.ReadAsStringAsync().Result;

            JObject jObject = JObject.Parse(jsonContent);
            var Username = jObject["userName"].ToString();
            System.Diagnostics.Debug.WriteLine(Username);
            var user = manager.FindByName(Username);
            if (user != null)
                return "username available";
            else
                return "not available";
        }


        [HttpGet]
        [Route("api/GetImg")]
        [AllowAnonymous]
        public string GetImg(string Username)
        {
            var httpRequest = HttpContext.Current.Request;
            var userStore = new UserStore<ApplicationUser>(new ApplicationDBContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            var user = manager.FindByName(Username);
            ApplicationDBContext db = new ApplicationDBContext();

            var user1 = db.images.FirstOrDefault(a => a.AId == user.Id);
            if (user1 != null)
            {
                System.Diagnostics.Debug.WriteLine(user1.ImageUrl);
                return user1.ImageUrl;
            }
            else
                return "not available";


        }

        [HttpPost]
        [Route("api/Edit")]
        [AllowAnonymous]
        public HttpResponseMessage Edit_Post(string UserName)
        {
            //var UserName = HttpContext.Current.Request.Form["UserName"];
            var userStore = new UserStore<ApplicationUser>(new ApplicationDBContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            var user = manager.FindByName(UserName);
            if (user != null)
            {
                manager.Update(user);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
                return null;
                
        }

        [HttpGet]
        [Route("api/Edit")]
        public ApplicationUser Edit(string username)
        {
            var userStore = new UserStore<ApplicationUser>(new ApplicationDBContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            var user = manager.FindByName(username);

            return user;


        }
    }
}