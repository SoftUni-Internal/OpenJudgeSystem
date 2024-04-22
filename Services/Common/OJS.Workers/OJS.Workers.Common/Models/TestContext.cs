#nullable disable
namespace OJS.Workers.Common.Models
{
    public class TestContext
    {
        public int Id { get; set; }

        public string Input { get; set; }

        public string Output { get; set; }

        public bool IsTrialTest { get; set; }

        public int OrderBy { get; set; }
    }
}