﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookStore_UI.Models
{
    public class Author
    {
        public int id { get; set; }
        [Required]
        [DisplayName("First Name")]
        public string Firstname { get; set; }
        [Required]
        [DisplayName("Lasst Name")]
        public string Lastname { get; set; }
        [Required]
        [DisplayName("Biography")]
        [StringLength(250)]
        public string Bio { get; set; }

        public virtual IList<Book> Books { get; set; }

    }
}
