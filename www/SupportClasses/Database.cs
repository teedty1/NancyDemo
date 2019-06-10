using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace www.SupportClasses
{
    public interface IDatabase
    {
        Dictionary<string, Guid> GetTopUsers();
        IEnumerable<string> GetTopUsernames();
        IEnumerable<string> GetUsers(Guid clientGUID);
        bool UpdateUser(Guid clientGUID, int userID, string newLastName);
    }

    public class Database : IDatabase
    {
        private readonly string connString;
        private readonly IConfiguration config;
        private readonly Dictionary<string, Guid> topUsers;
        public Database(IConfiguration config)
        {
            this.config = config;

            connString = config.GetConnectionString("AuroraRO");
            //using (var conn = new MySqlConnection(connString))
            //    topUsers = conn.Query("SELECT username, clientGUID FROM Avalon.Users LIMIT 50").ToDictionary(k => (string)k.username, v => (Guid)v.clientGUID);
        }

        public Dictionary<string, Guid> GetTopUsers()
        {
            return topUsers;
        }

        public IEnumerable<string> GetTopUsernames()
        {
            return topUsers.Select(x => x.Key);
        }

        public IEnumerable<string> GetUsers(Guid clientGUID)
        {
            using (var conn = new MySqlConnection(connString))
                return conn.Query("SELECT username, clientGUID FROM Configuration.config_Users WHERE clientGUID = @clientGUID LIMIT 5", new
                {
                    clientGUID
                }).Select(x => (string)x.username + "~" + (Guid)x.clientGUID);
        }

        public bool UpdateUser(Guid clientGUID, int userID, string newLastName)
        {
            try
            {
                using (var conn = new MySqlConnection(connString))
                    return conn.Execute("UPDATE Configuration.config_Users SET lastName = @newLastName WHERE clientGUID = @clientGUID AND userID = @userID", new
                    {
                        clientGUID,
                        userID,
                        newLastName
                    }) > 0;
            }
            catch (Exception e)
            {
                //Log this error somewhere
                return false;
            }
        }
    }
}
