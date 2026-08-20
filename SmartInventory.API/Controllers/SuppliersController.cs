using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.DTOs.Supplier;
using SmartInventory.Application.Services;
using SmartInventory.Domain.Entities;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly IMapper _mapper;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(ISupplierService supplierService, IMapper mapper, ILogger<SuppliersController> logger)
    {
        _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all suppliers with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>List of suppliers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SupplierDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync(pageNumber, pageSize);
            var supplierDtos = _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
            return Ok(supplierDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving suppliers" });
        }
    }

    /// <summary>
    /// Get supplier by ID
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>Supplier details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierDto>> GetById(int id)
    {
        try
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            if (supplier == null)
            {
                return NotFound(new { message = $"Supplier with ID {id} not found" });
            }

            var supplierDto = _mapper.Map<SupplierDto>(supplier);
            return Ok(supplierDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving the supplier" });
        }
    }

    /// <summary>
    /// Create a new supplier
    /// </summary>
    /// <param name="createDto">Supplier details</param>
    /// <returns>Created supplier</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupplierDto>> Create([FromBody] CreateSupplierDto createDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createDto.Name) || string.IsNullOrWhiteSpace(createDto.Email))
            {
                return BadRequest(new { message = "Supplier name and email are required" });
            }

            var supplier = _mapper.Map<Supplier>(createDto);
            var createdSupplier = await _supplierService.CreateSupplierAsync(supplier);
            var supplierDto = _mapper.Map<SupplierDto>(createdSupplier);

            _logger.LogInformation("Supplier created with ID {Id}", createdSupplier.Id);
            return CreatedAtAction(nameof(GetById), new { id = supplierDto.Id }, supplierDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while creating the supplier" });
        }
    }

    /// <summary>
    /// Update an existing supplier
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <param name="updateDto">Updated supplier details</param>
    /// <returns>No content</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto updateDto)
    {
        try
        {
            var existingSupplier = await _supplierService.GetSupplierByIdAsync(id);
            if (existingSupplier == null)
            {
                return NotFound(new { message = $"Supplier with ID {id} not found" });
            }

            _mapper.Map(updateDto, existingSupplier);
            await _supplierService.UpdateSupplierAsync(existingSupplier);

            _logger.LogInformation("Supplier with ID {Id} updated", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while updating the supplier" });
        }
    }

    /// <summary>
    /// Delete a supplier
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            if (supplier == null)
            {
                return NotFound(new { message = $"Supplier with ID {id} not found" });
            }

            await _supplierService.DeleteSupplierAsync(id);
            _logger.LogInformation("Supplier with ID {Id} deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while deleting the supplier" });
        }
    }

    /// <summary>
    /// Get supplier by name
    /// </summary>
    /// <param name="name">Supplier name</param>
    /// <returns>Supplier details</returns>
    [HttpGet("search/{name}")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierDto>> GetByName(string name)
    {
        try
        {
            var supplier = await _supplierService.GetSupplierByNameAsync(name);
            if (supplier == null)
            {
                return NotFound(new { message = $"Supplier with name '{name}' not found" });
            }

            var supplierDto = _mapper.Map<SupplierDto>(supplier);
            return Ok(supplierDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier by name {Name}", name);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving the supplier" });
        }
    }
}
