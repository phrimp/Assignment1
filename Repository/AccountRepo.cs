using DAO;
using Models;
using System;
using System.Collections.Generic;

namespace Repository
{
    public class AccountRepo : IAccountRepo
    {
        public SystemAccount Login(string email, string password) => AccountDAO.Instance.GetAccount(email, password);

        public IEnumerable<SystemAccount> GetAllAccounts() => AccountDAO.Instance.GetAllAccounts();

        public SystemAccount GetAccountById(int id) => AccountDAO.Instance.GetAccountById(id);

        public SystemAccount GetAccountByEmail(string email) => AccountDAO.Instance.GetAccountByEmail(email);

        public IEnumerable<SystemAccount> GetAccountsByRole(string role) => AccountDAO.Instance.GetAccountsByRole(role);

        public void AddAccount(SystemAccount account) => AccountDAO.Instance.AddAccount(account);

        public void UpdateAccount(SystemAccount account) => AccountDAO.Instance.UpdateAccount(account);

        public void DeleteAccount(int id) => AccountDAO.Instance.DeleteAccount(id);

        public IEnumerable<SystemAccount> SearchAccounts(string searchTerm) => AccountDAO.Instance.SearchAccounts(searchTerm);

        public bool IsEmailExists(string email) => AccountDAO.Instance.IsEmailExists(email);

        public bool IsEmailExists(string email, int excludeId) => AccountDAO.Instance.IsEmailExists(email, excludeId);

        public bool HasCreatedArticles(int accountId) => AccountDAO.Instance.HasCreatedArticles(accountId);

        public bool HasUpdatedArticles(int accountId) => AccountDAO.Instance.HasUpdatedArticles(accountId);

        public IEnumerable<NewsArticle> GetArticlesCreatedByAccount(int accountId) =>
            AccountDAO.Instance.GetArticlesCreatedByAccount(accountId);

        public IEnumerable<NewsArticle> GetArticlesUpdatedByAccount(int accountId) =>
            AccountDAO.Instance.GetArticlesUpdatedByAccount(accountId);
    }
}