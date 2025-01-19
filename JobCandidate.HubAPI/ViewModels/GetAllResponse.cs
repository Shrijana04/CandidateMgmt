namespace JobCandidate.HubAPI.ViewModels
{
    public class GetAllResponse
    {
        public int TotalCount { get; set; }
        public List<CandidateModel> Items { get; set; }
    }
}
