using System.Threading.Tasks;
using DeepComparison.Json;
using FluentAssertions;
using Newtonsoft.Json.Linq;

namespace Tests
{
    public static class JsonAssert
    {
        private static readonly JsonComparer Comparer = new JsonComparerBuilder().Build();

        public static Task ShouldBeOk(this Task t)
        {
            return t;
        }

        public static async Task ShouldBe(this Task<string> json, object anon)
        {
            var response = await json;
            response.ShouldBe(anon);
        }
        public static void ShouldBe(this string json, object anon)
        {
            Comparer.Compare(JToken.Parse(json), anon)
                .Message.Should().Be(null);
        }
    }
}