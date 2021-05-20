using AutoMapper;
using BookStore_Api.Contracts;
using BookStore_Api.Data;
using BookStore_Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_Api.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Authors in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public AuthorsController(IAuthorRepository authorRepository, ILoggerService logger, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Authors
        /// </summary>
        /// <returns> List of Authors </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            var location = GeControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call");

                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);

                _logger.LogInfo($"{location}: Successful");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }

        }

        /// <summary>
        /// Get Author of Specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns> Single Author of Specified ID </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetAuthor(int id)
        {
            var location = GeControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Attempted Call for id: {id}");
                var author = await _authorRepository.FindById(id);

                if (author == null)
                {
                    _logger.LogWarn($"{location}: Record with id: {id} not found");
                    return NotFound();
                }

                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($"{location}: Successfully got record with ID: {id}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Creates an Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns> Author Object </returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            var location = GeControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Create Attempted");
                if (authorDTO == null)
                {
                    _logger.LogWarn($"{location}: Empty Request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Data was incomplete");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);

                if (!isSuccess)
                {
                    return InternalError($"{location}: Operation Failed");
                }

                _logger.LogInfo($"{location}: Created Successfully");
                return Created("Create", new { author });
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Updates an Author
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns> Author Object </returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator, Customer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            var location = GeControllerActionNames();
            try
            {
                _logger.LogWarn($"{location}: Updated Attempted");
                if (id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _logger.LogWarn($"{location}: Data Incomplete / Bad Data ");
                    return BadRequest();
                }
                var isExists = await _authorRepository.isExists(id);
                if (!isExists)
                {
                    _logger.LogWarn($"{location}: Record with id: {id} not found");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Data was incomplete");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);

                if (!isSuccess)
                {
                    return InternalError($"{location}: Operation Failed");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Delete an Author with specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns> No Content </returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GeControllerActionNames();
            try
            {
                _logger.LogWarn($"{location}: Delete Attempted");
                if (id < 1)
                {
                    _logger.LogWarn($"{location}: Data Incomplete / Bad Data ");
                    return BadRequest();
                }
                var isExists = await _authorRepository.isExists(id);
                if (!isExists)
                {
                    _logger.LogWarn($"{location}: Record with id: {id} not found");
                    return NotFound();
                }
                var author = await _authorRepository.FindById(id);
                var isSuccess = await _authorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Operation Failed");
                }
                _logger.LogWarn($"{location}: Record with id: {id} successfully deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        private string GeControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact Admin");
        }
    }
}
