using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SurveyApp.Models
{
    public class Survey
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany")]
        public string Title { get; set; }

        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public string CreatorId { get; set; }
        public IdentityUser Creator { get; set; }

        public List<Option> Options { get; set; } = new List<Option>();
    }

    public class Option
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Treść opcji jest wymagana")]
        public string Text { get; set; }
        
        public int SurveyId { get; set; }
        public Survey Survey { get; set; }

        public List<Vote> Votes { get; set; } = new List<Vote>();
    }

    public class Vote
    {
        public int Id { get; set; }
        
        public int OptionId { get; set; }
        public Option Option { get; set; }

        public string VoterId { get; set; }
        public IdentityUser Voter { get; set; }

        public DateTime VotedAt { get; set; } = DateTime.Now;
    }
}