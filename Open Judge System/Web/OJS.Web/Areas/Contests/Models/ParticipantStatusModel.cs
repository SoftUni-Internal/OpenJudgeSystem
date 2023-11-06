namespace OJS.Web.Areas.Contests.Models
{
    using System;
    using System.Linq.Expressions;
    using OJS.Data.Models;

    public class ParticipantStatusModel
    {
        public static Expression<Func<Participant, ParticipantStatusModel>> FromParticipant =>
            participant => new ParticipantStatusModel
            {
                UserId = participant.UserId,
                ContestId = participant.ContestId,
                IsOfficial = participant.IsOfficial,
                ParticipationEndTime = participant.ParticipationEndTime,
            };

        public string UserId { get; set; }
        
        public int ContestId { get; set; }

        public bool IsOfficial { get; set; }

        public DateTime? ParticipationEndTime { get; set; }
    }
}