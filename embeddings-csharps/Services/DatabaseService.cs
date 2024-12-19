using System.Data.SQLite;

namespace embeddings_csharps.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;

            // Tabloyu otomatik oluştur
            EnsureTableExists();
        }

        private void EnsureTableExists()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS advertisements (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT NOT NULL,
                    description TEXT NOT NULL,
                    embedding BLOB NOT NULL
                )", connection);
            command.ExecuteNonQuery();
        }

        public void SaveAdvertisement(string title, string description, byte[] embedding)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            // Aynı başlık ve açıklamaya sahip kayıt var mı kontrol et
            var checkCommand = new SQLiteCommand(
                "SELECT COUNT(*) FROM advertisements WHERE title = @title AND description = @description",
                connection
            );
            checkCommand.Parameters.AddWithValue("@title", title);
            checkCommand.Parameters.AddWithValue("@description", description);

            var count = Convert.ToInt32(checkCommand.ExecuteScalar());
            if (count > 0)
            {
                Console.WriteLine($"Kayıt zaten mevcut: {title}");
                return;
            }

            // Yeni kaydı ekle
            var insertCommand = new SQLiteCommand(
                "INSERT INTO advertisements (title, description, embedding) VALUES (@title, @description, @embedding)",
                connection
            );
            insertCommand.Parameters.AddWithValue("@title", title);
            insertCommand.Parameters.AddWithValue("@description", description);
            insertCommand.Parameters.AddWithValue("@embedding", embedding);
            insertCommand.ExecuteNonQuery();
        }

        public List<(int Id, string Title, string Description, byte[] Embedding)> GetAdvertisements()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = new SQLiteCommand("SELECT id, title, description, embedding FROM advertisements", connection);
            using var reader = command.ExecuteReader();

            var advertisements = new List<(int, string, string, byte[])>();
            while (reader.Read())
            {
                advertisements.Add((
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    (byte[])reader["embedding"]
                ));
            }
            return advertisements;
        }
    }

}
