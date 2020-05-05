using System;

namespace Game.Domain
{
    public class Player
    {
        public Guid UserId { get; }
        public string Name { get; }
        public int Score { get; set; }
        
        public Player(Guid userId, string name)
        {
            UserId = userId;
            Name = name;
        }
    }
}