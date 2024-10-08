﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YTCG_Deck_Builder_API.Data;
using YTCG_Deck_Builder_API.Models.Dto;
using YTCG_Deck_Builder_API.Models.Entitities;

namespace YTCG_Deck_Builder_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeckController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<DeckController> _logger;
        public DeckController(DataContext dataContext, ILogger<DeckController> logger, UserManager<User> userManager)
        {
            _dataContext = dataContext;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> getAllDecksFromUser([FromQuery] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }



            var decks = _dataContext.Decks.Where(d => d.UserId.Equals(user.Id)).Select(d => new Deck() { Id = d.Id, Name = d.Name, Cards = d.Cards.Select(c => new Card() { Id = c.Id, Name = c.Name }).ToList() }).ToList();

            return Ok(decks);
        }


        [HttpGet("deckId")]
        public IActionResult getDeckById([FromQuery] int deckId)
        {
            var deck = _dataContext.Decks.Where(d => d.Id == deckId).Select(d => new Deck()
            {
                Id = d.Id,
                Name = d.Name,
                Cards = d.Cards.Select(c => new Card()
                {
                    UrlId = c.UrlId,
                    Name = c.Name,
                    Atk = c.Atk,
                    Def = c.Def,
                    Desc = c.Desc,
                    Attribute = c.Attribute,
                    FrameType = c.FrameType,
                    ImageUrl = c.ImageUrl,
                    Level = c.Level,
                    Race = c.Race,
                    ShopUrl = c.ShopUrl,
                    Type = c.Type
                }).ToList()
            }).FirstOrDefault();

            if (deck == null)
            {
                return BadRequest(ModelState);
            }
            return Ok(deck);
        }



        [HttpPost]
        public async Task<IActionResult> createDeckProfile([FromBody] DeckCreateDto deckCreateDto)
        {
            var user = await _userManager.FindByIdAsync(deckCreateDto.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var deck = new Deck()
            {
                Name = deckCreateDto.Name,
                Cards = new List<Card>().ToList(),
                UserId = user.Id,
            };

            _dataContext.Decks.Add(deck);
            _dataContext.SaveChanges();

            var deckResponseDto = new Deck()
            {
                Id = deck.Id,
                Name = deck.Name,
                UserId = deck.UserId
            };

            return Ok(deckResponseDto);
        }

        [HttpDelete]
        public async Task<IActionResult> removeDeck(string userId, int deckId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var deck = _dataContext.Decks.Include(d => d.Cards).Where(d => d.Id == deckId).FirstOrDefault();

            if (deck == null)
            {
                return NotFound();
            }


            if (deck.Cards != null)
            {
                _dataContext.Cards.RemoveRange(deck.Cards);
            }

            _dataContext.Remove(deck);
            _dataContext.SaveChanges();

            return Ok("Deck removed successfully");
        }
    }
}
