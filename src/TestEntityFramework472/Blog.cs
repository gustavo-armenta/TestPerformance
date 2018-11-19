using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestEntityFramework472
{
    class Blog
    {
        [Key, Column(Order = 0)]
        [MaxLength(255)]
        public string Key { get; set; }

        [Column(Order = 1)]
        public string Data { get; set; }
    }
}
