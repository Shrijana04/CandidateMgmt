using System.ComponentModel.DataAnnotations;

namespace JobCandidate.HubAPI.ViewModels
{
    public class CreateModel
    {

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string CallTimeInterval { get; set; }
        public string LinkedInProfileUrl { get; set; }
        public string GitHubProfileUrl { get; set; }

        [Required]
        public string Comments { get; set; }
    }
}
