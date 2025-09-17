using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Zenject;

namespace Tests.PlayMode
{
    /// <summary>
    /// Интеграционные тесты для проверки конфигурации и создания юнитов
    /// </summary>
    public class ConfigurationIntegrationTests
    {
        private SceneTransitionDataService _dataService;
        private GameController _gameController;
        private GameSceneConfigurationProvider _configProvider;
        
        [SetUp]
        public void Setup()
        {
            // Создаем тестовые объекты
            SetupTestObjects();
        }
        
        [TearDown]
        public void TearDown()
        {
            // Очищаем тестовые объекты
            CleanupTestObjects();
        }
        
        [UnityTest]
        public IEnumerator TestConfigurationLoading()
        {
            // Arrange
            var testConfigs = CreateTestConfigurations();
            _dataService.SetTransitionData("AvailableConfigs", testConfigs);
            
            // Act
            yield return new WaitForEndOfFrame();
            
            // Assert
            Assert.IsNotNull(_dataService.GetAvailableConfigurations(), "Available configurations should not be null");
            Assert.AreEqual(2, _dataService.GetAvailableConfigurations().Length, "Should have 2 test configurations");
            
            Debug.Log("✅ Configuration loading test passed");
        }
        
        [UnityTest]
        public IEnumerator TestConfigurationSelection()
        {
            // Arrange
            var testConfigs = CreateTestConfigurations();
            _dataService.SetTransitionData("AvailableConfigs", testConfigs);
            _dataService.SetSelectedConfiguration(1);
            
            // Act
            yield return new WaitForEndOfFrame();
            
            // Assert
            var selectedConfig = _dataService.GetSelectedConfiguration();
            Assert.IsNotNull(selectedConfig, "Selected configuration should not be null");
            Assert.AreEqual(1, _dataService.GetSelectedConfigurationIndex(), "Selected index should be 1");
            
            Debug.Log("✅ Configuration selection test passed");
        }
        
        [UnityTest]
        public IEnumerator TestGameControllerConfigurationProvider()
        {
            // Arrange
            var testConfigs = CreateTestConfigurations();
            _dataService.SetTransitionData("AvailableConfigs", testConfigs);
            _dataService.SetSelectedConfiguration(0);
            
            // Act
            yield return new WaitForEndOfFrame();
            
            // Assert
            Assert.IsNotNull(_configProvider, "Configuration provider should not be null");
            var selectedConfig = _configProvider.GetSelectedConfiguration();
            Assert.IsNotNull(selectedConfig, "Selected configuration from provider should not be null");
            
            Debug.Log("✅ GameController configuration provider test passed");
        }
        
        [UnityTest]
        public IEnumerator TestGridContentCreation()
        {
            // Arrange
            var testConfigs = CreateTestConfigurations();
            _dataService.SetTransitionData("AvailableConfigs", testConfigs);
            _dataService.SetSelectedConfiguration(0);
            
            // Act
            yield return new WaitForEndOfFrame();
            
            if (_gameController != null)
            {
                // Use reflection to call private method for testing
                var method = typeof(GameController).GetMethod("CreateGridContentFromConfiguration", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(_gameController, null);
            }
            
            yield return new WaitForEndOfFrame();
            
            // Assert
            Debug.Log("✅ Grid content creation test completed");
        }
        
        private void SetupTestObjects()
        {
            // Создаем SceneTransitionDataService
            var dataServiceGO = new GameObject("TestDataService");
            _dataService = dataServiceGO.AddComponent<SceneTransitionDataService>();
                
            // Создаем GameController
            var gameControllerGO = new GameObject("TestGameController");
            _gameController = gameControllerGO.AddComponent<GameController>();
            
            // Создаем ConfigurationProvider
            _configProvider = new GameSceneConfigurationProvider(_dataService);
        }
        
        private void CleanupTestObjects()
        {
            if (_dataService != null)
            {
                Object.DestroyImmediate(_dataService.gameObject);
            }
            
            if (_gameController != null)
            {
                Object.DestroyImmediate(_gameController.gameObject);
            }
        }
        
        private GridContentEntrySO[] CreateTestConfigurations()
        {
            // Создаем тестовые конфигурации
            var config1 = ScriptableObject.CreateInstance<GridContentEntrySO>();
            config1.name = "TestConfig1";
            
            var config2 = ScriptableObject.CreateInstance<GridContentEntrySO>();
            config2.name = "TestConfig2";
            
            return new GridContentEntrySO[] { config1, config2 };
        }
    }
}
