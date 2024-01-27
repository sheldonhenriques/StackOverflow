namespace StackOverflow.Models
{
    public class SearchResultViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Votes { get; set; }
        public int AnswerCount { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Reputation { get; set; }
        public int Badges { get; set; }
    }
}
