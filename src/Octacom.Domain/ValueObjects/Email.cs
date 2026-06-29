using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Octacom.Domain.ValueObjects
{
    public class Email
    {
        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.", nameof(value));

            if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Email is not valid.", nameof(value));

            Value = value.ToLowerInvariant();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Email other) return false;
            return Value == other.Value;
        }

        public override int GetHashCode() => Value.GetHashCode();
    }
}
