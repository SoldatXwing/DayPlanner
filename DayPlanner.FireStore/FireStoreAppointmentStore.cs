using AutoMapper;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.Backend.Extensions;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;
using DayPlanner.FireStore.Models;
using Google.Cloud.Firestore;

namespace DayPlanner.FireStore;

public class FireStoreAppointmentStore(FirestoreDb db, IMapper mapper) : IAppointmentStore
{
    private readonly FirestoreDb _fireStoreDb = db;
    private readonly IMapper _mapper = mapper;

    private const string _collectionName = "appointments";

    public async Task<IEnumerable<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end)
    {
        Query query = _fireStoreDb.Collection(_collectionName)
            .WhereEqualTo("userId", userId)
            .WhereGreaterThanOrEqualTo("startTime", start)
            .WhereLessThanOrEqualTo("endTime", end);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        IEnumerable<FirestoreAppointment> appointments = snapshot.Documents.Select(doc => doc.ConvertTo<FirestoreAppointment>());
        ThrowIfUnauthorized(userId, appointments);

        return appointments.Select(_mapper.Map<Appointment>);
    }

    public async Task<IEnumerable<Appointment>> GetUsersAppointments(string userId, int page, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 0, nameof(page));
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1, nameof(pageSize));

        Query query = _fireStoreDb.Collection(_collectionName)
            .WhereEqualTo("userId", userId)
            .Offset(page * pageSize)
            .Limit(pageSize);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        IEnumerable<FirestoreAppointment> appointments = snapshot.Documents.Select(doc => doc.ConvertTo<FirestoreAppointment>());
        ThrowIfUnauthorized(userId, appointments);

        return appointments.Select(_mapper.Map<Appointment>);
    }

    public async Task<Appointment> CreateAppointment(string userId, AppointmentRequest request)
    {
        Appointment appointment = _mapper.Map<Appointment>(request);
        appointment.UserId = userId;

        DocumentReference appointmentRef = _fireStoreDb.Collection(_collectionName).Document();
        await appointmentRef.SetAsync(appointment.ToDictionary());

        appointment.Id = appointmentRef.Id;
        return appointment;
    }

    public async Task DeleteAppointment(string userId, string appointmentId)
    {
        DocumentReference appointmentRef = _fireStoreDb.Collection(_collectionName)
            .Document(appointmentId);

        DocumentSnapshot snapshot = await appointmentRef.GetSnapshotAsync();
        if (!snapshot.Exists)
        {
            throw new InvalidOperationException($"Appointment with id {appointmentId} not found.");
        }
        var appointment = snapshot.ConvertTo<FirestoreAppointment>();

        ThrowIfUnauthorized(userId, [appointment]);
        await appointmentRef.DeleteAsync();
    }

    public async Task<Appointment?> GetAppointmentById(string userId, string appointmentId)
    {
        DocumentReference appointmentRef = _fireStoreDb.Collection(_collectionName).Document(appointmentId);
        DocumentSnapshot snapshot = await appointmentRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            var appointment = snapshot.ConvertTo<FirestoreAppointment>();
            ThrowIfUnauthorized(userId, [appointment]);

            return _mapper.Map<Appointment>(appointment);
        }
        else
        {
            return null;
        }
    }

    public async Task<long?> GetAppointmentsCount(string userId)
    {
        var query = _fireStoreDb.Collection(_collectionName)
         .WhereEqualTo("userId", userId)
         .Count();

        var snapshot = await query.GetSnapshotAsync();
        //TODO: Auth check
        return snapshot.Count;
    }

    public async Task<Appointment> UpdateAppointment(string appointmentId, string userId, AppointmentRequest request)
    {
        DocumentReference appointmentRef = _fireStoreDb.Collection(_collectionName).Document(appointmentId);
        DocumentSnapshot existingSnapshot = await appointmentRef.GetSnapshotAsync();
        if (!existingSnapshot.Exists)
        {
            throw new ArgumentException($"No appointment with id {appointmentId} found.", nameof(appointmentId));
        }

        var existingAppointment = existingSnapshot.ConvertTo<FirestoreAppointment>();
        ThrowIfUnauthorized(userId, [existingAppointment]);

        var updatedAppointment = _mapper.Map<FirestoreAppointment>(request);
        updatedAppointment.Id = appointmentId; // Ensure the ID remains consistent
        updatedAppointment.UserId = userId;    // Maintain ownership

        await appointmentRef.SetAsync(updatedAppointment);

        var updatedSnapshot = await appointmentRef.GetSnapshotAsync();
        if (!updatedSnapshot.Exists)
        {
            throw new Exception("Failed to retrieve the updated appointment.");
        }

        return _mapper.Map<Appointment>(updatedSnapshot.ConvertTo<FirestoreAppointment>());
    }

    public async Task<Appointment> ImportAppointment(string userId, string externalId, AppointmentRequest request)
    {
        var appointment = _mapper.Map<Appointment>(request);

        appointment.UserId = userId;
        appointment.Id = externalId;
        DocumentReference appointmentRef = _fireStoreDb.Collection(_collectionName).Document(externalId);
        await appointmentRef.SetAsync(appointment.ToDictionary());

        return appointment;
    }

    private static void ThrowIfUnauthorized(string givenUserId, IEnumerable<FirestoreAppointment> dbAppointments)
    {
        if (dbAppointments.Any(c => c.UserId != givenUserId))
        {
            throw new UnauthorizedAccessException("User is not authorized to access this appointment.");
        }
    }
}