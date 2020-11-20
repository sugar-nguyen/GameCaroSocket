using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameCaroSocket.Models
{
    public class Room
    {
        public string RoomId { get; set; }
        public List<Player> Players { get; set; }
        
    }
}
