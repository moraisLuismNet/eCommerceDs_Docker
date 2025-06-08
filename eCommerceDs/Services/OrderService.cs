﻿using AutoMapper;
using eCommerceDs.DTOs;
using eCommerceDs.Models;
using eCommerceDs.Repository;

namespace eCommerceDs.Services
{
    public class OrderService : IOrderService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartDetailRepository _cartDetailRepository;
        private readonly IOrderRepository _orderRepository;
        private IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public OrderService(
            ICartRepository cartRepository,
            ICartDetailRepository cartDetailRepository,
            IOrderRepository orderRepository,
            IMapper mapper,
            IUserRepository userRepository)
        {
            _cartRepository = cartRepository;
            _cartDetailRepository = cartDetailRepository;
            _mapper = mapper;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }


        public async Task<IEnumerable<OrderDTO>> GetOrdersOrderService()
        {
            var orders = await _orderRepository.GetOrdersOrderRepository();
            return _mapper.Map<IEnumerable<OrderDTO>>(orders); 
        }


        public async Task<IEnumerable<OrderDTO>> GetOrdersByUserEmailOrderService(string userEmail)
        {
            var orders = await _orderRepository.GetOrdersByUserEmailOrderRepository(userEmail);
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }


        public async Task<OrderDTO> CreateOrderFromCartOrderService(string userEmail, string paymentMethod)
        {
            // Validate and set default value if necessary
            string finalPaymentMethod = string.IsNullOrWhiteSpace(paymentMethod)
                ? "Credit Card"
                : paymentMethod;

            await ValidateUserAndCartOrderService(userEmail);

            var cart = await _cartRepository.GetActiveCartByUserEmailCartRepository(userEmail);

            if (cart == null)
            {
                throw new Exception("No active cart found for user");
            }

            if (cart.Enabled == false)
            {
                throw new Exception("Cart is already disabled");
            }

            var cartDetails = await _cartDetailRepository.GetCartDetailByCartIdCartDetailRepository(cart.IdCart);

            if (!cartDetails.Any())
            {
                throw new Exception("Cart is empty");
            }

            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                PaymentMethod = finalPaymentMethod,
                Total = cart.TotalPrice,
                UserEmail = userEmail,
                CartId = cart.IdCart,
                OrderDetails = cartDetails.Select(cd => new OrderDetail
                {
                    RecordId = cd.RecordId,
                    Amount = cd.Amount,
                    Price = cd.Price
                }).ToList()
            };

            cart.TotalPrice = 0;
            await _cartRepository.UpdateCartTotalPriceCartRepository(cart);

            await _cartDetailRepository.RemoveAllCartDetailsCartDetailRepository(cart.IdCart);

            var createdOrder = await _orderRepository.CreateOrderOrderRepository(order);

            return _mapper.Map<OrderDTO>(createdOrder);
        }


        public async Task<bool> ValidateUserAndCartOrderService(string userEmail)
        {
            var userExists = await _userRepository.UserExistsUserRepository(userEmail);
            if (!userExists)
            {
                throw new Exception($"User with email {userEmail} not found");
            }

            var activeCart = await _cartRepository.GetActiveCartByUserEmailCartRepository(userEmail);
            if (activeCart == null)
            {
                var allUserCarts = await _cartRepository.GetCartsByUserEmailCartRepository(userEmail);
                var message = allUserCarts.Any()
                    ? "User has carts but none are active (Enabled = true)"
                    : "User has no carts at all";
                throw new Exception(message);
            }

            return true;
        }

    }
}
