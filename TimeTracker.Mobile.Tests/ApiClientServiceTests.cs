//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using Moq;
//using Moq.Protected;
//using TimeTracker.Core.DTOs;
//using TimeTracker.Mobile.Services;
//using Xunit;

//namespace TimeTracker.Mobile.Tests.Services
//{
//    public class ApiClientServiceTests
//    {
//        private static HttpClient CreateMockHttpClient(HttpResponseMessage response, Action<HttpRequestMessage>? onSend = null)
//        {
//            var handlerMock = new Mock<HttpMessageHandler>();
//            handlerMock.Protected()
//                .Setup<Task<HttpResponseMessage>>(
//                    "SendAsync",
//                    ItExpr.IsAny<HttpRequestMessage>(),
//                    ItExpr.IsAny<CancellationToken>())
//                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) => {
//                    onSend?.Invoke(request);
//                    return response;
//                });

//            return new HttpClient(handlerMock.Object)
//            {
//                BaseAddress = new Uri("http://localhost/")
//            };
//        }

//        [Fact]
//        public async Task GetEmployeesAsync_ShouldReturnCachedValue_OnSecondCall()
//        {
//            // Arrange
//            var employees = new List<EmployeeDto> { new() { Id = 1, Name = "Alice" } };
//            int callCount = 0;
//            var response = new HttpResponseMessage(HttpStatusCode.OK)
//            {
//                Content = JsonContent.Create(employees)
//            };
//            var client = CreateMockHttpClient(response, _ => callCount++);
//            var service = new ApiClientService(client);

//            // Act
//            var result1 = await service.GetEmployeesAsync(); // 1er appel: réseau
//            var result2 = await service.GetEmployeesAsync(); // 2e appel: cache

//            // Assert
//            Assert.True(result1.IsSuccess);
//            Assert.True(result2.IsSuccess);
//            Assert.Equal(1, callCount); // Une seule requête HTTP
//            Assert.Equal(result1.Value, result2.Value); // Même objet (cache)
//        }

//        [Fact]
//        public async Task GetEmployeesAsync_ForceRefresh_ShouldTriggerSecondRequest()
//        {
//            // Arrange
//            var employees = new List<EmployeeDto> { new() { Id = 1, Name = "Alice" } };
//            int callCount = 0;
//            var response = new HttpResponseMessage(HttpStatusCode.OK)
//            {
//                Content = JsonContent.Create(employees)
//            };
//            var client = CreateMockHttpClient(response, _ => callCount++);
//            var service = new ApiClientService(client);

//            // Act
//            await service.GetEmployeesAsync();
//            await service.GetEmployeesAsync(forceRefresh: true);

//            // Assert
//            Assert.Equal(2, callCount); // Deux requêtes HTTP
//        }

//        [Fact]
//        public async Task GetEmployeesAsync_ShouldMutualizeTask_WhenCalledSimultaneously()
//        {
//            // Arrange
//            var employees = new List<EmployeeDto> { new() { Id = 1, Name = "Alice" } };
//            int callCount = 0;
//            var tcs = new TaskCompletionSource();
//            var response = new HttpResponseMessage(HttpStatusCode.OK)
//            {
//                Content = JsonContent.Create(employees)
//            };
//            var client = CreateMockHttpClient(response, _ => {
//                callCount++;
//                // Simule un délai réseau
//                tcs.Task.Wait();
//            });
//            var service = new ApiClientService(client);

//            // Act
//            var task1 = service.GetEmployeesAsync();
//            var task2 = service.GetEmployeesAsync();
//            tcs.SetResult();
//            await Task.WhenAll(task1, task2);

//            // Assert
//            Assert.Equal(1, callCount); // Une seule requête HTTP
//            Assert.Equal(task1.Result.Value, task2.Result.Value);
//        }
//    }
//}