using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Specialized;

using System.Data.SqlClient;

namespace TypeItWebRole
{

    public class WordStat
    {
        public bool Bored { get; set; }
        public bool Panic { get; set; }
        public int Duration { get; set; }
        public string WordText { get; set; }
        public string RewardUrl { get; set; }
        public string ImageUrl { get; set; }
    }

    public class UserConfig
    {
        public string Name { get; set; }
        public bool Highlight { get; set; }
        public bool ShowWord { get; set; }
        public bool BlinkLetter { get; set; }
        public bool SoundOn { get; set; }
    }

    public class MissedLetter
    {
        public char Letter { get; set; }
        public int MissCount { get; set; }
    }

    public class UserInfo
    {
        //stores a user's info such as preferences for categories, user uploaded images, statistics, etc.
        CloudStorageAccount _storageAccount;
        Microsoft.WindowsAzure.Storage.Table.CloudTable _statsTable;

        public UserInfo()
        {
            //todo: read from config!!
            _storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=typeit;AccountKey=yNz0Vysxc8ZLXmMyHh3WJl3tWvdTfg54a6avhfEC3QIxn6ZVhMWFxrWbIk6143hRrMUuYrwg5A750D9CnXAU/Q==");//
               // Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting("StorageConnectionString"));
        }


        /// <summary>
        /// sends a user image to storage
        /// </summary>
        /// <param name="fromPath"></param>
        public string UploadImage(string fromPath)
        {
            try
            {
                CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("usercontainer");

                container.CreateIfNotExists();
                
                //make public so we can download these from the browser
                container.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess =
                            BlobContainerPublicAccessType.Blob
                    });

                //send the file!
                string fileName = Path.GetFileName(fromPath);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

