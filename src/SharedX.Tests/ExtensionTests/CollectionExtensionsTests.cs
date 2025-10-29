using FluentAssertions;
using SharedX.Extensions.CollectionExt;
using Xunit;

namespace SharedX.Tests.ExtensionTests;

public class CollectionExtensionsTests
{
    #region AsList Tests

    [Fact]
    public void AsList_AlreadyList_ReturnsSameInstance()
    {
        // Arrange
        var originalList = new List<int> { 1, 2, 3 };

        // Act
        var result = originalList.AsList();

        // Assert
        result.Should().BeSameAs(originalList); // No new allocation
    }

    [Fact]
    public void AsList_Array_CreatesNewList()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };

        // Act
        var result = array.AsList();

        // Assert
        result.Should().BeOfType<List<int>>();
        result.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void AsList_NullInput_ThrowsException()
    {
        // Arrange
        IEnumerable<int>? nullEnum = null;

        // Act & Assert
        var act = () => nullEnum!.AsList();
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region IsNullOrEmpty Tests

    [Fact]
    public void IsNullOrEmpty_Null_ReturnsTrue()
    {
        // Arrange
        IEnumerable<int>? collection = null;

        // Act
        var result = collection.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_EmptyList_ReturnsTrue()
    {
        // Arrange
        var collection = new List<int>();

        // Act
        var result = collection.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_WithItems_ReturnsFalse()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3 };

        // Act
        var result = collection.IsNullOrEmpty();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasItems_WithItems_ReturnsTrue()
    {
        // Arrange
        var collection = new List<int> { 1 };

        // Act
        var result = collection.HasItems();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasItems_Empty_ReturnsFalse()
    {
        // Arrange
        var collection = new List<int>();

        // Act
        var result = collection.HasItems();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ForEach Tests

    [Fact]
    public void ForEach_AppliesActionToAllElements()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };
        var sum = 0;

        // Act
        list.ForEach(x => sum += x);

        // Assert
        sum.Should().Be(6);
    }

    [Fact]
    public void ForEach_WithIndex_ProvidesCorrectIndices()
    {
        // Arrange
        var list = new List<string> { "a", "b", "c" };
        var result = new List<string>();

        // Act
        list.ForEach((item, index) => result.Add($"{index}:{item}"));

        // Assert
        result.Should().Equal("0:a", "1:b", "2:c");
    }

    [Fact]
    public void ForEach_NullAction_ThrowsException()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act & Assert
        var act = () => list.ForEach(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion


    #region DistinctBy Tests (netstandard2.0)

#if !NET6_0_OR_GREATER
        [Fact]
        public void DistinctBy_RemovesDuplicatesByKey()
        {
            // Arrange
            var people = new[]
            {
                new { Name = "John", Age = 30 },
                new { Name = "Jane", Age = 25 },
                new { Name = "John", Age = 35 } // Duplicate name
            };

            // Act
            var result = people.DistinctBy(p => p.Name).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Select(p => p.Name).Should().Equal("John", "Jane");
        }

        [Fact]
        public void DistinctBy_NullKeySelector_ThrowsException()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };

            // Act & Assert
            var act = () => list.DistinctBy<int, int>(null!).ToList();
            act.Should().Throw<ArgumentNullException>();
        }
#endif

    #endregion

    #region Shuffle Tests

    [Fact]
    public void Shuffle_ContainsAllOriginalElements()
    {
        // Arrange
        var original = Enumerable.Range(1, 100).ToList();

        // Act
        var shuffled = original.Shuffle();

        // Assert
        shuffled.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Shuffle_ChangesOrder()
    {
        // Arrange
        var original = Enumerable.Range(1, 100).ToList();

        // Act
        var shuffled = original.Shuffle();

        // Assert - Very unlikely to be in same order
        shuffled.Should().NotEqual(original);
    }

    [Fact]
    public void Shuffle_WithSeed_ProducesSameResult()
    {
        // Arrange
        var original = Enumerable.Range(1, 50).ToList();
        var random1 = new Random(42);
        var random2 = new Random(42);

        // Act
        var shuffled1 = original.Shuffle(random1);
        var shuffled2 = original.Shuffle(random2);

        // Assert
        shuffled1.Should().Equal(shuffled2);
    }

    #endregion

    #region OrEmpty Tests

    [Fact]
    public void OrEmpty_NullInput_ReturnsEmpty()
    {
        // Arrange
        IEnumerable<int>? collection = null;

        // Act
        var result = collection.OrEmpty();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void OrEmpty_WithItems_ReturnsSameCollection()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3 };

        // Act
        var result = collection.OrEmpty();

        // Assert
        result.Should().BeSameAs(collection);
    }

    #endregion
}