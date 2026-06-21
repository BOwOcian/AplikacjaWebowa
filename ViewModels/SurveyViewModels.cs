using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SurveyApp.Models;

namespace SurveyApp.ViewModels
{
    public class CreateSurveyViewModel
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        
        public List<string> Options { get; set; } = new List<string> { "", "" };
    }

    public class ResultsViewModel
    {
        public Survey Survey { get; set; }
        public Dictionary<string, int> OptionVotes { get; set; }
        public int TotalVotes { get; set; }
        public bool HasVoted { get; set; }
    }
}