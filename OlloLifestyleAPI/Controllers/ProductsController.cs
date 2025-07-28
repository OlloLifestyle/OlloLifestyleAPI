using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OlloLifestyleAPI.Application.Services;
using OlloLifestyleAPI.Core.DTOs;

namespace OlloLifestyleAPI.Controllers;

[Authorize]
public class ProductsController : BaseController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all products for the current tenant
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of products</returns>
    [HttpGet]
    [Authorize(Policy = "products.view")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productService.GetAllAsync(cancellationToken);
            return Ok(products);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "products.view")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id, cancellationToken);
            if (product == null) return NotFound();
            return Ok(product);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="createDto">Product creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [Authorize(Policy = "products.create")]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductDto createDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="updateDto">Product update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "products.edit")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        int id,
        [FromBody] UpdateProductDto updateDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.UpdateAsync(id, updateDto, cancellationToken);
            if (product == null) return NotFound();
            return Ok(product);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "products.delete")]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _productService.DeleteAsync(id, cancellationToken);
            return HandleResult(result, r => r);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}