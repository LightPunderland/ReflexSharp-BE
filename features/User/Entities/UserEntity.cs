using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Features.User.DTOs;

namespace Features.User.Entities
{
    public class User : IComparable<User>, IEquatable<User>
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, Column(TypeName = "varchar(255)")]
        public string GoogleId { get; set; } = null!;

        [Required, Column(TypeName = "varchar(255)")]
        public string Email { get; set; } = null!;

        [Required, Column(TypeName = "varchar(100)")]
        public string DisplayName { get; set; } = null!;

        [Required, Column(TypeName = "varchar(50)")]
        public string Rank { get; set; } = "None";

        //comapre by rank
        public int CompareTo(User? other)
        {
            if (other == null) return 1;
            return string.Compare(this.Rank, other.Rank, StringComparison.Ordinal);
        }

        //equal if name and rank are equal
        public bool Equals(User? other)
        {
            if (other == null) return false;

            return this.DisplayName == other.DisplayName && this.Rank == other.Rank;
        }

        public override bool Equals(object? obj)
        {
            if (obj is User otherUser)
            {
                return Equals(otherUser);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DisplayName, Rank);
        }
    }
}
