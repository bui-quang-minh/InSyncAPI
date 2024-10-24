using AutoMapper;
using BusinessObjects.Models;
using FakeItEasy;
using FluentAssertions;
using InSyncAPI.Controllers;
using InSyncAPI.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Repositorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InSyncUnitTest.Controller
{
    public class UserControllerTest
    {
        private IUserRepository _userRepo;
        private IMapper _mapper;
        private ILogger<UsersController> _logger;
        private UsersController _userController;

        public UserControllerTest()
        {
            _userRepo = A.Fake<IUserRepository>();
            _mapper = A.Fake<IMapper>();
            _logger = A.Fake<ILogger<UsersController>>();
            _userController = new UsersController(_userRepo, _mapper, _logger);
        }

        #region CreateUser
        [Fact]
        public async Task CreateUser_WhenUserIsCreatedSuccessfully_ShouldReturnOk()
        {
            // Arrange
            var userDto = new CreateUserDto
            {
                Data = new Data(),
                Object = "TestObject",
                Type = "TestType"
            };
            var user = new User { Id = Guid.NewGuid() };

            A.CallTo(() => _mapper.Map<User>(userDto)).Returns(user);
            A.CallTo(() => _userRepo.Add(user)).Returns(Task.FromResult(user));

            // Act
            var result = await _userController.CreateUser(userDto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().Be(user);
        }

        [Fact]
        public async Task CreateUser_WhenModelStateIsInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            var userDto = new CreateUserDto();
            var controller = new UsersController(_userRepo, _mapper, _logger);
            controller.ModelState.AddModelError("Error", "Invalid data");

            // Act
            var result = await controller.CreateUser(userDto) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Value.Should().BeOfType<ValidationProblemDetails>();
        }

        [Fact]
        public async Task CreateUser_WhenServiceNotInjectIsNull_ShouldReturn500()
        {
            // Arrange
            _userController = new UsersController(null, _mapper, _logger);
            var userDto = new CreateUserDto();

            // Act
            var result = await _userController.CreateUser(userDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            result.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task CreateUser_ShouldHandleExceptionAndReturn500()
        {
            // Arrange
            var userDto = new CreateUserDto();
            var user = new User { Id = Guid.NewGuid() };

            A.CallTo(() => _mapper.Map<User>(userDto)).Returns(user);
            A.CallTo(() => _userRepo.Add(user)).Throws(new Exception("Database error"));

            // Act
            var result = await _userController.CreateUser(userDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            result.Value.Should().Be("Error creating user: Database error");
        }

        [Fact]
        public async Task CreateUser_WhenUserCreationFails_ShouldReturn500()
        {
            // Arrange
            var userDto = new CreateUserDto();
            var user = new User { Id = Guid.NewGuid() };

            A.CallTo(() => _mapper.Map<User>(userDto)).Returns(user);
            A.CallTo(() => _userRepo.Add(user)).Returns(Task.FromResult<User>(null)); // Simulate a failure to add user

            // Act
            var result = await _userController.CreateUser(userDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            result.Value.Should().Be("Error occurred while adding the user.");
        }
        #endregion

        #region Udpate User 

        [Fact]
        public async Task UpdateUser_WhenUserIsUpdatedSuccessfully_ShouldReturnOk()
        {
            // Arrange
            var userDto = new UpdateUserDto
            {
                Data = new Data(),
                Object = "TestObject",
                Type = "TestType"
            };
            var user = new User { Email = "test@gmail.com" };
            var existingUser = new User { Email = "test@gmail.com", DisplayName = "OldName" };

            A.CallTo(() => _mapper.Map<User>(userDto)).Returns(user);
            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult(existingUser));

            // Act
            var result = await _userController.UpdateUser(userDto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeEquivalentTo(new { message = "User updated successfully.", Id = existingUser.DisplayName });
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var userDto = new UpdateUserDto();
            _userController.ModelState.AddModelError("Error", "Invalid data");

            // Act
            var result = await _userController.UpdateUser(userDto) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Value.Should().BeOfType<ValidationProblemDetails>();
        }

        [Fact]
        public async Task UpdateUser_ShouldReturn500_WhenUserRepoOrMapperIsNull()
        {
            // Arrange
            _userController = new UsersController(null, _mapper, _logger);
            var userDto = new UpdateUserDto();

            // Act
            var result = await _userController.UpdateUser(userDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            result.Value.Should().Be("Application service has not been created");
        }

        [Fact]
        public async Task UpdateUser_WhenUserDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var userDto = new UpdateUserDto();
            var user = new User { Email = "nonexistent@example.com" };

            A.CallTo(() => _mapper.Map<User>(userDto)).Returns(user);
            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>._, A<string[]>._)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _userController.UpdateUser(userDto) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Value.Should().Be("User not found.");
        }

        [Fact]
        public async Task UpdateUser_ShouldHandleExceptionAndReturn500()
        {
            // Arrange
            var userDto = new UpdateUserDto();
            var user = new User { Email = "test@gmail.com" };

            A.CallTo(() => _mapper.Map<User>(userDto)).Returns(user);
            A.CallTo(() => _userRepo.GetSingleByCondition(A<Expression<Func<User, bool>>>.Ignored, A<string[]>._)).Throws(new Exception("Database error"));

            // Act
            var result = await _userController.UpdateUser(userDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            result.Value.Should().Be("Error updating user: Database error");
        }
        #endregion
    }
}
