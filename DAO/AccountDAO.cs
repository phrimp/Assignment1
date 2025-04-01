using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DAO
{
    public class AccountDAO
    {
        private NewsSystemContext _dbContext;
        private static AccountDAO instance;

        public AccountDAO()
        {
            _dbContext = new NewsSystemContext();
        }

        public static AccountDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AccountDAO();
                }
                return instance;
            }
        }

        public SystemAccount GetAccount(String email, String password)
        {
            return _dbContext.SystemAccounts.FirstOrDefault(m => m.AccountEmail.Equals(email) && m.AccountPassword.Equals(password));
        }

        public IEnumerable<SystemAccount> GetAllAccounts()
        {
            return _dbContext.SystemAccounts.ToList();
        }

        public SystemAccount GetAccountById(int id)
        {
            return _dbContext.SystemAccounts.FirstOrDefault(a => a.AccountId == id);
        }

        public SystemAccount GetAccountByEmail(string email)
        {
            return _dbContext.SystemAccounts.FirstOrDefault(a => a.AccountEmail == email);
        }

        public IEnumerable<SystemAccount> GetAccountsByRole(string role)
        {
            return _dbContext.SystemAccounts.Where(a => a.AccountRole == role).ToList();
        }

        public void AddAccount(SystemAccount account)
        {
            _dbContext.SystemAccounts.Add(account);
            _dbContext.SaveChanges();
        }

        public void UpdateAccount(SystemAccount account)
        {
            try
            {
                // Find the existing entity
                var existingAccount = _dbContext.SystemAccounts.Find(account.AccountId);

                if (existingAccount != null)
                {
                    // Detach the existing entity
                    _dbContext.Entry(existingAccount).State = EntityState.Detached;
                }

                // Now attach the updated entity and mark it as modified
                _dbContext.Entry(account).State = EntityState.Modified;
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // For debugging
                throw new Exception($"Error updating account: {ex.Message}", ex);
            }
        }

        public void DeleteAccount(int id)
        {
            var account = _dbContext.SystemAccounts.Find(id);
            if (account != null)
            {
                _dbContext.SystemAccounts.Remove(account);
                _dbContext.SaveChanges();
            }
        }

        public IEnumerable<SystemAccount> SearchAccounts(string searchTerm)
        {
            return _dbContext.SystemAccounts
                .Where(a => a.AccountName.Contains(searchTerm) ||
                           a.AccountEmail.Contains(searchTerm) ||
                           a.AccountRole.Contains(searchTerm))
                .ToList();
        }

        public bool IsEmailExists(string email)
        {
            return _dbContext.SystemAccounts.Any(a => a.AccountEmail == email);
        }

        public bool IsEmailExists(string email, int excludeId)
        {
            return _dbContext.SystemAccounts.Any(a => a.AccountEmail == email && a.AccountId != excludeId);
        }

        public bool HasCreatedArticles(int accountId)
        {
            return _dbContext.NewsArticles.Any(a => a.CreatedById == accountId);
        }

        public bool HasUpdatedArticles(int accountId)
        {
            return _dbContext.NewsArticles.Any(a => a.UpdatedById == accountId);
        }

        public IEnumerable<NewsArticle> GetArticlesCreatedByAccount(int accountId)
        {
            return _dbContext.NewsArticles
                .Include(a => a.Category)
                .Include(a => a.Tags)
                .Where(a => a.CreatedById == accountId)
                .OrderByDescending(a => a.CreatedDate)
                .ToList();
        }

        public IEnumerable<NewsArticle> GetArticlesUpdatedByAccount(int accountId)
        {
            return _dbContext.NewsArticles
                .Include(a => a.Category)
                .Include(a => a.Tags)
                .Where(a => a.UpdatedById == accountId)
                .OrderByDescending(a => a.ModifiedDate)
                .ToList();
        }
    }
}