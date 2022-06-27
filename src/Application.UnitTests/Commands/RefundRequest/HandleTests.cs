using Application.Clients.CybersourceRestApiClient.Interfaces;
using Application.Commands;
using Application.Data;
using Application.Entities;
using Application.Models;
using Application.Result;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Command = Application.Commands.RefundRequestCommand;
using Handler = Application.Commands.RefundRequestCommandHandler;

namespace Application.UnitTests.Commands.RefundRequest
{
    public class HandleTests
    {
        private const string SecretKey = "ddc4fc675f404a108feb82ae475cbc982da072350b7c42c6b647ae41d208a9d0ce71d501023345de981abd6a7ab1e9092f81b0c2b44845fabcc63ad9f85b4e1105be4e5446334446883e044ecd1b7c285d2a3647ccec477e9989fe0704f5920181a0b6f004f4438eba3142486e90a62b8708904253ca437e906c96de20dd0230";
        private readonly Handler _commandHandler;
        private Command _command;

        private readonly Mock<IAsyncRepository<Payment>> _mockPaymentRepository = new Mock<IAsyncRepository<Payment>>();
        private readonly Mock<ICybersourceRestApiClient> _mockCybersourceRestApiClient = new Mock<ICybersourceRestApiClient>();

        public HandleTests()
        {
            _commandHandler = new Handler(
                _mockCybersourceRestApiClient.Object,
                _mockPaymentRepository.Object);

            SetupConfig();
            SetupCommand(new Refund() {
                Amount = 5,
                ImsReference = "12345",
                Reference = "test",
                TransactionDate = DateTime.Now
            });

        }

        private void SetupConfig()
        {

            _mockCybersourceRestApiClient.Setup(x => x.RefundPayment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(new bool() );

            _mockPaymentRepository.Setup(x => x.Add(It.IsAny<Payment>()))
                .ReturnsAsync(new OperationResult<Payment>(true) { Data = new Payment() { Identifier = Guid.NewGuid() } });

        }


        private void SetupCommand(Refund refund)
        {
            _command = new Command() { Refund = refund};
        }

        [Fact]
        public async Task Handle_returns_a_ProcessPaymentResponseModel()
        {
            // Arrange


            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Should().BeOfType<RefundResult>();
        }
    }
}
