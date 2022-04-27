using Application.Clients.LocalGovImsPaymentApi;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Command = Application.Commands.NotificationRequestCommand;
using Handler = Application.Commands.NotificationRequestCommandHandler;

namespace Application.UnitTests.Commands.NotificationRequest
{
    public class HandleTests
    {
        private readonly Handler _commandHandler;
        private Command _command;

        private readonly Mock<ILogger<Handler>> _mockLogger = new Mock<ILogger<Handler>>();
        private readonly Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<ILocalGovImsPaymentApiClient> _mockLocalGovImsPaymentApiClient = new Mock<ILocalGovImsPaymentApiClient>();

        public HandleTests()
        {
            _commandHandler = new Handler(
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMapper.Object,
                _mockLocalGovImsPaymentApiClient.Object);

            SetupClient(System.Net.HttpStatusCode.OK);
        }

        private void SetupConfig(string notificationUser, string notificationPassword)
        {
            var notificationUserConfigSection = new Mock<IConfigurationSection>();
            notificationUserConfigSection.Setup(a => a.Value).Returns(notificationUser);

            var notificationPasswordConfigSection = new Mock<IConfigurationSection>();
            notificationPasswordConfigSection.Setup(a => a.Value).Returns(notificationPassword);

            _mockConfiguration.Setup(x => x.GetSection("SmartPay:NotificationUser")).Returns(notificationUserConfigSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("SmartPay:NotificationPassword")).Returns(notificationPasswordConfigSection.Object);
        }

        private void SetupClient(System.Net.HttpStatusCode statusCode)
        {
            _mockLocalGovImsPaymentApiClient.Setup(x => x.Notify(It.IsAny<NotificationModel>()))
                .ReturnsAsync(statusCode);
        }

        private void SetupCommand(string authorisationHeader)
        {
            _command = new Command() { Notification = new Models.Notification(), AuthorisationHeader = authorisationHeader };
        }

        [Theory]
        [InlineData("Username", "Password", null)]
        [InlineData("Username", "Password", "")]
        [InlineData("AnotherUsername", "AnotherPassword", null)]
        [InlineData("AnotherUsername", "AnotherPassword", "")]
        public async Task Handle_returns_Unauthorised_when_AuthenticationHeader_is_missing(string username, string password, string authorisationHeader)
        {
            // Arrange
            SetupConfig(username, password);
            SetupCommand(authorisationHeader);

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("UsernameDoesNotMatch", "Password", "Header VXNlcm5hbWU6UGFzc3dvcmQ=")]
        [InlineData("AnotherUsernameDoesNotMatch", "AnotherPassword", "Header QW5vdGhlclVzZXJuYW1lOkFub3RoZXJQYXNzd29yZA==")]
        public async Task Handle_returns_Forbidden_when_username_does_not_match(string username, string password, string authorisationHeader)
        {
            // Arrange
            SetupConfig(username, password);
            SetupCommand(authorisationHeader);

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Username", "PasswordDoesNotMatch", "Header VXNlcm5hbWU6UGFzc3dvcmQ=")]
        [InlineData("AnotherUsername", "AnotherPasswordDoesNotMatch", "Header QW5vdGhlclVzZXJuYW1lOkFub3RoZXJQYXNzd29yZA==")]
        public async Task Handle_returns_Forbidden_when_password_does_not_match(string username, string password, string authorisationHeader)
        {
            // Arrange
            SetupConfig(username, password);
            SetupCommand(authorisationHeader);

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Username", "PasswordDoesNotMatch", "Header ")]
        [InlineData("AnotherUsername", "AnotherPasswordDoesNotMatch", "Header ")]
        public async Task Handle_returns_BadRequest_when_an_exepected_authorisation_issue_occurs(string username, string password, string authorisationHeader)
        {
            // Arrange
            SetupConfig(username, password);
            SetupCommand(authorisationHeader);

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("Username", "Password", "Header VXNlcm5hbWU6UGFzc3dvcmQ=")]
        [InlineData("AnotherUsername", "AnotherPassword", "Header QW5vdGhlclVzZXJuYW1lOkFub3RoZXJQYXNzd29yZA==")]
        public async Task Handle_returns_BadRequest_when_API_request_throws_execption(string username, string password, string authorisationHeader)
        {
            // Arrange
            SetupConfig(username, password);
            SetupCommand(authorisationHeader);

            _mockLocalGovImsPaymentApiClient.Setup(x => x.Notify(It.IsAny<NotificationModel>()))
                .Throws(new NullReferenceException());

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("Username", "Password", "Header VXNlcm5hbWU6UGFzc3dvcmQ=", System.Net.HttpStatusCode.OK)]
        [InlineData("Username", "Password", "Header VXNlcm5hbWU6UGFzc3dvcmQ=", System.Net.HttpStatusCode.Accepted)]
        [InlineData("AnotherUsername", "AnotherPassword", "Header QW5vdGhlclVzZXJuYW1lOkFub3RoZXJQYXNzd29yZA==", System.Net.HttpStatusCode.OK)]
        [InlineData("AnotherUsername", "AnotherPassword", "Header QW5vdGhlclVzZXJuYW1lOkFub3RoZXJQYXNzd29yZA==", System.Net.HttpStatusCode.Accepted)]
        public async Task Handle_returns_API_response_code_when_successful(string username, string password, string authorisationHeader, System.Net.HttpStatusCode statusCode)
        {
            // Arrange
            SetupConfig(username, password);
            SetupCommand(authorisationHeader);
            SetupClient(statusCode);

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Should().Be(statusCode);
        }
    }
}
