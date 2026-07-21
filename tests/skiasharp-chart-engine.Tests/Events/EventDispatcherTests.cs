using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Events;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Events;

public class EventDispatcherTests
{
    private readonly Mock<ILogger<EventDispatcher>> _loggerMock;
    private readonly EventDispatcher _dispatcher;

    public EventDispatcherTests()
    {
        _loggerMock = new Mock<ILogger<EventDispatcher>>();
        _dispatcher = new EventDispatcher(_loggerMock.Object);
    }

    [Fact]
    public void Subscribe_WithNullEventType_ThrowsArgumentException()
    {
        // Arrange
        var handler = new Mock<IEventHandler>().Object;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _dispatcher.Subscribe(null!, handler));
        Assert.Throws<ArgumentException>(() => _dispatcher.Subscribe("", handler));
        Assert.Throws<ArgumentException>(() => _dispatcher.Subscribe("   ", handler));
    }

    [Fact]
    public void Subscribe_WithNullHandler_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dispatcher.Subscribe("test.event", null!));
    }

    [Fact]
    public void Subscribe_WithValidParameters_AddsHandlerToList()
    {
        // Arrange
        var handler = new Mock<IEventHandler>().Object;
        var eventType = "chart.created";

        // Act
        _dispatcher.Subscribe(eventType, handler);

        // Assert
        var count = _dispatcher.GetHandlerCount(eventType);
        count.Should().Be(1);
    }

    [Fact]
    public void Subscribe_WithSameEventType_AddsMultipleHandlers()
    {
        // Arrange
        var handler1 = new Mock<IEventHandler>().Object;
        var handler2 = new Mock<IEventHandler>().Object;
        var eventType = "chart.updated";

        // Act
        _dispatcher.Subscribe(eventType, handler1);
        _dispatcher.Subscribe(eventType, handler2);

        // Assert
        var count = _dispatcher.GetHandlerCount(eventType);
        count.Should().Be(2);
    }

    [Fact]
    public void Unsubscribe_WithNullEventType_DoesNothing()
    {
        // Arrange
        var handler = new Mock<IEventHandler>().Object;
        _dispatcher.Subscribe("test.event", handler);

        // Act
        _dispatcher.Unsubscribe(null!, handler);
        _dispatcher.Unsubscribe("", handler);
        _dispatcher.Unsubscribe("   ", handler);

        // Assert
        var count = _dispatcher.GetHandlerCount("test.event");
        count.Should().Be(1); // Should not remove anything
    }

    [Fact]
    public void Unsubscribe_WithNullHandler_DoesNothing()
    {
        // Arrange
        var handler = new Mock<IEventHandler>().Object;
        _dispatcher.Subscribe("test.event", handler);

        // Act
        _dispatcher.Unsubscribe("test.event", null!);

        // Assert
        var count = _dispatcher.GetHandlerCount("test.event");
        count.Should().Be(1); // Should not remove anything
    }

    [Fact]
    public void Unsubscribe_WithNonExistentHandler_DoesNothing()
    {
        // Arrange
        var handler1 = new Mock<IEventHandler>().Object;
        var handler2 = new Mock<IEventHandler>().Object;
        var eventType = "chart.deleted";

        _dispatcher.Subscribe(eventType, handler1);

        // Act
        _dispatcher.Unsubscribe(eventType, handler2); // Different handler

        // Assert
        var count = _dispatcher.GetHandlerCount(eventType);
        count.Should().Be(1);
    }

    [Fact]
    public void Unsubscribe_WithExistingHandler_RemovesHandler()
    {
        // Arrange
        var handler = new Mock<IEventHandler>().Object;
        var eventType = "chart.rendered";

        _dispatcher.Subscribe(eventType, handler);
        var countBefore = _dispatcher.GetHandlerCount(eventType);
        countBefore.Should().Be(1);

        // Act
        _dispatcher.Unsubscribe(eventType, handler);

        // Assert
        var countAfter = _dispatcher.GetHandlerCount(eventType);
        countAfter.Should().Be(0);
    }

    [Fact]
    public void Dispatch_WithNoHandlers_DoesNotThrow()
    {
        // Arrange
        var eventType = "chart.nonexistent";
        var eventData = new { Test = "data" };

        // Act
        Action act = () => _dispatcher.Dispatch(eventType, eventData);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispatch_WithSingleHandler_CallsHandler()
    {
        // Arrange
        var handlerMock = new Mock<IEventHandler>();
        var eventType = "chart.created";
        var eventData = new { ChartId = "123", Title = "Test Chart" };

        _dispatcher.Subscribe(eventType, handlerMock.Object);

        // Act
        _dispatcher.Dispatch(eventType, eventData);

        // Assert
        handlerMock.Verify(h => h.Handle(eventType, eventData), Times.Once);
    }

    [Fact]
    public void Dispatch_WithMultipleHandlers_CallsAllHandlers()
    {
        // Arrange
        var handler1Mock = new Mock<IEventHandler>();
        var handler2Mock = new Mock<IEventHandler>();
        var handler3Mock = new Mock<IEventHandler>();
        var eventType = "chart.updated";
        var eventData = new { ChartId = "456", ModifiedFields = new[] { "title", "data" } };

        _dispatcher.Subscribe(eventType, handler1Mock.Object);
        _dispatcher.Subscribe(eventType, handler2Mock.Object);
        _dispatcher.Subscribe(eventType, handler3Mock.Object);

        // Act
        _dispatcher.Dispatch(eventType, eventData);

        // Assert
        handler1Mock.Verify(h => h.Handle(eventType, eventData), Times.Once);
        handler2Mock.Verify(h => h.Handle(eventType, eventData), Times.Once);
        handler3Mock.Verify(h => h.Handle(eventType, eventData), Times.Once);
    }

    [Fact]
    public void Dispatch_WithExceptionInHandler_LogsErrorAndContinues()
    {
        // Arrange
        var handler1Mock = new Mock<IEventHandler>();
        var handler2Mock = new Mock<IEventHandler>();
        var eventType = "chart.rendered";
        var eventData = new { ChartId = "789", Width = 800, Height = 600 };

        handler1Mock.Setup(h => h.Handle(eventType, eventData))
                   .Throws(new InvalidOperationException("Test exception"));

        _dispatcher.Subscribe(eventType, handler1Mock.Object);
        _dispatcher.Subscribe(eventType, handler2Mock.Object);

        // Act
        Action act = () => _dispatcher.Dispatch(eventType, eventData);

        // Assert
        act.Should().NotThrow();
        handler1Mock.Verify(h => h.Handle(eventType, eventData), Times.Once);
        handler2Mock.Verify(h => h.Handle(eventType, eventData), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WithNoHandlers_DoesNotThrow()
    {
        // Arrange
        var eventType = "chart.async.nonexistent";
        var eventData = new { Test = "async data" };

        // Act
        Func<Task> act = async () => await _dispatcher.DispatchAsync(eventType, eventData);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DispatchAsync_WithSingleHandler_CallsHandler()
    {
        // Arrange
        var handlerMock = new Mock<IEventHandler>();
        var eventType = "chart.async.created";
        var eventData = new { ChartId = "async-123", Title = "Async Test Chart" };

        _dispatcher.Subscribe(eventType, handlerMock.Object);

        // Act
        await _dispatcher.DispatchAsync(eventType, eventData);

        // Assert
        handlerMock.Verify(h => h.Handle(eventType, eventData), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WithMultipleHandlers_CallsAllHandlers()
    {
        // Arrange
        var handler1Mock = new Mock<IEventHandler>();
        var handler2Mock = new Mock<IEventHandler>();
        var eventType = "chart.async.updated";
        var eventData = new { ChartId = "async-456", ModifiedFields = new[] { "data" } };

        _dispatcher.Subscribe(eventType, handler1Mock.Object);
        _dispatcher.Subscribe(eventType, handler2Mock.Object);

        // Act
        await _dispatcher.DispatchAsync(eventType, eventData);

        // Assert
        handler1Mock.Verify(h => h.Handle(eventType, eventData), Times.Once);
        handler2Mock.Verify(h => h.Handle(eventType, eventData), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WithAsyncHandler_CallsAsyncHandler()
    {
        // Arrange
        var asyncHandlerMock = new Mock<IAsyncEventHandler>();
        var eventType = "chart.async.rendered";
        var eventData = new { ChartId = "async-789", Width = 1024, Height = 768 };

        _dispatcher.Subscribe(eventType, asyncHandlerMock.Object);

        // Act
        await _dispatcher.DispatchAsync(eventType, eventData);

        // Assert
        asyncHandlerMock.Verify(h => h.HandleAsync(eventType, eventData), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WithMixedSyncAndAsyncHandlers_CallsAllHandlers()
    {
        // Arrange
        var syncHandlerMock = new Mock<IEventHandler>();
        var asyncHandlerMock = new Mock<IAsyncEventHandler>();
        var eventType = "chart.async.mixed";
        var eventData = new { ChartId = "mixed-123" };

        _dispatcher.Subscribe(eventType, syncHandlerMock.Object);
        _dispatcher.Subscribe(eventType, asyncHandlerMock.Object);

        // Act
        await _dispatcher.DispatchAsync(eventType, eventData);

        // Assert
        syncHandlerMock.Verify(h => h.Handle(eventType, eventData), Times.Once);
        asyncHandlerMock.Verify(h => h.HandleAsync(eventType, eventData), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WithExceptionInHandler_LogsErrorAndContinues()
    {
        // Arrange
        var handler1Mock = new Mock<IEventHandler>();
        var handler2Mock = new Mock<IAsyncEventHandler>();
        var eventType = "chart.async.error";
        var eventData = new { ChartId = "error-123", ErrorMessage = "Test error" };

        handler1Mock.Setup(h => h.Handle(eventType, eventData))
                   .Throws(new InvalidOperationException("Sync exception"));

        _dispatcher.Subscribe(eventType, handler1Mock.Object);
        _dispatcher.Subscribe(eventType, handler2Mock.Object);

        // Act
        Func<Task> act = async () => await _dispatcher.DispatchAsync(eventType, eventData);

        // Assert
        await act.Should().NotThrowAsync();
        handler1Mock.Verify(h => h.Handle(eventType, eventData), Times.Once);
        handler2Mock.Verify(h => h.HandleAsync(eventType, eventData), Times.Once);
    }

    [Fact]
    public void GetSubscribedEventTypes_ReturnsAllSubscribedTypes()
    {
        // Arrange
        var eventType1 = "chart.created";
        var eventType2 = "chart.updated";
        var eventType3 = "chart.deleted";

        var handler = new Mock<IEventHandler>().Object;

        _dispatcher.Subscribe(eventType1, handler);
        _dispatcher.Subscribe(eventType2, handler);
        _dispatcher.Subscribe(eventType3, handler);

        // Act
        var subscribedTypes = _dispatcher.GetSubscribedEventTypes();

        // Assert
        subscribedTypes.Should().BeEquivalentTo(new[] { eventType1, eventType2, eventType3 });
    }

    [Fact]
    public void GetHandlerCount_WithNoHandlers_ReturnsZero()
    {
        // Act
        var count = _dispatcher.GetHandlerCount("nonexistent.event");

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void Clear_RemovesAllHandlers()
    {
        // Arrange
        var handler = new Mock<IEventHandler>().Object;
        var eventType1 = "chart.created";
        var eventType2 = "chart.updated";

        _dispatcher.Subscribe(eventType1, handler);
        _dispatcher.Subscribe(eventType2, handler);

        var countBefore = _dispatcher.GetHandlerCount(eventType1);
        countBefore.Should().Be(1);

        // Act
        _dispatcher.Clear();

        // Assert
        var countAfter1 = _dispatcher.GetHandlerCount(eventType1);
        var countAfter2 = _dispatcher.GetHandlerCount(eventType2);
        countAfter1.Should().Be(0);
        countAfter2.Should().Be(0);
    }

    [Fact]
    public void Dispatch_WithCaseInsensitiveEventType_MatchesCorrectly()
    {
        // Arrange
        var handlerMock = new Mock<IEventHandler>();
        var eventType = "chart.created";
        var eventData = new { ChartId = "case-test" };

        _dispatcher.Subscribe(eventType, handlerMock.Object);

        // Act
        _dispatcher.Dispatch("CHART.CREATED", eventData);
        _dispatcher.Dispatch("Chart.Created", eventData);
        _dispatcher.Dispatch("chart.CREATED", eventData);

        // Assert
        handlerMock.Verify(h => h.Handle(It.IsAny<string>(), eventData), Times.Exactly(3));
    }

    [Fact]
    public async Task DispatchAsync_WithCaseInsensitiveEventType_MatchesCorrectly()
    {
        // Arrange
        var handlerMock = new Mock<IEventHandler>();
        var eventType = "chart.async.created";
        var eventData = new { ChartId = "async-case-test" };

        _dispatcher.Subscribe(eventType, handlerMock.Object);

        // Act
        await _dispatcher.DispatchAsync("CHART.ASYNC.CREATED", eventData);
        await _dispatcher.DispatchAsync("Chart.Async.Created", eventData);

        // Assert
        handlerMock.Verify(h => h.Handle(It.IsAny<string>(), eventData), Times.Exactly(2));
    }
}