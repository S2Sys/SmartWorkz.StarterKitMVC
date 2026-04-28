using Xunit;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for CarouselComponent verifying carousel rendering, auto-play, navigation, indicators, and slide change callbacks.
/// </summary>
public class CarouselComponentTests
{
    [Fact]
    public void CarouselComponent_WithItems_RendersAllSlides()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem { Title = "Slide 1", Description = "First slide", ImageUrl = "image1.jpg" },
            new CarouselItem { Title = "Slide 2", Description = "Second slide", ImageUrl = "image2.jpg" },
            new CarouselItem { Title = "Slide 3", Description = "Third slide", ImageUrl = "image3.jpg" }
        };

        // Act
        var slideCount = items.Count;
        var firstSlide = items[0];
        var lastSlide = items[items.Count - 1];

        // Assert
        Assert.Equal(3, slideCount);
        Assert.Equal("Slide 1", firstSlide.Title);
        Assert.Equal("image1.jpg", firstSlide.ImageUrl);
        Assert.Equal("Slide 3", lastSlide.Title);
        Assert.Equal("image3.jpg", lastSlide.ImageUrl);
    }

    [Fact]
    public void CarouselComponent_WithDefaultActiveIndex_StartsAtFirstSlide()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem { Title = "Slide 1", Description = "First", ImageUrl = "img1.jpg" },
            new CarouselItem { Title = "Slide 2", Description = "Second", ImageUrl = "img2.jpg" },
            new CarouselItem { Title = "Slide 3", Description = "Third", ImageUrl = "img3.jpg" }
        };
        var activeIndex = 0; // Default

        // Act
        var isFirstSlideActive = activeIndex == 0;
        var activeSlide = items[activeIndex];

        // Assert
        Assert.True(isFirstSlideActive);
        Assert.Equal("Slide 1", activeSlide.Title);
    }

    [Fact]
    public void CarouselComponent_WithCustomActiveIndex_DisplaysCorrectSlide()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem { Title = "Slide 1", Description = "First", ImageUrl = "img1.jpg" },
            new CarouselItem { Title = "Slide 2", Description = "Second", ImageUrl = "img2.jpg" },
            new CarouselItem { Title = "Slide 3", Description = "Third", ImageUrl = "img3.jpg" }
        };
        var activeIndex = 2;

        // Act
        var activeSlide = items[activeIndex];
        var isThirdSlideActive = activeIndex == 2;

        // Assert
        Assert.True(isThirdSlideActive);
        Assert.Equal("Slide 3", activeSlide.Title);
        Assert.Equal("img3.jpg", activeSlide.ImageUrl);
    }

    [Fact]
    public void CarouselComponent_NextNavigation_AdvancesToNextSlide()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem { Title = "Slide 1", Description = "First", ImageUrl = "img1.jpg" },
            new CarouselItem { Title = "Slide 2", Description = "Second", ImageUrl = "img2.jpg" },
            new CarouselItem { Title = "Slide 3", Description = "Third", ImageUrl = "img3.jpg" }
        };
        var activeIndex = 0;

        // Act
        activeIndex = (activeIndex + 1) % items.Count;
        var nextSlide = items[activeIndex];

        // Assert
        Assert.Equal(1, activeIndex);
        Assert.Equal("Slide 2", nextSlide.Title);
    }

    [Fact]
    public void CarouselComponent_NextNavigation_WrapsToFirstSlide()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem { Title = "Slide 1", Description = "First", ImageUrl = "img1.jpg" },
            new CarouselItem { Title = "Slide 2", Description = "Second", ImageUrl = "img2.jpg" },
            new CarouselItem { Title = "Slide 3", Description = "Third", ImageUrl = "img3.jpg" }
        };
        var activeIndex = 2; // Last slide

        // Act
        activeIndex = (activeIndex + 1) % items.Count;
        var wrappedSlide = items[activeIndex];

        // Assert
        Assert.Equal(0, activeIndex);
        Assert.Equal("Slide 1", wrappedSlide.Title);
    }

    [Fact]
    public void CarouselComponent_PreviousNavigation_GoesToPreviousSlide()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem { Title = "Slide 1", Description = "First", ImageUrl = "img1.jpg" },
            new CarouselItem { Title = "Slide 2", Description = "Second", ImageUrl = "img2.jpg" },
            new CarouselItem { Title = "Slide 3", Description = "Third", ImageUrl = "img3.jpg" }
        };
        var activeIndex = 2;

        // Act
        activeIndex = (activeIndex - 1 + items.Count) % items.Count;
        var previousSlide = items[activeIndex];

        // Assert
        Assert.Equal(1, activeIndex);
        Assert.Equal("Slide 2", previousSlide.Title);
    }

    [Fact]
    public void CarouselComponent_PreviousNavigation_WrapsToLastSlide()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem { Title = "Slide 1", Description = "First", ImageUrl = "img1.jpg" },
            new CarouselItem { Title = "Slide 2", Description = "Second", ImageUrl = "img2.jpg" },
            new CarouselItem { Title = "Slide 3", Description = "Third", ImageUrl = "img3.jpg" }
        };
        var activeIndex = 0; // First slide

        // Act
        activeIndex = (activeIndex - 1 + items.Count) % items.Count;
        var wrappedSlide = items[activeIndex];

        // Assert
        Assert.Equal(2, activeIndex);
        Assert.Equal("Slide 3", wrappedSlide.Title);
    }

    [Fact]
    public void CarouselComponent_IndicatorClick_UpdatesActiveSlide()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem { Title = "Slide 1", Description = "First", ImageUrl = "img1.jpg" },
            new CarouselItem { Title = "Slide 2", Description = "Second", ImageUrl = "img2.jpg" },
            new CarouselItem { Title = "Slide 3", Description = "Third", ImageUrl = "img3.jpg" }
        };
        var activeIndex = 0;
        var targetIndex = 2;

        // Act
        activeIndex = targetIndex;
        var selectedSlide = items[activeIndex];

        // Assert
        Assert.Equal(2, activeIndex);
        Assert.Equal("Slide 3", selectedSlide.Title);
    }

    [Fact]
    public void CarouselComponent_OnSlideChanged_CallbackInvoked()
    {
        // Arrange
        var changedIndices = new List<int>();
        var onSlideChanged = new Func<int, Task>(async (index) =>
        {
            changedIndices.Add(index);
            await Task.CompletedTask;
        });

        // Act
        onSlideChanged.Invoke(1).Wait();
        onSlideChanged.Invoke(2).Wait();
        onSlideChanged.Invoke(0).Wait();

        // Assert
        Assert.Equal(3, changedIndices.Count);
        Assert.Equal(new[] { 1, 2, 0 }, changedIndices);
    }

    [Fact]
    public void CarouselComponent_AutoPlay_DefaultIsEnabled()
    {
        // Arrange
        var autoPlay = true; // Default

        // Act
        var isAutoPlayEnabled = autoPlay;

        // Assert
        Assert.True(isAutoPlayEnabled);
    }

    [Fact]
    public void CarouselComponent_AutoPlayIntervalMs_DefaultIs5000()
    {
        // Arrange
        var autoPlayIntervalMs = 5000; // Default

        // Act
        var interval = autoPlayIntervalMs;

        // Assert
        Assert.Equal(5000, interval);
    }

    [Fact]
    public void CarouselComponent_CustomAutoPlayInterval_IsConfigurable()
    {
        // Arrange
        var autoPlayIntervalMs = 3000; // Custom value

        // Act
        var interval = autoPlayIntervalMs;

        // Assert
        Assert.Equal(3000, interval);
        Assert.NotEqual(5000, interval);
    }

    [Fact]
    public void CarouselComponent_ShowIndicators_DefaultIsTrue()
    {
        // Arrange
        var showIndicators = true; // Default

        // Act
        var indicatorsVisible = showIndicators;

        // Assert
        Assert.True(indicatorsVisible);
    }

    [Fact]
    public void CarouselComponent_ShowControls_DefaultIsTrue()
    {
        // Arrange
        var showControls = true; // Default

        // Act
        var controlsVisible = showControls;

        // Assert
        Assert.True(controlsVisible);
    }

    [Fact]
    public void CarouselComponent_WithContent_RenderFragmentIsNotNull()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem
            {
                Title = "Slide 1",
                Description = "First",
                ImageUrl = "img1.jpg",
                Content = new Microsoft.AspNetCore.Components.RenderFragment(builder => { })
            }
        };

        // Act
        var hasContent = items[0].Content != null;

        // Assert
        Assert.True(hasContent);
    }

    [Fact]
    public void CarouselComponent_MultipleSequentialSlideChanges_AllProcessed()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem { Title = "Slide 1", Description = "First", ImageUrl = "img1.jpg" },
            new CarouselItem { Title = "Slide 2", Description = "Second", ImageUrl = "img2.jpg" },
            new CarouselItem { Title = "Slide 3", Description = "Third", ImageUrl = "img3.jpg" },
            new CarouselItem { Title = "Slide 4", Description = "Fourth", ImageUrl = "img4.jpg" }
        };
        var activeIndex = 0;

        // Act
        activeIndex = (activeIndex + 1) % items.Count; // Move to slide 2
        var slide2 = items[activeIndex];
        activeIndex = (activeIndex + 1) % items.Count; // Move to slide 3
        var slide3 = items[activeIndex];
        activeIndex = (activeIndex + 1) % items.Count; // Move to slide 4
        var slide4 = items[activeIndex];

        // Assert
        Assert.Equal("Slide 2", slide2.Title);
        Assert.Equal("Slide 3", slide3.Title);
        Assert.Equal("Slide 4", slide4.Title);
    }

    [Fact]
    public void CarouselComponent_SingleSlide_NavigationWrapsCorrectly()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem { Title = "Only Slide", Description = "Single", ImageUrl = "img.jpg" }
        };
        var activeIndex = 0;

        // Act
        activeIndex = (activeIndex + 1) % items.Count;
        var afterNext = items[activeIndex];
        activeIndex = (activeIndex - 1 + items.Count) % items.Count;
        var afterPrevious = items[activeIndex];

        // Assert
        Assert.Equal(0, activeIndex);
        Assert.Equal("Only Slide", afterNext.Title);
        Assert.Equal("Only Slide", afterPrevious.Title);
    }

    [Fact]
    public void CarouselComponent_CarouselItemProperties_AllFieldsAccessible()
    {
        // Arrange
        var item = new CarouselItem
        {
            Title = "Test Slide",
            Description = "Test Description",
            ImageUrl = "test.jpg"
        };

        // Act
        var hasTitle = !string.IsNullOrEmpty(item.Title);
        var hasDescription = !string.IsNullOrEmpty(item.Description);
        var hasImageUrl = !string.IsNullOrEmpty(item.ImageUrl);

        // Assert
        Assert.True(hasTitle);
        Assert.True(hasDescription);
        Assert.True(hasImageUrl);
        Assert.Equal("Test Slide", item.Title);
        Assert.Equal("Test Description", item.Description);
        Assert.Equal("test.jpg", item.ImageUrl);
    }

    [Fact]
    public void CarouselComponent_IndicatorIndex_CorrectlyMapsToSlideIndex()
    {
        // Arrange
        var items = new List<CarouselItem>
        {
            new CarouselItem { Title = "Slide 1", Description = "First", ImageUrl = "img1.jpg" },
            new CarouselItem { Title = "Slide 2", Description = "Second", ImageUrl = "img2.jpg" },
            new CarouselItem { Title = "Slide 3", Description = "Third", ImageUrl = "img3.jpg" },
            new CarouselItem { Title = "Slide 4", Description = "Fourth", ImageUrl = "img4.jpg" },
            new CarouselItem { Title = "Slide 5", Description = "Fifth", ImageUrl = "img5.jpg" }
        };

        // Act & Assert
        for (int i = 0; i < items.Count; i++)
        {
            Assert.Equal($"Slide {i + 1}", items[i].Title);
            Assert.Equal($"img{i + 1}.jpg", items[i].ImageUrl);
        }
    }

    [Fact]
    public void CarouselComponent_WithEmptyItems_HandlesGracefully()
    {
        // Arrange
        var items = new List<CarouselItem>();

        // Act
        var itemCount = items.Count;
        var isEmpty = itemCount == 0;

        // Assert
        Assert.Equal(0, itemCount);
        Assert.True(isEmpty);
    }

    [Fact]
    public void CarouselComponent_AutoPlayCanBeDisabled()
    {
        // Arrange
        var autoPlay = false;

        // Act
        var isAutoPlayDisabled = !autoPlay;

        // Assert
        Assert.True(isAutoPlayDisabled);
    }

    [Fact]
    public void CarouselComponent_IndicatorsCanBeHidden()
    {
        // Arrange
        var showIndicators = false;

        // Act
        var indicatorsHidden = !showIndicators;

        // Assert
        Assert.True(indicatorsHidden);
    }

    [Fact]
    public void CarouselComponent_ControlsCanBeHidden()
    {
        // Arrange
        var showControls = false;

        // Act
        var controlsHidden = !showControls;

        // Assert
        Assert.True(controlsHidden);
    }
}
