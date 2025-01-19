namespace JobCandidate.HubAPI.Entities
{
    public class Candidate
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string CallTimeInterval { get; set; }
        public string LinkedInProfileUrl { get; set; }
        public string GitHubProfileUrl { get; set; }
        public string Comments { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public DateTime? LasteUpdated { get; set; }
    }
}
