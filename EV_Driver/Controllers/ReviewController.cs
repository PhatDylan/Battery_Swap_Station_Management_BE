using System.ComponentModel.DataAnnotations;
using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController(IReviewService reviewService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ResponseObject<List<ReviewResponse>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var reviews = await reviewService.GetAllReviewAsync(page, pageSize, search);
            return Ok(new ResponseObject<List<ReviewResponse>>
            {
                Message = "Get reviews successfully",
                Code = "200",
                Success = true
            }.UnwrapPagination(reviews));
        }

        [HttpGet("detail")]
        public async Task<ActionResult<ResponseObject<ReviewResponse>>> GetReviewDetail()
        {
            var review = await reviewService.GetReviewDetailAsync();
            return Ok(new ResponseObject<ReviewResponse>
            {
                Message = "Get review detail successfully",
                Code = "200",
                Success = true,
                Content = review
            });
        }

        [HttpGet("station/{stationId}")]
        public async Task<ActionResult<ResponseObject<ReviewResponse>>> GetByStation(string stationId)
        {
            var review = await reviewService.GetByStationAsync(stationId);
            return Ok(new ResponseObject<ReviewResponse>
            {
                Message = "Get review by station successfully",
                Code = "200",
                Success = true,
                Content = review
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ResponseObject<ReviewResponse>>> GetByUser(string userId)
        {
            var review = await reviewService.GetByUserAsync(userId);
            return Ok(new ResponseObject<ReviewResponse>
            {
                Message = "Get review by user successfully",
                Code = "200",
                Success = true,
                Content = review
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseObject<ReviewResponse>>> GetById(string id)
        {
            var review = await reviewService.GetByIdAsync(id);
            if (review == null)
            {
                return Ok(new ResponseObject<ReviewResponse>
                {
                    Message = "Review not found",
                    Code = "404",
                    Success = false,
                    Content = null
                });
            }

            return Ok(new ResponseObject<ReviewResponse>
            {
                Message = "Get review successfully",
                Code = "200",
                Success = true,
                Content = review
            });
        }

        [HttpPost]
        public async Task<ActionResult<ResponseObject<object>>> Add([FromBody] ReviewRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await reviewService.AddAsync(request);
            return Ok(new ResponseObject<object>
            {
                Message = "Review created successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }

        [HttpPut]
        public async Task<ActionResult<ResponseObject<object>>> Update([FromBody] ReviewRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await reviewService.UpdateAsync(request);
            return Ok(new ResponseObject<object>
            {
                Message = "Review updated successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseObject<object>>> Delete(string id)
        {
            await reviewService.DeleteAsync(id);
            return Ok(new ResponseObject<object>
            {
                Message = "Review deleted successfully",
                Code = "200",
                Success = true,
                Content = null
            });
        }
    }
}