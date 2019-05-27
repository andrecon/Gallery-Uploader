using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageUploader.Data;
using ImageUploader.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
//Uploads files to server
namespace ImageUploader.Controllers
{
    //api/gallery
    [Route("api/[controller]")]
    public class GalleryController : Controller
    {
        //Initialize application DB context (Dependency injection Feature)
        private readonly ApplicationDbContext _db;

        //Since we need to upload images to the server, we need the server path in order to upload it to correct folders
        private readonly IHostingEnvironment _env;

        public GalleryController(ApplicationDbContext db, IHostingEnvironment env)
        {
            _db = db;
            _env = env;
        }

        //testing it: https://localhost:5001/api/gallery/   we dont need to specify ID
        //HTTp (GET) method to give us a list of ll the gallery IDs that we have in our databse
        [HttpGet]
        public IActionResult GetImageGallery()
        {
            var result = _db.Galleries.ToList();
            return Ok(result.Select(t => t.GelleryID));
        }

        //testing it: https://localhost:5001/api/gallery/3   3 being the id that were looking for
        //HTTP (GET) method to get all the images of a specific gallery by its ID and display it in uur browser in JSON format
        [HttpGet("{id}")]
        public IActionResult GetImageGallery([FromRoute] int id)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //using link to query our data, iterte through Gallery and images to find the image with the same ID
            var result = from gallery in _db.Galleries
                         join images in _db.GalleryImages.Where(t => t.GalleryId == id)
                         on gallery.GelleryID equals images.GalleryId
                         select new
                         {
                             Gallery_Id = gallery.GelleryID,
                             Gallery_Title = gallery.Title,
                             Gallery_Path = gallery.GalleryUrl,
                             Image_Id = images.ImageID,
                             Image_Path = images.ImageUrl,
                             Image_Caption = images.Caption
                         };
            if(result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }



        //(POST) Takes in Gallery Object (title,url,id)
        [HttpPost]
        public async Task<IActionResult> PostFormData(Gallery gallery, IFormCollection formdata)
        {
            int i = 0;
            string GalleryTitle = formdata["GalleryTitle"];

            //An asynchronous method so we use await keyword
            int id = await CreateGalleryID(gallery);

            //Provide the path for folder wwwroot/Uploads/Gallery
            string GalleryPath = Path.Combine(_env.WebRootPath + $"{Path.DirectorySeparatorChar}Uploads{Path.DirectorySeparatorChar}Gallery{Path.DirectorySeparatorChar}", id.ToString());

            string dbImageGalleryPath = Path.Combine($"{Path.DirectorySeparatorChar}Uploads{Path.DirectorySeparatorChar}Gallery{Path.DirectorySeparatorChar}", id.ToString());

            CreateDirectory(GalleryPath);

            foreach (var file in formdata.Files)
            {
                if(file.Length > 0)
                {
                    //Jpg,PNG, ect.
                    var extension = Path.GetExtension(file.FileName);

                    //Current date and time for filename
                    var filename = DateTime.Now.ToString("yymmssfff");

                    //Where we are going to copy or store our image
                    var path = Path.Combine(GalleryPath, filename) + extension;

                    var dbImagePath = Path.Combine(dbImageGalleryPath + $"{Path.DirectorySeparatorChar}", filename) + extension;

                    //Will get the caption from the form they submit
                    string ImageCaption = formdata["ImageCaption[]"][i];

                    //We create an Image object
                    GalleryImage Image = new GalleryImage();
                    Image.GalleryId = id;
                    Image.ImageUrl = dbImagePath;
                    Image.Caption = ImageCaption;

                    //Add this image object to our database
                    await _db.GalleryImages.AddAsync(Image);

                    //Copy the image itself to the folder on our server
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    i = i + 1;
                }
            }

            //Add gallery properties to our class then to database
            gallery.Title = GalleryTitle;
            gallery.GalleryUrl = dbImageGalleryPath;

            //Since we already created our gallery in the DB (CreateGalleryID), so we just update it
            _db.Galleries.Update(gallery);
            await _db.SaveChangesAsync();


            //Inform client if it was successfull
            return new JsonResult("Successfully Added: " + GalleryTitle);
        }

        //Method to create a new folder each time
        private void CreateDirectory(string gallerypath)
        {
            if(!Directory.Exists(gallerypath))
            {
                Directory.CreateDirectory(gallerypath);
            }
        }

        //Create a Gallery and return gallery ID
        private async Task<int> CreateGalleryID(Gallery gallery)
        {
            _db.Galleries.Add(gallery);
            await _db.SaveChangesAsync();
            await _db.Entry(gallery).GetDatabaseValuesAsync();
            int id = gallery.GelleryID;
            return id;
        }


     
    }
}
