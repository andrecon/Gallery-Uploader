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
using Microsoft.EntityFrameworkCore;

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
            return Ok(result.Select(t => new { t.GelleryID, t.Title }));
        }

        //testing it: https://localhost:5001/api/gallery/3   3 being the id that were looking for
        //HTTP (GET) method to get all the images of a specific gallery by its ID and display it in our browser in JSON format
        [HttpGet("{id}")]
        public IActionResult GetImageGallery([FromRoute] int id)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //using link to query our data, iterate through Gallery and images to find the image with the same ID: Create Anonymous Type
            var result = from gallery in _db.Galleries
                         join images in _db.GalleryImages.Where(t => t.GalleryId == id)
                         on gallery.GelleryID equals images.GalleryId
                         select new
                         {
                             gallery_Id = gallery.GelleryID,
                             gallery_Title = gallery.Title,
                             gallery_Path = gallery.GalleryUrl,
                             image_Id = images.ImageID,
                             image_Path = images.ImageUrl,
                             image_Caption = images.Caption
                         };
            if(result == null)
            {
                return NotFound();
            }

            //Return our resulted json data
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

            //Provide the path for folder MY_COMPUTER_PATH/.../wwwroot/Uploads/Gallery
            string GalleryPath = Path.Combine(_env.WebRootPath + $"{Path.DirectorySeparatorChar}Uploads{Path.DirectorySeparatorChar}Gallery{Path.DirectorySeparatorChar}", id.ToString());

            //Provide the path for folder wwwroot/Uploads/Gallery
            string dbImageGalleryPath = Path.Combine($"{Path.DirectorySeparatorChar}Uploads{Path.DirectorySeparatorChar}Gallery{Path.DirectorySeparatorChar}", id.ToString());

            //Creates directory from created path
            CreateDirectory(GalleryPath);

            //Iterate through our files
            foreach (var file in formdata.Files)
            {
                //If files were provided
                if(file.Length > 0)
                {
                    //Jpg,PNG, ect.
                    var extension = Path.GetExtension(file.FileName);

                    //Current date and time for filename: YEAR:HOUR:SECONDS:MILLISECONDS
                    var filename = DateTime.Now.ToString("yymmssfff");

                    //Where we are going to copy or store our image: MY_COMPUTER_PATH/.../wwwroot/Uploads/Gallery.extension
                    var path = Path.Combine(GalleryPath, filename) + extension;

                    //wwwroot/Uploads/Gallery.extension
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

                    //Copy the image itself to the folder on our server (Folder was already created)
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
            //If a directory with the same ID is not in the path, we create one
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

            //Gets the new ID from databse
            await _db.Entry(gallery).GetDatabaseValuesAsync();

            //Passes down the ID to variable id
            int id = gallery.GelleryID;
            return id;
        }

        //Method to delete gallery from database
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGallery([FromRoute] int id)
        {
            //Is our Model state valid? Has any model errors been added to ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Will find the gallery by its id provided
            var findGallery = await _db.Galleries.FindAsync(id);

            //If the result was null (Could not find ID)
            if(findGallery == null)
            {
                return NotFound();
            }

            //If gallery with the id was found, remove it from database
            _db.Galleries.Remove(findGallery);

            //Now we need to delete the gallery folder from the server
            DeleteGalleryDirectory(id);

            await _db.SaveChangesAsync();

            //Return Success result to the cline/browser
            return new JsonResult("Gallery Deleted : " + id);
        }

        //Method to delete gallery folder from server
        private void DeleteGalleryDirectory(int id)
        {
            //Getting the path from the Gallery folder: MY_COMPUTER_PATH/.../wwwroot/Uploads/GalleryID
            string GalleryPath = Path.Combine(_env.WebRootPath + $"{Path.DirectorySeparatorChar}Uploads{Path.DirectorySeparatorChar}Gallery{Path.DirectorySeparatorChar}", id.ToString());

            //Storing ll the files with the gallery folder in this array
            string[] files = Directory.GetFiles(GalleryPath);

            //Check if the gallery folder with that id exist
            if(Directory.Exists(GalleryPath))
            {
                //If it exist, delete the files inside the gallery first
                foreach (var file in files)
                {
                    //Change file permission to normal so we can delete files
                    System.IO.File.SetAttributes(file, FileAttributes.Normal);
                    System.IO.File.Delete(file);
                }

                //Now we can delete the gallery path.
                Directory.Delete(GalleryPath);
            }
        }


        //Method to update Gallery
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGallery([FromRoute] int id, IFormCollection formData)
        {

            //Is our Model state valid?
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Counters for image files 
            int i = 0;

            // Counters for image captions
            int j = 0;

            //Variable that will hold the value of the gallery title
            string Title = formData["GalleryTitleEdit"];




            //Use this to get the details of the Gallery that needs to be updated
            var oGallery = await _db.Galleries.FirstOrDefaultAsync(m => m.GelleryID == id);

            //Get path of our galllery
            string GalleryPath = Path.Combine(_env.WebRootPath + oGallery.GalleryUrl);

            if (formData.Files.Count > 0)
            {
                //Create an empty array to store old files
                string[] filesToDeletePath = new string[formData.Files.Count];

                foreach(var file in formData.Files)
                {
                    if(file.Length > 0)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        var filename = DateTime.Now.ToString("yymmssfff");
                        var path = Path.Combine(GalleryPath, filename) + extension;
                        var dbImagePath = Path.Combine(oGallery.GalleryUrl + $"{Path.DirectorySeparatorChar}", filename) + extension;
                        string ImageId = formData["imageId[]"][i];

                        //Variable that will get us the info of the image that needs to be updated
                        var updateImage = _db.GalleryImages.FirstOrDefault(o => o.ImageID == Convert.ToInt32(ImageId));

                        filesToDeletePath[i] = Path.Combine(_env.WebRootPath + updateImage.ImageUrl);
                        updateImage.ImageUrl = dbImagePath;


                        //Copying new files to the server - Gallery folder
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        //Saving changes to the databse, then destroy variable, dependency injection
                        using (var dbContextTransaction = _db.Database.BeginTransaction())
                        {
                            try

                            { 
                                _db.Entry(updateImage).State = EntityState.Modified;

                                await _db.SaveChangesAsync();

                                dbContextTransaction.Commit();
                            }
                            catch (Exception)
                            {
                                dbContextTransaction.Rollback();
                            }
                        }
                        i++;
                    }

                }
                //Delete the old files
                foreach(var item in filesToDeletePath)
                {
                    //If gallery exist - delete the files inside the gallery first
                    System.IO.File.SetAttributes(item, FileAttributes.Normal);
                    System.IO.File.Delete(item);
                }

            }

            //Validate if we have any image captions
            if(formData["imageCaption[]"].Count > 0)
            {
                oGallery.Title = Title;
                _db.Entry(oGallery).State = EntityState.Modified;

                foreach (var imgcap in formData["imageCaption[]"])
                {
                    string ImageIdCap = formData["imageId[]"][j];
                    string Caption = formData["imageCaption[]"][j];

                    var updateCaption = _db.GalleryImages.FirstOrDefault(o => o.ImageID == Convert.ToInt32(ImageIdCap));
                    updateCaption.Caption = Caption;

                    using (var dbContextTransaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            _db.Entry(updateCaption).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            dbContextTransaction.Commit();
                        }
                        catch (Exception)
                        {
                            dbContextTransaction.Rollback();
                        }
                    }
                    j++;
                }
            }

            return new JsonResult("This worked ");
        }

    }
}
