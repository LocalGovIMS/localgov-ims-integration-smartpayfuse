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

        private const string SmartPayUrl = "SmartPayUrl";
        private const string HmacKey = "FC81CC7410D19B75B6513FF413BE2E2762CE63D25BA2DFBA63A3183F796530FC";
        private const string SmartPayMerchantAccount = "SmartPayMerchantAccount";
        private const string SmartPayMerchantAccountCnp = "SmartPayMerchantAccountCNP";
        private const string SmartPaySkinCode = "SmartPaySkinCode";
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
            var smartPayUrlConfigSection = new Mock<IConfigurationSection>();
            smartPayUrlConfigSection.Setup(a => a.Value).Returns(SmartPayUrl);

            var smartPayHmacKeyConfigSection = new Mock<IConfigurationSection>();
            smartPayHmacKeyConfigSection.Setup(a => a.Value).Returns(HmacKey);

            var smartPayMerchantAccountConfigSection = new Mock<IConfigurationSection>();
            smartPayMerchantAccountConfigSection.Setup(a => a.Value).Returns(SmartPayMerchantAccount);

            var smartPayMerchantAccountCNPConfigSection = new Mock<IConfigurationSection>();
            smartPayMerchantAccountCNPConfigSection.Setup(a => a.Value).Returns(SmartPayMerchantAccountCnp);

            var smartPaySkinCodeConfigSection = new Mock<IConfigurationSection>();
            smartPaySkinCodeConfigSection.Setup(a => a.Value).Returns(SmartPaySkinCode);

            var paymentPortalUrlConfigSection = new Mock<IConfigurationSection>();
            paymentPortalUrlConfigSection.Setup(a => a.Value).Returns(PaymentPortalUrl);

            _mockConfiguration.Setup(x => x.GetSection("SmartPay:Url")).Returns(smartPayUrlConfigSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("SmartPay:HmacKey")).Returns(smartPayHmacKeyConfigSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("SmartPay:MerchantAccount")).Returns(smartPayMerchantAccountConfigSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("SmartPay:MerchantAccountCNP")).Returns(smartPayMerchantAccountCNPConfigSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("SmartPay:SkinCode")).Returns(smartPaySkinCodeConfigSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("PaymentPortalUrl")).Returns(paymentPortalUrlConfigSection.Object);
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
        public void Build_sets_PaymentAmount()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.PaymentAmount.Should().Be(((int)(_args.Amount * 100)).ToString());
        }

        [Fact]
        public void Build_sets_MerchantReference()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.MerchantReference.Should().Be("TestReference");
        }

        [Fact]
        public void Build_sets_HppUrl()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.HppUrl.Should().Be(SmartPayUrl);
        }

        [Fact]
        public void Build_sets_HmacKey()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.HmacKey.Should().Be(HmacKey);
        }

        [Fact]
        public void Build_sets_ShipBeforeDate()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.ShipBeforeDate.Should().Be(DateTime.Today.ToString("yyyy-MM-dd"));
        }

        [Fact]
        public void Build_sets_SkinCode()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.SkinCode.Should().Be(SmartPaySkinCode);
        }

        [Fact]
        public void Build_sets_ResUrl()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.ResUrl.Should().Be($"{PaymentPortalUrl}/Payment/PaymentResponse");
        }

        [Fact]
        public void Build_sets_CurrencyCode()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.CurrencyCode.Should().Be("GBP");
        }

        [Fact]
        public void Build_sets_ShopperLocale()
        {
            // Arrange
            Arrange();

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.ShopperLocale.Should().Be("en_GB");
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("MC", "MC")]
        public void Build_sets_MopCode(string transactionMopCode, string expectedMopCode)
        {
            // Arrange
            Arrange();
            _args.Transaction.MopCode = transactionMopCode;

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.PaymentMopCode.Should().Be(expectedMopCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("2010-01-01T00:00:00")]
        public void Build_sets_SessionValidity(string expiryDate)
        {
            // Arrange
            Arrange();
            _ = DateTime.TryParse(expiryDate, out var expiryDateToTry);
            _args.Transaction.ExpiryDate = expiryDateToTry;

            // Act 
            var result = _builder.Build(_args);

            // Assert
            result.SessionValidity.Should().Be(expiryDateToTry.ToString("yyyy-MM-ddTHH:mm:ssK"));
        }
    }
}
