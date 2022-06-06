using Application.Commands;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalGovImsApiClient.Model;
using Xunit;
using Command = Application.Commands.PaymentResponseCommand;
using Handler = Application.Commands.PaymentResponseCommandHandler;
using Keys = Application.Commands.PaymentResponseParameterKeys;
using System.Threading;
using Application.Data;
using Application.Entities;
using Application.Clients.CybersourceRestApiClient.Interfaces;

namespace Application.UnitTests.Commands.PaymentResponse
{
    public class HandleTests
    {
        private const string SecretKey = "ddc4fc675f404a108feb82ae475cbc982da072350b7c42c6b647ae41d208a9d0ce71d501023345de981abd6a7ab1e9092f81b0c2b44845fabcc63ad9f85b4e1105be4e5446334446883e044ecd1b7c285d2a3647ccec477e9989fe0704f5920181a0b6f004f4438eba3142486e90a62b8708904253ca437e906c96de20dd0230";
        private readonly Handler _commandHandler;
        private Command _command;
        private Models.PaymentResponse _paymentResponse;

        private readonly Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private readonly Mock<LocalGovImsApiClient.Api.IPendingTransactionsApi> _mockPendingTransactionsApi = new Mock<LocalGovImsApiClient.Api.IPendingTransactionsApi>();
        private readonly Mock<IAsyncRepository<Payment>> _mockPaymentRepository = new Mock<IAsyncRepository<Payment>>();
        private readonly Mock<ICybersourceRestApiClient> _mockCybersourceRestApiClient = new Mock<ICybersourceRestApiClient>();

        public HandleTests()
        {
            _commandHandler = new Handler(
                _mockConfiguration.Object,
                _mockPendingTransactionsApi.Object,
                _mockPaymentRepository.Object,
                _mockCybersourceRestApiClient.Object);

            SetupConfig();
            SetupClient(System.Net.HttpStatusCode.OK);
            SetUpPaymentResponse();
            SetupCommand(new Dictionary<string, string> {
                { Keys.AuthorisationResult, AuthorisationResult.Authorised },
                { Keys.MerchantSignature, "NZL0OxbvIzufD/ejZODSJ3SzcNQKMJ1JhzQaKH9LWtM=" },
                { Keys.PspReference, "8816281505278071" },
                { Keys.PaymentMethod, "Card" }
            } , _paymentResponse
            );
        }

        private void SetupConfig()
        {
            var hmacKeyConfigSection = new Mock<IConfigurationSection>();
            hmacKeyConfigSection.Setup(a => a.Value).Returns("FC81CC7410D19B75B6513FF413BE2E2762CE63D25BA2DFBA63A3183F796530FC");

            var smartPaySecretKeyConfigSection = new Mock<IConfigurationSection>();
            smartPaySecretKeyConfigSection.Setup(a => a.Value).Returns(SecretKey);

            _mockConfiguration.Setup(x => x.GetSection("SmartPay:HmacKey")).Returns(hmacKeyConfigSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("SmartPayFuse:SecretKey")).Returns(smartPaySecretKeyConfigSection.Object);

        }

        private void SetUpPaymentResponse()
        {
            _paymentResponse = TestData.GetPaymentResponseModel();
        }

        private void SetupClient(System.Net.HttpStatusCode statusCode)
        {
            _mockPendingTransactionsApi.Setup(x => x.PendingTransactionsProcessPaymentAsync(It.IsAny<string>(), It.IsAny<ProcessPaymentModel>(),0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessPaymentResponse());
        }

        private void SetupCommand(Dictionary<string, string> parameters, Application.Models.PaymentResponse paymentResponse)
        {
            _command = new Command() { Paramaters = parameters, paymentResponse = paymentResponse };
        }

        [Fact]
        public async Task Handle_throws_PaymentException_when_request_is_not_valid()
        {
            // Arrange
            SetupCommand(new Dictionary<string, string> {
                { Keys.AuthorisationResult, AuthorisationResult.Authorised },
                { Keys.MerchantSignature, "1NZL0OxbvIzufD/ejZODSJ3SzcNQKMJ1JhzQaKH9LWtM=" },
                { Keys.PspReference, "8816281505278071" },
                { Keys.PaymentMethod, "Card" },
                { Keys.SigningField, "transaction_id"}
            }, _paymentResponse);

            // Act
            async Task task() => await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            var result = await Assert.ThrowsAsync<PaymentException>(task);
            result.Message.Should().Be("Unable to process the payment");
        }

        [Theory]
        [InlineData(AuthorisationResult.Authorised, "kEz1zuPyA9A7IovYcmMR5Hks/kzrCcJJA7pVAVIAWhI=")]
    //    [InlineData("Another value", "97Y0KDL1+KEe0gTQJzQ/mBQJIj1dTsIubOwItb+Hsx0=")]
        public async Task Handle_returns_a_ProcessPaymentResponseModel(string authorisationResult, string merchantSignature)
        {
            // Arrange
            SetupCommand(new Dictionary<string, string> {
                { Keys.AuthorisationResult, authorisationResult },
                { Keys.MerchantSignature, merchantSignature },
                { Keys.PspReference, "8816281505278071" },
                { Keys.PaymentMethod, "Card" },
                { Keys.SigningField, "transaction_id"}
            }, _paymentResponse);

            // Act
            var result = await _commandHandler.Handle(_command, new System.Threading.CancellationToken());

            // Assert
            result.Should().BeOfType<PaymentResponseCommandResult>();
        }
    }
}
