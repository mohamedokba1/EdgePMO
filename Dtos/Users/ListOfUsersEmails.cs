using System.ComponentModel.DataAnnotations;

namespace EdgePMO.API.Dtos
{
    public class ListOfUsersEmails
    {
        [Required]
        public List<string> Emails { get; init; }
    }
}
