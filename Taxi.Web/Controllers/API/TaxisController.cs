using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taxi.Web.Data;
using Taxi.Web.Data.Entities;
using Taxi.Web.Helpers;

namespace Taxi.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxisController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;

        public TaxisController(
            DataContext context,
            IConverterHelper converterHelper)
        {
            _context = context;
            _converterHelper = converterHelper;
        }

      
       

        // GET: api/Taxis/5
        [HttpGet("{plate}")]
        public async Task<ActionResult<TaxiEntity>> GetTaxiEntity(string plate)
        {
            plate = plate.ToUpper();
            var taxiEntity = await _context.Taxis
                .Include(t=>t.User) //Driver
                .Include(t=> t.Trips)
                .ThenInclude(t=>t.TripDetails)
                .Include(t=>t.Trips)
                .Include(t=>t.User) //Passenger
                .FirstOrDefaultAsync(t => t.Plate == plate);

            if (taxiEntity == null)
            {
                taxiEntity = new TaxiEntity { Plate= plate.ToUpper() };
                _context.Taxis.Add(taxiEntity);
                await _context.SaveChangesAsync();
            }

            return Ok( _converterHelper.ToTaxiResponse(taxiEntity));
        }

        
        private bool TaxiEntityExists(int id)
        {
            return _context.Taxis.Any(e => e.Id == id);
        }
    }
}
