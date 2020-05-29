using System;
using ReactOnlineActivity.Hubs.Constants;
using ReactOnlineActivity.Models;

namespace ReactOnlineActivity.Services
{
    public static class MessageFactory
    {
        public static Message CreateJoinNotification(string userName) =>
            new Message
            {
                Id = Guid.NewGuid().ToString(),
                From = $"{userName} присоединился(лась) к игре"
            };
        
        public static Message CreateGiveUpNotification(string userName) =>
            new Message
            {
                Id = Guid.NewGuid().ToString(),
                From = $"{userName} сдался(лась)",
                Text = "",
                State = MessageState.NotRated
            };
        
        public static Message CreateLeaveNotification(string userName) => 
            new Message
            {
                Id = Guid.NewGuid().ToString(),
                From = $"{userName} покинул(а) игру",
                Text = "",
                State = MessageState.NotRated
            };
        
        public static Message CreateGuessedNotification(string userName) =>
            new Message
            {
                Id = Guid.NewGuid().ToString(),
                From = $"{userName} угадал(а) слово",
                Text = "",
                State = MessageState.NotRated
            };
        
        public static Message CreateNewRoundNotification(int roundNumber) => 
            new Message
            {
                Id = Guid.NewGuid().ToString(),
                From = $"Раунд №{roundNumber}",
                Text = "",
                State = MessageState.NotRated
            };

        public static Message CreateVictoryNotification(string userName) =>
            new Message
            {
                Id = Guid.NewGuid().ToString(),
                From = $"{userName} победил(а)!"
            };
    }
}