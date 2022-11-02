using Application.Commands;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalGovImsApiClient.Model;
using Xunit;
using Command = Application.Commands.ProcessUncapturedRefundsCommand;
using Handler = Application.Commands.ProcessUncapturedRefundsCommandHander;
using System.Threading;
using Application.Data;
using Application.Entities;
using Application.Clients.CybersourceRestApiClient.Interfaces;
using System.Linq.Expressions;
using Application.Result;
using System;
using Microsoft.Extensions.Logging;
using LocalGovImsApiClient.Client;

namespace Application.UnitTests.Commands.ProcessUncapturedRefunds
{
    public class HandleTests
    {
        private const string SecretKey = "ddc4fc675f404a108feb82ae475cbc982da072350b7c42c6b647ae41d208a9d0ce71d501023345de981abd6a7ab1e9092f81b0c2b44845fabcc63ad9f85b4e1105be4e5446334446883e044ecd1b7c285d2a3647ccec477e9989fe0704f5920181a0b6f004f4438eba3142486e90a62b8708904253ca437e906c96de20dd0230";
        private readonly Handler _commandHandler;
        private Command _command;

        //     private Models.PaymentResponse _paymentResponse;

        private readonly Mock<ICybersourceRestApiClient> _mockCybersourceRestApiClient = new Mock<ICybersourceRestApiClient>();
        private readonly Mock<ILogger<Handler>> _mockLogger = new();
        private readonly Mock<LocalGovImsApiClient.Api.IPendingTransactionsApi> _mockPendingTransactionsApi = new Mock<LocalGovImsApiClient.Api.IPendingTransactionsApi>();
        private readonly Mock<IAsyncRepository<Payment>> _mockPaymentRepository = new Mock<IAsyncRepository<Payment>>();

        public HandleTests()
        {
            _commandHandler = new Handler(
               _mockCybersourceRestApiClient.Object,
               _mockLogger.Object,
               _mockPendingTransactionsApi.Object,
               _mockPaymentRepository.Object
                );

            SetupConfig();
            SetupClient(System.Net.HttpStatusCode.OK);
            SetupCommand();
        }

        private void SetupConfig()
        {

            _mockCybersourceRestApiClient.Setup(x => x.SearchRefunds(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Payment>() {
                    new Payment()
                    {
                        Amount = 0,
                        Reference = "Test1"
                    }
                });

            _mockPaymentRepository.Setup(x => x.List(It.IsAny<Expression<Func<Payment, bool>>>()))
                .ReturnsAsync(new OperationResult<List<Payment>>(true)
                {
                    Data = new List<Payment>()
                    {
                    }
                });

            _mockPaymentRepository.Setup(x => x.Update(It.IsAny<Payment>()))
                .ReturnsAsync(new OperationResult<Payment>(true) { Data = new Payment() { Identifier = Guid.NewGuid(), PaymentId = "paymentId", Reference = "refernce" } });

        }

        private void SetupClient(System.Net.HttpStatusCode statusCode)
        {
            _mockPendingTransactionsApi.Setup(x => x.PendingTransactionsGetAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PendingTransactionModel>() {
                    new PendingTransactionModel()
                    {
                        Reference = "Test",
                        FundCode = "F1"
                    }
                });

            _mockPendingTransactionsApi.Setup(x => x.PendingTransactionsProcessPaymentAsync(It.IsAny<string>(), It.IsAny<ProcessPaymentModel>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessPaymentResponse() { Success = true });
        }

        private void SetupCommand()
        {
            _command = new Command(1);
        }

        [Fact]
        public async Task Handle_returns_SuccessWhenNoRefundsToProcess()
        {
            // Arrange

            // Act
            var result = await _commandHandler.Handle(_command, new CancellationToken());

            // Assert
            result.Should().BeOfType<ProcessUncapturedRefundsCommandResult>();
            result.TotalIdentified.Should().Be(0);
            result.TotalErrors.Should().Be(0);
            result.TotalMarkedAsCaptured.Should().Be(0);
        }

        [Fact]
        public async Task Handle_returns_ErrorWhenPendingTransactionNotFound()
        {
            // Arrange
            _mockPaymentRepository.Setup(x => x.List(It.IsAny<Expression<Func<Payment, bool>>>()))
                .ReturnsAsync(new OperationResult<List<Payment>>(true)
                {
                    Data = new List<Payment>()
                    {
                        new Payment() { Identifier = Guid.NewGuid(), Reference = "Test1", PaymentId = "PaymentId1", Finished = false, RefundReference = "test3" },
                    }
                });

            _mockPendingTransactionsApi.Setup(x => x.PendingTransactionsGetAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PendingTransactionModel>()
                {
                });
            // Act
            var result = await _commandHandler.Handle(_command, new CancellationToken());

            // Assert
            result.Should().BeOfType<ProcessUncapturedRefundsCommandResult>();
            result.TotalIdentified.Should().Be(1);
            result.TotalErrors.Should().Be(1);
            result.TotalMarkedAsCaptured.Should().Be(0);
        }

        [Fact]
        public async Task Handle_returns_ErrorWhenPendingTransactionNotFound_via_404_error()
        {
            // Arrange
            _mockPaymentRepository.Setup(x => x.List(It.IsAny<Expression<Func<Payment, bool>>>()))
                .ReturnsAsync(new OperationResult<List<Payment>>(true)
                {
                    Data = new List<Payment>()
                    {
                        new Payment() { Identifier = Guid.NewGuid(), Reference = "Test1", PaymentId = "PaymentId1", Finished = false, RefundReference = "test3" },
                    }
                });

            _mockPendingTransactionsApi.Setup(x => x.PendingTransactionsGetAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()))
                .Throws(new ApiException(404,""));
            // Act
            var result = await _commandHandler.Handle(_command, new CancellationToken());

            // Assert
            result.Should().BeOfType<ProcessUncapturedRefundsCommandResult>();
            result.TotalIdentified.Should().Be(1);
            result.TotalErrors.Should().Be(1);
            result.TotalMarkedAsCaptured.Should().Be(0);
        }

        [Fact]
        public async Task Handle_returns_SuccessWhenARefundIsProcessed()
        {
            // Arrange
            _mockPaymentRepository.Setup(x => x.List(It.IsAny<Expression<Func<Payment, bool>>>()))
                .ReturnsAsync(new OperationResult<List<Payment>>(true)
                {
                    Data = new List<Payment>()
                    {
                        new Payment() { Identifier = Guid.NewGuid(), Reference = "Test1", PaymentId = "PaymentId1", Finished = false, RefundReference = "test3" },
                    }
                });
            // Act
            var result = await _commandHandler.Handle(_command, new CancellationToken());

            // Assert
            result.Should().BeOfType<ProcessUncapturedRefundsCommandResult>();
            result.TotalIdentified.Should().Be(1);
            result.TotalErrors.Should().Be(0);
            result.TotalMarkedAsCaptured.Should().Be(1);
        }
    }
}
