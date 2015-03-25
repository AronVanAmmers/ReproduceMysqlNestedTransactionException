using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReproduceMySqlNestedTransactionException.Models
{
    public class Author
    {
        public int Id { get; set; }
        [Required]
        [Index(IsUnique=true)]
        [MaxLength(128)]
        public string Name { get; set; }
    }
}