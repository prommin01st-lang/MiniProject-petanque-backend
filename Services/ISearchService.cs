using JWTdemo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JWTdemo.Services
{
    public interface ISearchService
    {
        Task<IEnumerable<GlobalSearchResultDto>> SearchAsync(string query, Guid userId, bool isAdmin);
    }
}