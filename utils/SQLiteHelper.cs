using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using Dapper;
using KeyDrop_Sniffer.data;

namespace KeyDrop_Sniffer.utils
{
    public class SQLiteHelper
    {
        public static SQLiteHelper Instance {
            get {
                if (core == null)
                    core = new SQLiteHelper();
                return core;
            }
        }
        private static SQLiteHelper core;

        private string connectionString;
        private string dbPath;

        public SQLiteHelper()
        {
            string appPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeydropSniffer");
            string dbPath = System.IO.Path.Combine(appPath, "datasource.db");
            
            connectionString = "Data Source=" + dbPath + ";Version=3;";
            this.dbPath = dbPath;
        }

        public async Task<bool> CreateTables()
        {
            try
            {
                using (IDbConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.QueryAsync("BEGIN;" +
                        "CREATE TABLE IF NOT EXISTS `codes` (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, code VARCHAR(50) NOT NULL, success INTEGER(1) NOT NULL DEFAULT 0);" +
                        "CREATE INDEX IF NOT EXISTS code_index ON codes(code);" +
                        "COMMIT;", new DynamicParameters());
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<SQLCode>> GetCache()
        {
            try
            {
                using (IDbConnection conn = new SQLiteConnection(connectionString))
                {
                    var result = await conn.QueryAsync<SQLCode>("SELECT * FROM codes", new DynamicParameters());
                    return result.ToList();
                }
            } catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> AddCodes(List<SQLCode> toInsert)
        {
            try
            {
                using (IDbConnection conn = new SQLiteConnection(connectionString))
                {
                    await conn.ExecuteAsync("INSERT INTO codes (code, success) VALUES (@code, @success)", toInsert);
                    return true;
                }
            }
            catch (Exception ex) {
                System.Windows.MessageBox.Show(ex.Message);
            }
            return false;
        }
        public async Task<List<HistoryData>> GetHistory()
        {
            try
            {
                using (IDbConnection conn = new SQLiteConnection(connectionString))
                {
                    var result = await conn.QueryAsync<HistoryData>("SELECT * FROM codes", new DynamicParameters());

                    List<HistoryData> local = result.ToList();
                    local.Reverse();
                    return local;
                }
            } catch (Exception)
            {
                return new List<HistoryData>();
            }
        }
    }
}
