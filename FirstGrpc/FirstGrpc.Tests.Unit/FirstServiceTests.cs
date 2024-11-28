using Basics;
using FirstGrpc.Services;
using FirstGrpc.Tests.Unit.Helpers;
using FluentAssertions;

namespace FirstGrpc.Tests.Unit
{
    public class FirstServiceTests
    {
        private readonly IFirstService sut;
        public FirstServiceTests()
        {
            sut = new FirstService();
        }

        [Fact]
        public async void Unary_ShouldReturn_an_Object()
        {
            //Arrange
            var request = new Request()
            {
                Content = "message"
            };

            var callContext = TestServerCallContext.Create();
            var expectedResponse = new Response()
            {
                Message = "messagefrom server"
            };
            //Act
            var actualResponse = sut.Unary(request, callContext);
            //Assert
            actualResponse.Should().BeEquivalentTo(expectedResponse);

        }
    }
}