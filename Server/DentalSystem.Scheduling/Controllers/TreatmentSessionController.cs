﻿namespace DentalSystem.Dealers.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DentalSystem.Controllers;
    using DentalSystem.Services.Identity;
    using Microsoft.AspNetCore.Mvc;
    using DentalSystem.Scheduling.Services;
    using DentalSystem.Scheduling.Models;
    using System;
    using DentalSystem.Scheduling.Data.Models;
    using DentalSystem.Scheduling.Data.ValueObjects;
    using DentalSystem.Infrastructure;

    public class TreatmentSessionController : ApiController
    {
        private readonly ITreatmentService _treatmentService;
        private readonly ITreatmentSessionService _treatmentSessions;
        private readonly IDentalTeamService _dentalTeams;
        private readonly IPatientService _patients;

        public TreatmentSessionController(
            ITreatmentService treatmentService,
            ITreatmentSessionService treatmentSessions,
            IDentalTeamService dentalTeams,
            IPatientService patients,
            ICurrentUserService currentUser)
        {
            _treatmentService = treatmentService;
            _treatmentSessions = treatmentSessions;
            _dentalTeams = dentalTeams;
            _patients = patients;
        }

        [HttpPost("/treatmentSession/request")]
        [AuthorizePatient]
        public async Task<IActionResult> MakeRequest(
            [FromQuery] RequestTreatmentSessionInput input)
        {
            var dentalTeam = await _dentalTeams.FindByReferenceId(input.DentalTeamReferenceId.Value);
            var patient = await _patients.FindByReferenceId(input.PatientReferenceId.Value);
            var treatment = await _treatmentService.FindByReferenceId(input.TreatmentReferenceId.Value);

            var treatmentSession = new TreatmentSession()
            {
                DentalTeamId = dentalTeam?.Id ?? Guid.Empty,
                PatientId = patient?.Id ?? Guid.Empty,
                TreatmentId = treatment?.Id ?? Guid.Empty,
                Status = TreatmentSessionStatus.Requested,
                Period = new Period{ Start = input.Start.Value, End = input.End.Value }
            };

            _treatmentSessions.Add(treatmentSession);

            await _treatmentSessions.Save();

            return Ok();
        }

        [HttpGet("/treatmentSession/patient")]
        [AuthorizePatient]
        public async Task<IEnumerable<PatientTreatmentSessionsOutputModel>> Patient(
            Guid patientId,
            [FromQuery] PatientTreatmentSessionsQuery query)
        {
            var result = await _treatmentSessions.GetPatientTreatmentSessions(patientId, query);

            return result;
        }
    }
}