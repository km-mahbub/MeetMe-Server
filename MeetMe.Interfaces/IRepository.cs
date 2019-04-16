using MeetMe.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMe.Interfaces
{
    public interface IRepository<TEntity> where TEntity: class
    {

        IQueryable<TEntity> GetAll();
        //Task<TEntity> GetById(int id);
        Task Add(TEntity entity);
        //Task Update(int id, TEntity entity);
        void Delete(TEntity entity);

    }
}
