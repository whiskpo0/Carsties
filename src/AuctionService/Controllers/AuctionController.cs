using System.IO.Compression;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

    public AuctionController(AuctionDbContext context, IMapper mapper)
    {
            _mapper = mapper;
            _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync(); 

        return _mapper.Map<List<AuctionDto>>(auctions); 
    }

    [HttpGet("{Id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid Id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == Id); 

        if (auction == null)
        {
            return NotFound(); 
        }
        return _mapper.Map<AuctionDto>(auction); 
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction  = _mapper.Map<Auction>(auctionDto); 
        //TODO Add Current User as seller
        auction.Seller = "test"; 

        _context.Auctions.Add(auction); 

        var result = await _context.SaveChangesAsync() > 0; 

        if (!result)
        {
            return BadRequest("Could not save changes to the DB"); 
        }

        return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, _mapper.Map<AuctionDto>(auction)); 
    }

    [HttpPut("{Id}")]
    public async Task<ActionResult> UpdateAuction(Guid Id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == Id); 

        if (auction == null)
        {
            return NotFound(); 
        }
        // TODO: Check seller == username
        
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0; 

        if (result)
        {
            return Ok();
        }
       
        return BadRequest("Problem saving changes"); 
    }

    [HttpDelete("{Id}")]
    public async Task<ActionResult> DeleteAuction(Guid Id)
    {
        var auction = await _context.Auctions.FindAsync(Id); 

        if (auction == null)
        {
            return NotFound(); 
        }

        //TODO: check seller == username 

        _context.Auctions.Remove(auction); 

        var result = await _context.SaveChangesAsync() > 0; 

        if(!result) return BadRequest("Could not update Db"); 

        return Ok(); 
    }

}
