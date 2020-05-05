using System;
using FluentAssertions;
using NUnit.Framework;
using ReactOnlineActivity.Services;

namespace OnlineActivity.Tests
{
    [TestFixture]
    public class GameTests
    {
        [TestCase("TIMURCHEK")]
        public void Constructor_DoesNotThrow(string word)
        {
            Action action = () => new Game(new []{ word }, new Player[0]);
            action.Should().NotThrow();
        }
        
        [TestCase(null, TestName = "WhenWordIsNull")]
        public void Constructor_ShouldThrowArgumentException(string word)
        {
            Action action = () => new Game(new []{ word }, new Player[0]);
            action.Should().Throw<ArgumentException>();
        }

        [TestCase("1", "1", TestName = "WhenSame")]
        [TestCase("1", "   1 ", TestName = "WhenThereIdent")]
        public void CheckWord_ShouldReturnsTrue(string hiddenWord, string playerWord)
        {
            var game = new Game(new []{ hiddenWord }, new Player[0]);
            game.CheckWord(playerWord).Should().BeTrue();
        }
        
        [TestCase("1", "2", TestName = "WhenDifferent")]
        public void CheckWord_ShouldReturnsFalse(string hiddenWord, string playerWord)
        {
            var game = new Game(new []{ hiddenWord }, new Player[0]);
            game.CheckWord(playerWord).Should().BeFalse();
        }
    }
}