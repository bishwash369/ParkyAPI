using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyAPI.Models;
using ParkyAPI.Models.Dtos;
using ParkyAPI.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NationalParksController : ControllerBase
    {
        private INationalParkRepository _npRepo;           //using repositories for Http methods
        private readonly IMapper _mapper;               //using mapping to access repositories

        public NationalParksController(INationalParkRepository npRepo, IMapper mapper)
        {
            _npRepo = npRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetNationalParks()
        {
            var objList = _npRepo.GetNationalParks();

            var objDto = new List<NationalParkDto>();   //fetching data from DTO instrad of main models

            /*  foreach (var obj in objList)                //objList fetches list from repository whereas objDto fetches list from our Dto
              {
                  objDto.Add(_mapper.Map<NationalParkDto>(obj));     //mapping object from NationalParkDto [where obj is the source of the object]
              }*/
            objDto.AddRange(_mapper.Map<List<NationalParkDto>>(objDto));
            return Ok(objDto);
        }
    

        [HttpGet("{nationalParkId:int}", Name = "GetNationalPark")]
        public IActionResult GetNationalPark(int nationalParkId)
        {
            var obj = _npRepo.GetNationalPark(nationalParkId);
            if(obj==null)
            {
                return NotFound();
            }
            var objDto = _mapper.Map<NationalParkDto>(obj);
            return Ok(objDto);

        }
    
        [HttpPost]
        public IActionResult CreateNationalPark([FromBody] NationalParkDto nationalParkDto)
        {
            if (nationalParkDto == null)                    //if null then 400 badrequest i.e. client error(invalid syntaz/request/message, etc)
            {
                return BadRequest(ModelState);              //Modelstate stores validation errors and returns them if error is encountered
            }

            if(_npRepo.NationalParkExists(nationalParkDto.Name))
            {
                ModelState.AddModelError("", "National Park Exists!");          //returns if an existing item is posted again
                return StatusCode(404, ModelState);
            }
             
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nationalParkObj = _mapper.Map<NationalPark>(nationalParkDto);

            if (!_npRepo.CreateNationalPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Something went wrong when saving the record {nationalParkObj.Name}");
                return StatusCode(500, ModelState);             //statuscode 500 is returns internal server error
            }

            return CreatedAtRoute("GetNationalPark", new { nationalParkId = nationalParkObj.Id }, nationalParkObj);                   //creates and returns by Id after routing
        }

        [HttpPatch("{nationalParkId:int}", Name = "UpdateNationalPark")]
        public IActionResult UpdateNationalPark(int nationalParkId, [FromBody] NationalParkDto nationalParkDto)
        {
            if (nationalParkDto == null || nationalParkId!=nationalParkDto.Id)                    
            {
                return BadRequest(ModelState);              
            }

            var nationalParkObj = _mapper.Map<NationalPark>(nationalParkDto);
            if (!_npRepo.UpdateNationalPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Something went wrong when updating the record {nationalParkObj.Name}");
                return StatusCode(500, ModelState);             //statuscode 500 is returns internal server error
            }

            return NoContent();

        }

        [HttpDelete("{nationalParkId:int}", Name = "UpdateNationalPark")]
        public IActionResult DeleteNationalPark(int nationalParkId)
        {
            if(!_npRepo.NationalParkExists(nationalParkId))
            {
                return NotFound();
            }

            var nationalParkObj = _npRepo.GetNationalPark(nationalParkId);
            if (!_npRepo.DeleteNationalPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Something went wrong when deleting the record {nationalParkObj.Name}");
                return StatusCode(500, ModelState);             //statuscode 500 is returns internal server error
            }

            return NoContent();

        }
    } 

}
