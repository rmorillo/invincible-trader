using System;
using Xunit;

namespace InvincibleTrader.Common.UnitTests
{
    public class PingCommandTests
    {
        [Fact]
        public void CallEncoding_Successful()
        {
            //Arrange
            var pingCommand = new PingCommand();

            long actual = 123456789;

            var encoded = pingCommand.EncodeCall(actual);

            //Act
            var (success, expected) = pingCommand.DecodeCall(encoded);

            //Assert
            Assert.True(success);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnEncoding_Successful()
        {
            //Arrange
            var pingCommand = new PingCommand();

            long actual = 123456789;

            var encoded = pingCommand.EncodeReturn(actual);

            //Act
            var (success, expected) = pingCommand.DecodeReturn(encoded);

            //Assert
            Assert.True(success);
            Assert.Equal(expected, actual);
        }
    }
}
