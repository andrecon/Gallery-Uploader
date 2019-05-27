using System;
using System.ComponentModel.DataAnnotations;

namespace ImageUploader.Models
{
    public class GalleryImage
    {
        //Propperties for our GalleryImage class
        [Key]
        public int ImageID { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public int GalleryId { get; set; }

        //One-to-many relationship
        public Gallery Gallery { get; set; }
    }
}
