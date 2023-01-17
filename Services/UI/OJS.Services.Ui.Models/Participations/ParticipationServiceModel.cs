﻿namespace OJS.Services.Ui.Models.Participations
{
    using System;

    public class ParticipationServiceModel
    {
        public int Id { get; set; }
        public int ContestId { get; set; }

        public string ContestName { get; set; } = null!;

        public int? CompeteResult { get; set; }

        public int? PracticeResult { get; set; }

        public int? ContestCompeteMaximumPoints { get; set; }

        public int? ContestPracticeMaximumPoints { get; set; }

        public DateTime RegistrationTime { get; set; }
    }
}