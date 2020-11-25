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
        /// <summary>
        /// Hành động phải làm khi caller join vào room đã tạo
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="caller">Thông tin của khách truy cập</param>
        /// <param name="boss">Thông tin của chủ phòng</param>
        /// <returns></returns>
        Task onCallerJoinRoom(string roomId, Player caller, Player boss);
        /// <summary>
        /// Tạo thông báo cho chủ phòng
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="guess">Thông tin của khách đang join</param>
        /// <param name="boss">Thông tin của chủ phòng</param>
        /// <returns></returns>
        Task onGuessJoinRoom(string roomId, Player guess, Player boss);
        Task userNameExists();
        Task roomIdNotExists();
        Task onUserSendMessage(Player fromPlayer, Player toPlayer, string msg, bool isCaller = true);
        Task onUserDisConnected(Player player);
        Task playGame();

        #region Caro
        Task onUserPickChess(Player caller, string coordinates);
        Task onUserWin(Player playerWin, Player playerLost, string arrCoordinate);
        Task startRuntime();
        Task restartRuntime();
        #endregion
    }
}
