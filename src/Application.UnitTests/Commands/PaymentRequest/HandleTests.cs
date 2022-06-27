using Application.Builders;
using Application.Cryptography;
using Application.Models;
using Application.Data;
using Application.Entities;
using Domain.Exceptions;
using FluentAssertions;
using LocalGovImsApiClient.Model;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Command = Application.Commands.PaymentRequestCommand;
using Handler = Application.Commands.PaymentRequestCommandHandler;
using Application.Result;
using System;

namespace Application.UnitTests.Commands.PaymentRequest
{
    public class HandleTests
    {
        private readonly Handler _commandHandler;
        private Command _command;

        private readonly Mock<ICryptographyService> _mockCryptographyService = new Mock<ICryptographyService>();
        private readonly Mock<LocalGovImsApiClient.Api.IPendingTransactionsApi> _mockPendingTransactionsApi = new Mock<LocalGovImsApiClient.Api.IPendingTransactionsApi>();
        private readonly Mock<LocalGovImsApiClient.Api.IProcessedTransactionsApi> _mockProcessedTransactionsApi = new Mock<LocalGovImsApiClient.Api.IProcessedTransactionsApi>();
        private readonly Mock<IBuilder<PaymentBuilderArgs, SmartPayFusePayment>> _mockBuilder = new Mock<IBuilder<PaymentBuilderArgs, SmartPayFusePayment>>();
        private readonly Mock<IAsyncRepository<Payment>> _mockPaymentRepository = new Mock<IAsyncRepository<Payment>>();

        public HandleTests()
        {
            _commandHandler = new Handler(
                _mockCryptographyService.Object,
                _mockBuilder.Object,
                _mockPendingTransactionsApi.Object,
                _mockProcessedTransactionsApi.Object,
                _mockPaymentRepository.Object
);

            SetupClient(System.Net.HttpStatusCode.OK);
            SetupCryptographyService();
            SetupCommand("reference", "hash");
        }

        private void SetupClient(System.Net.HttpStatusCode statusCode)
        {
            _mockProcessedTransactionsApi.Setup(x => x.ProcessedTransactionsSearchAsync(
              It.IsAny<string>(),
              It.IsAny<List<string>>(),
              It.IsAny<string>(),
              It.IsAny<decimal?>(),
              It.IsAny<DateTime?>(),
              It.IsAny<DateTime?>(),
              It.IsAny<string>(),
              It.IsAny<List<string>>(),
              It.IsAny<string>(),
              It.IsAny<string>(),
              It.IsAny<string>(),
              It.IsAny<int>(),
              It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<ProcessedTransactionModel>)null);

            _mockProcessedTransactionsApi.Setup(x => x.ProcessedTransactionsGetAsync(It.IsAny<string>(),0, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ProcessedTransactionModel)null);

            _mockPendingTransactionsApi.Setup(x => x.PendingTransactionsGetAsync(It.IsAny<string>(),0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PendingTransactionModel>() {
                    new PendingTransactionModel()
                    {
                        Reference = "reference"
                    }
                });

            _mockPaymentRepository.Setup(x => x.Add(It.IsAny<Payment>()))
                .ReturnsAsync(new OperationResult<Payment>(true) { Data = new Payment() { Identifier = Guid.NewGuid() } });

            _mockBuilder.Setup(x => x.Build(It.IsAny<PaymentBuilderArgs>()))
                .Returns(new SmartPayFusePayment());

        }

        private void SetupCryptographyService()
        {
            _mockCryptographyService.Setup(x => x.GetHash(It.IsAny<string>()))
                .Returns("hash");
        }

        private void SetupCommand(string reference, string hash)
        {
            _command = new Command() { Reference = reference, Hash = hash };
        }

        [Fact]
        public async Task Handle_throws_PaymentException_when_the_reference_is_null()
        {
            // Arrange
            SetupCommand(null, "hash");

            // Act
            async Task task() => await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            var result = await Assert.ThrowsAsync<PaymentException>(task);
            result.Message.Should().Be("The reference provided is not valid");
        }

        [Fact]
        public async Task Handle_throws_PaymentException_when_the_hash_is_null()
        {
            // Arrange
            SetupCommand("reference", null);

            // Act
            async Task task() => await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            var result = await Assert.ThrowsAsync<PaymentException>(task);
            result.Message.Should().Be("The reference provided is not valid");
        }

        [Fact]
        public async Task Handle_throws_PaymentException_when_the_hash_doesn_not_match_the_computed_hash()
        {
            // Arrange
            SetupCommand("reference", "hash that doesn't match");

            // Act
            async Task task() => await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            var result = await Assert.ThrowsAsync<PaymentException>(task);
            result.Message.Should().Be("The reference provided is not valid");
        }

        [Fact]
        public async Task Handle_throws_PaymentException_when_processed_transactions_exists_for_the_reference()
        {
            // Arrange
            _mockProcessedTransactionsApi.Setup(x => x.ProcessedTransactionsSearchAsync(
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProcessedTransactionModel>() {
                    new ProcessedTransactionModel()
                    {
                        Reference = "Test"
                    }
                });

            // Act
            async Task task() => await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            var result = await Assert.ThrowsAsync<PaymentException>(task);
            result.Message.Should().Be("The reference provided is no longer a valid pending payment");
        }

        [Fact]
        public async Task Handle_throws_PaymentException_when_pending_transactions_do_not_exist_for_the_reference()
        {
            // Arrange
            _mockPendingTransactionsApi.Setup(x => x.PendingTransactionsGetAsync(It.IsAny<string>(),0, It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<PendingTransactionModel>)null);

            // Act
            async Task task() => await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            var result = await Assert.ThrowsAsync<PaymentException>(task);
            result.Message.Should().Be("The reference provided is no longer a valid pending payment");
        }

        [Fact]
        public async Task Handle_returns_Payment_when_successful()
        {
            // Arrange

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Should().BeOfType<SmartPayFusePayment>();
        }
    }
}
