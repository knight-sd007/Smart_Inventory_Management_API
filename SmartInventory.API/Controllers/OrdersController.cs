using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.DTOs.Order;
using SmartInventory.Application.Services;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, IProductService productService, IMapper mapper, ILogger<OrdersController> logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all orders with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>List of orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync(pageNumber, pageSize);
            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
            return Ok(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving orders" });
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found" });
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            return Ok(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving the order" });
        }
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="createDto">Order details</param>
    /// <returns>Created order</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto createDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createDto.DeliveryAddress) || createDto.OrderItems.Count == 0)
            {
                return BadRequest(new { message = "Delivery address and at least one order item are required" });
            }

            // Create order
            var order = new Order
            {
                OrderNumber = $"ORD-{DateTime.UtcNow.Ticks}",
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                DeliveryAddress = createDto.DeliveryAddress,
                Notes = createDto.Notes,
                TotalAmount = 0,
                OrderItems = []
            };

            // Add order items
            decimal totalAmount = 0;
            foreach (var itemDto in createDto.OrderItems)
            {
                var product = await _productService.GetProductByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    return BadRequest(new { message = $"Product with ID {itemDto.ProductId} not found" });
                }

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * itemDto.Quantity
                };

                order.OrderItems.Add(orderItem);
                totalAmount += orderItem.TotalPrice;
            }

            order.TotalAmount = totalAmount;
            var createdOrder = await _orderService.CreateOrderAsync(order);
            var orderDto = _mapper.Map<OrderDto>(createdOrder);

            _logger.LogInformation("Order created with ID {Id}", createdOrder.Id);
            return CreatedAtAction(nameof(GetById), new { id = orderDto.Id }, orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while creating the order" });
        }
    }

    /// <summary>
    /// Update an existing order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="updateDto">Updated order details</param>
    /// <returns>No content</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto updateDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(updateDto.Status) || !Enum.TryParse<OrderStatus>(updateDto.Status, true, out var newStatus))
            {
                return BadRequest(new { message = $"Invalid order status '{updateDto.Status}'. Valid values are: Pending, Confirmed, Shipped, Delivered, Cancelled" });
            }

            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found" });
            }

            existingOrder.Status = newStatus;
            existingOrder.DeliveryAddress = updateDto.DeliveryAddress;
            existingOrder.Notes = updateDto.Notes;

            await _orderService.UpdateOrderAsync(existingOrder);
            _logger.LogInformation("Order with ID {Id} updated", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while updating the order" });
        }
    }

    /// <summary>
    /// Delete an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found" });
            }

            await _orderService.DeleteOrderAsync(id);
            _logger.LogInformation("Order with ID {Id} deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while deleting the order" });
        }
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <returns>Order details</returns>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetByOrderNumber(string orderNumber)
    {
        try
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            if (order == null)
            {
                return NotFound(new { message = $"Order with number '{orderNumber}' not found" });
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            return Ok(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order by number {OrderNumber}", orderNumber);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving the order" });
        }
    }

    /// <summary>
    /// Get orders by status
    /// </summary>
    /// <param name="status">Order status (Pending, Processing, Completed, Cancelled)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>List of orders with specified status</returns>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByStatus(string status, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                return BadRequest(new { message = "Invalid order status. Valid values are: Pending, Processing, Completed, Cancelled" });
            }

            var orders = await _orderService.GetOrdersByStatusAsync(orderStatus, pageNumber, pageSize);
            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
            return Ok(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders by status {Status}", status);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving orders" });
        }
    }

    /// <summary>
    /// Get recent orders
    /// </summary>
    /// <param name="days">Number of days to look back (default: 30)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>List of recent orders</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetRecent(int days = 30, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var orders = await _orderService.GetRecentOrdersAsync(days, pageNumber, pageSize);
            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
            return Ok(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent orders");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving orders" });
        }
    }
}
