// ReSharper disable InconsistentNaming
namespace Iit.Fibertest.Dto
{
    public class TestsRelation
    {
        public string id { get; set; }
        public string testAId { get; set; }
        public string testBId { get; set; }
        public string type { get; set; } = "fibertest_fast_precise";
    }
}