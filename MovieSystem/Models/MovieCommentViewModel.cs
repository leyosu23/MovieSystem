using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieSystem.Models
{
    public class MovieCommentViewModel
    {
        public Movie movie { get; set; }
        public IEnumerable<Comment> comments { get; set; }
        public Comment comment { get; set; }
    }
}
