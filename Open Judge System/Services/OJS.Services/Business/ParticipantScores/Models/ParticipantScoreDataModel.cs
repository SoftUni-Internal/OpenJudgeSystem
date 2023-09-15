using System;
using System.Linq;
using System.Linq.Expressions;
using OJS.Data.Models;

namespace OJS.Services.Business.ParticipantScores.Models
{
    public class ParticipantScoreDataModel
    {
        public Participant Participant { get; set; }

        public bool IsOfficial { get; set; }

        public string UserName { get; set; }

        public static Expression<Func<Participant, ParticipantScoreDataModel>> FromParticipant =>
            participant => new ParticipantScoreDataModel
            {
                Participant = participant,
                IsOfficial = participant.IsOfficial,
                UserName = participant.User.UserName,
            };
    }
}