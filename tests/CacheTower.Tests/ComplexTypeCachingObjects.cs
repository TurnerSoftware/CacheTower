using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace CacheTower.Tests;

internal static class SequenceComparison
{
	public static bool Compare<T>(IEnumerable<T> x, IEnumerable<T> y)
	{
		if (x is not null && y is not null)
		{
			return x.SequenceEqual(y);
		}
		return x is null && y is null;
	}
}

[ProtoContract]
public record BasicTypeCaching_TypeOne
{
	[ProtoMember(1)]
	public string ExampleString { get; set; }
}

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
			SequenceComparison.Compare(ListOfNumbers, other.ListOfNumbers);
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
			SequenceComparison.Compare(ArrayOfObjects, other.ArrayOfObjects) &&
			SequenceComparison.Compare(DictionaryOfNumbers, other.DictionaryOfNumbers);
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
