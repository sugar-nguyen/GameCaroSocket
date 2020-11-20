using GameCaroSocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameCaroSocket.Hubs
{
    public interface ICaroHub
    {
        Task onUserConnected(int roomId, string userName);
        Task onUserCreateRoom(string roomId, Player player);
        Task overflowMember();
        Task addUserToRoom(string roomId, Player caller, Player user, bool isPlayer2 = false);
        Task userNameExists();
        Task roomIdNotExists();
       
    }
}
