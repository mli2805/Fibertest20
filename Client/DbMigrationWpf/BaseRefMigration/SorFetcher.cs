using MySql.Data.MySqlClient;

namespace DbMigrationWpf
{
    public static class SorFetcher
    {
        public static byte[] GetMeasBytes(MySqlConnection conn, int fileId)
        {
            conn.Open();
            string cmdString = "select filesize, uncompress(filebin) from measfiles where Id = " + fileId;
            MySqlCommand cmd = new MySqlCommand(cmdString, conn);

            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            var filesize = (int)reader[0];
            var result = new byte[filesize];
            reader.GetBytes(1, 0, result, 0, filesize);

            reader.Close();
            conn.Close();
            return result;
        }
    }
}