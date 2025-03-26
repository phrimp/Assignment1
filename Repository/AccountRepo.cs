using DAO;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class AccountRepo : IAccountRepo
    {
        public SystemAccount Login(string email, string password) => AccountDAO.Instance.GetAccount(email, password);
    }
}
