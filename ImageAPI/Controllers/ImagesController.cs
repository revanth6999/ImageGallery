using ImageAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ImageAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ImagesController : ApiController
    {
        private ProjectEntities db = new ProjectEntities();

        [HttpGet]
        [Route("api/images")]
        public List<Image> GetImages()
        {
            return db.Images.ToList();
        }

        [HttpGet]
        [Route("api/search/{query}")]
        public List<Image> GetImage(string query)
        {
            return db.Images.Where((i) => i.Filename.Contains(query)).ToList();
        }

        [HttpPost]
        [Route("api/fileupload")]
        public HttpResponseMessage FileUpload()
        {
            ResponseMsg response = new ResponseMsg();
            response.IsError = true;
            try
            {
                string filepath = "";
                filepath = System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads/");
                System.Web.HttpFileCollection files = System.Web.HttpContext.Current.Request.Files;

                System.Web.HttpPostedFile file = files[0];
                string type = file.ContentType.Split('/')[0];

                if (file.ContentLength <= 0)
                {
                    response.ErrorMessage = "Please attach a file to upload";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                if (!type.Equals("image"))
                {
                    response.ErrorMessage = "File type is not a recognised image format";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                if (file.ContentLength > 1048576)
                {
                    response.ErrorMessage = "File size is too large";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                if (File.Exists(filepath + Path.GetFileName(file.FileName)))
                {
                    response.ErrorMessage = "File with the same name already exists!";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }

                file.SaveAs(filepath + Path.GetFileName(file.FileName));

                response.IsError = false;
                response.Extension = file.ContentType;
                response.Datetime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                response.Filesize = file.ContentLength.ToString();
                response.Filepath = "https://localhost:44363/Uploads/" + file.FileName;
                response.Filename = file.FileName.Split('.')[0];
                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception e)
            {
                response.ErrorMessage = "Upload failed! " + e;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [HttpPost]
        [Route("api/upload")]
        public HttpResponseMessage DUpload()
        {
            try
            {
                Image i = new Image();
                i.Filesize = System.Web.HttpContext.Current.Request["Filesize"];
                i.Filepath = System.Web.HttpContext.Current.Request["Filepath"];
                i.Extension = System.Web.HttpContext.Current.Request["Extension"];
                i.Datetime = System.Web.HttpContext.Current.Request["Datetime"];
                i.Filename = System.Web.HttpContext.Current.Request["Filename"];
                i.Username = System.Web.HttpContext.Current.Request["Username"];

                using (ProjectEntities entities = new ProjectEntities())
                {
                    entities.Images.Add(i);
                    entities.SaveChanges();
                }
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created);
                return Request.CreateResponse(HttpStatusCode.Created, "Success");
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e);
            }
        }

        // DELETE: api/Images/5
        [HttpDelete]
        [Route("api/delete/{id}")]
        public HttpResponseMessage DeleteImage(int id)
        {
            try
            {
                using (ProjectEntities entities = new ProjectEntities())
                {
                    string[] filepath = entities.Images.SingleOrDefault(i => i.Id == id).Filepath.Split('\\');
                    String filename = filepath[filepath.Length - 1];
                    File.Delete(System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads/" + filename));
                    Image image = entities.Images.Find(id);
                    if (image == null)
                    {
                        return Request.CreateErrorResponse(System.Net.HttpStatusCode.NotFound, "The image was not found");
                    }

                    entities.Images.Remove(image);
                    entities.SaveChanges();

                    return Request.CreateResponse(System.Net.HttpStatusCode.OK);
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, e.Message);
            }

        }
    }
}