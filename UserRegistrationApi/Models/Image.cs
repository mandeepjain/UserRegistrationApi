using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UserRegistrationApi.Models
{
    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageId { get; set; }
        public string ImageUrl { get; set; } = "download.jpg";

        [ForeignKey("ApplicationUser")]
        public string AId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}