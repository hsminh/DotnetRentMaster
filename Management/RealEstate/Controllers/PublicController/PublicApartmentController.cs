using Microsoft.AspNetCore.Mvc;
using RentMaster.Management.RealEstate.Services;
using RentMaster.Management.RealEstate.Types.Request;

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
        public async Task<IActionResult> GetFullApartments([FromQuery] ApartmentFilterRequest filter)
        {
            var apartments = await _service.GetFullApartments(filter);
            return Ok(apartments);
        }

        [HttpGet("apartment-rooms")]
        public async Task<IActionResult> GetAllRooms([FromQuery] RoomFilterRequest filter)
        {
            var rooms = await _apartmentRoomService.GetAllRooms(filter);
            return Ok(rooms);
        }
    }
}