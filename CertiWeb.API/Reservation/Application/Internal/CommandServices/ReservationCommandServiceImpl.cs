using CertiWeb.API.Reservation.Domain.Model.Aggregates;
using CertiWeb.API.Reservation.Domain.Model.Commands;
using CertiWeb.API.Reservation.Domain.Repositories;
using CertiWeb.API.Reservation.Domain.Services;
using CertiWeb.API.Shared.Domain.Repositories;
using CertiWeb.API.Certifications.Domain.Services;
using CertiWeb.API.Certifications.Domain.Model.Commands;
using ReservationEntity = CertiWeb.API.Reservation.Domain.Model.Aggregates.Reservation;

namespace CertiWeb.API.Reservation.Application.Internal.CommandServices;

/// <summary>
/// Implementation of the reservation command service.
/// </summary>
public class ReservationCommandServiceImpl : IReservationCommandService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICarCommandService _carCommandService;

    /// <summary>
    /// Initializes a new instance of the ReservationCommandServiceImpl class.
    /// </summary>
    /// <param name="reservationRepository">The reservation repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="carCommandService">The car command service for creating cars from accepted reservations.</param>
    public ReservationCommandServiceImpl(IReservationRepository reservationRepository, IUnitOfWork unitOfWork, ICarCommandService carCommandService)
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _carCommandService = carCommandService;
    }

    /// <summary>
    /// Creates a new reservation in the system.
    /// </summary>
    /// <param name="command">The command containing the reservation data.</param>
    /// <returns>The created reservation.</returns>
    public async Task<ReservationEntity?> Handle(CreateReservationCommand command)
    {
        // Clean and validate license plate format
        var cleanLicensePlate = command.LicensePlate?.Replace("-", "").ToUpper() ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(cleanLicensePlate) || 
            cleanLicensePlate.Length > 6 || 
            !cleanLicensePlate.All(c => char.IsLetterOrDigit(c)))
        {
            throw new ArgumentException("License plate must be max 6 characters with letters and numbers only (format: XXX-XXX or XXXXXX).");
        }

        // Check if license plate already has a reservation for the same date/time
        // Use the clean license plate for the check
        var existingReservation = await _reservationRepository
            .ExistsReservationForLicensePlateAndDateTimeAsync(cleanLicensePlate, command.InspectionDateTime);
        
        if (existingReservation)
        {
            throw new InvalidOperationException("A reservation already exists for this license plate at the specified date and time.");
        }

        

        var reservation = new ReservationEntity(command);
        
        try
        {
            await _reservationRepository.AddAsync(reservation);
            await _unitOfWork.CompleteAsync();
            return reservation;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Updates the status of an existing reservation.
    /// </summary>
    /// <param name="command">The command containing the reservation ID and new status.</param>
    /// <returns>The updated reservation.</returns>
    public async Task<ReservationEntity?> Handle(UpdateReservationStatusCommand command)
    {
        var reservation = await _reservationRepository.FindByIdAsync(command.ReservationId);
        if (reservation == null) return null;

        // Validate status values
        var validStatuses = new[] { "pending", "accepted", "rejected" };
        if (!validStatuses.Contains(command.Status.ToLower()))
        {
            throw new ArgumentException("Status must be one of: pending, accepted, rejected.");
        }

        var previousStatus = reservation.Status;
        reservation.Status = command.Status.ToLower();
        
        try
        {
            _reservationRepository.Update(reservation);
            await _unitOfWork.CompleteAsync();
            
            // If status changed to "accepted", automatically create a car from this reservation
            if (previousStatus != "accepted" && reservation.Status == "accepted")
            {
                try
                {
                    Console.WriteLine($"Auto-creating car from accepted reservation ID {reservation.Id}");
                    
                    // Extract price as decimal (removing "SOL" if present)
                    decimal price = 100;
                    if (decimal.TryParse(reservation.Price?.Replace("SOL", "").Trim(), out var parsedPrice))
                    {
                        price = parsedPrice;
                    }
                    
                    var createCarCommand = new CreateCarCommand(
                        Title: $"{reservation.Brand} {reservation.Model} Certification",
                        Owner: reservation.ReservationName,
                        OwnerEmail: reservation.ReservationEmail,
                        Year: DateTime.Now.Year,
                        BrandId: 1, 
                        Model: reservation.Model,
                        Description: $"Auto-created from reservation {reservation.Id}",
                        PdfCertification: null,
                        ImageUrl: reservation.ImageUrl,
                        Price: price,
                        LicensePlate: reservation.LicensePlate,
                        OriginalReservationId: reservation.Id
                    );
                    
                    var createdCar = await _carCommandService.Handle(createCarCommand);
                    Console.WriteLine($"Car created successfully with ID: {createdCar?.Id ?? 0}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to auto-create car from reservation {reservation.Id}: {ex.Message}");
                }
            }
            
            return reservation;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Handles the deletion of a reservation.
    /// </summary>
    /// <param name="command">The command containing the reservation ID to delete.</param>
    /// <returns>True if the reservation was deleted successfully, false otherwise.</returns>
    public async Task<bool> Handle(DeleteReservationCommand command)
    {
        try
        {
            var reservation = await _reservationRepository.FindByIdAsync(command.ReservationId);
            if (reservation == null)
            {
                return false;
            }

            _reservationRepository.Remove(reservation);
            await _unitOfWork.CompleteAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting reservation with ID {command.ReservationId}: {ex.Message}");
            return false;
        }
    }
}