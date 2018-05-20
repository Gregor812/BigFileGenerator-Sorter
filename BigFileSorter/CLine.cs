using System;

namespace BigFileSorter
{
    public class CLine : IComparable<CLine>, IComparable
    {
        public UInt32 Number { get; }
        public String String { get; }

        private CLine(UInt32 number, String @string)
        {
            Number = number;
            String = @string;
        }

        public static CLine Parse(String @string)
        {
            if (String.IsNullOrWhiteSpace(@string))
                throw new ArgumentNullException(nameof(@string));

            var delimiterIndex = @string.IndexOf('.');

            if (delimiterIndex == -1 ||
                !UInt32.TryParse(@string.Substring(0, delimiterIndex), out UInt32 number))
                throw new FormatException(@string);

            return new CLine(number, @string.Substring(delimiterIndex + 2));
        }

        public override Int32 GetHashCode()
        {
            return String.GetHashCode();
        }

        public override Boolean Equals(Object obj)
        {
            if (obj is CLine line)
                return String.Equals(line.String) && Number.Equals(line.Number);

            return false;
        }

        public override String ToString()
        {
            return $"{Number}. {String}";
        }

        public Int32 CompareTo(CLine other)
        {
            var stringComparison = String.Compare(String, other.String, StringComparison.InvariantCultureIgnoreCase);
            if (stringComparison != 0)
                return stringComparison;

            return Number.CompareTo(other.Number);
        }

        public Int32 CompareTo(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return 1;

            if (!(obj is CLine line))
                throw new ArgumentException($"Wrong object type: {obj.GetType()}. It must be {nameof(CLine)}");

            return CompareTo(line);
        }
    }
}