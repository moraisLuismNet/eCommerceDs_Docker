﻿using AutoMapper;
using eCommerceDs.DTOs;
using eCommerceDs.Models;
using eCommerceDs.Repository;
using Microsoft.EntityFrameworkCore;

public class CartRepository : ICartRepository
{
    private readonly eCommerceDsContext _context;
    private readonly IMapper _mapper;

    public CartRepository(eCommerceDsContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }


    public async Task<IEnumerable<Cart>> GetAllCartsCartRepository()
    {
        return await _context.Carts.ToListAsync();
    }


    public async Task<Cart> GetByIdCartRepository(int cartId)
    {
        return await _context.Carts
            .FirstOrDefaultAsync(c => c.IdCart == cartId);
    }


    public async Task<Cart> GetCartByEmailCartRepository(string userEmail)
    {
        return await _context.Carts
            .Include(c => c.CartDetails)
                .ThenInclude(cd => cd.Record)
            .FirstOrDefaultAsync(c => c.UserEmail == userEmail);
    }


    public async Task<IEnumerable<CartDTO>> GetDisabledCartsCartRepository()
    {
        return await _context.Carts
            .Where(c => c.Enabled == false)
            .Select(c => new CartDTO
            {
                IdCart = c.IdCart,
                UserEmail = c.UserEmail,
                TotalPrice = c.TotalPrice,
                Enabled = c.Enabled
            })
            .ToListAsync();
    }


    public async Task<Cart?> GetActiveCartByUserEmailCartRepository(string userEmail)
    {
        return await _context.Carts
            .FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.Enabled == true);
    }


    public async Task<IEnumerable<Cart>> GetCartsByUserEmailCartRepository(string userEmail)
    {
        return await _context.Carts
            .Where(c => c.UserEmail == userEmail)
            .ToListAsync();
    }


    public async Task<CartStatusDTO> GetCartStatusCartRepository(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException("Email cannot be null or empty");
        }

        var cart = await _context.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserEmail == email);

        return new CartStatusDTO
        {
            Enabled = cart?.Enabled ?? false // If it does not exist, it is considered disabled
        };
    }


    public async Task<Cart> AddCartCartRepository(CartDTO cartDTO)
    {
        if (cartDTO == null)
        {
            throw new ArgumentNullException(nameof(cartDTO));
        }

        var cart = _mapper.Map<Cart>(cartDTO);
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        return cart;
    }


    public async Task UpdateCartTotalPriceCartRepository(Cart cart)
    {
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync();
    }


    public async Task<CartDTO> EnableCartCartRepository(string userEmail)
    {
        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.UserEmail == userEmail && !c.Enabled);

        if (cart == null)
        {
            throw new InvalidOperationException("No disabled cart found for this user");
        }

        cart.Enabled = true;
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync();

        return new CartDTO
        {
            IdCart = cart.IdCart,
            UserEmail = cart.UserEmail,
            TotalPrice = cart.TotalPrice,
            Enabled = cart.Enabled
        };
    }


    public async Task UpdateCartDisabledCartCartRepository(Cart cart)
    {
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync();
    }


    public async Task DeleteCartCartRepository(Cart cart)
    {
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync();
    }

}
