using Models;
using System;
using System.Collections.Generic;

namespace Repository
{
    public interface IAccountRepo
    {
        SystemAccount Login(string email, string password);
        IEnumerable<SystemAccount> GetAllAccounts();
        SystemAccount GetAccountById(int id);
        SystemAccount GetAccountByEmail(string email);
        IEnumerable<SystemAccount> GetAccountsByRole(string role);
        void AddAccount(SystemAccount account);
        void UpdateAccount(SystemAccount account);
        void DeleteAccount(int id);
        IEnumerable<SystemAccount> SearchAccounts(string searchTerm);
        bool IsEmailExists(string email);
        bool IsEmailExists(string email, int excludeId);
        bool HasCreatedArticles(int accountId);
        bool HasUpdatedArticles(int accountId);
        IEnumerable<NewsArticle> GetArticlesCreatedByAccount(int accountId);
        IEnumerable<NewsArticle> GetArticlesUpdatedByAccount(int accountId);
    }
}