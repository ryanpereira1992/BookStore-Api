using BookStore_Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_Api.Contracts
{
    public interface IBookRepository : IRepositoryBase<Book>
    {
    }
}
