using System;
namespace SearchAndBook.Shared
{
    internal class BookingDTO
    {
        public int gameId { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public int price { get; set; }
        public string city { get; set; }
        public string ownerCity { get; set; }

        public int minimumNrPlayers { get; set; }
        public int maximumNrPlayers { get; set; }

        public int userId { get; set; }
        public string displayName { get; set; }
        public bool isSuspended { get; set; }
        public string avatarURL { get; set; }
        public DateTime createdAt { get; set; }
    }
}