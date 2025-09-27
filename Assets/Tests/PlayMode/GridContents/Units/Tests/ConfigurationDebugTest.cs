using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using NUnit.Framework;

namespace Tests.PlayMode
{
    /// <summary>
    /// Простой тест для отладки конфигурации
    /// </summary>
    [TestFixture]
    public class ConfigurationDebugTest
    {
        [UnityTest]
        public IEnumerator TestConfigurationFlow()
        {
            Debug.Log("=== Configuration Debug Test Started ===");
            
            // Ensure SceneTransitionDataService exists for playmode test environment
            var svc = PlayModeTestSetup.EnsureSceneTransitionDataService();
            if (svc == null)
            {
                Debug.LogError("❌ SceneTransitionDataService.Instance is null!");
                yield break;
            }
            Debug.Log("✅ SceneTransitionDataService.Instance found");
            
            // Проверяем доступные конфигурации
            var availableConfigs = SceneTransitionDataService.Instance.GetAvailableConfigurations();
            if (availableConfigs == null)
            {
                Debug.LogWarning("⚠️ Available configurations is null");
            }
            else
            {
                Debug.Log($"✅ Available configurations count: {availableConfigs.Length}");
            }
            
            // Проверяем выбранную конфигурацию
            var selectedConfig = SceneTransitionDataService.Instance.GetSelectedConfiguration();
            if (selectedConfig == null)
            {
                Debug.LogWarning("⚠️ Selected configuration is null");
            }
            else
            {
                Debug.Log($"✅ Selected configuration: {selectedConfig.name}");
            }
            
            Debug.Log("=== Configuration Debug Test Completed ===");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestConfigurationProvider()
        {
            Debug.Log("=== Configuration Provider Test Started ===");
            
            if (SceneTransitionDataService.Instance == null)
            {
                Debug.LogError("❌ SceneTransitionDataService.Instance is null!");
                yield break;
            }
            
            // Создаем провайдер конфигурации
            var provider = new GameSceneConfigurationProvider(SceneTransitionDataService.Instance);
            provider.Initialize();
            
            yield return new WaitForEndOfFrame();
            
            // Проверяем провайдер
            var config = provider.GetSelectedConfiguration();
            if (config == null)
            {
                Debug.LogWarning("⚠️ Configuration provider returned null config");
            }
            else
            {
                Debug.Log($"✅ Configuration provider returned: {config.name}");
            }
            
            Debug.Log("=== Configuration Provider Test Completed ===");
            
            yield return null;
        }
    }
}
