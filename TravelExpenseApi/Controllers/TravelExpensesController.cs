using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelExpenseApi.Models;
using TravelExpenseApi.Services;

namespace TravelExpenseApi.Controllers;

/// <summary>
/// 旅費申請API
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TravelExpensesController : ControllerBase
{
    private readonly TravelExpenseService _service;
    private readonly FraudDetectionService _fraudDetectionService;
    private readonly ILogger<TravelExpensesController> _logger;

    public TravelExpensesController(
        TravelExpenseService service, 
        FraudDetectionService fraudDetectionService,
        ILogger<TravelExpensesController> logger)
    {
        _service = service;
        _fraudDetectionService = fraudDetectionService;
        _logger = logger;
    }

    /// <summary>
    /// すべての旅費申請を取得
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TravelExpenseResponse>>> GetAllExpenses()
    {
        try
        {
            var expenses = await _service.GetAllExpensesAsync();
            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all expenses");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// IDで旅費申請を取得
    /// </summary>
    [HttpGet("{partitionKey}/{rowKey}")]
    public async Task<ActionResult<TravelExpenseResponse>> GetExpenseById(string partitionKey, string rowKey)
    {
        try
        {
            var expense = await _service.GetExpenseByIdAsync(partitionKey, rowKey);
            
            if (expense == null)
            {
                return NotFound();
            }

            return Ok(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get expense by id");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// 新規旅費申請を作成
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TravelExpenseResponse>> CreateExpense([FromBody] TravelExpenseRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var expense = await _service.CreateExpenseAsync(request);
            
            return CreatedAtAction(
                nameof(GetExpenseById), 
                new { partitionKey = expense.ApplicationDate.ToString("yyyy-MM"), rowKey = expense.Id }, 
                expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create expense");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// 旅費申請を更新
    /// </summary>
    [HttpPut("{partitionKey}/{rowKey}")]
    public async Task<ActionResult<TravelExpenseResponse>> UpdateExpense(
        string partitionKey, 
        string rowKey, 
        [FromBody] TravelExpenseRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var expense = await _service.UpdateExpenseAsync(partitionKey, rowKey, request);
            
            if (expense == null)
            {
                return NotFound();
            }

            return Ok(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update expense");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// 旅費申請のステータスを更新
    /// </summary>
    [HttpPatch("{partitionKey}/{rowKey}/status")]
    public async Task<ActionResult<TravelExpenseResponse>> UpdateStatus(
        string partitionKey, 
        string rowKey, 
        [FromBody] string status)
    {
        try
        {
            var expense = await _service.UpdateStatusAsync(partitionKey, rowKey, status);
            
            if (expense == null)
            {
                return NotFound();
            }

            return Ok(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update status");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// 旅費申請を削除
    /// </summary>
    [HttpDelete("{partitionKey}/{rowKey}")]
    public async Task<IActionResult> DeleteExpense(string partitionKey, string rowKey)
    {
        try
        {
            var result = await _service.DeleteExpenseAsync(partitionKey, rowKey);
            
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete expense");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// サマリー情報を取得
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<TravelExpenseSummary>> GetSummary()
    {
        try
        {
            var summary = await _service.GetSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get summary");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// 不正検知チェックを実行
    /// </summary>
    [HttpPost("{partitionKey}/{rowKey}/fraud-check")]
    public async Task<ActionResult<TravelExpenseResponse>> RunFraudCheck(string partitionKey, string rowKey)
    {
        try
        {
            _logger.LogInformation("Running fraud check for expense: {PartitionKey}/{RowKey}", partitionKey, rowKey);

            // 不正検知を実行
            var fraudCheckResult = await _fraudDetectionService.CheckExpenseAsync(partitionKey, rowKey);

            // 結果をデータベースに保存
            var expense = await _service.GetExpenseByIdAsync(partitionKey, rowKey);
            if (expense == null)
            {
                return NotFound();
            }

            // TravelExpenseServiceに不正検知結果を更新するメソッドを呼び出す
            var updatedExpense = await _service.UpdateFraudCheckResultAsync(
                partitionKey, 
                rowKey, 
                fraudCheckResult.Result, 
                fraudCheckResult.Details);

            if (updatedExpense == null)
            {
                return NotFound();
            }

            return Ok(updatedExpense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run fraud check");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
