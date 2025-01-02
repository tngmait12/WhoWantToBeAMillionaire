using Microsoft.AspNetCore.SignalR;
using System;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models;

namespace WhoWantToBeAMillionaire.Hubs
{
    public class OnlineCountHub : Hub
    {
        private readonly DataContext _dbContext;

        public OnlineCountHub(DataContext dbContext)
        {
            _dbContext = dbContext;
        }
        private static int _onlineUsers = 0;

        public override async Task OnConnectedAsync()
        {
            Interlocked.Increment(ref _onlineUsers); // Tăng số lượng người dùng
            await Clients.All.SendAsync("updateCount", _onlineUsers); // Gửi cập nhật đến tất cả client
            //await UpdateDailyAccessCount();//Update Statisticals
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Interlocked.Decrement(ref _onlineUsers); // Giảm số lượng người dùng
            await Clients.All.SendAsync("updateCount", _onlineUsers); // Gửi cập nhật đến tất cả client
            await base.OnDisconnectedAsync(exception);
        }

        
    }
}
