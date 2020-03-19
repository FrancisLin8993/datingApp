using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
  public class DatingRepository : IDatingRepository
  {
    private readonly DataContext _context;

    public DatingRepository(DataContext context)
    {
      _context = context;
    }
    public void Add<T>(T entity) where T : class
    {
      _context.Add(entity);
    }

    public void Delete<T>(T entity) where T : class
    {
      _context.Remove(entity);
    }

    public async Task<Like> GetLike(int userId, int recipientId)
    {
      return await _context.Likes.FirstOrDefaultAsync(l => l.LikerId == userId && l.LikeeId == recipientId);
    }

    public async Task<Photo> GetMainPhoto(int userId)
    {
      var photo = await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
      return photo;
    }

    public async Task<Photo> GetPhoto(int id)
    {
      var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
      return photo;
    }

    public async Task<User> GetUser(int id)
    {
      var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
      return user;
    }

    public async Task<PagedList<User>> GetUsers(UserParams userParams)
    {
      var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
      users = users.Where(u => u.Id != userParams.UserId);
      users = users.Where(u => u.Gender == userParams.Gender);

      if (userParams.HasLikers)
      {
        var userLikers = await GetUserLikes(userParams.UserId, userParams.HasLikers);
        users = users.Where(u => userLikers.Contains(u.Id));
      }

      if (userParams.HasLikees)
      {
        var userLikees = await GetUserLikes(userParams.UserId, userParams.HasLikers);
        users = users.Where(u => userLikees.Contains(u.Id));
      }

      if (userParams.MinAge != 18 || userParams.MaxAge != 99)
      {
        var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
        var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

        users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
      }

      if (!string.IsNullOrEmpty(userParams.OrderBy))
      {
        if (userParams.OrderBy == "created")
        {
          users = users.OrderByDescending(u => u.Created);
        }
        else
        {
          users = users.OrderByDescending(u => u.LastActive);
        }
      }

      return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
    }

    public async Task<bool> SaveAll()
    {
      return await _context.SaveChangesAsync() > 0;
    }

    private async Task<IEnumerable<int>> GetUserLikes(int id, bool HasLikers)
    {
      var user = await _context.Users
          .Include(l => l.Likers)
          .Include(l => l.Likees)
          .FirstOrDefaultAsync(u => u.Id == id);

      if (HasLikers)
      {
        return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
      }
      else
      {
        return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
      }
    }
  }
}