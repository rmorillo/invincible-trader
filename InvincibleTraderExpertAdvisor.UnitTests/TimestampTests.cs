using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace InvincibleTraderExpertAdvisor.UnitTests
{
    public class TimestampTests
    {
        [Fact]
        public void FromDateTimeConvertsDateTimeToTimestamp_Successfully()
        {
            //Arrange
            var fromDateTime = new DateTime(2019, 11, 14, 19, 44, 45, 899);

            //Act
            var result = Timestamp.FromDateTime(fromDateTime);

            //Assert
            Assert.Equal(20191114194445, result.dateTime);
            Assert.Equal(899, result.milliseconds);
        }

        [Fact]
        public void ToDateTimeConvertsTimestampToDateTime_Successfully()
        {
            //Arrange
            long dateTimeTimestamp = 20191114194445;
            int milliseconds = 899;            

            //Act
            var result = Timestamp.ToDateTime(dateTimeTimestamp, milliseconds);

            //Assert
            Assert.Equal(2019, result.Year);
            Assert.Equal(11, result.Month);
            Assert.Equal(14, result.Day);
            Assert.Equal(19, result.Hour);
            Assert.Equal(44, result.Minute);
            Assert.Equal(45, result.Second);
            Assert.Equal(899, result.Millisecond);
        }
    }
}