                // Create or overwrite the "myblob" blob with contents from a local file.
                using (var fileStream = File.OpenRead(fromPath))
                {
                    blockBlob.UploadFromStream(fileStream);
                }
                return blockBlob.Uri.ToString();
            }
            catch (FileNotFoundException ex)
            {
                string s = "File not found: " + fromPath + ", " + ex;
            }
            return String.Empty;
        }

        //Totally refactor this thing
        public string UploadImage(Stream input, string uniquefilename)
        {
                var blobClient = _storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("usercontainer");

                container.CreateIfNotExists();

                //make public so we can download these from the browser
                container.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess =
                            BlobContainerPublicAccessType.Blob
                    });

                //send the file!
                var fileName = Path.GetFileName(uniquefilename);
                var blockBlob = container.GetBlockBlobReference(fileName);
                blockBlob.UploadFromStream(input);
            return blockBlob.Uri.ToString();
        }

        string sqlConnectionString = "Data Source=rn8lvy4rm7.database.windows.net;Initial Catalog=TypeItDB;Persist Security Info=True;User ID=typeit;Password=hackautismQ!W@";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetWord"></param>
        /// <param name="duration"></param>
        /// <param name="panic"></param>
        /// <param name="bored"></param>
        /// <param name="missed"></param>
        public void UploadStats(string userName, string targetWord, int duration, bool panic, bool bored, List<MissedLetter> missed)
        {
/*            if (_statsTable == null )
            {
                CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

                _statsTable = tableClient.GetTableReference("userstats");
                _statsTable.CreateIfNotExists();
            }
            */

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                SqlCommand commandStat = new SqlCommand("InsertWordStat", connection);
                commandStat.Parameters.Add("@userName", System.Data.SqlDbType.NVarChar, 128);
                commandStat.Parameters["@username"].Value = userName;
                commandStat.Parameters.Add("@word", System.Data.SqlDbType.NVarChar);
                commandStat.Parameters["@word"].Value = targetWord;
                commandStat.Parameters.Add("@bored", System.Data.SqlDbType.Bit);
                commandStat.Parameters["@bored"].Value = bored;
                commandStat.Parameters.Add("@panic", System.Data.SqlDbType.Bit);
                commandStat.Parameters["@panic"].Value = panic;
                commandStat.Parameters.Add("@duration", System.Data.SqlDbType.Int);
                commandStat.Parameters["@duration"].Value = duration;
                commandStat.CommandType = System.Data.CommandType.StoredProcedure;
                commandStat.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetWord"></param>
        public List<WordStat> GetStats(string userName, string targetWord)
        {
            /*if (_statsTable == null)
            {
                CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

                _statsTable = tableClient.GetTableReference("userstats");
                _statsTable.CreateIfNotExists();
            }

            //todo: missed rates
            TableQuery<UserStatEntry> query =
                new TableQuery<UserStatEntry>().Where(
                    TableQuery.GenerateFilterCondition("TargetWord", 
                    QueryComparisons.Equal, targetWord));
            */

            List<WordStat> stats = new List<WordStat>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("GetWordStats", connection);
                command.Parameters.Add("@userName", System.Data.SqlDbType.NVarChar);
                command.Parameters["@userName"].Value = userName;
                command.Parameters.Add("@word", System.Data.SqlDbType.NVarChar);
                command.Parameters["@word"].Value = targetWord;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    //todo: JSON!
                    WordStat stat = new WordStat();
                    stat.Bored = Convert.ToBoolean(rdr["Bored"]);
                    stat.Panic = Convert.ToBoolean(rdr["Panic"]);
                    stat.Duration = Convert.ToInt32 (rdr["Duration"]);
                    stat.WordText = Convert.ToString(rdr["WordText"]);
                    stat.ImageUrl = Convert.ToString(rdr["ImageUrl"]);
                    stat.RewardUrl = Convert.ToString(rdr["RewardUrl"]);
                    stats.Add(stat);
                }


            }
            return stats;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetWord"></param>
        /// <param name="duration"></param>
        /// <param name="panic"></param>
        /// <param name="bored"></param>
        /// <param name="missed"></param>
        public void UpdateWord(string userName, string targetWord, string imageUrl, string rewardUrl)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("AddWordData", connection);
                command.Parameters.Add("@userName", System.Data.SqlDbType.NVarChar, 128);
                command.Parameters["@username"].Value = userName;
                command.Parameters.Add("@word", System.Data.SqlDbType.NVarChar, 128);
                command.Parameters["@word"].Value = targetWord;
                command.Parameters.Add("@rewardUrl", System.Data.SqlDbType.VarChar, 256);
                command.Parameters["@rewardUrl"].Value = rewardUrl;
                command.Parameters.Add("@imageUrl", System.Data.SqlDbType.VarChar, 256);
                command.Parameters["@imageUrl"].Value = imageUrl;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.ExecuteNonQuery();
            }
        }

        public UserConfig GetUserData(string userName)
        {
            UserConfig cfg = new UserConfig();

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("GetUserInfo", connection);
                command.Parameters.Add("@userName", System.Data.SqlDbType.NVarChar);
                command.Parameters["@userName"].Value = userName;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    cfg.Name = Convert.ToString(rdr["Name"]);
                    cfg.Highlight = Convert.ToBoolean(rdr["Highlight"]);
                    cfg.BlinkLetter = Convert.ToBoolean(rdr["BlinkLetter"]);
                    cfg.ShowWord = Convert.ToBoolean(rdr["ShowWord"]);
                    cfg.SoundOn = Convert.ToBoolean(rdr["SoundOn"]);
                }
            }
            return cfg;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="highlight"></param>
        /// <param name="showWord"></param>
        /// <param name="sounds"></param>
        /// <param name="blink"></param>
        public void UpdateUserData(string userName, bool highlight, bool showWord, bool sounds, bool blink)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("UpdateUserInfo", connection);
                command.Parameters.Add("@userName", System.Data.SqlDbType.NVarChar, 128);
                command.Parameters["@username"].Value = userName;
                command.Parameters.Add("@highlight", System.Data.SqlDbType.Bit);
                command.Parameters["@highlight"].Value = highlight;
                command.Parameters.Add("@showWord", System.Data.SqlDbType.Bit);
                command.Parameters["@showWord"].Value = showWord;
                command.Parameters.Add("@soundOn", System.Data.SqlDbType.Bit);
                command.Parameters["@soundOn"].Value = sounds;
                command.Parameters.Add("@blinkLetter", System.Data.SqlDbType.Bit);
                command.Parameters["@blinkLetter"].Value = blink;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.ExecuteNonQuery();
            }
        }

        public List<WordStat> GetWords(string userName)
        {
            List<WordStat> words = new List<WordStat>();

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("GetWords", connection);
                command.Parameters.Add("@userName", System.Data.SqlDbType.NVarChar);
                command.Parameters["@userName"].Value = userName;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    WordStat word = new WordStat();
                    word.WordText = Convert.ToString(rdr["WordText"]);
                    word.ImageUrl = Convert.ToString(rdr["ImageUrl"]);
                    word.RewardUrl = Convert.ToString(rdr["RewardUrl"]);
                    words.Add(word);
                }
            }
            return words;

        }
        
    }
}