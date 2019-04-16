using MeetMe.Entities;
using MeetMe.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetMe.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly MeetMeDbContext _context;

        public Repository(MeetMeDbContext context)
        {
            _context = context;
        }

        public IQueryable<TEntity> GetAll()
        {
            return _context.Set<TEntity>()
                .AsNoTracking();
        }

        //public async Task<TEntity> GetById(int id)
        //{
        //    return await _context.Set<TEntity>()
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(e => e.Id == id);
        //}

        public async Task Add(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        //public async Task Update(int id, TEntity entity)
        //{
        //    _context.Set<TEntity>().Update(entity);
        //    await _context.SaveChangesAsync();
        //}

        public void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }
    }
}
