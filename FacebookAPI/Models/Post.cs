using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FacebookAPI.Models
{
    public class Post
    {
        [Key]
        public int ID { get; set; }
        public string PostID { get; set; }
        public string Text { get; set; }
        public string Picture { get; set; }
        public DateTime Creation { get; set; }
        public DateTime Modification { get; set; }
        public bool Deletion { get; set; }
    }
}
