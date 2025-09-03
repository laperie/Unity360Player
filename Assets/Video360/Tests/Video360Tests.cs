using UnityEngine;
using UnityEngine.Video;
using System.Collections;

// Simple test validation class that doesn't require NUnit framework
// To use proper Unity Test Framework, install Test Framework package from Package Manager
public class Video360Tests : MonoBehaviour
{
    [System.Serializable]
    public class TestResult
    {
        public string testName;
        public bool passed;
        public string message;
    }

    public TestResult[] testResults = new TestResult[0];
    
    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        var results = new System.Collections.Generic.List<TestResult>();
        
        results.Add(Test_Video360Config_HasValidSource_WithURL());
        results.Add(Test_Video360Config_HasValidSource_WithoutSource());
        results.Add(Test_Video360Config_GetSkyboxMaterial());
        
        testResults = results.ToArray();
        
        Debug.Log($"[Video360Tests] Completed {testResults.Length} tests");
        foreach (var result in testResults)
        {
            if (result.passed)
                Debug.Log($"✅ {result.testName}: {result.message}");
            else
                Debug.LogError($"❌ {result.testName}: {result.message}");
        }
    }
    
    TestResult Test_Video360Config_HasValidSource_WithURL()
    {
        try
        {
            var config = ScriptableObject.CreateInstance<Video360Config>();
            config.videoURL = "http://example.com/video.mp4";
            
            bool result = config.HasValidSource();
            
            DestroyImmediate(config);
            
            return new TestResult
            {
                testName = "Video360Config HasValidSource with URL",
                passed = result,
                message = result ? "URL source detected correctly" : "Failed to detect URL source"
            };
        }
        catch (System.Exception e)
        {
            return new TestResult
            {
                testName = "Video360Config HasValidSource with URL",
                passed = false,
                message = $"Exception: {e.Message}"
            };
        }
    }
    
    TestResult Test_Video360Config_HasValidSource_WithoutSource()
    {
        try
        {
            var config = ScriptableObject.CreateInstance<Video360Config>();
            config.videoURL = "";
            config.videoClip = null;
            
            bool result = config.HasValidSource();
            
            DestroyImmediate(config);
            
            return new TestResult
            {
                testName = "Video360Config HasValidSource without source",
                passed = !result,
                message = !result ? "Correctly detected no source" : "Incorrectly detected source when none provided"
            };
        }
        catch (System.Exception e)
        {
            return new TestResult
            {
                testName = "Video360Config HasValidSource without source",
                passed = false,
                message = $"Exception: {e.Message}"
            };
        }
    }
    
    TestResult Test_Video360Config_GetSkyboxMaterial()
    {
        try
        {
            var config = ScriptableObject.CreateInstance<Video360Config>();
            var monoMaterial = new Material(Shader.Find("Standard"));
            var sbsMaterial = new Material(Shader.Find("Standard"));
            var ouMaterial = new Material(Shader.Find("Standard"));
            
            config.skyboxMaterialMono = monoMaterial;
            config.skyboxMaterialSBS = sbsMaterial;
            config.skyboxMaterialOU = ouMaterial;
            
            config.stereoLayout = Video360Config.StereoLayout.None;
            bool test1 = config.GetSkyboxMaterial() == monoMaterial;
            
            config.stereoLayout = Video360Config.StereoLayout.SideBySide;
            bool test2 = config.GetSkyboxMaterial() == sbsMaterial;
            
            config.stereoLayout = Video360Config.StereoLayout.OverUnder;
            bool test3 = config.GetSkyboxMaterial() == ouMaterial;
            
            DestroyImmediate(config);
            DestroyImmediate(monoMaterial);
            DestroyImmediate(sbsMaterial);
            DestroyImmediate(ouMaterial);
            
            bool allPassed = test1 && test2 && test3;
            
            return new TestResult
            {
                testName = "Video360Config GetSkyboxMaterial",
                passed = allPassed,
                message = allPassed ? "All skybox materials returned correctly" : "Some skybox materials failed"
            };
        }
        catch (System.Exception e)
        {
            return new TestResult
            {
                testName = "Video360Config GetSkyboxMaterial",
                passed = false,
                message = $"Exception: {e.Message}"
            };
        }
    }

    void Start()
    {
        // Optionally run tests on start
        // RunAllTests();
    }
}