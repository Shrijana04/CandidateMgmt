using JobCandidate.HubAPI.Entities;
using JobCandidate.HubAPI.Shared;
using Microsoft.EntityFrameworkCore;

namespace JobCandidate.HubAPI
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<Candidate> Candidates { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Candidate>(entity =>
            {
                // Define unique index for Email
                entity.HasIndex(c => c.Email)
                      .IsUnique();
                entity.Property(c => c.FirstName)
                      .HasMaxLength(CandidateConsts.FirstNameMaxLength)
                      .IsRequired();
                entity.Property(c => c.LastName)
                      .HasMaxLength(CandidateConsts.FirstNameMaxLength)
                      .IsRequired();
                entity.Property(c => c.PhoneNumber)
                    .HasMaxLength(CandidateConsts.PhoneNumberMaxLength);
                entity.Property(c => c.Email)
                      .HasMaxLength(CandidateConsts.EmailMaxLength)
                      .IsRequired();
                entity.Property(c => c.CallTimeInterval)
                     .HasMaxLength(CandidateConsts.CallTimeIntervalMaxLength);
                entity.Property(c => c.LinkedInProfileUrl)
                .HasMaxLength(CandidateConsts.LinkedInProfileUrlMaxLength);
                entity.Property(c => c.GitHubProfileUrl)
                .HasMaxLength(CandidateConsts.GitHubProfileUrlMaxLength);
                entity.Property(c => c.Comments)
              .HasMaxLength(CandidateConsts.CommentMaxLength)
              .IsRequired();

            });
        }
    }
}