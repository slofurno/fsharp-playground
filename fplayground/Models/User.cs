using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MvcSample.Web.Models
{
    public class User
    {
        [Required]
        [MinLength(4)]
        public string Name { get; set; }
        public string Address { get; set; }
        public int Age { get; set; }
    }
    
    public class PlaygroundViewModel
    {
        public string Content { get; set; }
        public IEnumerable<string> RecentHashes { get; set; }
        
        public PlaygroundViewModel(){
            
        }
        
    }
}