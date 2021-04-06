using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SentimentAnalyzer.Tests
{
    public class Benchmarks
    {
        private readonly ITestOutputHelper output;

        public Benchmarks(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Use IMDB dataset of 50k reviews to benchmark analysis.
        /// See: https://github.com/nas5w/imdb-data
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Loop_Of_50000_IMDB_Reviews()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("SentimentAnalyzer.Tests.reviews.json");

            Assert.NotNull(stream);

            var reviews = await JsonSerializer.DeserializeAsync<Review[]>(stream);
            Assert.Equal(50000, reviews.Length);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for(var i = 0; i < reviews.Length; i++)
            {
                Sentiments.Predict(reviews[i].t);
            }

            stopWatch.Stop();

            Assert.True(stopWatch.ElapsedMilliseconds > 0);
            output.WriteLine($"Loop duration: {stopWatch.ElapsedMilliseconds}ms");
        }
    }

    public class Review
    {
        public string t { get; set; }
        public int s { get; set; }
    }
}
