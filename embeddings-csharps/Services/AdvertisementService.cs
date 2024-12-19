using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using embeddings_csharps.Models;
using embeddings_csharps.Services;

namespace embeddings_csharps.Services
{
    public class AdvertisementService
    {
        private readonly OpenAIService _openAIService;
        private readonly DatabaseService _databaseService;

        public AdvertisementService(OpenAIService openAIService, DatabaseService databaseService)
        {
            _openAIService = openAIService;
            _databaseService = databaseService;
        }

        public async Task AddAdvertisementAsync(string title, string description, byte[] embedding)
        {
            // DatabaseService'ye title, description ve embedding parametrelerini gönderiyoruz
            _databaseService.SaveAdvertisement(title, description, embedding);
        }

        public List<Advertisement> GetAdvertisements()
        {
            // Tuple listesini Advertisement modeline dönüştürüyoruz
            var advertisementsData = _databaseService.GetAdvertisements();
            return advertisementsData.Select(ad => new Advertisement
            {
                Id = ad.Id,
                Title = ad.Title,
                Description = ad.Description,
                Embedding = ad.Embedding
            }).ToList();
        }

        public async Task<Advertisement> FindBestAdvertisementAsync(byte[] userEmbedding)
        {
            var advertisements = GetAdvertisements(); // Veritabanındaki tüm reklamları al

            float bestScore = -1;
            Advertisement bestAd = null;

            foreach (var ad in advertisements)
            {
                var similarity = CalculateCosineSimilarity(userEmbedding, ad.Embedding);
                if (similarity > bestScore)
                {
                    bestScore = similarity;
                    bestAd = ad;
                }
            }

            return bestAd;
        }

        private float CalculateCosineSimilarity(byte[] embedding1, byte[] embedding2)
        {
            var vec1 = ByteArrayToFloatArray(embedding1);
            var vec2 = ByteArrayToFloatArray(embedding2);

            float dotProduct = 0;
            float magnitude1 = 0;
            float magnitude2 = 0;

            for (int i = 0; i < vec1.Length; i++)
            {
                dotProduct += vec1[i] * vec2[i];
                magnitude1 += vec1[i] * vec1[i];
                magnitude2 += vec2[i] * vec2[i];
            }

            magnitude1 = (float)Math.Sqrt(magnitude1);
            magnitude2 = (float)Math.Sqrt(magnitude2);

            return dotProduct / (magnitude1 * magnitude2);
        }

        private float[] ByteArrayToFloatArray(byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / sizeof(float)];
            Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
            return floatArray;
        }
    }
}
