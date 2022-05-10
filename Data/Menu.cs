using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sunburst.Data
{
    public class Menu
    {
        [Key]
        public int ID { get; set; }
        [StringLength(25)]
        public string Name { get; set; }
        public decimal Price { get; set; }
        [StringLength(250)]
        public string Description { get; set; }
        public Boolean Active { get; set; }
        public string ImageDescription { get; set; }
        public byte[] ImageData { get; set; }

    }
}

