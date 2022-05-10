using Application.Builders;
using Application.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using Xunit;

namespace Application.UnitTests.Builders.PaymentBuilderTests
{
    public class BuildTests
    {
        private Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();

        private const string AccessKey = "1e2cf8edc97939e383eebb09198ff577";
        private const string ProfileId = "9FF12BEC-2057-46AC-B935-E42D1A85F4B8";
        private const string SmartPayFusePaymentEndpoint = "SmartPayFusePaymentEndpoint";
        private const string SecretKey = "ddc4fc675f404a108feb82ae475cbc982da072350b7c42c6b647ae41d208a9d0ce71d501023345de981abd6a7ab1e9092f81b0c2b44845fabcc63ad9f85b4e1105be4e5446334446883e044ecd1b7c285d2a3647ccec477e9989fe0704f5920181a0b6f004f4438eba3142486e90a62b8708904253ca437e906c96de20dd0230";
        private const string PaymentPortalUrl = "PaymentPortalUrl";

        private IBuilder<PaymentBuilderArgs, Payment> _builder;
        private PaymentBuilderArgs _args;

        private void Arrange()
        {
            SetupConfiguration();
            SetupBuilder();
            SetupArgs();
        }

        private void SetupConfiguration()
        {
            var smartPayAccessKeyConfigSection = new Mock<IConfigurationSection>();
            smartPayAccessKeyConfigSection.Setup(a => a.Value).Returns(AccessKey);

            var smartPayProfileIdConfigSection = new Mock<IConfigurationSection>();
            smartPayProfileIdConfigSection.Setup(a => a.Value).Returns(ProfileId);

            var smartPayEndPointConfigSection = new Mock<IConfigurationSection>();
            smartPayEndPointConfigSection.Setup(a => a.Value).Returns(SmartPayFusePaymentEndpoint);

            var smartPaySecretKeyConfigSection = new Mock<IConfigurationSection>();
            smartPaySecretKeyConfigSection.Setup(a => a.Value).Returns(SecretKey);

            var smartPayPaymentPortalUrlConfigSection = new Mock<IConfigurationSection>();
            smartPayPaymentPortalUrlConfigSection.Setup(a => a.Value).Returns(PaymentPortalUrl);

            var smartPayOverrideCustomCancelPageConfigSection = new Mock<IConfigurationSection>();
            smartPayOverrideCustomCancelPageConfigSection.Setup(a => a.Value).Returns(PaymentPortalUrl);

            var smartPayOverrideCustomReceiptPageConfigSection = new Mock<IConfigurationSection>();
            smartPayOverrideCustomReceiptPageConfigSection.Setup(a => a.Value).Returns(PaymentPortalUrl);

            _mockConfiguration.Setup(x => x.GetSection("SmartPayFuse:AccessKey")).Returns(smartPayAccessKeyConfigSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("SmartPayFuse:ProfileId")).Returns(smartPayProfileIdConfigSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("SmartPayFuse:HostedCheckoutEndpoint")).Returns(smartPayEndPointConfigSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("SmartPayFuse:SecretKey")).Returns(smartPaySecretKeyConfigSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("PaymentPortalUrl")).Returns(smartPayPaymentPortalUrlConfigSection.Object);

        }

        private void SetupBuilder()
        {
            _builder = new PaymentBuilder(_mockConfiguration.Object);
        }

        private void SetupArgs()
        {
            _args = new PaymentBuilderArgs()
            {
                Amount = 0.10M,
                Reference = "TestReference",
                CardSelfServiceMopCode = "00",
                Transaction = TestData.GetPendingTransactionModel()
            };
        }

         [Fact]
        public void Build_sets_AccessKey()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.AccessKey.Should().Be(AccessKey);
        }

        [Fact]
        public void Build_sets_ProfileId()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.ProfileId.Should().Be(ProfileId);
        }

        [Fact]
        public void Build_sets_Amount()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.Amount.Should().Be((decimal)(_args.Amount));
        }

        [Fact]
        public void Build_sets_ReferenceNumber()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.ReferenceNumber.Should().Be("TestReference");
        }

        [Fact]
        public void Build_sets_SmartPayFusePaymentEndpoint()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.ProfileId.Should().Be(ProfileId);
        }

        [Fact]
        public void Build_sets_SecretKeyy()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.SecretKey.Should().Be(SecretKey);
        }

        [Fact]
        public void Build_sets_OverrideBackofficePostUrl()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.OverrideBackofficePostUrl.Should().Be(PaymentPortalUrl + "/Payment/PaymentResponse");
        }

        [Fact]
        public void Build_sets_OverrideCustomCancelPage()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.OverrideCustomCancelPage.Should().Be(PaymentPortalUrl + "/Payment/PaymentResponse");
        }

        [Fact]
        public void Build_sets_OverrideCustomReceiptPage()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.OverrideCustomReceiptPage.Should().Be(PaymentPortalUrl + "/Payment/PaymentResponse");
        }

        [Fact]
        public void Build_sets_Currency()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.Currency.Should().Be("GBP");
        }

        [Fact]
        public void Build_sets_Locale()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.Locale.Should().Be("en");
        }
    }
}
