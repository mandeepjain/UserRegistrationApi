using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        /**
         * Register Method to create a new record
         * @param A Form which contains an object of the AccountModel class
         *  This methods creates a record in two tables(User and Image)  
         * @returns record created
         **/
        public IdentityResult Register(AccountModel model)
        {
            var userStore = new UserStore<ApplicationUser>(new ApplicationDBContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            var user = new ApplicationUser() { UserName = model.UserName, Email = model.Email };

            //including new fields in the database
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Time = DateTime.Now.ToString();

            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 3
            };
            //creates a record in table user
            IdentityResult result = manager.Create(user, model.Password);

            ApplicationDBContext db = new ApplicationDBContext();
            //adding default image to the new user
            Image img = new Image { AId = user.Id, ImageUrl = "download.jpg" };
            db.images.Add(img);
            db.SaveChanges();

            return result;
        }


        [Authorize]
        [HttpGet]
        [Route("api/GetUserClaims")]
        /**
         * GetUserClaims returns claims
         * @returns A model with the required information
         **/
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
                Id=identityClaims.FindFirst("Id").Value,
            };
            return model;
        }



        [HttpPost]
        [AllowAnonymous]
        [Route("api/UploadImage")]
        /**
         *UploadImage method is used to store image in the local directory 
         * and the image name in the Image Table
         * @returns Status Ok
         **/
        public HttpResponseMessage UploadImage()
        {
            string imageName = null;
            var httpRequest = HttpContext.Current.Request;

            //to save file in the local storage
            var postedFile = httpRequest.Files["Image"];
            imageName = new string(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
            imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
            var filePath = HttpContext.Current.Server.MapPath("~/Image/" + imageName);
            postedFile.SaveAs(filePath);

            //Find the user to update his image
            var UserName = httpRequest.Form["Username"];
            var userStore = new UserStore<ApplicationUser>(new ApplicationDBContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            var user = manager.FindByName(UserName);
            System.Diagnostics.Debug.WriteLine(user.FirstName);

            //update the image of the user
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
        /**
         * This method check whether the username already exists
         * @param takes a username from the json object
         **/
        public HttpResponseMessage UName()
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
                return Request.CreateResponse("username already exists") ;
            else
                return Request.CreateResponse("OK");
        }


        [HttpGet]
        [Route("api/GetImg")]
        [AllowAnonymous]
        /**
         *This method is defined to find the imageurl of the respective user
         * @param Username to find the record in the database  
         * @returns imageUrl of the user
         **/
        public HttpResponseMessage GetImg(string Username)
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
                return Request.CreateResponse(user1.ImageUrl);
            }
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);


        }

        [HttpPut]
        [Route("api/Edit/{Username}")]
        [AllowAnonymous]
        /**
         * This method is used to edit the existing user
         * @param username whose record has to br edited and a model containing new Data
         **/
        public HttpResponseMessage Edit_Post(string Username,AccountModel model)
        {
            var userStore = new UserStore<ApplicationUser>(new ApplicationDBContext());
            var manager = new UserManager<ApplicationUser>(userStore);
            System.Diagnostics.Debug.WriteLine(Username);
            var user = manager.FindByName(Username);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            System.Diagnostics.Debug.WriteLine(Username);
            System.Diagnostics.Debug.WriteLine(user.FirstName);
            if (user != null)
            {  
                manager.Update(user);
                return Request.CreateResponse(HttpStatusCode.OK);
            } 
            else
                return null;
                
        }
    }
}