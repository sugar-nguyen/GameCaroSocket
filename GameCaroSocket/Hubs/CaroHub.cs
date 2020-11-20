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

        public async Task OnUserConnected(string roomId, string username)
        {
            if (string.IsNullOrEmpty(roomId))
            {
                string id = Context.ConnectionId;
                string[] arrId = _Rooms.Any() ? _Rooms.Select(x => x.RoomId).ToArray() : null;
                roomId = await GeneratorId(arrId);
                var player = new Player() { UserId = id, RoomId = roomId, UserName = username, IsBoss = true };
                var curRoom = new Room();
                curRoom.RoomId = roomId;
                curRoom.Players.Add(player);
                _Rooms.Add(curRoom);

                await Groups.AddToGroupAsync(id, roomId);
                await Clients.Caller.onUserCreateRoom(roomId, player);
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
                            curRoom.Players.Add(player);

                            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                            await Clients.Clients(curRoom.Players.First().UserId).addUserToRoom(roomId, curRoom.Players.First(), player);
                            await Clients.Caller.addUserToRoom(roomId, player, curRoom.Players.First(), true);
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
    }
}
