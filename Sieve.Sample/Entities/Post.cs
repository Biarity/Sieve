using System;
using System.ComponentModel.DataAnnotations.Schema;
using Sieve.Attributes;

namespace Sieve.Sample.Entities
{
    public class Post
    {
        public int Id { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string Title { get; set; } = Guid.NewGuid().ToString().Replace("-", string.Empty)[..8];

        [Sieve(CanFilter = true, CanSort = true)]
        public int LikeCount { get; set; } = new Random().Next(0, 1000);


        [Sieve(CanFilter = true, CanSort = true)]
        public int CommentCount { get; set; } = new Random().Next(0, 1000);

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;

        [Sieve(CanFilter = true, CanSort = true)]
        [Column(TypeName = "datetime")]
        public DateTime DateLastViewed { get; set; } = DateTime.UtcNow;

        [Sieve(CanFilter = true, CanSort = true)]
        public int? CategoryId { get; set; } = new Random().Next(0, 4);
    }
}
