using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Game.Domain;

namespace OnlineActivity.Tests
{
    [TestFixture]
    public class CanvasTests
    {
        private Canvas _canvas;

        [SetUp]
        public void SetUp()
        {
            _canvas = new Canvas(20, 20);
        }

        [TestCase(10, 10, TestName = "WhenSizesSameAndPositive")]
        [TestCase(10, 15, TestName = "WhenSizeDifferentAndPositive")]
        public void Constructor_DoesNotThrow(int x, int y)
        {
            Action action = () => new Canvas(x, y);
            action.Should().NotThrow();
        }

        [TestCase(0, 3, TestName = "WhenOneCoordinatesIsZero")]
        [TestCase(3, -1, TestName = "WhenOneCoordinatesIsNegative")]
        public void Constructor_ShouldThrowArgumentException(int x, int y)
        {
            Action action = () => new Canvas(x, y);
            action.Should().Throw<ArgumentException>();
        }

        [TestCase(0, 0, 1, 2, 3, 4, 5)]
        public void PaintOverPixels_ShouldPaint(int x, params int[] coordinatesY)
        {
            var color = new Rgb(255, 255, 255);
            var pixels = coordinatesY
                .Select(y => new Pixel(
                    color,
                    new Point(x, y)
                ));


            _canvas.PaintOverPixels(pixels);


            foreach (var y in coordinatesY)
                _canvas.Pixels[x, y].Should().Be(color);
        }

        [TestCase(0, 0, 1, 2, 3, 4, 5)]
        public void PaintOverPixels_ShouldNotPaintUnnecessary(int x, params int[] coordinatesY)
        {
            var pixels = coordinatesY
                .Select(y => new Pixel(
                    new Rgb(255, 255, 255),
                    new Point(x, y)
                ));
            var previousCount = CountPainted(_canvas);


            _canvas.PaintOverPixels(pixels);

            CountPainted(_canvas).Should().Be(previousCount + pixels.Count());
        }

        private IEnumerable<(int, int)> Range2D(int startX, int countX, int startY, int countY)
        {
            return Enumerable
                .Range(startX, countX)
                .Select(x => Enumerable.Range(startY, countY).Select(y => (x, y)))
                .SelectMany(e => e);
        }

        private int CountPainted(Canvas canvas)
        {
            return Range2D(0, canvas.Size.Width, 0, canvas.Size.Height)
                .Count(tuple =>
                {
                    var (x, y) = tuple;
                    var rgb = canvas.Pixels[x, y];
                    return rgb.R == 255 && rgb.G == 255 && rgb.B == 255;
                });
        }
    }
}