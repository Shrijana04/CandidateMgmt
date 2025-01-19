namespace JobCandidate.HubAPI.ViewModels
{
    public class GetCandidateInput
    {
        public string SearchText { get; set; } = string.Empty;
        public string Sorting { get; set; } = string.Empty;
        public bool IsDescending { get; set; } = false;
        public int MaxResultCount { get; set; } = 10;
        public int SkipCount { get; set; } = 0;
    }
}
