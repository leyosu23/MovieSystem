using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieSystem.Models
{
    [DynamoDBTable("Comment")]
    public class Comment
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        public string Description { get; set; }
        public string MovieId { get; set; }
    }
}
