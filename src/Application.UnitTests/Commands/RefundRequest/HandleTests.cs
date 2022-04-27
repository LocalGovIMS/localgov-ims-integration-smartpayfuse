using Application.Commands;
using Application.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using SmartPayService;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Command = Application.Commands.RefundRequestCommand;
using Handler = Application.Commands.RefundRequestCommandHandler;

namespace Application.UnitTests.Commands.RefundRequest
{
    public class HandleTests
    {
        private readonly Handler _commandHandler;
        private Command _command;

        private readonly Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private readonly Mock<IPaymentPortTypeClient> _mockPaymentPortTypeClient = new Mock<IPaymentPortTypeClient>();

        private const decimal Amount = 10;
        private const string SuccessfulClientResponse = "[refund-received]";
        private const string UnsuccessfulClientResponse = "";
        private const string DefaultPspReference = "123456";

        public HandleTests()
        {
            _commandHandler = new Handler(
                _mockConfiguration.Object,
                _mockPaymentPortTypeClient.Object);

            SetupConfig();
            SetupClient(SuccessfulClientResponse);
            SetupCommand();
        }

        private void SetupConfig()
        {
            var merchantAccountConfigSection = new Mock<IConfigurationSection>();
            merchantAccountConfigSection.Setup(a => a.Value).Returns("MerchantAccountDetails");

            _mockConfiguration.Setup(x => x.GetSection("SmartPay:MerchantAccount")).Returns(merchantAccountConfigSection.Object);
        }

        private void SetupClient(string response)
        {
            SetupClient(response, DefaultPspReference);
        }

        private void SetupClient(string response, string pspReference)
        {
            _mockPaymentPortTypeClient.Setup(x => x.refundAsync(It.IsAny<ModificationRequest>()))
                .ReturnsAsync(new ModificationResult() { response = response, pspReference = pspReference });
        }

        private void SetupCommand()
        {
            _command = new Command() { Refund = new Refund() { Amount = Amount, Reference = DefaultPspReference, TransactionDate = System.DateTime.Now } };
        }

        [Fact]
        public async Task Handle_sets_the_security_protocol()
        {
            // Arrange

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            ServicePointManager.SecurityProtocol.Should().Be(SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);
        }

        [Theory]
        [InlineData(SuccessfulClientResponse)]
        [InlineData(UnsuccessfulClientResponse)]
        public async Task Handle_returns_a_RefundResult(string clientResponse)
        {
            // Arrange
            SetupClient(clientResponse);

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Should().BeOfType<RefundResult>();
        }

        [Theory]
        [InlineData(SuccessfulClientResponse, true)]
        [InlineData(UnsuccessfulClientResponse, false)]
        public async Task Handle_returns_the_expected_RefundRequest_Success_value(string clientResponse, bool expetectedSuccess)
        {
            // Arrange
            SetupClient(clientResponse);

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Success.Should().Be(expetectedSuccess);
        }

        [Theory]
        [InlineData("A PSP Reference")] // Request worked
        [InlineData("Another PSP Reference")] // Request failed
        public async Task Handle_returns_the_PspReference_when_successful(string pspReference)
        {
            // Arrange
            SetupClient(SuccessfulClientResponse, pspReference);

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.PspReference.Should().Be(pspReference);
        }

        [Theory]
        [InlineData("A PSP Reference")] // Request worked
        [InlineData("Another PSP Reference")] // Request failed
        public async Task Handle_returns_the_Amount_when_successful(string pspReference)
        {
            // Arrange
            SetupClient(SuccessfulClientResponse, pspReference);

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Amount.Should().Be(Amount * 100);
        }
    }
}
