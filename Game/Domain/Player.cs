using System;

namespace Game.Domain
{
    public class Player
    {
        public string Id { get; }
        public string Name { get; }
        public int Score { get; set; }
        
        public Player(string userId, string name)
        {
            Id = userId;
            Name = name;
        }
    }
}