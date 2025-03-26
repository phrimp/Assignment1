using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO
{
    public class AccountDAO
    {
        private NewsSystemContext _dbContext;
        private static AccountDAO instance;
        public AccountDAO() { 
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
    }
}
