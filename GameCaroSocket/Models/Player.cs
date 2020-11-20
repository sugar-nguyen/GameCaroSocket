using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameCaroSocket.Models
{
    public class Player
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string RoomId { get; set; }
        public bool IsBoss { get; set; }
    }
}
