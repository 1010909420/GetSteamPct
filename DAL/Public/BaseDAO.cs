using EF;
using Microsoft.EntityFrameworkCore;
using Model.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAL.Public
{
    public class BaseDAO<T> where T : BaseEntity<T>
    {
        public IQueryable<T> GetAll()
        {
            SteamPctDbContext db = new SteamPctDbContext();
            return db.Set<T>();
        }

        public int Add(T entity)
        {
            SteamPctDbContext db = new SteamPctDbContext();
            db.Set<T>().Add(entity);
            return db.SaveChanges();
        }

        public int SaveList(List<T> list)
        {
            SteamPctDbContext db = new SteamPctDbContext();
            int result = 0;
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (T entity in list)
                    {
                        db.Set<T>().Add(entity);
                        result = db.SaveChanges();
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    result = -1;
                }
            }
            return result;
        }

        public int Update(T entity)
        {
            SteamPctDbContext db = new SteamPctDbContext();
            db.Entry(entity).State = EntityState.Modified;
            return db.SaveChanges();
        }

        public int DelById(int id)
        {
            SteamPctDbContext db = new SteamPctDbContext();
            T entity = db.Set<T>().Find(id);
            int c = 0;
            if (entity != null)
            {
                db.Set<T>().Remove(entity);
                c = db.SaveChanges();
            }
            return c;
        }

        public int DelByIds(List<int> list)
        {
            SteamPctDbContext db = new SteamPctDbContext();
            int result = 0;
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (int id in list)
                    {
                        T entity = db.Set<T>().Find(id);
                        db.Set<T>().Remove(entity);
                    }
                    result = db.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    result = -1;
                }
            }
            return result;
        }

        public int BatchUpdate(List<T> entities)
        {
            SteamPctDbContext db = new SteamPctDbContext();
            int result = 0;
            //这里应当加上事务，其中一条出现错误应该回滚
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    entities.ForEach(m =>
                    {
                        db.Entry<T>(m).State = EntityState.Modified;
                    });
                    result = db.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    result = -1;
                }
            }
            return result;
        }
    }
}
