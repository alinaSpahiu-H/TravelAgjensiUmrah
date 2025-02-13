﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Areas.Client.Models.ReservationViewModel;
using System.Security.Claims;
using TravelAgjensiUmrah.App.Constants;
using TravelAgjensiUmrah.App.Interfaces;
using TravelAgjensiUmrah.Data.Entities;

namespace Presentation.Areas.Client.Controllers
{
    [Area(AreasConstants.Client)]
    public class ReservationsController : Controller
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;

        // Konstruktori
        public ReservationsController(IPackageRepository packageRepository, IReservationRepository reservationRepository, IUserRepository userRepository, IUserService userService)
        {
            _packageRepository = packageRepository;
            _reservationRepository = reservationRepository;
            _userRepository = userRepository;
            _userService = userService;
        }

        // Index
        public IActionResult Index()
        {
            return View();
        }


        // Create Reservation
        [HttpGet]
        public IActionResult CreateReservation(int packageId)
        {
            var package = _packageRepository.GetPackageById(packageId);
            if (package == null)
            {
                // Handle the case where the package is not found
                return NotFound("Package not found.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // This gets the ID of the logged-in user
            var user = _userRepository.GetByStringId(userId);
            if (user == null)
            {
                // Handle the case where the user is not found
                return NotFound("User not found.");
            }

            var model = new ReservationViewModel
            {
                PackageId = packageId,
                UserId = user.Id,
                NumberOfPeople = 1,
                PackageName = package.PackageName,
                BookingDate = DateTime.Now,
                //   UserName = user.UserName,
                TotalPrice = package.TicketPrice + package.MealPrice + package.VisaPrice + package.IhramPrice + package.ZemzemPrice + package.TransportInArabiaPrice + package.Service,

                PaymentMethods = new SelectList(new List<SelectListItem>
        {
            new SelectListItem { Value = PaymentMethodConstants.Cash, Text = PaymentMethodConstants.Cash },
            new SelectListItem { Value = PaymentMethodConstants.Maestro, Text = PaymentMethodConstants.Maestro },
            new SelectListItem { Value = PaymentMethodConstants.Visa, Text = PaymentMethodConstants.Visa },
        }, "Value", "Text")
            };

            ViewData["ShowNavbar"] = true;

            return View(model);
        }


        //Process Payment
        [HttpPost]
        public async Task<IActionResult> ProcessReservation(ReservationViewModel? model)
        {
            if (ModelState.IsValid)
            {
                var reservation = new Reservation
                {
                    UserId = model!.UserId,
                    PackageId = model!.PackageId,
                    NumberOfPeople = model.NumberOfPeople,
                    BookingDate = model.BookingDate,
                    TotalPrice = model.TotalPrice * model.NumberOfPeople,
                    Status = model.Status,
                    AdditionalRequests = model.AdditionalRequests,
                    PaymentMethod = model.PaymentMethod

                    //// Map other necessary properties
                };

                _reservationRepository.AddReservation(reservation);
                _reservationRepository.Save();

                // Check if PackageId has a value before using it
                if (model.PackageId.HasValue)
                {
                    var package = _packageRepository.GetPackageById(model.PackageId.Value);
                    if (package != null && package.Pax > 0)
                    {
                        package.Pax -= 1; // Decrease pax by one
                        _packageRepository.Update(package); // Update the package
                        _packageRepository.SaveChanges(); // Save changes to the repository
                    }
                }

                return RedirectToAction("Confirmation", new { reservationId = reservation.Id });
            }

            ViewData["ShowNavbar"] = true;
            return View("CreateReservation", model);
        }


        public IActionResult Confirmation(int reservationId)
        {

            var reservation = _reservationRepository.GetReservationById(reservationId);
            if (reservation == null)
            {
                return NotFound();
            }

            var model = new ReservationViewModel
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                PackageId = reservation.PackageId,
                NumberOfPeople = reservation.NumberOfPeople,
                BookingDate = reservation.BookingDate,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status,
                UserName = _userService.GetJustName(),
                AdditionalRequests = reservation.AdditionalRequests,
            };

            ViewData["ShowNavbar"] = true;
            return View(model);
        }
    }
}
