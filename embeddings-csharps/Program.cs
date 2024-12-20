using embeddings_csharps.Services;
using DotNetEnv;

namespace embeddings_csharps

{
    class Program
    {

        static void Main(string[] args)
        {
            // Ana asenkron işlemi çalıştırıyoruz
            RunAsync(args).GetAwaiter().GetResult();   
        }

   

        static async Task RunAsync(string[] args)
        {

            try
            {
                string connectionString = "Data Source=advertisements1.db";
                var openAIKey = "UjUY7_J3_ikkA";

                var openAIService = new Services.OpenAIService(openAIKey);
                var databaseService = new Services.DatabaseService(connectionString);
                var advertisementService = new Services.AdvertisementService(openAIService, databaseService);

                // Reklam metinlerini veritabanına kaydet
                var advertisements = new List<(string Title, string Description)>
                {
                    ("Asus ZenBook", "Hafif ve taşınabilir laptop. Üstün performansla her yere taşıyın."),
                    ("MacBook Air", "Şıklık ve performans bir arada. Apple güvencesiyle MacBook Air."),
                    ("Lenovo ThinkPad", "İş dünyasının güvenilir laptopu. Profesyonellerin tercihi."),
                    ("MSI Gaming", "Oyun severler için yüksek performanslı laptop serisi."),
                    ("Dell XPS 13", "Tasarım ve güçlü donanım, Dell XPS 13'te buluşuyor."),
                    ("HP Spectre x360", "Dönüştürülebilir laptop ile her an her yerde çalışın."),
                    ("iPhone 14", "En yeni iPhone. Daha hızlı, daha güçlü, daha yenilikçi."),
                    ("Samsung Galaxy S23", "Güçlü işlemci ve üstün kamera. Akıllı telefon yeniden tanımlandı."),
                    ("Sony PlayStation 5", "Oyun dünyasına adım atın. PS5'le eğlenceyi yaşayın."),
                    ("Bose QuietComfort 45", "Efsanevi gürültü engelleme ile mükemmel ses deneyimi.")
                };

                Console.WriteLine("Reklamlar veritabanına kaydediliyor...");
                foreach (var (title, description) in advertisements)
                {
                    try
                    {
                        // Her reklam için embedding alınıyor ve kaydediliyor
                        var embedding = await openAIService.GetEmbeddingAsync(title+ "-" +description);
                        var embeddingBytes = FloatArrayToByteArray(embedding);
                        await advertisementService.AddAdvertisementAsync(title, description, embeddingBytes);
                        Console.WriteLine($"Reklam kaydedildi: {title} - {description}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Reklam kaydedilirken bir hata oluştu: {ex.Message}");
                    }
                }


                Console.WriteLine("Reklamlar veritabanına kaydedildi.");

                // Kullanıcıdan mesaj al
                Console.WriteLine("Lütfen bir metin girin:");
                string userInput = Console.ReadLine();

                if (string.IsNullOrEmpty(userInput))
                {
                    Console.WriteLine("Geçerli bir metin girmelisiniz.");
                    return;
                }

                try
                {
                    // Kullanıcının mesajını embedding olarak al
                    var userEmbedding = await openAIService.GetEmbeddingAsync(userInput);
                    var userEmbeddingBytes = FloatArrayToByteArray(userEmbedding);

                    // Veritabanındaki reklamlar ile karşılaştırma yap
                    var bestAd = await advertisementService.FindBestAdvertisementAsync(userEmbeddingBytes);

                    // Sonuçları kontrol et
                    if (bestAd != null)
                    {
                        Console.WriteLine("\nEn uygun reklam:");
                        Console.WriteLine($"Başlık: {bestAd.Title}");
                        Console.WriteLine($"Açıklama: {bestAd.Description}");
                    }
                    else
                    {
                        Console.WriteLine("Uygun reklam bulunamadı.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Kullanıcı girişiyle ilgili bir hata oluştu: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ana işlemde bir hata oluştu: {ex.Message}");
            }

            // Programın hemen kapanmaması için kullanıcıdan bir tuşa basmasını bekleyin
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        // float[]'ı byte[]'a dönüştürme metodu
        static byte[] FloatArrayToByteArray(float[] floatArray)
        {
            byte[] byteArray = new byte[floatArray.Length * sizeof(float)];
            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }
    }
}
