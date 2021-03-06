﻿using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint to interactwith the books inthe bookstore API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // overall requirement, overruled by any statement at endpoint level
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BooksController(IBookRepository bookRepository, ILoggerService logger, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
            _webHostEnvironment  = webHostEnvironment;
        }
        /// <summary>
        /// Get all Books
        /// </summary>
        /// <returns>List of Books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            try
            {
                Info($"Attempted GetBooks");
                var books = await _bookRepository.FindAll();
                var bookDTOs = _mapper.Map<IList<BookDTO>>(books);
                foreach (var bookDTO in bookDTOs)
                {
                    SetBookFile(bookDTO);
                }
                Info($"Successfully got all books");
                return Ok(bookDTOs);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        private void SetBookFile(BookDTO bookDTO)
        {
            if (!string.IsNullOrEmpty(bookDTO.Image))
            {
                var imgPath = GetImagePath(bookDTO.Image);
                if (System.IO.File.Exists(imgPath))
                {
                    var imgBytes = System.IO.File.ReadAllBytes(imgPath);
                    bookDTO.File = Convert.ToBase64String(imgBytes);
                }
            }
        }

        /// <summary>
        /// Gets the book identified by a given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Book record</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            try
            {
                Info($"Attempted GetBook({id})");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    Warn($"No book {id}");
                    return NotFound();
                }
                var bookDTO = _mapper.Map<BookDTO>(book);
                SetBookFile(bookDTO);
                Info($"Successfully got book {id}");
                return Ok(bookDTO);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        /// <summary>
        /// Create a new book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            try
            {
                Info($"Book creation attempted");
                if (bookDTO == null)
                {
                    Warn($"Empty request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    Warn($"Invalid modelstate '{ModelState}'");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);
                if (!isSuccess)
                {
                    return InternalError($"{By()}Book {book} creation failed.");
                }
                // note that File is not included in book, only in the DTO
                if(!string.IsNullOrEmpty(bookDTO.File))
                {
                    var imgPath = GetImagePath(bookDTO.Image);
                    var imageBytes = Convert.FromBase64String(bookDTO.File);
                    System.IO.File.WriteAllBytes(imgPath, imageBytes);
                }
                Info($"Book created");
                return Created("Create", new { book });

            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        /// <summary>
        /// This endpoint updates the book object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            try
            {
                Info($"Book update attempted for id {id}");
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    Warn($"Update failed (bad id {id})");
                    return BadRequest(ModelState);
                }
                var doesExist = await _bookRepository.DoesExist(id);
                if(!doesExist)
                {
                    Warn($"Failed to retrieve book with id {id}.");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    Warn($"Invalid modelstate '{ModelState}'");
                    return BadRequest(ModelState);
                }
                var oldImage = await _bookRepository.GetImageFileName(id);
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Update(book);
                if (!isSuccess)
                {
                    return InternalError($"{By()}Book {id} '{book}' update failed.");
                }
                if (!bookDTO.Image.Equals(oldImage)
                    && System.IO.File.Exists(GetImagePath(oldImage)))
                {
                        System.IO.File.Delete(GetImagePath(oldImage));
                }
                if(!string.IsNullOrEmpty(bookDTO.File))
                {
                    var imageBytes = Convert.FromBase64String(bookDTO.File);
                    System.IO.File.WriteAllBytes
                        (GetImagePath(bookDTO.Image), imageBytes);
                }
                Info($"Book with id {id} updated");
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        /// <summary>
        /// Endpoint to delete a book
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Info($"Book delete attempted");
                if (id < 1)
                {
                    Warn($"id < 1 was submitted");
                    return BadRequest(ModelState);
                }
                var doesExist = await _bookRepository.DoesExist(id);
                if (!doesExist)
                {
                    Warn($"Unknown book {id}");
                    return NotFound();
                }
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    Error($"Should not occur after previous check!");
                    return NotFound();
                }
                var isSuccess = await _bookRepository.Delete(book);
                if (!isSuccess)
                    return InternalError($"{By()}Book {id} delete failed.");
                Info($"Book {id} deleted");
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        private string GetImagePath(string fileName)
            => ($"{_webHostEnvironment.ContentRootPath}/uploads/{fileName}"); 
        // is same as:
        //private string GetImagePath(string fileName)
        //{
        //    return $"{_webHostEnvironment.ContentRootPath}/uploads/{fileName}"; 
        //}
        private void Error(string message)
        {
            _logger.LogError($"{By()}: {message}");
        }
        private void Info(string message)
        {
            _logger.LogInfo($"{By()}: {message}");
        }
        private void Warn(string message)
        {
            _logger.LogWarn($"{By()}: {message}");
        }
        private string By()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action= ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }
        private ObjectResult InternalError(Exception e)
        {
            _logger.LogError($"{By()}: {e.Message} - {e.InnerException}");
            return StatusCode(500, "Something went wrong. Please contact the Administrator");
        }
        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the Administrator");
        }

    }
}
