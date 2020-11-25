using GameCaroSocket.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameCaroSocket.Hubs
{
    public class CaroHub : Hub<ICaroHub>
    {
        static List<Room> _Rooms = new List<Room>();
        const int MEMBER_IN_ROOM = 2;
        #region chat và config
        public async Task OnUserConnected(string roomId, string username)
        {
            if (string.IsNullOrEmpty(roomId))
            {
                string id = Context.ConnectionId;
                string[] arrId = _Rooms.Any() ? _Rooms.Select(x => x.RoomId).ToArray() : null;
                string _roomId = await GeneratorId(arrId);
                var player = new Player() { UserId = id, RoomId = _roomId, UserName = username, IsBoss = true };
                var curRoom = new Room();
                curRoom.RoomId = _roomId;
                curRoom.Players.Add(player);
                _Rooms.Add(curRoom);

                await Groups.AddToGroupAsync(id, _roomId);
                await Clients.Caller.onUserCreateRoom(_roomId, player);
            }
            else
            {
                var curRoom = _Rooms.SingleOrDefault(x => x.RoomId.Trim().ToUpper() == roomId.Trim().ToUpper());
                if (curRoom != null)
                {
                    if (curRoom.Players.Count == MEMBER_IN_ROOM)
                    {
                        await Clients.Caller.overflowMember();
                    }
                    else
                    {
                        var player = curRoom.Players.SingleOrDefault(x => x.UserName.ToUpper() == username.ToUpper());
                        if (player == null)
                        {
                            player = new Player() { RoomId = curRoom.RoomId, UserName = username, UserId = Context.ConnectionId, IsBoss = false };
                            var playerBoss = curRoom.Players.First();
                            curRoom.Players.Add(player);

                            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                            await Clients.Client(playerBoss.UserId).onGuessJoinRoom(roomId, player, playerBoss);
                            await Clients.Caller.onCallerJoinRoom(roomId, player, playerBoss);
                        }
                        else
                        {
                            await Clients.Caller.userNameExists();
                        }
                    }

                }
                else
                {
                    await Clients.Caller.roomIdNotExists();
                }
            }

        }

        public async Task SendPrivateMessage(string message)
        {
            try
            {
                var room = GetRoom();
                if (room.Players.Count == 2)
                {
                    Player player = GetCaller();
                    Player player2 = GetOrtherPlayer(player, room);

                    await Clients.Caller.onUserSendMessage(player, player2, message);
                    await Clients.Client(player2.UserId).onUserSendMessage(player2, player, message, false);
                }
            }
            catch
            {
                //ignore trường hợp room lỗi.
            }

        }

        private Player GetCaller()
        {
            return _Rooms.SelectMany(x => x.Players.Where(x => x.UserId.Equals(Context.ConnectionId))).SingleOrDefault();
        }
        private Player GetOrtherPlayer(Player player, Room room)
        {
            if (room.Players.Count == 1)
            {
                return null;
            }

            Player player2 = room.Players.Last();
            if (player2.UserId == player.UserId) player2 = room.Players.First();
            return player2;
        }
        private Room GetRoom()
        {
            return _Rooms.SingleOrDefault(x => x.Players.Any(y => y.UserId == Context.ConnectionId));
        }

        private async Task<string> GeneratorId(string[] arr)
        {
            string myString = "0123456789";
            Random random = new Random();

            var result = new string(Enumerable.Repeat(myString, 5).Select(s => s[random.Next(s.Length)]).ToArray());

            if (arr != null && Array.Exists(arr, x => x == result))
            {
                return await GeneratorId(arr);
            }
            return result;
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var room = GetRoom();
            if (room != null)
            {
                var caller = GetCaller();
                //Remove player
                _Rooms.ForEach(x => x.Players.Remove(x.Players.Single(y => y.UserId == Context.ConnectionId)));

                if (room.Players.Any())
                {
                    // đổi player còn lại thành chủ phòng
                    room.Players.ForEach(x => x.IsBoss = true);
                }
                //remove room nếu phòng không còn player.
                RemoveRoom();

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.RoomId);
                await Clients.Group(room.RoomId).onUserDisConnected(caller);
            }
            await base.OnDisconnectedAsync(exception);
        }
        private void RemoveRoom()
        {
            var Rooms = new List<Room>(_Rooms);
            foreach (var room in Rooms)
            {
                if (room.Players.Count == 0)
                {
                    _Rooms.Remove(room);
                }
            }
        }
        public async Task PlayGame()
        {
            var room = GetRoom();

            if (room != null && room.Players.Count == 2)
            {
                await Clients.Group(GetRoom().RoomId).playGame();
            }

        }
        #endregion

        #region caro
        public async Task OnUserPickChess(string coordinates, string callerWinCoordinate)
        {
            var player = GetCaller();
            var room = GetRoom();
            var ortherPlayer = GetOrtherPlayer(player, room);

            await Clients.Client(ortherPlayer.UserId).onUserPickChess(player, coordinates);
            //  await Clients.Caller.onUserPickChess(player, coordinates);
            if (!string.IsNullOrEmpty(callerWinCoordinate))
            {
                await Clients.Group(room.RoomId).onUserWin(player, ortherPlayer, callerWinCoordinate);
            }
            else
            {
                await Clients.Group(room.RoomId).startRuntime();
              //  await Clients.Group(room.RoomId).restartRuntime();
            }
        }
        #endregion
    }
}
