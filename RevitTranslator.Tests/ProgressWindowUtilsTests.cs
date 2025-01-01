using Moq;
using RevitTranslator.Models;
using RevitTranslator.Utils.App;
using Xunit;

namespace RevitTranslatorAddin.Tests
{
    public class ConcurrentTranslationHandlerTests
    {
        [Fact]
        public void CalculateTotalTranslations_ShouldReturnCorrectCount()
        {
            // Arrange
            var unitGroups = new List<DocumentTranslationEntityGroup>
            {
                new DocumentTranslationEntityGroup { TranslationEntities = new List<TranslationEntity> { new TranslationEntity(), new TranslationEntity() } },
                new DocumentTranslationEntityGroup { TranslationEntities = new List<TranslationEntity> { new TranslationEntity() } }
            };
            var handler = new MultiTaskTranslationHandler(null, unitGroups, null);

            // Act
            var totalTranslations = handler.CalculateTotalTranslations(unitGroups);

            // Assert
            Assert.Equal(3, totalTranslations);
        }

        [Fact]
        public void PerformTranslation_ShouldReturnSuccessResult()
        {
            // Arrange
            var translationUtilsMock = new Mock<TranslationUtils>();
            var progressWindowUtilsMock = new Mock<ProgressWindowUtils>();
            var unitGroups = new List<DocumentTranslationEntityGroup>
            {
                new DocumentTranslationEntityGroup { TranslationEntities = new List<TranslationEntity> { new TranslationEntity(), new TranslationEntity() } }
            };
            var handler = new MultiTaskTranslationHandler(translationUtilsMock.Object, unitGroups, progressWindowUtilsMock.Object);

            // Act
            var result = handler.TranslateUnits();

            // Assert
            Assert.True(result.Completed);
            Assert.Equal(TranslationProcessResult.AbortReasons.None, result.AbortReasonResult);
            Assert.Equal(string.Empty, result.ErrorMessage);
        }

        [Fact]
        public void CreateTranslationTasks_ShouldHandleCancellation()
        {
            // Arrange
            var translationUtilsMock = new Mock<TranslationUtils>();
            var progressWindowUtilsMock = new Mock<ProgressWindowUtils>();
            var unitGroups = new List<DocumentTranslationEntityGroup>
            {
                new DocumentTranslationEntityGroup { TranslationEntities = new List<TranslationEntity> { new TranslationEntity(), new TranslationEntity() } }
            };
            var handler = new MultiTaskTranslationHandler(translationUtilsMock.Object, unitGroups, progressWindowUtilsMock.Object);
            handler._test = true; // Enable test mode

            // Simulate cancellation
            handler.TokenHandler = new CancellationTokenHandler();
            handler.TokenHandler.Create();
            handler.TokenHandler.Cts.Cancel();

            // Act
            var result = handler.TranslateUnits();

            // Assert
            Assert.False(result.Completed);
            Assert.Equal(TranslationProcessResult.AbortReasons.Canceled, result.AbortReasonResult);
            Assert.Equal("Translation process was cancelled by user", result.ErrorMessage);
        }

        [Fact]
        public void AddTranslationTask_ShouldAddTaskToList()
        {
            // Arrange
            var translationUtilsMock = new Mock<TranslationUtils>();
            var progressWindowUtilsMock = new Mock<ProgressWindowUtils>();
            var unitGroups = new List<DocumentTranslationEntityGroup>
            {
                new DocumentTranslationEntityGroup { TranslationEntities = new List<TranslationEntity> { new TranslationEntity() } }
            };
            var handler = new MultiTaskTranslationHandler(translationUtilsMock.Object, unitGroups, progressWindowUtilsMock.Object);
            handler._test = true; // Enable test mode

            // Act
            handler.CreateTranslationTasks();

            // Assert
            Assert.Single(handler._translationTasks);
        }

        [Fact]
        public void HandleAggregateException_ShouldHandleCanceledException()
        {
            // Arrange
            var translationUtilsMock = new Mock<TranslationUtils>();
            var progressWindowUtilsMock = new Mock<ProgressWindowUtils>();
            var unitGroups = new List<DocumentTranslationEntityGroup>
            {
                new DocumentTranslationEntityGroup { TranslationEntities = new List<TranslationEntity> { new TranslationEntity() } }
            };
            var handler = new MultiTaskTranslationHandler(translationUtilsMock.Object, unitGroups, progressWindowUtilsMock.Object);
            var aggregateException = new AggregateException(new OperationCanceledException());

            // Act
            handler.HandleAggregateException(aggregateException);

            // Assert
            Assert.False(handler._processResult.Completed);
            Assert.Equal(TranslationProcessResult.AbortReasons.Canceled, handler._processResult.AbortReasonResult);
            Assert.Equal("Translation process was cancelled by user", handler._processResult.ErrorMessage);
        }
    }
}