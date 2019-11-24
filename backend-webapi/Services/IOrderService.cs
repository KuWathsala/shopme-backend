﻿using System.Collections.Generic;
using webapi.Dtos;
using webapi.Entities;
using webapi.ViewModels;

namespace webapi.Services
{
    public interface IOrderService
    {
        OrderDto CreateNewOrder(OrderVM orderVM);
        List<OrderDetails> GetAllOrderDetailsByCustomer(int customerId);
        IEnumerable<OrderDto> GetAllOrdersByCustomer(int customerId);
        // List<OrderDetails> GetAllOrderDetailsByDeliverer(int delivererId);
        object GetOrderDetailsById(int orderId);
        object GetOrdersNearByDeliverers(double latitude, double longitude);
        List<ProductDto> GetProductsByOrder(Order order);
        List<OrderDetails> GetWaitingOrderDetailsBySeller(int sellerId);
        void UpdateOrderStatus(int id, string status);
        bool DeleteOrder(int orderId);
        void Rate(int id, double sellerRate, double delivererRate);
        List<OrderDeliveryDetails> GetAllOrderDetailsByDeliverer(int delivererId);
        void UpdateOrderDeliverer(int orderId, int delivererId);
        bool UpdatePayment(int orderId, int status);
    }
}