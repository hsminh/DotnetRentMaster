using Microsoft.AspNetCore.Mvc;
using RentMaster.Management.RealEstate.Services;
using RentMaster.Management.RealEstate.Models;
using RentMaster.Management.RealEstate.Types.Response;

namespace RentMaster.Management.RealEstate.Controllers.PublicController
{
    [ApiController]
    [Route("public/api")]
    public class PublicApartmentController : ControllerBase
    {
        private readonly ApartmentService _service;
        private readonly ApartmentRoomService _apartmentRoomService;


        public PublicApartmentController(ApartmentService service, ApartmentRoomService apartmentRoomService)
        {
            _service = service;
            _apartmentRoomService = apartmentRoomService;
        }

        [HttpGet("apartments")]
        public async Task<IActionResult> GetFullApartments()
        {
            var apartments = await _service.GetFullApartments();
            return Ok(apartments);
        }
        [HttpGet("apartment-rooms")]
        public async Task<IEnumerable<ApartmentRoom>> GetAllRooms()
        {
            return await _apartmentRoomService.GetAllRooms();
        }
    }
}