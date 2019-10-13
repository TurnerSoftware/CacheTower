using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CacheTower.Tests
{
	[ProtoContract]
	public class ComplexTypeCaching_TypeOne : IEquatable<ComplexTypeCaching_TypeOne>
	{
		[ProtoMember(1)]
		public int ExampleNumber { get; set; }
		[ProtoMember(2)]
		public string ExampleString { get; set; }
		[ProtoMember(3)]
		public List<int> ListOfNumbers { get; set; }

		public bool Equals(ComplexTypeCaching_TypeOne other)
		{
			if (other == null)
			{
				return false;
			}

			return ExampleNumber == other.ExampleNumber &&
				ExampleString == other.ExampleString &&
				ListOfNumbers.SequenceEqual(other.ListOfNumbers);
		}

		public override bool Equals(object obj)
		{
			if (obj is ComplexTypeCaching_TypeOne complexType)
			{
				return Equals(complexType);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return ExampleNumber.GetHashCode() ^
				ExampleString.GetHashCode() ^
				ListOfNumbers.GetHashCode();
		}
	}

	[ProtoContract]
	public class ComplexTypeCaching_TypeTwo : IEquatable<ComplexTypeCaching_TypeTwo>
	{
		[ProtoMember(1)]
		public string ExampleString { get; set; }
		[ProtoMember(2)]
		public ComplexTypeCaching_TypeOne[] ArrayOfObjects { get; set; }
		[ProtoMember(3)]
		public Dictionary<string, int> DictionaryOfNumbers { get; set; }

		public bool Equals(ComplexTypeCaching_TypeTwo other)
		{
			if (other == null)
			{
				return false;
			}

			return ExampleString == other.ExampleString &&
				ArrayOfObjects.SequenceEqual(other.ArrayOfObjects) &&
				DictionaryOfNumbers.SequenceEqual(other.DictionaryOfNumbers);
		}

		public override bool Equals(object obj)
		{
			if (obj is ComplexTypeCaching_TypeTwo complexType)
			{
				return Equals(complexType);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return ExampleString.GetHashCode() ^
				ArrayOfObjects.GetHashCode() ^
				DictionaryOfNumbers.GetHashCode();
		}
	}
}
