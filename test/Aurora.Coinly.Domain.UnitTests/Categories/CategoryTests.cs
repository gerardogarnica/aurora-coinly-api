using Aurora.Coinly.Domain.UnitTests.Users;

namespace Aurora.Coinly.Domain.UnitTests.Categories;

public class CategoryTests : BaseTest
{
    [Fact]
    public void Create_Should_SetProperties()
    {
        // Arrange

        // Act
        var category = Category.Create(
            UserData.GetUser().Id,
            CategoryData.Name,
            CategoryData.Group,
            TransactionType.Expense,
            CategoryData.MaxDaysToReverse,
            CategoryData.Color,
            CategoryData.Notes,
            DateTime.UtcNow);

        // Assert
        category.Name.Should().Be(CategoryData.Name);
        category.Type.Should().Be(TransactionType.Expense);
        category.Group.Should().Be(CategoryData.Group);
        category.Notes.Should().Be(CategoryData.Notes);
        category.IsDeleted.Should().Be(false);
    }

    [Fact]
    public void Update_Should_SetProperties()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var updatedName = "Updated Name";
        var updatedMaxDaysToReverse = 15;
        var updatedColor = Color.FromHex("#FF0000");
        var updatedGroup = CategoryGroup.Entertainment;
        var updatedNotes = "Updated Notes";

        // Act
        var result = category.Update(
            updatedName,
            updatedMaxDaysToReverse,
            updatedColor,
            updatedGroup,
            updatedNotes,
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        category.Name.Should().Be(updatedName);
        category.Notes.Should().Be(updatedNotes);
        category.UpdatedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Update_Should_Fail_WhenIsDeleted()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        var updatedName = "Updated Name";
        var updatedMaxDaysToReverse = 15;
        var updatedColor = Color.FromHex("#FF0000");
        var updatedGroup = CategoryGroup.Entertainment;
        var updatedNotes = "Updated Notes";

        category.Delete(DateTime.UtcNow);

        // Act
        var result = category.Update(
            updatedName,
            updatedMaxDaysToReverse,
            updatedColor,
            updatedGroup,
            updatedNotes,
            DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(CategoryErrors.IsDeleted);
    }

    [Fact]
    public void Delete_Should_SetProperties()
    {
        // Arrange
        var category = CategoryData.GetCategory();

        // Act
        var result = category.Delete(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        category.IsDeleted.Should().BeTrue();
        category.DeletedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Delete_Should_Fail_WhenIsDeleted()
    {
        // Arrange
        var category = CategoryData.GetCategory();
        category.Delete(DateTime.UtcNow);

        // Act
        var result = category.Delete(DateTime.UtcNow);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(CategoryErrors.IsDeleted);
    }
}